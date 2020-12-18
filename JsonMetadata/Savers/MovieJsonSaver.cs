using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System.Collections.Generic;
using System.IO;
using System;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

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
      var paths = GetMovieSavePaths(new ItemInfo(item), FileSystem);
      return paths.Count == 0 ? null : paths[0];
    }

    public static List<string> GetMovieSavePaths(ItemInfo item, IFileSystem fileSystem) {
      var list = new List<string>();

      var isDvd = string.Equals(item.Container, MediaContainer.Dvd.ToString(), StringComparison.OrdinalIgnoreCase);

      if (isDvd) {
        var path = item.ContainingFolderPath;

        list.Add(Path.Combine(path, "VIDEO_TS", "VIDEO_TS.json"));
      }

      if (isDvd || string.Equals(item.Container, MediaContainer.Bluray.ToString(), StringComparison.OrdinalIgnoreCase)) {
        var path = item.ContainingFolderPath;

        list.Add(Path.Combine(path, Path.GetFileName(path) + ".json"));
      } else {
        list.Add(Path.ChangeExtension(item.Path, ".json"));

        if (!item.IsInMixedFolder) {
          list.Add(Path.Combine(item.ContainingFolderPath, "movie.json"));
        }
      }

      return list;
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType) {
      if (!item.IsSaveLocalMetadataEnabled(LibraryManager.GetLibraryOptions(item))) {
        return false;
      }
      if (item is Movie) {
        return updateType >= MinimumUpdateType;
      }
      return false;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager) {
      var output = new JsonMovie()
      {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        originaltitle = item.OriginalTitle ?? string.Empty,
        sorttitle = item.ForcedSortName ?? string.Empty,
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
      };
      if (long.TryParse(item.GetProviderId(MetadataProviders.Tmdb), out var l)) {
        output.tmdbid = l;
      } else {
        output.tmdbid = null;
      }
      AddPeople(item, output, libraryManager);
      return output;
    }
  }
}
