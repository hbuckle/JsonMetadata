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
      var json = DeserializeToObject(metadataFile, typeof(JsonPerson)) as JsonPerson;
      item.Name = json.name;
      item.Overview = json.overview;
      item.PremiereDate = json.birthdate;
      item.ProductionYear = json.birthyear;
      item.ProductionLocations = new string[] { json.placeofbirth };
      item.EndDate = json.deathdate;
      item.SetProviderId(MetadataProviders.Imdb, json.imdbid);
      if (json.tmdbid.HasValue) {
        item.SetProviderId(MetadataProviders.Tmdb, json.tmdbid.Value.ToString());
      }
      else {
        item.SetProviderId(MetadataProviders.Tmdb, string.Empty);
      }
      item.IsLocked = json.lockdata;
      item.Tags = json.tags.ToArray();
    }
  }
}
