using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Tasks;

namespace JsonMetadata.Tasks
{
  public class PeoplePathTask : IScheduledTask, IConfigurableScheduledTask
  {
    private ILibraryManager libraryManager;
    private IServerConfigurationManager configurationManager;
    private ILogger logger;
    public PeoplePathTask(
      ILibraryManager libraryManager, IServerConfigurationManager configurationManager,
      ILogger logger
    )
    {
      this.libraryManager = libraryManager;
      this.configurationManager = configurationManager;
      this.logger = logger;
    }

    public bool IsHidden => false;

    public bool IsEnabled => true;

    public bool IsLogged => true;

    public string Name
    {
      get { return "Set people paths"; }
    }

    public string Description
    {
      get { return "Sets filesystem paths for people."; }
    }

    public string Category
    {
      get { return "Library"; }
    }

    public string Key
    {
      get { return "PeoplePaths"; }
    }

    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
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

    public Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
    {
      var people = libraryManager.GetPeople(new InternalItemsQuery());
      double count = 1;
      foreach (var person in people.Items)
      {
        double percent = (count / people.TotalRecordCount) * 100;
        cancellationToken.ThrowIfCancellationRequested();
        progress.Report(percent);
        var personId = person.Item1.GetProviderId(MetadataProviders.Tmdb);
        var safeName = person.Item1.Name;
        foreach (var invalidChar in System.IO.Path.GetInvalidFileNameChars())
        {
          safeName = safeName.Replace(invalidChar.ToString(), "");
        }
        var sortfolder = safeName[0];
        var personFolder = $"{safeName} ({personId})";
        var basepath = configurationManager.ApplicationPaths.InternalMetadataPath;
        var path = $"{basepath}\\People\\{sortfolder}\\{personFolder}";
        if (person.Item1.Path != path)
        {
          person.Item1.Path = path;
          libraryManager.UpdateItem(person.Item1, person.Item1.GetParent(), ItemUpdateType.MetadataImport);
          logger.Log(LogSeverity.Info, $"JsonMetadata: Setting path {path} for person {person.Item1.Name}");
        }
        count++;
      }
      return Task.CompletedTask;
    }
  }
}