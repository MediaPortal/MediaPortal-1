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
using System.Windows.Forms;

namespace Mediaportal.TV.Server.SetupTV.Dialogs
{
  public partial class FormEnterText : Form
  {
    private string _text = null;

    public FormEnterText(string caption, string explanation, string defaultText)
    {
      InitializeComponent();
      Text = caption;
      labelText.Text = explanation;
      textBoxText.Text = defaultText;
    }

    private void buttonOkay_Click(object sender, EventArgs e)
    {
      if (string.IsNullOrWhiteSpace(textBoxText.Text))
      {
        MessageBox.Show("Please enter something.", SetupControls.SectionSettings.MESSAGE_CAPTION);
        return;
      }

      DialogResult = DialogResult.OK;
      _text = textBoxText.Text;
      Close();
    }

    private void buttonCancel_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    public string TextValue
    {
      get
      {
        return _text;
      }
    }
  }
}