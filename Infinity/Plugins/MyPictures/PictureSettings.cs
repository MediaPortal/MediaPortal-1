using System.Collections.ObjectModel;
using ProjectInfinity.Settings;

namespace ProjectInfinity.Pictures
{
  public class PictureSettings
  {
    private Collection<string> _pictureFolders = new Collection<string>(new string[] { @"c:\"});

    private Collection<string> _extensions = new Collection<string>(new string[] {".jpg",".gif",".png"});

    [Setting(SettingScope.Global, "")]
    public Collection<string> Extensions
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
  }
}