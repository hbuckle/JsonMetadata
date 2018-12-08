using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Audio;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using JsonMetadata.Configuration;
using JsonMetadata.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Runtime.Serialization.Json;
using MediaBrowser.Controller.Extensions;
using MediaBrowser.Model.Extensions;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Xml;

namespace JsonMetadata.Savers
{
  public abstract class BaseJsonSaver : IMetadataFileSaver
  {
    public static readonly string YouTubeWatchUrl = "https://www.youtube.com/watch?v=";

    private static readonly Dictionary<string, string> CommonTags = new[] {

                    "plot",
                    "customrating",
                    "lockdata",
                    "dateadded",
                    "title",
                    "rating",
                    "year",
                    "sorttitle",
                    "mpaa",
                    "aspectratio",
                    "collectionnumber",
                    "tmdbid",
                    "rottentomatoesid",
                    "language",
                    "tvcomid",
                    "tagline",
                    "studio",
                    "genre",
                    "tag",
                    "runtime",
                    "actor",
                    "criticrating",
                    "fileInfo",
                    "director",
                    "writer",
                    "trailer",
                    "premiered",
                    "releasedate",
                    "outline",
                    "id",
                    "credits",
                    "originaltitle",
                    "watched",
                    "playcount",
                    "lastplayed",
                    "art",
                    "resume",
                    "biography",
                    "formed",
                    "review",
                    "style",
                    "imdbid",
                    "imdb_id",
                    "country",
                    "audiodbalbumid",
                    "audiodbartistid",
                    "enddate",
                    "lockedfields",
                    "zap2itid",
                    "tvrageid",
                    "gamesdbid",
                    "musicbrainzartistid",
                    "musicbrainzalbumartistid",
                    "musicbrainzalbumid",
                    "musicbrainzreleasegroupid",
                    "tvdbid",
                    "collectionitem",
                    "isuserfavorite",
                    "userrating",
                    "countrycode"

        }.ToDictionary(i => i, StringComparer.OrdinalIgnoreCase);

    protected BaseJsonSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger)
    {
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

    protected ItemUpdateType MinimumUpdateType
    {
      get
      {
        if (ConfigurationManager.GetJsonConfiguration().SaveImagePathsInNfo)
        {
          return ItemUpdateType.ImageUpdate;
        }

        return ItemUpdateType.MetadataDownload;
      }
    }

    public string Name
    {
      get
      {
        return SaverName;
      }
    }

    public static string SaverName
    {
      get
      {
        return "Json";
      }
    }

    public string GetSavePath(BaseItem item)
    {
      return GetLocalSavePath(item);
    }

    /// <summary>
    /// Gets the save path.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>System.String.</returns>
    protected abstract string GetLocalSavePath(BaseItem item);

    /// <summary>
    /// Gets the name of the root element.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>System.String.</returns>
    protected abstract string GetRootElementName(BaseItem item);

    /// <summary>
    /// Determines whether [is enabled for] [the specified item].
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="updateType">Type of the update.</param>
    /// <returns><c>true</c> if [is enabled for] [the specified item]; otherwise, <c>false</c>.</returns>
    public abstract bool IsEnabledFor(BaseItem item, ItemUpdateType updateType);

    protected virtual List<string> GetTagsUsed(BaseItem item)
    {
      var list = new List<string>();
      foreach (var providerKey in item.ProviderIds.Keys)
      {
        var providerIdTagName = GetTagForProviderKey(providerKey);
        if (!CommonTags.ContainsKey(providerIdTagName))
        {
          list.Add(providerIdTagName);
        }
      }
      return list;
    }

    public void Save(BaseItem item, CancellationToken cancellationToken)
    {
      var path = GetSavePath(item);
      using (var memoryStream = new MemoryStream())
      {
        Save(item, memoryStream, path);

        memoryStream.Position = 0;

        cancellationToken.ThrowIfCancellationRequested();

        SaveToFile(memoryStream, path);
      }
    }

    private void SaveToFile(Stream stream, string path)
    {
      FileSystem.CreateDirectory(FileSystem.GetDirectoryName(path));
      // On Windows, saving the file will fail if the file is hidden or readonly
      FileSystem.SetAttributes(path, false, false);

      using (var filestream = FileSystem.GetFileStream(path, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.Read))
      {
        stream.CopyTo(filestream);
      }

      if (ConfigurationManager.Configuration.SaveMetadataHidden)
      {
        SetHidden(path, true);
      }
    }

    private void SetHidden(string path, bool hidden)
    {
      try
      {
        FileSystem.SetHidden(path, hidden);
      }
      catch (Exception ex)
      {
        Logger.Error("Error setting hidden attribute on {0} - {1}", path, ex.Message);
      }
    }

    private void Save(BaseItem item, Stream stream, string xmlPath)
    {
      var options = ConfigurationManager.GetJsonConfiguration();
      var serializeditem = SerializeItem(item, ConfigurationManager, LibraryManager);
      using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false, true, "  "))
      {
        var serializer = new DataContractJsonSerializer(typeof(JsonMovie));
        serializer.WriteObject(writer, serializeditem);
      }
    }

    protected abstract JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager);

    protected abstract void WriteCustomElements(BaseItem item, XmlWriter writer);

    public static void AddMediaInfo<T>(T item, XmlWriter writer)
     where T : IHasMediaSources
    {
      throw new NotImplementedException();
    }


    public const string DateAddedFormat = "yyyy-MM-dd HH:mm:ss";

    /// <summary>
    /// Gets the output trailer URL.
    /// </summary>
    /// <param name="url">The URL.</param>
    /// <returns>System.String.</returns>
    private string GetOutputTrailerUrl(string url)
    {
      // This is what xbmc expects
      return url.Replace(YouTubeWatchUrl, "plugin://plugin.video.youtube/?action=play_video&videoid=", StringComparison.OrdinalIgnoreCase);
    }

    private void AddImages(BaseItem item, XmlWriter writer, ILibraryManager libraryManager, IServerConfigurationManager config)
    {
      throw new NotImplementedException();
    }

    private void AddUserData(BaseItem item, XmlWriter writer, IUserManager userManager, IUserDataManager userDataRepo, XbmcMetadataOptions options)
    {
      throw new NotImplementedException();
    }

    protected string GetImagePathToSave(ItemImageInfo image, ILibraryManager libraryManager, IServerConfigurationManager config)
    {
      if (!image.IsLocalFile)
      {
        return image.Path;
      }

      return libraryManager.GetPathAfterNetworkSubstitution(image.Path);
    }

    private bool IsPersonType(PersonInfo person, PersonType type)
    {
      return person.Type == type;
    }

    private string GetTagForProviderKey(string providerKey)
    {
      return providerKey.ToLower() + "id";
    }
  }
}
