using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
      string extension = Path.GetExtension(item.Path);
      string name = Path.GetFileNameWithoutExtension(item.Path);
      string file = Path.ChangeExtension(item.Path, ".json");
      if (Regex.IsMatch(name, $"^[\\w\\s]+Part [1-9]$")) {
        string trimmedName = name.Substring(0, name.Length - " Part x".Length);
        if (Directory.EnumerateFiles(item.ContainingFolderPath, $"{trimmedName} Part *{extension}").Count() > 1) {
          file = Path.Combine(item.ContainingFolderPath, $"{trimmedName}.json");
        }
      }
      return file;
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
        lockdata = item.IsLocked,
        genres = item.Genres,
        studios = item.Studios,
        tags = item.Tags.ToList(),
        collections = item.Collections.Select(x => x.Name).ToList(),
      };
      if (long.TryParse(item.GetProviderId(MetadataProviders.Tmdb), out var l)) {
        output.tmdbid = l;
      }
      else {
        output.tmdbid = null;
      }
      if (long.TryParse(item.GetProviderId(MetadataProviders.TmdbCollection), out var c)) {
        output.tmdbcollectionid = c;
      }
      else {
        output.tmdbcollectionid = null;
      }
      AddPeople(item, output, libraryManager);
      return output;
    }
  }
}
