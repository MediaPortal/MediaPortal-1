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
      textBoxTune.Text = layer.GetSetting("timeoutTune", "2").Value;
      textBoxPAT.Text = layer.GetSetting("timeoutPAT", "5").Value;
      textBoxCAT.Text = layer.GetSetting("timeoutCAT", "5").Value;
      textBoxPMT.Text = layer.GetSetting("timeoutPMT", "10").Value;
      textBoxSDT.Text = layer.GetSetting("timeoutSDT", "20").Value;

      checkBoxEnableEPGWhileIdle.Checked=(layer.GetSetting("idleEPGGrabberEnabled", "yes").Value == "yes");
      textBoxEpgTimeOut.Text = layer.GetSetting("timeoutEPG", "10").Value;
      textBoxEPGRefresh.Text = layer.GetSetting("timeoutEPGRefresh", "240").Value;

      checkBoxEnableEpgWhileTimeshifting.Checked = (layer.GetSetting("timeshiftingEpgGrabberEnabled", "no").Value == "yes");
      textBoxTSEpgTimeout.Text = layer.GetSetting("timeshiftingEpgGrabberTimeout", "2").Value;


      textBoxMinfiles.Text = layer.GetSetting("timeshiftMinFiles", "6").Value;
      textBoxMaxFiles.Text = layer.GetSetting("timeshiftMaxFiles", "20").Value;
      textBoxMaxFileSize.Text = layer.GetSetting("timeshiftMaxFileSize", "256").Value;

      checkBoxEnableLinkageScanner.Checked=(layer.GetSetting("linkageScannerEnabled","no").Value=="yes");

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
      catch (Exception e)
      {        
        mpComboBoxPrio.SelectedIndex = 3; //fall back to default which is normal=3
      }

      edTitleTemplate.Text = layer.GetSetting("epgTitleTemplate", "%TITLE%").Value;
      edDescriptionTemplate.Text = layer.GetSetting("epgDescriptionTemplate", "%DESCRIPTION%").Value;

			textBoxWaitTimeshifting.Text = layer.GetSetting("timeshiftWaitForTimeshifting", "15").Value;
			textBoxWaitUnscrambled.Text = layer.GetSetting("timeshiftWaitForUnscrambled", "5").Value;

      
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

      s = layer.GetSetting("idleEPGGrabberEnabled", "yes");
      if (checkBoxEnableEPGWhileIdle.Checked)
        s.Value = "yes";
      else
        s.Value = "no";
      s.Persist();

      s = layer.GetSetting("timeoutEPG", "10");
      s.Value = textBoxEpgTimeOut.Text;
      s.Persist();

      s = layer.GetSetting("timeoutEPGRefresh", "240");
      s.Value = textBoxEPGRefresh.Text;
      s.Persist();

      s = layer.GetSetting("timeshiftingEpgGrabberEnabled", "no");
      if (checkBoxEnableEpgWhileTimeshifting.Checked)
        s.Value = "yes";
      else
        s.Value = "no";
      s.Persist();

      s = layer.GetSetting("timeshiftingEpgGrabberTimeout", "2");
      s.Value = textBoxTSEpgTimeout.Text;
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
			s.Value = textBoxWaitTimeshifting.Text;
			s.Persist();

			s = layer.GetSetting("timeshiftWaitForUnscrambled", "5");
			s.Value = textBoxWaitUnscrambled.Text;
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
      NameValueCollection defaults=new NameValueCollection();
      defaults.Add("%TITLE%","Over the hedge");
      defaults.Add("%DESCRIPTION%","A scheming raccoon fools a mismatched family of forest creatures into helping him repay a debt of food, by invading the new suburban sprawl that popped up while they were hibernating...and learns a lesson about family himself.");
      defaults.Add("%GENRE%","movie/drama (general)");
      defaults.Add("%STARRATING%","6");
      defaults.Add("%STARRATING_STR%","***+");
      defaults.Add("%CLASSIFICATION%","PG");
      defaults.Add("%PARENTALRATING%","8");
      defaults.Add("%NEWLINE%", Environment.NewLine);
      edTitleTest.Text = EvalTemplate(edTitleTemplate.Text, defaults);
      edDescriptionTest.Text = EvalTemplate(edDescriptionTemplate.Text, defaults);
     }
  }
}
