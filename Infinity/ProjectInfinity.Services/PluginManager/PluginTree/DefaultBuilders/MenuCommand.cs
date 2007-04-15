#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 *  Code modified from SharpDevelop AddIn code
 *  Thanks goes to: Mike Krüger
 */

#endregion

using System;
using ProjectInfinity.Localisation;

namespace ProjectInfinity.Plugins
{
  public class MenuCommand
  {
    object _caller;
    bool _visable;
    NodeItem _item;
    StringId _label;
    IPlugin _menuCommand = null;
    string _description = "";

    public string Description
    {
      get
      {
        return _description;
      }
      set
      {
        _description = value;
      }
    }

    public string Name
    {
      get { return ServiceScope.Get<ILocalisation>().ToString(_label); }
    }

    public string ImagePath
    {
      get { return _item.Properties["image"]; }
    }


    public void Execute()
    {

      if (_menuCommand == null)
      {
        try
        {
          _menuCommand = (IPlugin)_item.Plugin.CreateObject(_item.Properties["class"]);
        }
        catch (Exception e)
        {
          //MessageService.ShowError(e, "Can't create menu command : " + codon.Id);
          return;
        }
      }

      _menuCommand.Initialize();
    }

    //public ICommand Command
    //{
    //  get
    //  {
    //    if (menuCommand == null)
    //    {
    //      CreateCommand();
    //    }
    //    return menuCommand;
    //  }
    //}

    //// HACK: find a better way to allow the host app to process link commands
    //public static Converter<string, ICommand> LinkCommandCreator;

    //void CreateCommand()
    //{
    //  try
    //  {
    //    string link = codon.Properties["link"];
    //    if (link != null && link.Length > 0)
    //    {
    //      if (LinkCommandCreator == null)
    //        throw new NotSupportedException("MenuCommand.LinkCommandCreator is not set, cannot create LinkCommands.");
    //      menuCommand = LinkCommandCreator(codon.Properties["link"]);
    //    }
    //    else
    //    {
    //      menuCommand = (ICommand)codon.AddIn.CreateObject(codon.Properties["class"]);
    //    }
    //    if (menuCommand != null)
    //    {
    //      menuCommand.Owner = caller;
    //    }
    //  }
    //  catch (Exception e)
    //  {
    //    //MessageService.ShowError(e, "Can't create menu command : " + codon.Id);
    //  }
    //}

    public MenuCommand(NodeItem item, object caller)
      : this(item, caller, false)
    {

    }

    public MenuCommand(NodeItem item, object caller, bool createCommand)
    {
      this._caller = caller;
      this._item = item;
      this._label = new StringId(item.Properties["label"]);

      //if (createCommand)
      //{
      //  CreateCommand();
      //}
    }

    public MenuCommand(string label)
    {
      this._item = null;
      this._caller = null;
      this._label = new StringId(label);
    }
  }
}
