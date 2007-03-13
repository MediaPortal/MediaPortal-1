/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using DirectShowLib;

using TvDatabase;

using TvControl;
using TvLibrary;
using TvLibrary.Log;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;

namespace SetupTv.Sections
{
  public partial class ScanSettings : SectionSettings
  {
    public ScanSettings()
      : this("General settings")
    {
    }
    public ScanSettings(string name)
      : base(name)
    {
      InitializeComponent();
    }
    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      textBoxTune.Text = layer.GetSetting("timeoutTune", "2").Value;
      textBoxPAT.Text = layer.GetSetting("timeoutPAT", "5").Value;
      textBoxCAT.Text = layer.GetSetting("timeoutCAT", "5").Value;
      textBoxPMT.Text = layer.GetSetting("timeoutPMT", "10").Value;
      textBoxSDT.Text = layer.GetSetting("timeoutSDT", "20").Value;
      textBoxEpgTimeOut.Text = layer.GetSetting("timeoutEPG", "10").Value;
      textBoxEPGRefresh.Text = layer.GetSetting("timeoutEPGRefresh", "240").Value;


      textBoxMinfiles.Text = layer.GetSetting("timeshiftMinFiles", "6").Value;
      textBoxMaxFiles.Text = layer.GetSetting("timeshiftMaxFiles", "20").Value;
      textBoxMaxFileSize.Text = layer.GetSetting("timeshiftMaxFileSize", "256").Value;
    }
    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting s = layer.GetSetting("timeoutTune", "2");
      s.Value = textBoxTune.Text;
      s.Persist();

      s = layer.GetSetting("timeoutPAT", "5");
      s.Value = textBoxPAT.Text;
      s.Persist();

      s = layer.GetSetting("timeoutCAT", "5");
      s.Value = textBoxCAT.Text;
      s.Persist();

      s = layer.GetSetting("timeoutPMT", "10");
      s.Value = textBoxPMT.Text;
      s.Persist();

      s = layer.GetSetting("timeoutSDT", "20");
      s.Value = textBoxSDT.Text;
      s.Persist();

      s = layer.GetSetting("timeoutEPG", "10");
      s.Value = textBoxEpgTimeOut.Text;
      s.Persist();

      s = layer.GetSetting("timeoutEPGRefresh", "240");
      s.Value = textBoxEPGRefresh.Text;
      s.Persist();

      s = layer.GetSetting("timeshiftMinFiles", "6");
      s.Value = textBoxMinfiles.Text;
      s.Persist();

      s = layer.GetSetting("timeshiftMaxFiles", "20");
      s.Value = textBoxMaxFiles.Text;
      s.Persist();

      s = layer.GetSetting("timeshiftMaxFileSize", "256");
      s.Value = textBoxMaxFileSize.Text;
      s.Persist();
    }

    private void groupBox1_Enter(object sender, EventArgs e)
    {

    }

    private void ScanSettings_Load(object sender, EventArgs e)
    {

    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {

    }

    private void label2_Click(object sender, EventArgs e)
    {

    }

    private void label16_Click(object sender, EventArgs e)
    {

    }
  }
}