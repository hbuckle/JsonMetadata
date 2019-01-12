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
  class SeriesJsonParser : BaseJsonParser<Series>
  {
    public SeriesJsonParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
    {
    }

    protected override void DeserializeItem(MetadataResult<Series> metadataResult, string metadataFile, ILogger logger)
    {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      using (var stream = FileSystem.OpenRead(metadataFile))
      {
        using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null))
        {
          var settings = new DataContractJsonSerializerSettings();
          settings.EmitTypeInformation = EmitTypeInformation.Never;
          settings.DateTimeFormat = new DateTimeFormat("yyyy-MM-dd");
          var serializer = new DataContractJsonSerializer(typeof(JsonSeries), settings);
          var jsonseries = serializer.ReadObject(reader) as JsonSeries;
          item.Name = jsonseries.title;
          item.OriginalTitle = jsonseries.originaltitle;
          item.ForcedSortName = jsonseries.sorttitle;
          if (!string.IsNullOrEmpty(jsonseries.status)) {
            item.Status =  (SeriesStatus)Enum.Parse(typeof(SeriesStatus), jsonseries.status);
          }
          item.CommunityRating = jsonseries.communityrating;
          item.Overview = jsonseries.overview;
          item.PremiereDate = jsonseries.releasedate;
          item.ProductionYear = jsonseries.year;
          item.EndDate = jsonseries.enddate;
          item.AirDays = jsonseries.airdays.Select(x => (DayOfWeek)Enum.Parse(typeof(DayOfWeek), x)).ToArray();
          item.AirTime = jsonseries.airtime;
          // item.RunTimeTicks = jsonseries.runtime;
          // parentalrating
          item.CustomRating = jsonseries.customrating;
          item.DisplayOrder = jsonseries.displayorder;
          item.SetProviderId(MetadataProviders.Imdb, jsonseries.imdbid);
          item.SetProviderId(MetadataProviders.Tmdb, jsonseries.tmdbid);
          item.SetProviderId(MetadataProviders.Tvdb, jsonseries.tvdbid);
          item.SetProviderId(MetadataProviders.Zap2It, jsonseries.zap2itid);
          item.Genres = jsonseries.genres;
          metadataResult.ResetPeople();
          foreach (var jsonperson in jsonseries.people)
          {
            logger.Log(LogSeverity.Info, $"JsonMetadata: Adding person {jsonperson.name}");
            var person = new PersonInfo()
            {
              Name = jsonperson.name,
              Type = (PersonType)Enum.Parse(typeof(PersonType), jsonperson.type),
              Role = jsonperson.role != null ? jsonperson.role : string.Empty,
            };
            if (!string.IsNullOrEmpty(jsonperson.thumb))
            {
              logger.Log(LogSeverity.Info, $"JsonMetadata: Adding person thumb {jsonperson.thumb}");
              var primary = new ItemImageInfo()
              {
                Path = jsonperson.thumb,
                Type = ImageType.Primary,
                Orientation = null,
                Height = 0,
                Width = 0,
                DateModified = File.GetLastWriteTimeUtc(jsonperson.thumb),
              };
              person.ImageInfos = new ItemImageInfo[] {primary};
            }
            person.SetProviderId(MetadataProviders.Tmdb, jsonperson.tmdbid);
            person.SetProviderId(MetadataProviders.Imdb, jsonperson.imdbid);
            metadataResult.AddPerson(person);
          }
          item.Studios = jsonseries.studios;
          item.Tags = jsonseries.tags;
          item.IsLocked = jsonseries.lockdata;
        }
      }
    }
  }
}