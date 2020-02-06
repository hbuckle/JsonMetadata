using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers {
  class EpisodeJsonParser : BaseJsonParser<Episode> {
    public EpisodeJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem,
      ILibraryManager libraryManager
    ) : base(
      logger, config, providerManager,
      fileSystem, libraryManager
    ) { }

    protected override void DeserializeItem(MetadataResult<Episode> metadataResult, string metadataFile, ILogger logger) {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      var json = DeserializeToObject(metadataFile, typeof(JsonEpisode)) as JsonEpisode;
      item.Name = json.title;
      item.ForcedSortName = json.sorttitle;
      item.ParentIndexNumber = json.seasonnumber;
      item.IndexNumber = json.episodenumber;
      item.CommunityRating = json.communityrating;
      item.Overview = json.overview;
      item.PremiereDate = json.releasedate;
      item.ProductionYear = json.year;
      item.OfficialRating = json.parentalrating;
      item.CustomRating = json.customrating;
      item.SetProviderId(MetadataProviders.Tvdb, json.tvdbid);
      item.IsLocked = json.lockdata;
      item.Genres = json.genres;
      AddPeople(metadataResult, json.people);
      item.Studios = json.studios;
      item.Tags = json.tags;
    }
  }
}
