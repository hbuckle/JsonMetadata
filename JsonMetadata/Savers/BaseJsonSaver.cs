using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using JsonMetadata.Configuration;
using JsonMetadata.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using MediaBrowser.Model.IO;

namespace JsonMetadata.Savers {
  public abstract class BaseJsonSaver : IMetadataFileSaver {
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

    protected ItemUpdateType MinimumUpdateType {
      get {
        if (ConfigurationManager.GetJsonConfiguration().SaveImagePathsInNfo) {
          return ItemUpdateType.ImageUpdate;
        }
        return ItemUpdateType.MetadataDownload;
      }
    }

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

    public void Save(BaseItem item, CancellationToken cancellationToken) {
      var path = GetSavePath(item);
      var serializeditem = SerializeItem(item, ConfigurationManager, LibraryManager);
      var options = new JsonSerializerOptions
      {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
      };
      options.Converters.Add(new DateTimeConverter("yyyy-MM-dd"));
      var json = JsonSerializer.Serialize(serializeditem, serializeditem.GetType(), options);
      FileSystem.CreateDirectory(FileSystem.GetDirectoryName(path));
      FileSystem.SetAttributes(path, false, false);
      File.WriteAllText(path, json);
    }

    protected abstract JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager);

    protected string GetImagePathToSave(ItemImageInfo image, ILibraryManager libraryManager, IServerConfigurationManager config) {
      if (!image.IsLocalFile) {
        return image.Path;
      }

      return libraryManager.GetPathAfterNetworkSubstitution(
        new ReadOnlySpan<char>(image.Path.ToCharArray()), new LibraryOptions() { }
      );
    }

    protected void AddPeople(BaseItem item, JsonObject output, ILibraryManager libraryManager) {
      if (!item.SupportsPeople) { return; }
      var people = libraryManager.GetItemPeople(new InternalPeopleQuery
      {
        ItemIds = new[] { item.InternalId },
        EnableImages = true,
        EnableGuids = true,
        EnableIds = true
      });
      var outpeople = new List<JsonCastCrew>();
      foreach (var person in people) {
        var personitem = libraryManager.GetItemById(person.Id);
        var image = person.ImageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        var jsonperson = new JsonCastCrew();
        jsonperson.name = person.Name ?? string.Empty;
        jsonperson.id = person.Id;
        jsonperson.tmdbid = personitem.GetProviderId(MetadataProviders.Tmdb) ?? string.Empty;
        jsonperson.imdbid = personitem.GetProviderId(MetadataProviders.Imdb) ?? string.Empty;
        jsonperson.type = person.Type.ToString();
        jsonperson.path = personitem.Path;
        switch (person.Type) {
          case PersonType.Actor:
            jsonperson.role = person.Role ?? string.Empty;
            outpeople.Add(jsonperson);
            break;
          case PersonType.Director:
            jsonperson.role = string.Empty;
            outpeople.Insert(0, jsonperson);
            break;
          default:
            break;
        }
      }
      if (output is JsonBoxSet) {
        ((JsonBoxSet)output).people = outpeople;
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
    }
  }
}
