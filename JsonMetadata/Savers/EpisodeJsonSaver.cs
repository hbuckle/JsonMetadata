using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System;
using System.IO;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Savers {
  public class EpisodeJsonSaver : BaseJsonSaver {
    public EpisodeJsonSaver(
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
      return item is Episode && item.IsFileProtocol;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager) {
      var episode = item as Episode;
      var output = new JsonEpisode() {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        sorttitle = item.SortName ?? string.Empty,
        // dateadded = item.DateCreated.LocalDateTime,
        seasonnumber = episode.ParentIndexNumber,
        episodenumber = episode.IndexNumber,
        communityrating = item.CommunityRating,
        overview = item.Overview ?? string.Empty,
        releasedate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        year = item.ProductionYear,
        parentalrating = item.OfficialRating ?? string.Empty,
        customrating = item.CustomRating ?? string.Empty,
        imdbid = item.GetProviderId(MetadataProviders.Imdb) ?? string.Empty,
        tvdbid = item.GetProviderId(MetadataProviders.Tvdb) ?? string.Empty,
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
