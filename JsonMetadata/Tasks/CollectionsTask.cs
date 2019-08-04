using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Tasks;
using JsonMetadata.Models;

namespace JsonMetadata.Tasks {
  public class CollectionsTask : IScheduledTask, IConfigurableScheduledTask {
    private ILibraryManager libraryManager;
    private IFileSystem fileSystem;
    private ICollectionManager collectionManager;
    private ILogger logger;
    public CollectionsTask(
      ILibraryManager libraryManager, IFileSystem fileSystem,
      ICollectionManager collectionManager, ILogger logger
    ) {
      this.libraryManager = libraryManager;
      this.fileSystem = fileSystem;
      this.collectionManager = collectionManager;
      this.logger = logger;
    }

    public bool IsHidden => false;

    public bool IsEnabled => true;

    public bool IsLogged => true;

    public string Name {
      get { return "Create collections"; }
    }

    public string Description {
      get { return "Create collections."; }
    }

    public string Category {
      get { return "Library"; }
    }

    public string Key {
      get { return "Collections"; }
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() {
      return new[]
      { 
        // Every so often
        new TaskTriggerInfo
        {
          Type = TaskTriggerInfo.TriggerInterval,
          IntervalTicks = TimeSpan.FromHours(24).Ticks,
        }
      };
    }

    protected Object DeserializeToObject(XmlDictionaryReader reader, Type type) {
      var settings = new DataContractJsonSerializerSettings();
      settings.EmitTypeInformation = EmitTypeInformation.Never;
      settings.DateTimeFormat = new DateTimeFormat("yyyy-MM-dd");
      var serializer = new DataContractJsonSerializer(type, settings);
      return serializer.ReadObject(reader);
    }

    public Task Execute(CancellationToken cancellationToken, IProgress<double> progress) {
      try {
        var dirs = Directory.EnumerateDirectories("\\\\CRUCIBLE\\Metadata\\collections");
        var collections = new List<string>();
        var items = libraryManager.GetItemList(
          new InternalItemsQuery() { IncludeItemTypes = new string[] { "BoxSet" } }
        ).ToList();
        logger.Log(LogSeverity.Info, $"JsonMetaData : Found {items.Count} collections in library");
        foreach (var dir in dirs) {
          var metadataFile = Path.Combine(dir, "collection.json");
          logger.Log(LogSeverity.Info, $"JsonMetadata: Deserializing {metadataFile}");
          JsonBoxSet json;
          using (var stream = fileSystem.OpenRead(metadataFile)) {
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(stream, Encoding.UTF8, XmlDictionaryReaderQuotas.Max, null)) {
              json = DeserializeToObject(reader, typeof(JsonBoxSet)) as JsonBoxSet;
            }
          }
          BoxSet item;
          if (items.FirstOrDefault(x => x.Name == json.title) == null) {
            var options = new CollectionCreationOptions()
            {
              IsLocked = true,
              Name = json.title,
            };
            logger.Log(LogSeverity.Info, $"JsonMetaData : Creating collection {json.title}");
            var boxset = collectionManager.CreateCollection(options);
            item = libraryManager.GetItemById(boxset.InternalId) as BoxSet;
          } else {
            logger.Log(LogSeverity.Info, $"JsonMetaData : Updating collection {json.title}");
            item = items.First(x => x.Name == json.title) as BoxSet;
          }
          item.ForcedSortName = json.sorttitle;
          item.CommunityRating = json.communityrating;
          item.Overview = json.overview;
          item.PremiereDate = json.releasedate;
          item.ProductionYear = json.year;
          item.OfficialRating = json.parentalrating;
          item.CustomRating = json.customrating;
          item.DisplayOrder = (CollectionDisplayOrder)Enum.Parse(typeof(CollectionDisplayOrder), json.displayorder);
          item.IsLocked = json.lockdata;
          item.Genres = json.genres;
          item.Studios = json.studios;
          item.Tags = json.tags;
          item.Path = dir;
          libraryManager.UpdateItem(item, item.GetParent(), ItemUpdateType.MetadataEdit);
          collectionManager.RemoveFromCollection(item.InternalId, item.GetItemList(
            new InternalItemsQuery()
          ).Select(x => x.InternalId).ToArray());
          var itemIds = new long[json.collectionitems.Count];
          for (int i = 0; i < json.collectionitems.Count; i++) {
            var internalItems = libraryManager.GetInternalItemIds(
              new InternalItemsQuery() { Path = json.collectionitems[i].path }
            );
            if (internalItems.Length > 1) {
              throw new Exception($"JsonMetaData : Multiple library items found with path {json.collectionitems[i].path}");
            } else if (internalItems.Length == 0) {
              throw new Exception($"JsonMetaData : No library items found with path {json.collectionitems[i].path}");
            } else {
              logger.Log(LogSeverity.Info, $"JsonMetaData : Adding {json.collectionitems[i].path} to collection {json.title}");
              itemIds[i] = internalItems[0];
            }
          }
          collectionManager.AddToCollection(item.InternalId, itemIds);
          collections.Add(json.title);
        }
        foreach (var item in items) {
          if (!collections.Contains(item.Name)) {
            logger.Log(LogSeverity.Info, $"JsonMetaData : Removing collection {item.Name} from library");
            libraryManager.DeleteItem(item, new DeleteOptions());
          }
        }
        return Task.CompletedTask;
      } catch (Exception e) {
        logger.Log(LogSeverity.Error, $"JsonMetaData : {e.Message}");
        return Task.CompletedTask;
      }
    }
  }
}