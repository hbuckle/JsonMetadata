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
  public class EpisodeJsonSaver : BaseJsonSaver
  {
    public EpisodeJsonSaver(IFileSystem fileSystem, IServerConfigurationManager configurationManager, ILibraryManager libraryManager, IUserManager userManager, IUserDataManager userDataManager, ILogger logger) : base(fileSystem, configurationManager, libraryManager, userManager, userDataManager, logger)
    {
    }

    protected override string GetLocalSavePath(BaseItem item)
    {
      return Path.ChangeExtension(item.Path, ".json");
    }

    public override bool IsEnabledFor(BaseItem item, ItemUpdateType updateType)
    {
      if (!item.SupportsLocalMetadata)
      {
        return false;
      }
      return item is Episode && updateType >= MinimumUpdateType;
    }

    protected override JsonObject SerializeItem(BaseItem item, IServerConfigurationManager options, ILibraryManager libraryManager)
    {
      var episode = item as Episode;
      var output = new JsonEpisode()
      {
        id = item.InternalId,
        title = item.Name ?? string.Empty,
        sorttitle = item.ForcedSortName ?? string.Empty,
        // dateadded = item.DateCreated.LocalDateTime,
        seasonnumber = episode.ParentIndexNumber,
        episodenumber = episode.IndexNumber,
        communityrating = item.CommunityRating,
        overview = item.Overview ?? string.Empty,
        releasedate = item.PremiereDate.HasValue ? item.PremiereDate.Value.LocalDateTime : new DateTime?(),
        year = item.ProductionYear,
        parentalrating = item.OfficialRating ?? string.Empty,
        customrating = item.CustomRating ?? string.Empty,
        imdbid = item.GetProviderId(MetadataProviders.Imdb) ?? string.Empty,
        tvdbid = item.GetProviderId(MetadataProviders.Tvdb) ?? string.Empty,
        lockdata = item.IsLocked,
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
