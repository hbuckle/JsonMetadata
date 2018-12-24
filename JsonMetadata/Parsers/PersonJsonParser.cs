using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers
{
  class PersonJsonParser : BaseJsonParser<Person>
  {
    public PersonJsonParser(ILogger logger, IConfigurationManager config, IProviderManager providerManager, IFileSystem fileSystem) : base(logger, config, providerManager, fileSystem)
    {
    }

    protected override void DeserializeItem(MetadataResult<Person> metadataResult, string metadataFile, ILogger logger)
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
          var serializer = new DataContractJsonSerializer(typeof(JsonPerson), settings);
          var jsonperson = serializer.ReadObject(reader) as JsonPerson;
          item.Name = jsonperson.name;
          item.Overview = jsonperson.overview;
          item.PremiereDate = jsonperson.birthdate;
          item.ProductionYear = jsonperson.birthyear;
          item.ProductionLocations = new string[] {jsonperson.placeofbirth};
          item.EndDate = jsonperson.deathdate;
          item.SetProviderId(MetadataProviders.Imdb, jsonperson.imdbid);
          item.SetProviderId(MetadataProviders.Tmdb, jsonperson.tmdbid);
          item.IsLocked = jsonperson.lockdata;
          foreach (var image in jsonperson.images)
          {
            var imagetype = (ImageType)Enum.Parse(typeof(ImageType), image.type);
            var exists = item.ImageInfos.FirstOrDefault(i => i.Path == image.path);
            if (exists == null)
            {
              var fsm = new FileSystemMetadata(){
                FullName = image.path,
                Exists = true,
              };
              item.AddImage(fsm, imagetype);
            }
            else
            {
              exists.Type = imagetype;
            }
          }
          item.Tags = jsonperson.tags.ToArray();
        }
      }
    }
  }
}