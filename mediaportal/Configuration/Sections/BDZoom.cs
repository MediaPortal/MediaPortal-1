#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public partial class BDZoom : SectionSettings
  {
    private bool _init = false;

    public BDZoom()
      : this("Blu-ray Video Zoom") { }

    public BDZoom(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      if (_init == false)
      {
        _init = true;

        //
        // Load all available aspect ratio
        //
        defaultZoomModeComboBox.Items.Clear();
        foreach (Geometry.Type item in Enum.GetValues(typeof (Geometry.Type)))
        {
          defaultZoomModeComboBox.Items.Add(Util.Utils.GetAspectRatio(item));
        }
        //
        // Change aspect ratio labels to the current core proj description
        //
        cbAllowNormal.Text = Util.Utils.GetAspectRatio(Geometry.Type.Normal);
        cbAllowOriginal.Text = Util.Utils.GetAspectRatio(Geometry.Type.Original);
        cbAllowZoom.Text = Util.Utils.GetAspectRatio(Geometry.Type.Zoom);
        cbAllowZoom149.Text = Util.Utils.GetAspectRatio(Geometry.Type.Zoom14to9);
        cbAllowStretch.Text = Util.Utils.GetAspectRatio(Geometry.Type.Stretch);
        cbAllowNonLinearStretch.Text = Util.Utils.GetAspectRatio(Geometry.Type.NonLinearStretch);
        cbAllowLetterbox.Text = Util.Utils.GetAspectRatio(Geometry.Type.LetterBox43);
        LoadSettings();
      }
    }

    public override void LoadSettings()
    {
      if (_init == false)
      {
        return;
      }

      using (Settings xmlreader = new MPSettings())
      {
        cbAllowNormal.Checked = xmlreader.GetValueAsBool("bdplayer", "allowarnormal", true);
        cbAllowOriginal.Checked = xmlreader.GetValueAsBool("bdplayer", "allowaroriginal", true);
        cbAllowZoom.Checked = xmlreader.GetValueAsBool("bdplayer", "allowarzoom", true);
        cbAllowZoom149.Checked = xmlreader.GetValueAsBool("bdplayer", "allowarzoom149", true);
        cbAllowStretch.Checked = xmlreader.GetValueAsBool("bdplayer", "allowarstretch", true);
        cbAllowNonLinearStretch.Checked = xmlreader.GetValueAsBool("bdplayer", "allowarnonlinear", true);
        cbAllowLetterbox.Checked = xmlreader.GetValueAsBool("bdplayer", "allowarletterbox", true);

        //
        // Set default aspect ratio
        //
        string defaultAspectRatio = xmlreader.GetValueAsString("movieplayer", "defaultar",
                                                               defaultZoomModeComboBox.Items[0].ToString());
        foreach (Geometry.Type item in Enum.GetValues(typeof (Geometry.Type)))
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

    public override void SaveSettings()
    {
      if (_init == false)
      {
        return;
      }

      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue("bdplayerAR", "defaultar", defaultZoomModeComboBox.SelectedItem);

        xmlwriter.SetValueAsBool("bdplayer", "allowarnormal", cbAllowNormal.Checked);
        xmlwriter.SetValueAsBool("bdplayer", "allowaroriginal", cbAllowOriginal.Checked);
        xmlwriter.SetValueAsBool("bdplayer", "allowarzoom", cbAllowZoom.Checked);
        xmlwriter.SetValueAsBool("bdplayer", "allowarzoom149", cbAllowZoom149.Checked);
        xmlwriter.SetValueAsBool("bdplayer", "allowarstretch", cbAllowStretch.Checked);
        xmlwriter.SetValueAsBool("bdplayer", "allowarnonlinear", cbAllowNonLinearStretch.Checked);
        xmlwriter.SetValueAsBool("bdplayer", "allowarletterbox", cbAllowLetterbox.Checked);
      }
    }
  }
}