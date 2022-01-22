using System;
using System.IO;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Drawing;

namespace JsonMetadata {
  public class Plugin : BasePlugin, IHasThumbImage {
    private Guid _id = new Guid("4678ee1d-91c7-47c3-9da6-fe48d2695e94");
    public override Guid Id {
      get { return _id; }
    }

    public override string Name {
      get { return StaticName; }
    }

    public static string StaticName {
      get { return "Json Metadata"; }
    }

    public override string Description {
      get {
        return "Json metadata support";
      }
    }

    public Stream GetThumbImage() {
      var type = GetType();
      return type.Assembly.GetManifestResourceStream(type.Namespace + ".thumb.png");
    }

    public ImageFormat ThumbImageFormat {
      get {
        return ImageFormat.Png;
      }
    }
  }
}
