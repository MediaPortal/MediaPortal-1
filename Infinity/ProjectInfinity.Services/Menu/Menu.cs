using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectInfinity.Menu
{
  public class Menu : IMenu
  {
    private List<IMenuItem> _items = new List<IMenuItem>();
    private string _text;

    public Menu(string text)
    {
      _text = text;
    }

    #region IMenu Members

    public List<IMenuItem> Items
    {
      get { return _items; }
    }

    #endregion

    #region IMenuItem Members

    public string Text
    {
      get { return _text; }
    }

    #endregion
  }
}
