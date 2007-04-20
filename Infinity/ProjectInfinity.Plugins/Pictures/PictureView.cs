using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Pictures
{
  public class PictureView : View
  {

    public PictureView()
    {
      DataContext = new PictureViewModel();
    }

    
   }
}