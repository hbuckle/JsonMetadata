using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Logging;
using JsonMetadata.Configuration;
using JsonMetadata.Models;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using MediaBrowser.Model.IO;

namespace JsonMetadata.Savers {
  public abstract class BaseJsonSaver : IMetadataFileSaver {
    protected BaseJsonSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) {
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

    /// <summary>
    /// Gets the save path.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <returns>System.String.</returns>
    protected abstract string GetLocalSavePath(BaseItem item);

    /// <summary>
    /// Determines whether [is enabled for] [the specified item].
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="updateType">Type of the update.</param>
    /// <returns><c>true</c> if [is enabled for] [the specified item]; otherwise, <c>false</c>.</returns>
    public abstract bool IsEnabledFor(BaseItem item, ItemUpdateType updateType);

    public void Save(BaseItem item, CancellationToken cancellationToken) {
      var path = GetSavePath(item);
      using (var memoryStream = new MemoryStream()) {
        Save(item, memoryStream);

        memoryStream.Position = 0;

        cancellationToken.ThrowIfCancellationRequested();

        SaveToFile(memoryStream, path);
      }
    }

    private void SaveToFile(Stream stream, string path) {
      FileSystem.CreateDirectory(FileSystem.GetDirectoryName(path));
      // On Windows, saving the file will fail if the file is hidden or readonly
      FileSystem.SetAttributes(path, false, false);

      using (var filestream = FileSystem.GetFileStream(path, FileOpenMode.Create, FileAccessMode.Write, FileShareMode.Read)) {
        stream.CopyTo(filestream);
      }

      if (ConfigurationManager.Configuration.SaveMetadataHidden) {
        SetHidden(path, true);
      }
    }

    private void SetHidden(string path, bool hidden) {
      try {
        FileSystem.SetHidden(path, hidden);
      } catch (Exception ex) {
        Logger.Error("Error setting hidden attribute on {0} - {1}", path, ex.Message);
      }
    }

    private void Save(BaseItem item, Stream stream) {
      var options = ConfigurationManager.GetJsonConfiguration();
      var serializeditem = SerializeItem(item, ConfigurationManager, LibraryManager);
      using (var writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, false, true, "  ")) {
        var settings = new DataContractJsonSerializerSettings();
        settings.EmitTypeInformation = EmitTypeInformation.Never;
        settings.DateTimeFormat = new DateTimeFormat("yyyy-MM-dd");
        var serializer = new DataContractJsonSerializer(typeof(JsonObject), settings);
        serializer.WriteObject(writer, serializeditem);
      }
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
  }
}
