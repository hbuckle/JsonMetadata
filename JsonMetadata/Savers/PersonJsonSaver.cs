using System;
using System.IO;
using System.Linq;
using JsonMetadata.Models;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;

namespace JsonMetadata.Savers {
  public class PersonJsonSaver : BaseJsonSaver {
    public PersonJsonSaver(
      IFileSystem fileSystem, IServerConfigurationManager configurationManager,
      ILibraryManager libraryManager, IUserManager userManager,
      IUserDataManager userDataManager, ILogger logger
    ) : base(
      fileSystem, configurationManager, libraryManager,
      userManager, userDataManager, logger
    ) { }

    protected override string GetLocalSavePath(BaseItem item) {
      return Path.Combine(item.GetInternalMetadataPath(), "person.json");
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType) {
      return item is Person;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager) {
      var output = new JsonPerson() {
        id = item.InternalId,
        name = item.Name ?? string.Empty,
        overview = item.Overview ?? string.Empty,
        birthdate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        birthyear = item.ProductionYear,
        placeofbirth = item.ProductionLocations.Length > 0 ? item.ProductionLocations[0] : string.Empty,
        deathdate = item.EndDate.HasValue ? item.EndDate.Value.LocalDateTime : new DateTime?(),
        imdbid = item.GetProviderId(MetadataProviders.Imdb) ?? string.Empty,
        lockdata = item.IsLocked,
        tags = item.Tags.ToList(),
      };
      if (long.TryParse(item.GetProviderId(MetadataProviders.Tmdb), out var l)) {
        output.tmdbid = l;
      }
      else {
        output.tmdbid = null;
      }
      return output;
    }
  }
}
