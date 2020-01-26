using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers {
  class SeasonJsonParser : BaseJsonParser<Season> {
    public SeasonJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem,
      ILibraryManager libraryManager
    ) : base(
      logger, config, providerManager,
      fileSystem, libraryManager
    ) { }

    protected override void DeserializeItem(MetadataResult<Season> metadataResult, string metadataFile, ILogger logger) {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      var json = DeserializeToObject(item.Path, typeof(JsonSeason)) as JsonSeason;
      item.Name = json.title;
      item.ForcedSortName = json.sorttitle;
      item.IndexNumber = json.seasonnumber;
      item.CommunityRating = json.communityrating;
      item.Overview = json.overview;
      item.PremiereDate = json.releasedate;
      item.ProductionYear = json.year;
      item.OfficialRating = json.parentalrating;
      item.CustomRating = json.customrating;
      item.SetProviderId(MetadataProviders.Tvdb, json.tvdbid);
      item.Genres = json.genres;
      AddPeople(metadataResult, json.people);
      item.Studios = json.studios;
      item.Tags = json.tags;
      item.IsLocked = json.lockdata;
    }
  }
}
