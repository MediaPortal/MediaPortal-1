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
using MediaPortal.Profile;

namespace MediaPortal.Configuration.Sections
{
  public abstract partial class BaseFileExtensions : SectionSettings
  {
    #region Properties

    public string Extensions
    {
      get
      {
        string extensions = string.Empty;

        foreach (ListViewItem extension in extensionsListView.Items)
        {
          if (extensions.Length > 0)
          {
            extensions += ",";
          }

          extensions += extension.Text;
        }

        return extensions;
      }
      set
      {
        string[] extensions = ((string)value).Split(',');
        foreach (var extension in extensions)
        {
          extensionsListView.Items.Add(extension);
        }
      }
    }

    protected abstract string SettingsSection { get; }
    protected abstract string DefaultExtensions { get; }

    #endregion

    #region Constructors

    public BaseFileExtensions()
      : base("<Unknown>")
    {
      InitializeComponent();
    }

    public BaseFileExtensions(string name)
      : base(name)
    {
      InitializeComponent();
    }

    #endregion

    #region Control events

    private void addButton_Click(object sender, EventArgs e)
    {
      string extension = extensionTextBox.Text.Trim();

      if (extension.Length != 0)
      {
        // Only grab what we got after the first .
        int dotPosition = extension.IndexOf(".");

        if (dotPosition < 0)
        {
          // We got no dot in the extension, append it
          extension = String.Format(".{0}", extension);
        }
        else
        {
          // Remove everything before the dot
          extension = extension.Substring(dotPosition);
        }

        // Remove unwanted characters
        extension = extension.Replace("*", "");

        // Check if we already have new extension in the list
        foreach (ListViewItem ext in extensionsListView.Items)
        {
          if (ext.Text == extension)
          {
            MessageBox.Show("Extension already exist.", "Information", MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
            extensionTextBox.Focus();
            extensionTextBox.SelectAll();
            return;
          }
        }

        // Add extension to the list
        extensionsListView.Items.Add(extension);

        // Clear text
        extensionTextBox.Text = string.Empty;
      }
    }

    private void removeButton_Click(object sender, EventArgs e)
    {
      int itemsSelected = extensionsListView.SelectedIndices.Count;

      // Make sure we have a valid item selected
      for (int index = 0; index < itemsSelected; index++)
      {
        extensionsListView.Items.RemoveAt(extensionsListView.SelectedIndices[0]);
      }
    }

    private void resetButton_Click(object sender, EventArgs e)
    {
      if (MessageBox.Show(
        "Do you really want to reset the extension list to the default?\r\nAny modification you did will be lost.",
        "MediaPortal Configuration", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2)
          == DialogResult.No) return;

      extensionsListView.Items.Clear();
      Extensions = DefaultExtensions;
    }

    private void extensionsListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
      removeButton.Enabled = (extensionsListView.SelectedItems.Count > 0);
    }

    private void extensionTextBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != System.Windows.Forms.Keys.Enter &&
          e.KeyCode != System.Windows.Forms.Keys.Return) return;
      if (string.IsNullOrEmpty(extensionTextBox.Text)) return;

      e.Handled = true;
      addButton_Click(null, null);
    }

    private void extensionsListBox_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode != System.Windows.Forms.Keys.Delete) return;
      if (extensionsListView.SelectedItems.Count <= 0) return;

      e.Handled = true;
      removeButton_Click(null, null);
    }

    #endregion

    #region Setting overrides

    public override object GetSetting(string name)
    {
      switch (name.ToLowerInvariant())
      {
        case "extensions":
          return Extensions;
      }

      return null;
    }

    public override void LoadSettings()
    {
      using (Settings xmlreader = new MPSettings())
      {
        Extensions = xmlreader.GetValueAsString(SettingsSection, "extensions", DefaultExtensions);
      }
    }

    public override void SaveSettings()
    {
      using (Settings xmlwriter = new MPSettings())
      {
        xmlwriter.SetValue(SettingsSection, "extensions", Extensions);
      }
    }

    #endregion

    #region temporary disable AcceptButton on SettingsForm

    private IButtonControl tempButton;

    /// <summary>
    /// Clear AcceptButton property of SettingsForm.
    /// When enter is clicked now, it will add the extension.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void extensionTextBox_Enter(object sender, EventArgs e)
    {
      tempButton = SettingsForm.ActiveForm.AcceptButton;
      SettingsForm.ActiveForm.AcceptButton = null;
    }

    /// <summary>
    /// Re-add AcceptButton property of SettingsForm.
    /// When enter is clicked now, the form will be closed as always.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void extensionTextBox_Leave(object sender, EventArgs e)
    {
      SettingsForm.ActiveForm.AcceptButton = tempButton;
      tempButton = null;
    }

    #endregion
  }
}