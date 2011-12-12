#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.Windows.Forms;
using MediaPortal.UserInterface.Controls;

namespace TvPlugin
{
  public partial class TvSetupAudioLanguageForm : MPConfigForm
  {
    public TvSetupAudioLanguageForm()
    {
      InitializeComponent();
    }

    public void InitForm(List<String> languageCodes, List<String> languages, string preferredLanguages)
    {
      mpListViewLanguages.Items.Clear();
      for (int i = 0; i < languages.Count; i++)
      {
        ListViewItem item = new ListViewItem();
        item.Text = languages[i];
        item.Tag = languageCodes[i];
        item.Checked = preferredLanguages.Contains(languageCodes[i]);
        mpListViewLanguages.Items.Add(item);
      }
    }

    public string GetConfig()
    {
      string prefLangs = "";
      foreach (ListViewItem item in mpListViewLanguages.Items)
      {
        if (item.Checked)
        {
          prefLangs += (string)item.Tag + ";";
        }
      }
      return prefLangs;
    }
  }
}