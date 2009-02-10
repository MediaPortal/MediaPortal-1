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
using System.Collections.Generic;
using System.Windows.Forms;
using TvDatabase;

namespace SetupTv.Sections
{
  public partial class FormEditCard : Form
  {
    private Card _card;
    private string _cardType;

    public FormEditCard()
    {
      InitializeComponent();
    }

    public Card Card
    {
      get
      {
        return _card;
      }
      set
      {
        _card = value;
      }
    }

    public String CardType
    {
      set
      {
        _cardType = value;
      }
    }

    private void FormEditCard_Load(object sender, EventArgs e)
    {
      if (_cardType.Equals("Analog")) //analog does not have these settings
      {
        checkBoxCAMenabled.Enabled = false;
        numericUpDownDecryptLimit.Value = 0;
        numericUpDownDecryptLimit.Enabled = false;
        checkBoxAllowEpgGrab.Checked = false;
        checkBoxAllowEpgGrab.Enabled = false;
      }
      else
      {
        ComboBoxCamType.SelectedIndex = _card.CamType;
        numericUpDownDecryptLimit.Value = _card.DecryptLimit;
        numericUpDownDecryptLimit.Enabled = true;
        checkBoxAllowEpgGrab.Checked = _card.GrabEPG;
        checkBoxAllowEpgGrab.Enabled = true;
      }

      IList<CardGroupMap> GrpList = _card.ReferringCardGroupMap();
      if (GrpList.Count != 0)
      {
        checkBoxPreloadCard.Enabled = false;
        _card.PreloadCard = false;
      }

      checkBoxPreloadCard.Checked = _card.PreloadCard;
      checkBoxCAMenabled.Checked = _card.CAM;

      setCAMLimitVisibility();
      Text += " " + _card.Name;
    }

    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      if (!_cardType.Equals("Analog")) //analog does not have these settings
      {
        _card.CamType = ComboBoxCamType.SelectedIndex;
        _card.DecryptLimit = Convert.ToInt32(numericUpDownDecryptLimit.Value);
        _card.GrabEPG = checkBoxAllowEpgGrab.Checked;
      }
      _card.PreloadCard = checkBoxPreloadCard.Checked;

      _card.CAM = checkBoxCAMenabled.Checked;
      Close();
    }

    private void mpButtonCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void setCAMLimitVisibility()
    {
      label1.Visible = checkBoxCAMenabled.Checked;
      label3.Visible = checkBoxCAMenabled.Checked;
      numericUpDownDecryptLimit.Visible = checkBoxCAMenabled.Checked;
      label4.Visible = checkBoxCAMenabled.Checked;
      ComboBoxCamType.Visible = checkBoxCAMenabled.Checked;
      label5.Visible = checkBoxCAMenabled.Checked;
    }

    private void checkBoxCAMenabled_CheckedChanged(object sender, EventArgs e)
    {
      setCAMLimitVisibility();
    }
  }
}
