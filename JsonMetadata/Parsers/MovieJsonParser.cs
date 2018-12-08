using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
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

    protected override void DeserializeItem(MetadataResult<Movie> metadataResult, string metadataFile)
    {
      var item = metadataResult.Item;

      using (var stream = FileSystem.OpenRead(metadataFile))
      {
        using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null))
        {
          var serializer = new DataContractJsonSerializer(typeof(JsonMovie));
          var jsonmovie = serializer.ReadObject(reader) as JsonMovie;
          item.Name = jsonmovie.title;
          item.ForcedSortName = jsonmovie.sorttitle;
          item.Overview = jsonmovie.overview;
          item.SetProviderId(MetadataProviders.Tmdb, jsonmovie.tmdbid);
          item.SetProviderId(MetadataProviders.Imdb, jsonmovie.imdbid);
          item.ProductionYear = jsonmovie.year;
          item.IsLocked = jsonmovie.lockdata;
          metadataResult.ResetPeople();
          foreach (var jsonperson in jsonmovie.people)
          {
            var person = new PersonInfo()
            {
              Name = jsonperson.name,
              Type = (PersonType)Enum.Parse(typeof(PersonType), jsonperson.type),
              Role = jsonperson.role
            };
            person.SetProviderId(MetadataProviders.Tmdb, jsonperson.tmdbid);
            person.SetProviderId(MetadataProviders.Imdb, jsonperson.imdbid);
            metadataResult.AddPerson(person);
          }
        }
      }
    }
  }
}