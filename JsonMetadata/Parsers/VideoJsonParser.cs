using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers {
  class VideoJsonParser : BaseJsonParser<Video> {
    public VideoJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem,
      ILibraryManager libraryManager
    ) : base(
      logger, config, providerManager,
      fileSystem, libraryManager
    ) { }

    protected override void DeserializeItem(MetadataResult<Video> metadataResult, string metadataFile, ILogger logger) {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      var json = DeserializeToObject(metadataFile, typeof(JsonVideo)) as JsonVideo;
      item.Name = json.title;
      item.SortName = json.sorttitle;
      // item.DateCreated = new DateTimeOffset(jsonmovie.dateadded);
      item.CommunityRating = json.communityrating;
      item.Tagline = json.tagline;
      item.Overview = json.overview;
      item.PremiereDate = json.releasedate;
      item.ProductionYear = json.year;
      item.OfficialRating = json.parentalrating;
      item.CustomRating = json.customrating;
      // originalaspectratio
      // 3dformat
      item.IsLocked = json.lockdata;
      item.Genres = json.genres;
      AddPeople(metadataResult, json.people);
      item.Studios = json.studios;
      item.Tags = json.tags;
    }
  }
}
