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
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using Mediaportal.TV.Server.SetupTV.Sections.Helpers;
using Mediaportal.TV.Server.TVControl.ServiceAgents;
using Mediaportal.TV.Server.TVDatabase.Entities;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormSatellite : Form
  {
    public Satellite Satellite { get; set; }

    public FormSatellite()
    {
      InitializeComponent();
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void mpButtonOK_Click(object sender, EventArgs e)
    {   
      DialogResult = DialogResult.OK;

      if (Satellite == null)
      {
        Satellite = new Satellite();
      }

      Satellite.LocalTransponderFile = txtLocalTranspoderFile.Text;

      int pos;
      bool parsed = Int32.TryParse(txtPos.Text, out pos);

      if (parsed)
      {        
        MessageBox.Show(this, "Please specify position as a valid integer!", "Position");
        return;
      }

      Satellite.Position = pos;
      Satellite.Name = txtSatName.Text;
      Satellite.TransponderListUrl = txtTransponderListUrl.Text;

      ServiceAgents.Instance.CardServiceAgent.SaveSatellite(Satellite);

      Close();
    }



    private void FormTunerSatellite_Load(object sender, EventArgs e)
    {
      if (Satellite != null)
      {
        txtLocalTranspoderFile.Text = Satellite.LocalTransponderFile;
        txtPos.Text = Satellite.Position.ToString(CultureInfo.InvariantCulture);
        txtSatName.Text = Satellite.Name;
        txtTransponderListUrl.Text = Satellite.TransponderListUrl;
      }
    }


  }
}