using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System.IO;
using System;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Savers {
  public class ExtraJsonSaver : BaseJsonSaver {
    public ExtraJsonSaver(
      IFileSystem fileSystem, IServerConfigurationManager configurationManager,
      ILibraryManager libraryManager, IUserManager userManager,
      IUserDataManager userDataManager, ILogger logger
    ) : base(
      fileSystem, configurationManager, libraryManager,
      userManager, userDataManager, logger
    ) { }

    protected override string GetLocalSavePath(BaseItem item) {
      return Path.ChangeExtension(item.Path, ".json");
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType) {
      return item.ExtraType.HasValue && IsSupportedExtraType(item.ExtraType.Value);
    }

    private bool IsSupportedExtraType(ExtraType type) {
      if (type == ExtraType.Clip) {
        return true;
      }
      if (type == ExtraType.BehindTheScenes) {
        return true;
      }
      if (type == ExtraType.DeletedScene) {
        return true;
      }
      if (type == ExtraType.Interview) {
        return true;
      }
      return false;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager) {
      var output = new JsonExtra()
      {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        sorttitle = item.SortName ?? string.Empty,
        // dateadded = item.DateCreated.LocalDateTime,
        communityrating = item.CommunityRating,
        tagline = item.Tagline ?? string.Empty,
        overview = item.Overview ?? string.Empty,
        releasedate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        year = item.ProductionYear,
        parentalrating = item.OfficialRating ?? string.Empty,
        customrating = item.CustomRating ?? string.Empty,
        // threedformat
        lockdata = item.IsLocked,
        genres = item.Genres,
        studios = item.Studios,
        tags = item.Tags,
      };
      AddPeople(item, output, libraryManager);
      return output;
    }
  }
}
