using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using JsonMetadata.Models;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Xml;
using MediaBrowser.Model.IO;

namespace JsonMetadata.Parsers {
  public class BaseJsonParser<T>
      where T : BaseItem {
    protected ILogger Logger { get; private set; }
    protected IFileSystem FileSystem { get; private set; }
    protected IProviderManager ProviderManager { get; private set; }
    protected ILibraryManager LibraryManager { get; private set; }

    private readonly IConfigurationManager _config;
    private Dictionary<string, string> _validProviderIds;

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

    public void Fetch(MetadataResult<T> metadataResult, string metadataFile, CancellationToken cancellationToken) {
      if (metadataResult == null) {
        throw new ArgumentNullException();
      }

      if (string.IsNullOrEmpty(metadataFile)) {
        throw new ArgumentNullException();
      }

      _validProviderIds = _validProviderIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

      var idInfos = ProviderManager.GetExternalIdInfos(metadataResult.Item);

      foreach (var info in idInfos) {
        var id = info.Key + "Id";
        if (!_validProviderIds.ContainsKey(id)) {
          _validProviderIds.Add(id, info.Key);
        }
      }

      //Additional Mappings
      _validProviderIds.Add("collectionnumber", "TmdbCollection");
      _validProviderIds.Add("tmdbcolid", "TmdbCollection");
      _validProviderIds.Add("imdb_id", "Imdb");

      DeserializeItem(metadataResult, metadataFile, Logger);
    }

    protected virtual void DeserializeItem(
      MetadataResult<T> metadataResult, string metadataFile, ILogger logger
    ) { }

    protected Object DeserializeToObject(XmlDictionaryReader reader, Type type) {
      var settings = new DataContractJsonSerializerSettings();
      settings.EmitTypeInformation = EmitTypeInformation.Never;
      settings.DateTimeFormat = new DateTimeFormat("yyyy-MM-dd");
      var serializer = new DataContractJsonSerializer(type, settings);
      return serializer.ReadObject(reader);
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
        person.SetProviderId(MetadataProviders.Tmdb, jsonperson.tmdbid);
        person.SetProviderId(MetadataProviders.Imdb, jsonperson.imdbid);
        metadataResult.AddPerson(person);
      }
    }
  }
}
