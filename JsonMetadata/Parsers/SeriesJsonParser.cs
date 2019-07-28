using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.Serialization.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers {
  class SeriesJsonParser : BaseJsonParser<Series> {
    public SeriesJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem,
      ILibraryManager libraryManager
    ) : base(
      logger, config, providerManager,
      fileSystem, libraryManager
    ) { }

    protected override void DeserializeItem(MetadataResult<Series> metadataResult, string metadataFile, ILogger logger) {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      using (var stream = FileSystem.OpenRead(metadataFile)) {
        using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null)) {
          var json = DeserializeToObject(reader, typeof(JsonSeries)) as JsonSeries;
          item.Name = json.title;
          item.OriginalTitle = json.originaltitle;
          item.ForcedSortName = json.sorttitle;
          if (!string.IsNullOrEmpty(json.status)) {
            item.Status = (SeriesStatus)Enum.Parse(typeof(SeriesStatus), json.status);
          }
          item.CommunityRating = json.communityrating;
          item.Overview = json.overview;
          item.PremiereDate = json.releasedate;
          item.ProductionYear = json.year;
          item.EndDate = json.enddate;
          item.AirDays = json.airdays.Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x)).ToArray();
          item.AirTime = json.airtime;
          // item.RunTimeTicks = jsonseries.runtime;
          item.OfficialRating = json.parentalrating;
          item.CustomRating = json.customrating;
          item.DisplayOrder = (SeriesDisplayOrder)Enum.Parse(typeof(SeriesDisplayOrder), json.displayorder);
          item.SetDisplayOrder(json.displayorder);
          item.SetProviderId(MetadataProviders.Imdb, json.imdbid);
          item.SetProviderId(MetadataProviders.Tmdb, json.tmdbid);
          item.SetProviderId(MetadataProviders.Tvdb, json.tvdbid);
          item.SetProviderId(MetadataProviders.Zap2It, json.zap2itid);
          item.Genres = json.genres;
          AddPeople(metadataResult, json.people);
          item.Studios = json.studios;
          item.Tags = json.tags;
          item.IsLocked = json.lockdata;
        }
      }
    }
  }
}