using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.IO;
using JsonMetadata.Configuration;
using JsonMetadata.Models;

namespace JsonMetadata.Savers
{
  public class SeriesJsonSaver : BaseJsonSaver
  {
    public SeriesJsonSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, configurationManager, libraryManager, userManager, userDataManager, logger)
    {
    }

    protected override string GetLocalSavePath(BaseItem item)
    {
      return Path.Combine(item.Path, "tvshow.json");
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
    {
      if (!item.SupportsLocalMetadata)
      {
        return false;
      }
      return item is Series && updateType >= MinimumUpdateType;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager)
    {
      var series = item as Series;
      var output = new JsonSeries()
      {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        originaltitle = item.OriginalTitle ?? string.Empty,
        sorttitle = item.ForcedSortName ?? string.Empty,
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
        tmdbid = item.GetProviderId(MetadataProviders.Tmdb) ?? string.Empty,
        tvdbid = item.GetProviderId(MetadataProviders.Tvdb) ?? string.Empty,
        zap2itid = item.GetProviderId(MetadataProviders.Zap2It) ?? string.Empty,
        lockdata = item.IsLocked
      };
      output.genres = item.Genres;
      var people = item.SupportsPeople ? libraryManager.GetItemPeople(new InternalPeopleQuery
      {
        ItemIds = new[] { item.InternalId },
        EnableImages = options.GetJsonConfiguration().SaveImagePathsInNfo,
        EnableGuids = true,
        EnableIds = true
      }) : new List<PersonInfo>();
      output.people = new List<JsonCastCrew>();
      foreach (var person in people)
      {
        var personitem = libraryManager.GetItemById(person.Id);
        var image = person.ImageInfos.FirstOrDefault(i => i.Type == ImageType.Primary);
        var jsonperson = new JsonCastCrew();
        jsonperson.thumb = image != null ? GetImagePathToSave(image, libraryManager, options) : string.Empty;
        jsonperson.name = person.Name ?? string.Empty;
        jsonperson.id = person.Id;
        jsonperson.tmdbid = personitem.GetProviderId(MetadataProviders.Tmdb) ?? string.Empty;
        jsonperson.imdbid = personitem.GetProviderId(MetadataProviders.Imdb) ?? string.Empty;
        jsonperson.type = person.Type.ToString();
        switch (person.Type)
        {
          case PersonType.Actor:
            jsonperson.role = person.Role ?? string.Empty;
            break;
          default:
            jsonperson.role = string.Empty;
            break;
        }
        output.people.Add(jsonperson);
      }
      output.studios = item.Studios;
      output.tags = item.Tags;
      return output;
    }
  }
}
