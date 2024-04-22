using System;
using System.IO;
using System.Linq;
using JsonMetadata.Models;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Logging;

namespace JsonMetadata.Savers {
  public class SeriesJsonSaver : BaseJsonSaver {
    public SeriesJsonSaver(
      IFileSystem fileSystem, IServerConfigurationManager configurationManager,
      ILibraryManager libraryManager, IUserManager userManager,
      IUserDataManager userDataManager, ILogger logger
    ) : base(
      fileSystem, configurationManager, libraryManager,
      userManager, userDataManager, logger
    ) { }

    protected override string GetLocalSavePath(BaseItem item) {
      return Path.Combine(item.Path, "tvshow.json");
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType) {
      return item is Series && item.IsFileProtocol;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager) {
      var series = item as Series;
      var output = new JsonSeries() {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        originaltitle = item.OriginalTitle ?? string.Empty,
        sorttitle = item.SortName ?? string.Empty,
        // dateadded = item.DateCreated.LocalDateTime,
        status = series.Status.HasValue ? series.Status.ToString() : string.Empty,
        communityrating = item.CommunityRating,
        overview = item.Overview ?? string.Empty,
        releasedate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        year = item.ProductionYear,
        enddate = item.EndDate.HasValue ? item.EndDate.Value.LocalDateTime : new DateTime?(),
        airdays = series.AirDays.Select(x => x.ToString()).ToList(),
        airtime = series.AirTime ?? string.Empty,
        runtime = item.RunTimeTicks.HasValue ? TimeSpan.FromTicks(item.RunTimeTicks.Value).TotalMinutes : new double?(),
        parentalrating = item.OfficialRating ?? string.Empty,
        customrating = item.CustomRating ?? string.Empty,
        displayorder = series.DisplayOrder.ToString(),
        imdbid = item.GetProviderId(MetadataProviders.Imdb) ?? string.Empty,
        tvdbid = item.GetProviderId(MetadataProviders.Tvdb) ?? string.Empty,
        zap2itid = item.GetProviderId(MetadataProviders.Zap2It) ?? string.Empty,
        lockdata = item.IsLocked,
        genres = item.Genres,
        studios = item.Studios,
        tags = item.Tags.ToList(),
      };
      if (long.TryParse(item.GetProviderId(MetadataProviders.Tmdb), out var l)) {
        output.tmdbid = l;
      }
      else {
        output.tmdbid = null;
      }
      AddPeople(item, output, libraryManager);
      return output;
    }
  }
}
