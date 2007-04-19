using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace ProjectInfinity.Pictures
{
  internal class FullScreenPictureView : Page
  {
    public FullScreenPictureView(object model)
    {
      string fileName = @"skin\default\MyPictures\MyPicturesFullScreen_resources.xaml";
      if (File.Exists(fileName))
      {
        using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
          if (fileStream.CanRead)
          {
            Resources = (ResourceDictionary) XamlReader.Load(fileStream);
          }
        }
      }
      Background = Application.Current.Resources["backGroundBrush"] as Brush;
      fileName = @"skin\default\MyPictures\MyPicturesFullScreen.xaml";
      if (File.Exists(fileName))
      {
        using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
        {
          if (fileStream.CanRead)
          {
            Content = XamlReader.Load(fileStream);
          }
        }
      }
      DataContext = model;
    }
  }
}