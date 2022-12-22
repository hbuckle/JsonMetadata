using System;
using System.IO;
using JsonMetadata.Models;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;

namespace JsonMetadata.Savers {
  public class CollectionJsonSaver : BaseJsonSaver {
    public CollectionJsonSaver(
      IFileSystem fileSystem, IServerConfigurationManager configurationManager,
      ILibraryManager libraryManager, IUserManager userManager,
      IUserDataManager userDataManager, ILogger logger
    ) : base(
      fileSystem, configurationManager, libraryManager,
      userManager, userDataManager, logger
    ) { }

    protected override string GetLocalSavePath(BaseItem item) {
      return Path.Combine(item.GetInternalMetadataPath(), "collection.json");
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType) {
      return item is BoxSet;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager) {
      var boxset = item as BoxSet;
      var output = new JsonCollection() {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        sorttitle = item.SortName ?? string.Empty,
        // dateadded = item.DateCreated.LocalDateTime,
        communityrating = item.CommunityRating,
        overview = item.Overview ?? string.Empty,
        releasedate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        year = item.ProductionYear,
        parentalrating = item.OfficialRating ?? string.Empty,
        customrating = item.CustomRating ?? string.Empty,
        displayorder = boxset.DisplayOrder.ToString(),
        lockdata = item.IsLocked,
        genres = item.Genres,
        studios = item.Studios,
        tags = item.Tags,
      };
      if (long.TryParse(item.GetProviderId(MetadataProviders.Tmdb), out var l)) {
        output.tmdbid = l;
      }
      else {
        output.tmdbid = null;
      }
      AddPeople(item, output, libraryManager);
      return output;
    }
  }
}
