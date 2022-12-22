using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers {
  class MovieJsonParser : BaseJsonParser<Movie> {
    public MovieJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem,
      ILibraryManager libraryManager
    ) : base(
      logger, config, providerManager,
      fileSystem, libraryManager
    ) { }

    protected override void DeserializeItem(MetadataResult<Movie> metadataResult, string metadataFile, ILogger logger) {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      var json = DeserializeToObject(metadataFile, typeof(JsonMovie)) as JsonMovie;
      item.Name = json.title;
      item.OriginalTitle = json.originaltitle;
      item.SortName = json.sorttitle;
      // item.DateCreated = new DateTimeOffset(jsonmovie.dateadded);
      item.CommunityRating = json.communityrating;
      item.CriticRating = json.criticrating;
      item.Tagline = json.tagline;
      item.Overview = json.overview;
      item.PremiereDate = json.releasedate;
      item.ProductionYear = json.year;
      item.OfficialRating = json.parentalrating;
      item.CustomRating = json.customrating;
      // originalaspectratio
      // 3dformat
      item.SetProviderId(MetadataProviders.Imdb, json.imdbid);
      if (json.tmdbid.HasValue) {
        item.SetProviderId(MetadataProviders.Tmdb, json.tmdbid.Value.ToString());
      }
      else {
        item.SetProviderId(MetadataProviders.Tmdb, string.Empty);
      }
      item.SetProviderId(MetadataProviders.TmdbCollection, json.tmdbcollectionid);
      item.IsLocked = json.lockdata;
      item.Genres = json.genres;
      AddPeople(metadataResult, json.people);
      item.Studios = json.studios;
      item.Tags = json.tags;
      item.SetCollections(json.collections);
    }
  }
}
