using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using JsonMetadata.Configuration;
using JsonMetadata.Models;

namespace JsonMetadata.Savers
{
  public class PersonJsonSaver : BaseJsonSaver
  {
    public PersonJsonSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, configurationManager, libraryManager, userManager, userDataManager, logger)
    {
    }
    protected override string GetLocalSavePath(BaseItem item)
    {
      var id = item.GetProviderId(MetadataProviders.Tmdb);
      var letter = item.Name[0];
      var basepath = ConfigurationManager.ApplicationPaths.InternalMetadataPath;
      return $"{basepath}\\People\\{letter}\\{item.Name} ({id})\\person.json";
    }
    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
    {
      if (!item.SupportsLocalMetadata)
      {
        return false;
      }
      if (item is Person)
      {
        return updateType >= MinimumUpdateType;
      }
      return false;
    }
    protected override string GetRootElementName(BaseItem item)
    {
      return "person";
    }
    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager)
    {
      var output = new JsonPerson()
      {
        id = item.InternalId,
        name = item.Name ?? string.Empty,
        overview = item.Overview ?? string.Empty,
        birthdate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        birthyear = item.ProductionYear,
        placeofbirth = item.ProductionLocations.Length > 0 ? item.ProductionLocations[0] : string.Empty,
        deathdate = item.EndDate.HasValue ? item.EndDate.Value.LocalDateTime : new DateTime?(),
        imdbid = item.GetProviderId(MetadataProviders.Imdb) ?? string.Empty,
        tmdbid = item.GetProviderId(MetadataProviders.Tmdb) ?? string.Empty,
        lockdata = item.IsLocked,
      };
      output.tags = new List<string>();
      foreach (var tag in item.Tags)
      {
        output.tags.Add(tag);
      }
      output.images = new List<JsonImage>();
      foreach (var image in item.ImageInfos)
      {
        output.images.Add(new JsonImage()
        {
          type = image.Type.ToString() ?? string.Empty,
          path = image.Path ?? string.Empty,
        });
      }
      return output;
    }
  }
}