using System.Collections.Generic;
using System.Windows.Data;

namespace ProjectInfinity.Menu
{
  /// <summary>
  /// ViewModel for a list of <see cref="IMenuItem"/>s.
  /// </summary>
  /// <remarks>
  /// This purpose of this class is to be databound by UIs.
  /// This ViewModel will translate each item in the <see cref="Items"/> list to its 
  /// respective ViewModel.
  /// </remarks>
  /// <seealso cref="http://blogs.sqlxml.org/bryantlikes/archive/2006/09/27/WPF-Patterns.aspx"/>
  public class MenuItemViewModel
  {
    private MenuCollectionView menuView;

    public MenuItemViewModel(IList<IMenuItem> model)
    {
      menuView = new MenuCollectionView(model);
    }

    public CollectionView Items
    {
      get { return menuView; }
    }


    private class MenuCollectionView : CollectionView
    {
      public MenuCollectionView(IList<IMenuItem> model) : base(model)
      {
      }

      public override object GetItemAt(int index)
      {
        IMenuItem item = (IMenuItem) base.GetItemAt(index);

        return TranslateItem(item);
      }

      private static object TranslateItem(IMenuItem item)
      {
        IPluginItem plugin = item as IPluginItem;
        if (plugin != null)
        {
          return new PluginItemViewModel(plugin);
        }
        IMenu menu = item as IMenu;
        if (menu != null)
        {
          return new MenuViewModel(menu);
        }
        return null;
      }
      public override object CurrentItem
      {
        get { return TranslateItem((IMenuItem) base.CurrentItem); }
      }
    }
  }
}