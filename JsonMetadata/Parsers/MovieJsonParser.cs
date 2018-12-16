using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers
{
  class MovieJsonParser : BaseJsonParser<Movie>
  {
    public MovieJsonParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
    {
    }

    protected override void DeserializeItem(MetadataResult<Movie> metadataResult, string metadataFile, ILogger logger)
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
          var serializer = new DataContractJsonSerializer(typeof(JsonMovie), settings);
          var jsonmovie = serializer.ReadObject(reader) as JsonMovie;
          item.Name = jsonmovie.title;
          item.OriginalTitle = jsonmovie.originaltitle;
          item.ForcedSortName = jsonmovie.sorttitle;
          // item.DateCreated = new DateTimeOffset(jsonmovie.dateadded);
          item.CommunityRating = jsonmovie.communityrating;
          item.CriticRating = jsonmovie.criticrating;
          item.Tagline = jsonmovie.tagline;
          item.Overview = jsonmovie.overview;
          item.PremiereDate = jsonmovie.releasedate;
          item.ProductionYear = jsonmovie.year;
          // parentalrating
          item.CustomRating = jsonmovie.customrating;
          // originalaspectratio
          // 3dformat
          item.SetProviderId(MetadataProviders.Imdb, jsonmovie.imdbid);
          item.SetProviderId(MetadataProviders.Tmdb, jsonmovie.tmdbid);
          item.SetProviderId(MetadataProviders.TmdbCollection, jsonmovie.tmdbcollectionid);
          item.IsLocked = jsonmovie.lockdata;
          item.Genres = jsonmovie.genres;
          metadataResult.ResetPeople();
          foreach (var jsonperson in jsonmovie.people)
          {
            logger.Log(LogSeverity.Info, $"JsonMetadata: Adding person {jsonperson.name}");
            var person = new PersonInfo()
            {
              Name = jsonperson.name,
              Type = (PersonType)Enum.Parse(typeof(PersonType), jsonperson.type),
              Role = jsonperson.role != null ? jsonperson.role : string.Empty
            };
            if (!string.IsNullOrEmpty(jsonperson.thumb))
            {
              logger.Log(LogSeverity.Info, $"JsonMetadata: Adding person thumb {jsonperson.thumb}");
              var primary = new ItemImageInfo()
              {
                Path = jsonperson.thumb,
                Type = ImageType.Primary,
              };
              person.ImageInfos = new ItemImageInfo[] {primary};
            }
            person.SetProviderId(MetadataProviders.Tmdb, jsonperson.tmdbid);
            person.SetProviderId(MetadataProviders.Imdb, jsonperson.imdbid);
            metadataResult.AddPerson(person);
          }
          item.Studios = jsonmovie.studios;
          item.Tags = jsonmovie.tags;
        }
      }
    }
  }
}