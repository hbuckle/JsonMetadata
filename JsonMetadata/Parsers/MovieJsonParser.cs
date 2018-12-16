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
          // item.InternalId = jsonmovie.id;
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
          // item.ParentIndexNumber = jsonmovie.parentalrating;
          item.CustomRating = jsonmovie.customrating;
          // original aspect ratio
          // 3dformat
          item.SetProviderId(MetadataProviders.Imdb, jsonmovie.imdbid);
          item.SetProviderId(MetadataProviders.Tmdb, jsonmovie.tmdbid);
          item.SetProviderId(MetadataProviders.TmdbCollection, jsonmovie.tmdbcollectionid);
          item.IsLocked = jsonmovie.lockdata;
          foreach (var genre in jsonmovie.genres)
          {
            item.AddGenre(genre);
          }
          metadataResult.ResetPeople();
          foreach (var jsonperson in jsonmovie.people)
          {
            var person = new PersonInfo()
            {
              Name = jsonperson.name,
              Type = (PersonType)Enum.Parse(typeof(PersonType), jsonperson.type),
              Role = jsonperson.role != null ? jsonperson.role : string.Empty
            };
            var image = person.ImageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
            if (image == null)
            {
              var primary = new FileSystemMetadata(){
                FullName = jsonperson.thumb,
                Exists = true
              };
              item.AddImage(primary, ImageType.Primary);
            }
            else
            {
              image.Path = jsonperson.thumb;
            }
            person.SetProviderId(MetadataProviders.Tmdb, jsonperson.tmdbid);
            person.SetProviderId(MetadataProviders.Imdb, jsonperson.imdbid);
            metadataResult.AddPerson(person);
          }
          foreach (var studio in jsonmovie.studios)
          {
            item.AddStudio(studio);
          }
          foreach (var tag in jsonmovie.tags)
          {
            item.AddTag(tag);
          }
        }
      }
    }
  }
}