using System.Collections.Generic;

namespace ProjectInfinity.Menu
{
  /// <summary>
  /// Represents a (sub)menu.
  /// </summary>
  public interface IMenu : IMenuItem
  {
    /// <summary>
    /// The list of items in the menu.
    /// </summary>
    List<IMenuItem> Items { get; }

    /// <summary>
    /// The default item in the menu.
    /// </summary>
    /// <remarks>
    /// If a DefaultItem is set, a click on the (sub)menu item is redirected to that item.
    /// </remarks>
    /// <example>
    /// In Infinity, when the user clicks on the My TV menu, the first item
    /// (also called My TV) in the submenu is started.
    /// </example>
    IMenuItem DefaultItem { get; }
  }
}