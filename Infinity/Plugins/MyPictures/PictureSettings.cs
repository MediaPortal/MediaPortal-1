using System.Collections.ObjectModel;
using ProjectInfinity.Settings;

namespace ProjectInfinity.Pictures
{
  public class PictureSettings
  {
    private Collection<string> _pictureFolders = new Collection<string>(new string[] { @"c:\"});
    private string _extensions ;
    bool _createThumbnails;

    [Setting(SettingScope.Global, ".jpg,.gif,.png")]
    public string Extensions
    {
      get { return _extensions; }
      set { _extensions = value; }
    }

    [Setting(SettingScope.Global, "")]
    public Collection<string> PictureFolders
    {
      get { return _pictureFolders; }
      set { _pictureFolders = value; }
    }

    [Setting(SettingScope.User, "True")]
    public bool AutoCreateThumbnails
    {
      get { return this._createThumbnails; }
      set { this._createThumbnails = value; }
    }

  }
}