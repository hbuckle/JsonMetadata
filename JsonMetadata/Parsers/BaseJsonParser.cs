using System;
using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using JsonMetadata.Models;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;

namespace JsonMetadata.Parsers {
  public class BaseJsonParser<T> where T : BaseItem {
    public BaseJsonParser(
      ILogger logger, IConfigurationManager config,
      IProviderManager providerManager, IFileSystem fileSystem,
      ILibraryManager libraryManager
    ) {
      this.Logger = logger;
      this._config = config;
      this.ProviderManager = providerManager;
      this.FileSystem = fileSystem;
      this.LibraryManager = libraryManager;
    }

    protected ILogger Logger { get; private set; }
    protected IFileSystem FileSystem { get; private set; }
    protected IProviderManager ProviderManager { get; private set; }
    protected ILibraryManager LibraryManager { get; private set; }

    private readonly IConfigurationManager _config;

    public void Fetch(MetadataResult<T> metadataResult, string metadataFile, CancellationToken cancellationToken) {
      if (metadataResult == null) {
        throw new ArgumentNullException();
      }

      if (string.IsNullOrEmpty(metadataFile)) {
        throw new ArgumentNullException();
      }
      DeserializeItem(metadataResult, metadataFile, Logger);
    }

    protected virtual void DeserializeItem(
      MetadataResult<T> metadataResult, string metadataFile, ILogger logger
    ) { }

    protected Object DeserializeToObject(string path, Type type) {
      var json = System.IO.File.ReadAllBytes(path);
      var options = new JsonSerializerOptions();
      options.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
      options.Converters.Add(new DateTimeConverter("yyyy-MM-dd"));
      return JsonSerializer.Deserialize(json, type, options);
    }

    protected void AddPeople(MetadataResult<T> metadataResult, List<JsonCastCrew> people) {
      metadataResult.ResetPeople();
      foreach (var jsonperson in people) {
        var person = new PersonInfo()
        {
          Name = jsonperson.name,
          Type = (PersonType)Enum.Parse(typeof(PersonType), jsonperson.type),
          Role = jsonperson.role != null ? jsonperson.role : string.Empty,
        };
        if (jsonperson.tmdbid.HasValue) {
          person.SetProviderId(MetadataProviders.Tmdb, jsonperson.tmdbid.Value.ToString());
        } else { person.SetProviderId(MetadataProviders.Tmdb, string.Empty); }
        person.SetProviderId(MetadataProviders.Imdb, jsonperson.imdbid);
        metadataResult.AddPerson(person);
      }
    }
  }
}
