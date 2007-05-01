using System;
using System.Collections.Generic;
using ProjectInfinity.Controls;

namespace ProjectInfinity.Menu
{
  /// <summary>
  /// Helper class to translate a tree of <see cref="IMenuItem"/>s to a tree of 
  /// <see cref="MenuItem"/> controls that the <see cref="Controls.Menu"/> control can use.
  /// </summary>
  /// <remarks>
  /// This class implements the Visitor Pattern <seealso Visitor Pattern cref="http://www.dofactory.com/Patterns/PatternVisitor.aspx"/>
  /// </remarks>
  internal class MenuViewCreator : IMenuItemVisitor
  {
    private readonly MenuCollection _menu = new MenuCollection();

    private MenuViewCreator()
    {}

    /// <summary>
    /// Gets the result of the conversion.
    /// </summary>
    internal MenuCollection Result
    {
      get { return _menu; }
    }

    #region IMenuItemVisitor Members

    /// <summary>
    /// Converts an <see cref="IMenu"/> implementation, and all of its items.
    /// </summary>
    /// <param name="menu">The <see cref="IMenu"/> to convert.</param>
    public void Visit(IMenu menu)
    {
      //Create a MenuMenuItem control for the item
      MenuItem menuItem = new MenuMenuItem(menu);
      //Add all the submenu items
      MenuViewCreator creator = new MenuViewCreator();
      foreach (IMenuItem item in menu.Items)
      {
        //Visitor Pattern:
        //We pass the MenuViewCreator to the item and the item will call the correct overload
        //of its Visit method.  Look ma, no Casting ;-)
        item.Accept(creator);
      }
      menuItem.SubMenus = creator.Result;
      _menu.Add(menuItem);
    }

    /// <summary>
    /// Converts an <see cref="IPluginItem"/> implementation.
    /// </summary>
    /// <param name="plugin">The <see cref="IPluginItem"/> convert.</param>
    public void Visit(IPluginItem plugin)
    {
      MenuItem menuItem = new MenuMenuItem(plugin);
      _menu.Add(menuItem);
    }

    /// <summary>
    /// Converts an <see cref="IMessageItem"/> implementation.
    /// </summary>
    /// <param name="message">The <see cref="IMessageItem"/> convert.</param>
    public void Visit(IMessageItem message)
    {
      MenuItem menuItem = new MenuMenuItem(message);
      _menu.Add(menuItem);
    }

    /// <summary>
    /// Converts an <see cref="ICommandItem"/> implementation.
    /// </summary>
    /// <param name="command">The <see cref="ICommandItem"/> convert.</param>
    public void Visit(ICommandItem command)
    {
      MenuItem menuItem = new MenuMenuItem(command);
      _menu.Add(menuItem);
    }

    [Obsolete("This overload should not be used. Use one of the specified overloads instead")]
    public void Visit(IMenuItem item)
    {
      throw new NotSupportedException();
    }

    #endregion

    /// <summary>
    /// Builds a tree of <see cref="MenuItem"/> controls, based on the passed list of <see cref="IMenuItem"/>s.
    /// </summary>
    /// <param name="menuItems">The list of <see cref="IMenuItem"/>s to convert.</param>
    /// <returns>A <see cref="MenuCollection"/> containing the <see cref="MenuItem"/> controls.</returns>
    public static MenuCollection Build(IList<IMenuItem> menuItems)
    {
      MenuViewCreator bld = new MenuViewCreator();
      foreach (IMenuItem item in menuItems)
      {
        //Visitor Pattern:
        //We pass the MenuViewCreator to the item and the item will call the correct overload
        //of its Visit method.  Look ma, no Casting ;-)
        item.Accept(bld);
      }
      return bld.Result;
    }
  }
}