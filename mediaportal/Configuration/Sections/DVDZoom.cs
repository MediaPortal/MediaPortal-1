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
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class DVDZoom : SectionSettings
  {
    public DVDZoom()
      : this("DVD Zoom")
    {
    }

    public DVDZoom(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        pixelRatioCheckBox.Checked = xmlreader.GetValueAsBool("dvdplayer", "pixelratiocorrection", false);
        aspectRatioComboBox.Text = xmlreader.GetValueAsString("dvdplayer", "armode", "Follow stream");
        displayModeComboBox.Text = xmlreader.GetValueAsString("dvdplayer", "displaymode", "Default");

        //
        // Load all available aspect ratio
        //
        defaultZoomModeComboBox.Items.Clear();
        foreach (Geometry.Type item in Enum.GetValues(typeof(Geometry.Type)))
        {
          defaultZoomModeComboBox.Items.Add(Util.Utils.GetAspectRatio(item));
        }

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("dvdplayer", "defaultar", defaultZoomModeComboBox.Items[0].ToString());
        foreach (Geometry.Type item in Enum.GetValues(typeof(Geometry.Type)))
        {
          string currentAspectRatio = Util.Utils.GetAspectRatio(item);
          if (defaultAspectRatio == currentAspectRatio)
          {
            defaultZoomModeComboBox.SelectedItem = currentAspectRatio;
            break;
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValueAsBool("dvdplayer", "pixelratiocorrection", pixelRatioCheckBox.Checked);
        xmlwriter.SetValue("dvdplayer", "armode", aspectRatioComboBox.Text);
        xmlwriter.SetValue("dvdplayer", "displaymode", displayModeComboBox.Text);

        xmlwriter.SetValue("dvdplayer", "defaultar", defaultZoomModeComboBox.SelectedItem);
      }
    }

  }
}
