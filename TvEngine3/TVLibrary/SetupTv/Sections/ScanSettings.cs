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
using System;
using System.Collections.Specialized;
using TvDatabase;
using TvLibrary.Log;

namespace SetupTv.Sections
{
  public partial class ScanSettings : SectionSettings
  {
    public ScanSettings()
      : this("General")
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

      numericUpDownTune.Value = Convert.ToDecimal(layer.GetSetting("timeoutTune", "2").Value);
      numericUpDownPAT.Value = Convert.ToDecimal(layer.GetSetting("timeoutPAT", "5").Value);
      numericUpDownCAT.Value = Convert.ToDecimal(layer.GetSetting("timeoutCAT", "5").Value);
      numericUpDownPMT.Value = Convert.ToDecimal(layer.GetSetting("timeoutPMT", "10").Value);
      numericUpDownSDT.Value = Convert.ToDecimal(layer.GetSetting("timeoutSDT", "20").Value);
      numericUpDownAnalog.Value = Convert.ToDecimal(layer.GetSetting("timeoutAnalog", "20").Value);

      delayDetectUpDown.Value = Convert.ToDecimal(layer.GetSetting("delayCardDetect", "0").Value);

      checkBoxEnableLinkageScanner.Checked = (layer.GetSetting("linkageScannerEnabled", "no").Value == "yes");

      mpComboBoxPrio.Items.Clear();
      mpComboBoxPrio.Items.Add("Realtime");
      mpComboBoxPrio.Items.Add("High");
      mpComboBoxPrio.Items.Add("Above Normal");
      mpComboBoxPrio.Items.Add("Normal");
      mpComboBoxPrio.Items.Add("Below Normal");
      mpComboBoxPrio.Items.Add("Idle");

      try
      {
        mpComboBoxPrio.SelectedIndex = Convert.ToInt32(layer.GetSetting("processPriority", "3").Value); //default is normal=3       
      } catch (Exception)
      {
        mpComboBoxPrio.SelectedIndex = 3; //fall back to default which is normal=3
      }

    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting s = layer.GetSetting("timeoutTune", "2");
      s.Value = numericUpDownTune.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutPAT", "5");
      s.Value = numericUpDownPAT.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutCAT", "5");
      s.Value = numericUpDownCAT.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutPMT", "10");
      s.Value = numericUpDownPMT.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutSDT", "20");
      s.Value = numericUpDownSDT.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutAnalog", "20");
      s.Value = numericUpDownAnalog.Value.ToString();
      s.Persist();

      s = layer.GetSetting("linkageScannerEnabled", "no");
      s.Value = checkBoxEnableLinkageScanner.Checked ? "yes" : "no";
      s.Persist();

      s = layer.GetSetting("processPriority", "3");
      s.Value = mpComboBoxPrio.SelectedIndex.ToString();
      s.Persist();

      s = layer.GetSetting("delayCardDetect", "0");
      s.Value = delayDetectUpDown.Value.ToString();
      s.Persist();
    }

    private void mpComboBoxPrio_SelectedIndexChanged(object sender, EventArgs e)
    {
      System.Diagnostics.Process process;
      try
      {
        process = System.Diagnostics.Process.GetProcessesByName("TVService")[0];
      } catch (Exception ex)
      {
        Log.Write("could not set priority on tvservice - the process might be terminated : " + ex.Message);
        return;
      }

      try
      {
        switch (mpComboBoxPrio.SelectedIndex)
        {
          case 0:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
            break;
          case 1:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
            break;
          case 2:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.AboveNormal;
            break;
          case 3:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            break;
          case 4:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.BelowNormal;
            break;
          case 5:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Idle;
            break;
          default:
            process.PriorityClass = System.Diagnostics.ProcessPriorityClass.Normal;
            break;
        }
      }
      catch (Exception exp)
      {
        Log.Write(string.Format("Could not set priority on tvservice. Error on setting process.PriorityClass: {0}", exp.Message));
        return;
      }
    }
  }
}