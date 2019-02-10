using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers
{
  class SeasonJsonParser : BaseJsonParser<Season>
  {
    public SeasonJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem) :
      base(logger, config, providerManager, fileSystem) {}

    protected override void DeserializeItem(MetadataResult<Season> metadataResult, string metadataFile, ILogger logger)
    {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      using (var stream = FileSystem.OpenRead(metadataFile))
      {
        using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null))
        {
          var json = DeserializeToObject(reader, typeof(JsonSeason)) as JsonSeason;
          item.Name = json.title;
          item.ForcedSortName = json.sorttitle;
          item.IndexNumber = json.seasonnumber;
          item.CommunityRating = json.communityrating;
          item.Overview = json.overview;
          item.PremiereDate = json.releasedate;
          item.ProductionYear = json.year;
          // parentalrating
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
  }
}