using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using JsonMetadata.Savers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace JsonMetadata.Providers {
  public abstract class BaseJsonProvider<T> : ILocalMetadataProvider<T>, IHasItemChangeMonitor
      where T : BaseItem, new() {
    protected IFileSystem FileSystem;
    protected ILibraryManager LibraryManager;

    public Task<MetadataResult<T>> GetMetadata(ItemInfo info,
        LibraryOptions libraryOptions,
        IDirectoryService directoryService,
        CancellationToken cancellationToken) {
      var result = new MetadataResult<T>();

      var file = GetJsonFile(info, directoryService);

      if (file == null) {
        return Task.FromResult(result);
      }

      var path = file.FullName;

      try {
        result.Item = new T();

        Fetch(result, path, cancellationToken);
        result.HasMetadata = true;
      }
      catch (FileNotFoundException) {
        result.HasMetadata = false;
      }
      catch (IOException) {
        result.HasMetadata = false;
      }

      return Task.FromResult(result);
    }

    protected abstract void Fetch(MetadataResult<T> result, string path, CancellationToken cancellationToken);

    protected BaseJsonProvider(IFileSystem fileSystem, ILibraryManager libraryManager) {
      FileSystem = fileSystem;
      LibraryManager = libraryManager;
    }

    protected abstract FileSystemMetadata GetJsonFile(ItemInfo info, IDirectoryService directoryService);

    public bool HasChanged(BaseItem item, LibraryOptions libraryOptions, IDirectoryService directoryService) {
      var file = GetJsonFile(new ItemInfo(item), directoryService);

      if (file == null) {
        return false;
      }

      return file.Exists && item.IsGreaterThanDateLastSaved(FileSystem.GetLastWriteTimeUtc(file));
    }

    public virtual string Name => "BaseJsonProvider";
  }
}
