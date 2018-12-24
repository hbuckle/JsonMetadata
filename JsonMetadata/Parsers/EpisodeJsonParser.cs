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
  class EpisodeJsonParser : BaseJsonParser<Episode>
  {
    public EpisodeJsonParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
    {
    }

    protected override void DeserializeItem(MetadataResult<Episode> metadataResult, string metadataFile, ILogger logger)
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
          var serializer = new DataContractJsonSerializer(typeof(JsonEpisode), settings);
          var jsonepisode = serializer.ReadObject(reader) as JsonEpisode;
          item.Name = jsonepisode.title;
          item.ForcedSortName = jsonepisode.sorttitle;
          item.ParentIndexNumber = jsonepisode.seasonnumber;
          item.IndexNumber = jsonepisode.episodenumber;
          item.CommunityRating = jsonepisode.communityrating;
          item.Overview = jsonepisode.overview;
          item.PremiereDate = jsonepisode.releasedate;
          item.ProductionYear = jsonepisode.year;
          // parentalrating
          item.CustomRating = jsonepisode.customrating;
          // originalaspectratio
          item.SetProviderId(MetadataProviders.Tvdb, jsonepisode.tvdbid);
          item.IsLocked = jsonepisode.lockdata;
          item.Genres = jsonepisode.genres;
          metadataResult.ResetPeople();
          foreach (var jsonperson in jsonepisode.people)
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
          item.Studios = jsonepisode.studios;
          item.Tags = jsonepisode.tags;
        }
      }
    }
  }
}