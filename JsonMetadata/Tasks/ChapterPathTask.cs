using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Persistence;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;

namespace JsonMetadata.Tasks {
  public class ChapterPathTask : IScheduledTask, IConfigurableScheduledTask {
    private ILibraryManager libraryManager;
    private IServerConfigurationManager configurationManager;
    private ILogger logger;
    private IItemRepository itemRepository;
    public ChapterPathTask(
      ILibraryManager libraryManager, IServerConfigurationManager configurationManager,
      ILogger logger, IItemRepository itemRepository
    ) {
      this.libraryManager = libraryManager;
      this.configurationManager = configurationManager;
      this.logger = logger;
      this.itemRepository = itemRepository;
    }

    public bool IsHidden => false;

    public bool IsEnabled => true;

    public bool IsLogged => true;

    public string Name {
      get { return "Set chapter paths"; }
    }

    public string Description {
      get { return "Sets chapter paths."; }
    }

    public string Category {
      get { return "Library"; }
    }

    public string Key {
      get { return "ChapterPaths"; }
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers() {
      return new[]
      {
        // Every so often
        new TaskTriggerInfo
        {
          Type = TaskTriggerInfo.TriggerInterval,
          IntervalTicks = TimeSpan.FromHours(1).Ticks,
        }
      };
    }

    public Task Execute(CancellationToken cancellationToken, IProgress<double> progress) {
      var items = libraryManager.GetItemList(new InternalItemsQuery());
      var movies = items.Where(x => x is Movie);
      var episodes = items.Where(x => x is Episode);
      SaveChaptersForItems(movies);
      SaveChaptersForItems(episodes);
      return Task.CompletedTask;
    }

    private void SaveChaptersForItems(IEnumerable<BaseItem> items) {
      foreach (var item in items) {
        List<string> chapters;
        try {
          var chapterspath = Path.Combine(item.ContainingFolderPath, "Chapters", item.FileNameWithoutExtension);
          logger.Log(LogSeverity.Info, $"JsonMetadata: Chapters path for {item.Name} is {chapterspath}");
          chapters = Directory.GetFiles(chapterspath, "*.jpg").ToList();
          logger.Log(LogSeverity.Info, $"JsonMetadata: Found {chapters.Count} chapters for {item.Name}");
          chapters.Sort(
            (x, y) =>
              (float.Parse(Path.GetFileNameWithoutExtension(x))).CompareTo(float.Parse(Path.GetFileNameWithoutExtension(y)))
          );
        }
        catch (DirectoryNotFoundException) {
          chapters = new List<string>();
        }
        catch (Exception e) {
          logger.Log(LogSeverity.Info, $"JsonMetadata: Exception '{e.Message}' finding chapters for {item.Name}");
          chapters = new List<string>();
        }
        var chapterinfos = new List<ChapterInfo>();
        var number = 1;
        foreach (var chapter in chapters) {
          chapterinfos.Add(new ChapterInfo {
            ImagePath = chapter,
            StartPositionTicks = TimeSpan.FromSeconds(double.Parse(Path.GetFileNameWithoutExtension(chapter))).Ticks,
            Name = $"Chapter {number}",
            ImageDateModified = new DateTimeOffset(File.GetLastWriteTimeUtc(chapter)),
          });
          number++;
        }
        logger.Log(LogSeverity.Info, $"JsonMetadata: Saving {chapters.Count} chapters for {item.Name}");
        itemRepository.SaveChapters(item.InternalId, chapterinfos);
      }
    }
  }
}
