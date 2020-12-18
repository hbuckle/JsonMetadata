using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using System.Collections.Generic;
using System.IO;
using System;
using MediaBrowser.Model.IO;
using JsonMetadata.Models;

namespace JsonMetadata.Savers {
  public class BoxSetJsonSaver : BaseJsonSaver {
    public BoxSetJsonSaver(
      IFileSystem fileSystem, IServerConfigurationManager configurationManager,
      ILibraryManager libraryManager, IUserManager userManager,
      IUserDataManager userDataManager, ILogger logger
    ) : base(
      fileSystem, configurationManager, libraryManager,
      userManager, userDataManager, logger
    ) { }

    protected override string GetLocalSavePath(BaseItem item) {
      return Path.Combine(item.Path, "collection.json");
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType) {
      if (!item.IsSaveLocalMetadataEnabled(LibraryManager.GetLibraryOptions(item))) {
        return false;
      }
      return item is BoxSet && updateType >= MinimumUpdateType;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager) {
      var boxset = item as BoxSet;
      var output = new JsonBoxSet()
      {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        sorttitle = item.ForcedSortName ?? string.Empty,
        // dateadded = item.DateCreated.LocalDateTime,
        communityrating = item.CommunityRating,
        overview = item.Overview ?? string.Empty,
        releasedate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        year = item.ProductionYear,
        parentalrating = item.OfficialRating ?? string.Empty,
        customrating = item.CustomRating ?? string.Empty,
        displayorder = boxset.DisplayOrder.ToString(),
        lockdata = item.IsLocked,
        genres = item.Genres,
        studios = item.Studios,
        tags = item.Tags,
      };
      if (long.TryParse(item.GetProviderId(MetadataProviders.Tmdb), out var l)) {
        output.tmdbid = l;
      } else {
        output.tmdbid = null;
      }
      AddPeople(item, output, libraryManager);
      var children = boxset.GetItemList(new InternalItemsQuery());
      output.collectionitems = new List<JsonObject>();
      foreach (var child in children) {
        output.collectionitems.Add(new JsonObject()
        {
          id = child.InternalId,
          path = child.Path,
        });
      }
      output.collectionitems.Sort(delegate (JsonObject x, JsonObject y) {
        return x.path.CompareTo(y.path);
      });
      return output;
    }
  }
}
