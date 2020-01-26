using System;
using System.Linq;
using System.Text;
using System.Xml;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Parsers {
  class PersonJsonParser : BaseJsonParser<Person> {
    public PersonJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem,
      ILibraryManager libraryManager
    ) : base(
      logger, config, providerManager,
      fileSystem, libraryManager
    ) { }

    protected override void DeserializeItem(MetadataResult<Person> metadataResult, string metadataFile, ILogger logger) {
      logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
      var item = metadataResult.Item;
      var json = DeserializeToObject(item.Path, typeof(JsonPerson)) as JsonPerson;
      item.Name = json.name;
      item.Overview = json.overview;
      item.PremiereDate = json.birthdate;
      item.ProductionYear = json.birthyear;
      item.ProductionLocations = new string[] { json.placeofbirth };
      item.EndDate = json.deathdate;
      item.SetProviderId(MetadataProviders.Imdb, json.imdbid);
      item.SetProviderId(MetadataProviders.Tmdb, json.tmdbid);
      item.IsLocked = json.lockdata;
      foreach (var image in json.images) {
        var imagetype = (ImageType)Enum.Parse(typeof(ImageType), image.type);
        var exists = item.ImageInfos.FirstOrDefault(i => i.Path == image.path);
        if (exists == null) {
          var fsm = new FileSystemMetadata()
          {
            FullName = image.path,
            Exists = true,
          };
          item.AddImage(fsm, imagetype);
        } else {
          exists.Type = imagetype;
        }
      }
      item.Tags = json.tags;
    }
  }
}
