#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
 */

#endregion

using System;
using System.Collections;
using MediaPortal.GUI.Library;

namespace MediaPortal.Dialogs
{
  /// <summary>
  /// Dialog for displaying DVB CI Menu 
  /// </summary>
  public class GUIDialogCIMenu : GUIDialogMenu, IDialogbox, IRenderLayer
  {
    [SkinControl(6)]
    protected GUILabelControl lblBottom = null;

    public GUIDialogCIMenu()
    {
      GetID = (int)Window.WINDOW_DIALOG_CIMENU;
    }

    public override bool Init()
    {
      return Load(GUIGraphicsContext.Skin + @"\DialogCIMenu.xml");
    }

    public override void Reset()
    {
      base.Reset();
      showQuickNumbers = true;
      SetHeading(String.Empty, String.Empty, String.Empty); // clear all text and content
    }

    public void SetHeading(String Title, String Subtitle, String Bottom)
    {
      LoadSkin();
      AllocResources();
      InitControls();
      lblHeading.Label = Title;
      lblHeading2.Label = Subtitle;
      lblBottom.Label = Bottom;
      selectedItemIndex = -1;
      listItems.Clear();
    }
  }

}