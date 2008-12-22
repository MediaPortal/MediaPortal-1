/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using System.Collections.Specialized;
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

      numericUpDownTune.Value = Convert.ToDecimal(layer.GetSetting("timeoutTune", "2").Value);
      numericUpDownPAT.Value = Convert.ToDecimal(layer.GetSetting("timeoutPAT", "5").Value);
      numericUpDownCAT.Value = Convert.ToDecimal(layer.GetSetting("timeoutCAT", "5").Value);
      numericUpDownPMT.Value = Convert.ToDecimal(layer.GetSetting("timeoutPMT", "10").Value);
      numericUpDownSDT.Value = Convert.ToDecimal(layer.GetSetting("timeoutSDT", "20").Value);
      numericUpDownAnalog.Value = Convert.ToDecimal(layer.GetSetting("timeoutAnalog", "20").Value);

      delayDetectUpDown.Value = Convert.ToDecimal(layer.GetSetting("delayCardDetect", "0").Value);

      checkBoxAlwaysFillHoles.Checked = (layer.GetSetting("generalEPGAlwaysFillHoles", "no").Value == "yes");
      checkBoxAlwaysUpdate.Checked = (layer.GetSetting("generalEPGAlwaysReplace", "no").Value == "yes");

      checkBoxEnableEPGWhileIdle.Checked = (layer.GetSetting("idleEPGGrabberEnabled", "yes").Value == "yes");
      numericUpDownEpgTimeOut.Value = Convert.ToDecimal(layer.GetSetting("timeoutEPG", "10").Value);
      numericUpDownEpgRefresh.Value = Convert.ToDecimal(layer.GetSetting("timeoutEPGRefresh", "240").Value);
      checkBoxEnableEpgWhileTimeshifting.Checked = (layer.GetSetting("timeshiftingEpgGrabberEnabled", "no").Value == "yes");
      numericUpDownTSEpgTimeout.Value = Convert.ToDecimal(layer.GetSetting("timeshiftingEpgGrabberTimeout", "2").Value);

      numericUpDownMinFiles.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftMinFiles", "6").Value), 3, 100);
      numericUpDownMaxFiles.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftMaxFiles", "20").Value), 3, 100);
      numericUpDownMaxFileSize.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftMaxFileSize", "256").Value), 20, 1024);

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
      }
      catch (Exception)
      {
        mpComboBoxPrio.SelectedIndex = 3; //fall back to default which is normal=3
      }

      edTitleTemplate.Text = layer.GetSetting("epgTitleTemplate", "%TITLE%").Value;
      edDescriptionTemplate.Text = layer.GetSetting("epgDescriptionTemplate", "%DESCRIPTION%").Value;

      numericUpDownWaitTimeshifting.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftWaitForTimeshifting", "15").Value), 1, 30);
      numericUpDownWaitUnscrambled.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftWaitForUnscrambled", "5").Value), 1, 30);
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

      s = layer.GetSetting("generalEPGAlwaysFillHoles", "no");
      if (checkBoxAlwaysFillHoles.Checked)
        s.Value = "yes";
      else
        s.Value = "no";
      s.Persist();

      s = layer.GetSetting("generalEPGAlwaysReplace", "no");
      if (checkBoxAlwaysUpdate.Checked)
        s.Value = "yes";
      else
        s.Value = "no";
      s.Persist();

      s = layer.GetSetting("idleEPGGrabberEnabled", "yes");
      if (checkBoxEnableEPGWhileIdle.Checked)
        s.Value = "yes";
      else
        s.Value = "no";
      s.Persist();

      s = layer.GetSetting("timeoutEPG", "10");
      s.Value = numericUpDownEpgTimeOut.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeoutEPGRefresh", "240");
      s.Value = numericUpDownEpgRefresh.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftingEpgGrabberEnabled", "no");
      if (checkBoxEnableEpgWhileTimeshifting.Checked)
        s.Value = "yes";
      else
        s.Value = "no";
      s.Persist();

      s = layer.GetSetting("timeshiftingEpgGrabberTimeout", "2");
      s.Value = numericUpDownTSEpgTimeout.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftMinFiles", "6");
      s.Value = numericUpDownMinFiles.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftMaxFiles", "20");
      s.Value = numericUpDownMaxFiles.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftMaxFileSize", "256");
      s.Value = numericUpDownMaxFileSize.Value.ToString();
      s.Persist();

      s = layer.GetSetting("linkageScannerEnabled", "no");
      if (checkBoxEnableLinkageScanner.Checked)
        s.Value = "yes";
      else
        s.Value = "no";
      s.Persist();

      s = layer.GetSetting("processPriority", "3");
      s.Value = mpComboBoxPrio.SelectedIndex.ToString();
      s.Persist();

      s = layer.GetSetting("epgTitleTemplate", "%TITLE%");
      s.Value = edTitleTemplate.Text;
      s.Persist();

      s = layer.GetSetting("epgDescriptionTemplate", "%DESCRIPTION%");
      s.Value = edDescriptionTemplate.Text;
      s.Persist();

      s = layer.GetSetting("timeshiftWaitForTimeshifting", "15");
      s.Value = numericUpDownWaitTimeshifting.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftWaitForUnscrambled", "5");
      s.Value = numericUpDownWaitUnscrambled.Value.ToString();
      s.Persist();

      s = layer.GetSetting("delayCardDetect", "0");
      s.Value = delayDetectUpDown.Value.ToString();
      s.Persist();
    }

    private void mpComboBoxPrio_SelectedIndexChanged(object sender, EventArgs e)
    {
      System.Diagnostics.Process process = null;
      try
      {
        process = System.Diagnostics.Process.GetProcessesByName("TVService")[0];
      }
      catch (Exception ex)
      {
        Log.Write("could not set priority on tvservice - the process might be terminated : " + ex.Message);
        return;
      }

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

    private string GetStarRatingStr(int starRating)
    {
      string rating = "<undefined>";
      switch (starRating)
      {
        case 1:
          rating = "*";
          break;
        case 2:
          rating = "*+";
          break;
        case 3:
          rating = "**";
          break;
        case 4:
          rating = "**+";
          break;
        case 5:
          rating = "***";
          break;
        case 6:
          rating = "***+";
          break;
        case 7:
          rating = "****";
          break;
      }
      return rating;
    }

    private string EvalTemplate(string template, NameValueCollection values)
    {
      for (int i = 0; i < values.Count; i++)
        template = template.Replace(values.Keys[i], values[i]);
      return template;
    }

    private void btnTest_Click(object sender, EventArgs e)
    {
      NameValueCollection defaults = new NameValueCollection();
      defaults.Add("%TITLE%", "Over the hedge");
      defaults.Add("%DESCRIPTION%", "A scheming raccoon fools a mismatched family of forest creatures into helping him repay a debt of food, by invading the new suburban sprawl that popped up while they were hibernating...and learns a lesson about family himself.");
      defaults.Add("%GENRE%", "movie/drama (general)");
      defaults.Add("%STARRATING%", "6");
      defaults.Add("%STARRATING_STR%", "***+");
      defaults.Add("%CLASSIFICATION%", "PG");
      defaults.Add("%PARENTALRATING%", "8");
      defaults.Add("%NEWLINE%", Environment.NewLine);
      edTitleTest.Text = EvalTemplate(edTitleTemplate.Text, defaults);
      edDescriptionTest.Text = EvalTemplate(edDescriptionTemplate.Text, defaults);
    }

    private decimal ValueSanityCheck(decimal Value, int Min, int Max)
    {
      if (Value < Min)
        return Min;
      if (Value > Max)
        return Max;
      return Value;
    }
  }
}