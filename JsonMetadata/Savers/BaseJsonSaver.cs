using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using JsonMetadata.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.IO;

namespace JsonMetadata.Savers {
  public abstract class BaseJsonSaver : IMetadataSaver {
    protected BaseJsonSaver(
      IFileSystem fileSystem, IServerConfigurationManager configurationManager,
      ILibraryManager libraryManager, IUserManager userManager,
      IUserDataManager userDataManager, ILogger logger
    ) {
      Logger = logger;
      UserDataManager = userDataManager;
      UserManager = userManager;
      LibraryManager = libraryManager;
      ConfigurationManager = configurationManager;
      FileSystem = fileSystem;
    }

    protected IFileSystem FileSystem { get; private set; }
    protected IServerConfigurationManager ConfigurationManager { get; private set; }
    protected ILibraryManager LibraryManager { get; private set; }
    protected IUserManager UserManager { get; private set; }
    protected IUserDataManager UserDataManager { get; private set; }
    protected ILogger Logger { get; private set; }

    public string Name {
      get {
        return SaverName;
      }
    }

    public static string SaverName {
      get {
        return "Json";
      }
    }

    public string GetSavePath(BaseItem item) {
      return GetLocalSavePath(item);
    }
    protected abstract string GetLocalSavePath(BaseItem item);

    public abstract bool IsEnabledFor(BaseItem item, ItemUpdateType updateType);

    public Task Save(BaseItem item, LibraryOptions libraryOptions, CancellationToken cancellationToken) {
      var path = GetSavePath(item);
      var serializeditem = SerializeItem(item, ConfigurationManager, LibraryManager);
      var options = new JsonSerializerOptions {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      };
      options.Converters.Add(new DateTimeConverter("yyyy-MM-dd"));
      if (FileSystem.FileExists(path)) {
        var existingJson = System.IO.File.ReadAllBytes(path);
        var jsonObject = JsonSerializer.Deserialize(existingJson, typeof(JsonObject), options) as JsonObject;
        serializeditem.customfields = jsonObject.customfields;
      }
      var json = JsonSerializer.Serialize(serializeditem, serializeditem.GetType(), options);
      FileSystem.CreateDirectory(FileSystem.GetDirectoryName(path));
      FileSystem.SetAttributes(path, false, false);
      File.WriteAllText(path, json);
      return Task.CompletedTask;
    }

    protected abstract JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager);

    protected void AddPeople(BaseItem item, JsonObject output, ILibraryManager libraryManager) {
      if (!item.SupportsPeople) { return; }
      var people = libraryManager.GetItemPeople(new InternalPeopleQuery {
        ItemIds = new[] { item.InternalId },
        EnableImages = true,
        EnableIds = true
      });
      var outpeople = new List<JsonCastCrew>();
      foreach (var person in people) {
        var personitem = libraryManager.GetItemById(person.Id);
        var image = person.ImageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        var jsonperson = new JsonCastCrew();
        jsonperson.name = person.Name ?? string.Empty;
        jsonperson.id = person.Id;
        if (long.TryParse(personitem.GetProviderId(MetadataProviders.Tmdb), out var l)) {
          jsonperson.tmdbid = l;
        }
        else {
          jsonperson.tmdbid = null;
        }
        jsonperson.imdbid = personitem.GetProviderId(MetadataProviders.Imdb) ?? string.Empty;
        jsonperson.type = person.Type.ToString();
        switch (person.Type) {
          case PersonType.Actor:
            jsonperson.role = person.Role ?? string.Empty;
            outpeople.Add(jsonperson);
            break;
          case PersonType.Director:
            jsonperson.role = string.Empty;
            outpeople.Insert(0, jsonperson);
            break;
          case PersonType.Writer:
            jsonperson.role = string.Empty;
            outpeople.Insert(1, jsonperson);
            break;
          default:
            break;
        }
      }
      if (output is JsonCollection) {
        ((JsonCollection)output).people = outpeople;
      }
      if (output is JsonEpisode) {
        ((JsonEpisode)output).people = outpeople;
      }
      if (output is JsonMovie) {
        ((JsonMovie)output).people = outpeople;
      }
      if (output is JsonSeason) {
        ((JsonSeason)output).people = outpeople;
      }
      if (output is JsonSeries) {
        ((JsonSeries)output).people = outpeople;
      }
      if (output is JsonExtra) {
        ((JsonExtra)output).people = outpeople;
      }
    }
  }
}
