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
using System.Collections;

namespace ProjectInfinity.Plugins
{
	/// <summary>
	/// Creates menu items from a location in the addin tree.
	/// </summary>
	/// <attribute name="label" use="required">
	/// Label of the menu item.
	/// </attribute>
	/// <attribute name="type" use="optional" enum="Separator;CheckBox;Item;Command;Menu;Builder">
	/// This attribute must be one of these values:
	/// Separator, CheckBox, Item=Command, Menu (=with subitems),
	/// Builder (=class implementing ISubmenuBuilder).
	/// Default: Command.
	/// </attribute>
	/// <attribute name="loadclasslazy" use="optional">
	/// Only for the type "Item"/"Command".
	/// When set to false, the command class is loaded
	/// immediately instead of the usual lazy-loading.
	/// </attribute>
	/// <attribute name="icon" use="optional">
	/// Icon of the menu item.
	/// </attribute>
	/// <attribute name="class" use="optional">
	/// Command class that is run when item is clicked.
	/// </attribute>
	/// <attribute name="link" use="optional">
	/// Only for the type "Item"/"Command". Opens a webpage instead of running a command when
	/// clicking the item.
	/// </attribute>
	/// <attribute name="shortcut" use="optional">
	/// Shortcut that activates the command (e.g. "Control|S").
	/// </attribute>
	/// <children childTypes="MenuItem">
	/// If "type" is "Menu", the item can have sub-menuitems.
	/// </children>
	/// <usage>Any menu strip paths or context menu paths, e.g. /SharpDevelop/Workbench/MainMenu</usage>
	/// <returns>
	/// Any ToolStrip* object, depending on the type attribute.
	/// </returns>
	/// <conditions>Conditions are handled by the item, "Exclude" maps to "Visible = false", "Disable" to "Enabled = false"</conditions>
	public class MenuItemBuilder : IBuilder
	{
    ///// <summary>
    ///// Gets if the doozer handles codon conditions on its own.
    ///// If this property return false, the item is excluded when the condition is not met.
    ///// </summary>
    //public bool HandleConditions {
    //  get {
    //    return true;
    //  }
    //}
		
		public object BuildItem(object caller, NodeItem item, ArrayList subItems)
		{
			string type = item.Properties.Contains("type") ? item.Properties["type"] : "Command";
			
			bool createCommand = item.Properties["loadclasslazy"] == "false";
			
			switch (type) {
        //case "Separator":
        //  return new MenuSeparator(codon, caller);
        //case "CheckBox":
        //  return new MenuCheckBox(codon, caller);
        case "Item":
        case "Command":
          return new MenuCommand(item, caller, createCommand);
        //case "Menu":
        //  return new Menu(codon, caller, subItems);
        //case "Builder":
        //  return codon.AddIn.CreateObject(codon.Properties["class"]);
				default:
					throw new System.NotSupportedException("unsupported menu item type : " + type);
			}
		}
	}
}
