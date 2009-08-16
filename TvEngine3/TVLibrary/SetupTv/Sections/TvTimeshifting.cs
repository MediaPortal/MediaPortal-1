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

#region Usings

using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Gentle.Framework;

using TvDatabase;
using TvControl;
using SetupTv.Dialogs;

#endregion

namespace SetupTv.Sections
{
  public partial class TvTimeshifting : SectionSettings
  {
    #region CardInfo class

    public class CardInfo
    {
      public Card card;
      public CardInfo(Card newcard)
      {
        card = newcard;
      }
      public override string ToString()
      {
        return card.Name;
      }
    }

    #endregion

    #region Vars

    bool _needRestart;

    #endregion

    #region Constructors

    public TvTimeshifting()
      : this("Timeshifting settings")
    {
    }

    public TvTimeshifting(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Serialization

    public override void LoadSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();

      numericUpDownMinFiles.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftMinFiles", "6").Value), 3, 100);
      numericUpDownMaxFiles.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftMaxFiles", "20").Value), 3, 100);
      numericUpDownMaxFileSize.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftMaxFileSize", "256").Value), 20, 1024);
      numericUpDownWaitUnscrambled.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftWaitForUnscrambled", "5").Value), 1, 30);
      numericUpDownWaitTimeshifting.Value = ValueSanityCheck(Convert.ToDecimal(layer.GetSetting("timeshiftWaitForTimeshifting", "15").Value), 1, 30);
    }

    public override void SaveSettings()
    {
      TvBusinessLayer layer = new TvBusinessLayer();
      Setting s = layer.GetSetting("timeshiftMinFiles", "6");
      s.Value = numericUpDownMinFiles.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftMaxFiles", "20");
      s.Value = numericUpDownMaxFiles.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftMaxFileSize", "256");
      s.Value = numericUpDownMaxFileSize.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftWaitForUnscrambled", "5");
      s.Value = numericUpDownWaitUnscrambled.Value.ToString();
      s.Persist();

      s = layer.GetSetting("timeshiftWaitForTimeshifting", "15");
      s.Value = numericUpDownWaitTimeshifting.Value.ToString();
      s.Persist();
    }

    #endregion

    #region GUI-Events

    private void comboBoxCards_SelectedIndexChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      textBoxTimeShiftFolder.Text = info.card.TimeShiftFolder;

      if (textBoxTimeShiftFolder.Text == "")
      {
        textBoxTimeShiftFolder.Text = String.Format(@"{0}\Team MediaPortal\MediaPortal TV Server\timeshiftbuffer", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        if (!Directory.Exists(textBoxTimeShiftFolder.Text))
        {
          Directory.CreateDirectory(textBoxTimeShiftFolder.Text);
        }
      }
    }

    public override void OnSectionActivated()
    {
      _needRestart = false;
      comboBoxCards.Items.Clear();
      IList<Card> cards = Card.ListAll();
      foreach (Card card in cards)
      {
        comboBoxCards.Items.Add(new CardInfo(card));
      }

      if (comboBoxCards.Items.Count > 0)
      {
        comboBoxCards.SelectedIndex = 0;
      }

      base.OnSectionActivated();
    }

    public override void OnSectionDeActivated()
    {
      base.OnSectionDeActivated();

      SaveSettings();
      if (_needRestart)
      {
        RemoteControl.Instance.ClearCache();
        RemoteControl.Instance.Restart();
      }
    }

    // Browse TimeShift folder
    private void buttonTimeShiftBrowse_Click(object sender, EventArgs e)
    {
      FolderBrowserDialog dlg = new FolderBrowserDialog();
      dlg.SelectedPath = textBoxTimeShiftFolder.Text;
      dlg.Description = "Specify timeshift folder";
      dlg.ShowNewFolderButton = true;
      if (dlg.ShowDialog(this) == DialogResult.OK)
      {
        textBoxTimeShiftFolder.Text = dlg.SelectedPath;
      }
    }

    // When TimeShift folder has been changed
    private void textBoxTimeShiftFolder_TextChanged(object sender, EventArgs e)
    {
      CardInfo info = (CardInfo)comboBoxCards.SelectedItem;
      if (info.card.TimeShiftFolder != textBoxTimeShiftFolder.Text)
      {
        info.card.TimeShiftFolder = textBoxTimeShiftFolder.Text;
        info.card.Persist();
        _needRestart = true;
      }
    }

    // Click on Same timeshift folder for all cards
    private void buttonSameTimeshiftFolder_Click(object sender, EventArgs e)
    {
      // Change timeshiftFolder for all cards
      for (int iIndex = 0; iIndex < comboBoxCards.Items.Count; iIndex++)
      {
        CardInfo info = (CardInfo)comboBoxCards.Items[iIndex];
        if (info.card.TimeShiftFolder != textBoxTimeShiftFolder.Text)
        {
          info.card.TimeShiftFolder = textBoxTimeShiftFolder.Text;
          info.card.Persist();
          if (!_needRestart)
          {
            _needRestart = true;
          }
        }
      }
    }

    private static decimal ValueSanityCheck(decimal Value, int Min, int Max)
    {
      if (Value < Min)
        return Min;
      if (Value > Max)
        return Max;
      return Value;
    }

    private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    #endregion

    private void groupBoxTimeshiftSettings_Enter(object sender, EventArgs e)
    {

    }
  }
}
