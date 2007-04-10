using System.Windows.Data;

namespace ProjectInfinity.Pictures
{
  /// <summary>
  /// Helper class for getting the <see cref="PictureViewModel"/> from the <see cref="ServiceScope"/>.
  /// </summary>
  /// <remarks>
  /// <para>The sole purpose of this class it to remove the need to add the ViewModel in the
  /// DataContext of the View from code.  With the help of this class you can use an
  /// <see cref="ObjectDataProvider"/> to bind ViewModel to the View in XAML.</para>
  /// <para>In XAML it is not possible (yet) to call the Generic methods on objects.  That is
  /// why we use a fully typed wrapper method in this static class.<seealso cref="ServiceScope.Get{T}()"/></para>
  /// </remarks>
  public static class PictureViewHelper
  {
    /// <summary>
    /// Gets the instance of the <see cref="PictureViewModel"/> from the <see cref="ServiceScope"/>.
    /// </summary>
    /// <returns>The <see cref="PictureViewModel"/> that is registered in the <see cref="ServiceScope"/>.</returns>
    public static PictureViewModel GetViewModel()
    {
      return ServiceScope.Get<PictureViewModel>();
    }
  }
}