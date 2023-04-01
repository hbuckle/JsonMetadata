using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Logging;

namespace JsonMetadata.Providers {
  public sealed class PersonNormalisationProvider : ICustomMetadataProvider<Person>, IMetadataProvider<Person>, ICustomMetadataProvider, IMetadataProvider, IForcedProvider, IHasOrder {
    private readonly ILogger _logger;
    private readonly ILibraryManager _libraryManager;
    public PersonNormalisationProvider(ILogger logger, ILibraryManager libraryManager) {
      _logger = logger;
      _libraryManager = libraryManager;
    }
    public string Name => "PersonNormalisationProvider";

    public int Order => 999;

    public Task<ItemUpdateType> FetchAsync(MetadataResult<Person> metadataResult, MetadataRefreshOptions options, LibraryOptions libraryOptions, CancellationToken cancellationToken) {
      Person item = metadataResult.Item;
      if (string.IsNullOrEmpty(item.Overview)) {
        return Task.FromResult(ItemUpdateType.None);
      }
      string overview = item.Overview;
      overview = overview.Replace("&amp;", "&");
      if (item.Overview.Equals(overview, System.StringComparison.Ordinal)) {
        return Task.FromResult(ItemUpdateType.None);
      }
      else {
        _logger.Log(LogSeverity.Info, $"JsonMetadata: Normalising person {item.InternalId}");
        item.Overview = overview;
        return Task.FromResult(ItemUpdateType.MetadataEdit);
      }
    }
  }
}
