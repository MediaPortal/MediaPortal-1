using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace ProjectInfinity.Pictures
{
  public class PictureView : Page
  {

    public PictureView()
    {
      using (FileStream fileStream = new FileStream(@"skin\default\MyPictures\MyPictures_resources.xaml", FileMode.Open, FileAccess.Read))
      {
        Resources = (ResourceDictionary)XamlReader.Load(fileStream);
      }
      Background = Application.Current.Resources["backGroundBrush"] as Brush;
      using (FileStream fileStream = new FileStream(@"skin\default\MyPictures\MyPictures.xaml", FileMode.Open, FileAccess.Read))
      {
        Content = XamlReader.Load(fileStream);
      }
      DataContext = new PictureViewModel();
    }

    
   }
}