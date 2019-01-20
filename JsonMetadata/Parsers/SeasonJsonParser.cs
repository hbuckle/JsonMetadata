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
    public SeasonJsonParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
    {
    }

    protected override void DeserializeItem(MetadataResult<Season> metadataResult, string metadataFile, ILogger logger)
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
          var serializer = new DataContractJsonSerializer(typeof(JsonSeason), settings);
          var jsonseason = serializer.ReadObject(reader) as JsonSeason;
          item.Name = jsonseason.title;
          item.ForcedSortName = jsonseason.sorttitle;
          item.IndexNumber = jsonseason.seasonnumber;
          item.CommunityRating = jsonseason.communityrating;
          item.Overview = jsonseason.overview;
          item.PremiereDate = jsonseason.releasedate;
          item.ProductionYear = jsonseason.year;
          // parentalrating
          item.CustomRating = jsonseason.customrating;
          item.SetProviderId(MetadataProviders.Tvdb, jsonseason.tvdbid);
          item.Genres = jsonseason.genres;
          metadataResult.ResetPeople();
          foreach (var jsonperson in jsonseason.people)
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
          item.Studios = jsonseason.studios;
          item.Tags = jsonseason.tags;
          item.IsLocked = jsonseason.lockdata;
        }
      }
    }
  }
}