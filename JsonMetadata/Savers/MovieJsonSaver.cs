using System;
using System.IO;
using System.Linq;
using JsonMetadata.Models;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;

namespace JsonMetadata.Savers {
  public class MovieJsonSaver : BaseJsonSaver {
    public MovieJsonSaver(
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
      return item is Movie && item.IsFileProtocol;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager) {
      var output = new JsonMovie() {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        originaltitle = item.OriginalTitle ?? string.Empty,
        sorttitle = item.SortName ?? string.Empty,
        // dateadded = item.DateCreated.LocalDateTime,
        communityrating = item.CommunityRating,
        criticrating = item.CriticRating,
        tagline = item.Tagline ?? string.Empty,
        overview = item.Overview ?? string.Empty,
        releasedate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        year = item.ProductionYear,
        parentalrating = item.OfficialRating ?? string.Empty,
        customrating = item.CustomRating ?? string.Empty,
        // threedformat
        imdbid = item.GetProviderId(MetadataProviders.Imdb) ?? string.Empty,
        tmdbcollectionid = item.GetProviderId(MetadataProviders.TmdbCollection) ?? string.Empty,
        lockdata = item.IsLocked,
        genres = item.Genres,
        studios = item.Studios,
        tags = item.Tags,
        collections = item.Collections.Select(x => x.Name).ToArray(),
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
