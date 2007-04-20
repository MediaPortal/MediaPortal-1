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