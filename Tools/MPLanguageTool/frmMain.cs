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
using System.Globalization;
using System.Xml;

namespace MPLanguageTool
{
  public partial class frmMain : Form
  {
    #region Variables
    NameValueCollection defaultTranslations;
    CultureInfo culture = null;
    bool DeployTool = false;
    bool MediaPortal = false;
    #endregion

    public frmMain()
    {
      InitializeComponent();
    }

    private int GetUntranslatedCount()
    {
      int count = 0;
      foreach (DataGridViewRow row in gv.Rows)
      {
        if ((string)row.Cells[1].Value == null)
          count++;
      }
      return count++;
    }

    private void gv_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
    {
      string key = (string)gv.Rows[e.RowIndex].Cells[0].Value;
      string value = (string)gv.Rows[e.RowIndex].Cells[1].Value;
      frmEdit dlg = new frmEdit();
      if (dlg.ShowDialog(key, value, defaultTranslations[key]) == DialogResult.OK)
      {
        string trans = dlg.GetTranslation();
        gv.Rows[e.RowIndex].Cells[1].Value = trans;
        if (trans == null)
          gv.Rows[e.RowIndex].Cells[0].Style.ForeColor = System.Drawing.Color.Red;
        else
          gv.Rows[e.RowIndex].Cells[0].Style.ForeColor = System.Drawing.Color.Black;
        ToolStripText(GetUntranslatedCount());
      }
    }

    #region Menu-events
    private void openDeployToolToolStripMenuItem_Click(object sender, EventArgs e)
    {
      defaultTranslations = ResxHandler.Load(null);
      if (defaultTranslations == null)
      {
        MessageBox.Show("The file [MediaPortal.DeployTool.resx] could not be found.\nThe LanguageTool does not work without it.", "MPLanguageTool -- Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        openDeployToolToolStripMenuItem.Enabled = false;
        saveToolStripMenuItem.Enabled = false;
        Environment.Exit(-1);
      }
      DeployTool = true;
      openMpToolStripMenuItem.Enabled = false;
      openDeployToolToolStripMenuItem.Enabled = false;
      SelectCulture dlg = new SelectCulture();
      if (dlg.ShowDialog() != DialogResult.OK) return;
      culture = dlg.GetSelectedCulture();
      this.Text = "MPLanguageTool -- Current language: " + culture.NativeName;
      NameValueCollection translations = ResxHandler.Load(culture.Name);
      int untranslated = 0;
      foreach (string key in defaultTranslations.AllKeys)
      {
        gv.Rows.Add(key, translations[key]);
        if (translations[key] == null)
        {
          gv.Rows[gv.RowCount - 1].Cells[0].Style.ForeColor = System.Drawing.Color.Red;
          untranslated++;
        }
      }
      ToolStripText(untranslated);
      saveToolStripMenuItem.Enabled = true;
    }

    private void openMpToolStripMenuItem_Click(object sender, EventArgs e)
    {
      defaultTranslations = XmlHandler.Load(null);
      if (defaultTranslations == null)
      {
        MessageBox.Show("The file [strings_en.xml] could not be found.\nThe LanguageTool does not work without it.", "MPLanguageTool -- Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        openMpToolStripMenuItem.Enabled = false;
        saveToolStripMenuItem.Enabled = false;
        Environment.Exit(-2);
      }
      MediaPortal = true;
      openMpToolStripMenuItem.Enabled = false;
      openDeployToolToolStripMenuItem.Enabled = false;
      SelectCulture dlg = new SelectCulture();
      if (dlg.ShowDialog() != DialogResult.OK) return;
      culture = dlg.GetSelectedCulture();
      this.Text = "MPLanguageTool -- Current language: " + culture.NativeName;
      NameValueCollection translations = XmlHandler.Load(culture.Name);
      int untranslated = 0;
      foreach (string key in defaultTranslations.AllKeys)
      {
        gv.Rows.Add(key, translations[key]);
        if (translations[key] == null)
        {
          gv.Rows[gv.RowCount - 1].Cells[0].Style.ForeColor = System.Drawing.Color.Red;
          untranslated++;
        }
      }
      ToolStripText(untranslated);
      saveToolStripMenuItem.Enabled = true;
    }

    private void saveToolStripMenuItem_Click(object sender, EventArgs e)
    {
      NameValueCollection translations = new NameValueCollection();
      foreach (DataGridViewRow row in gv.Rows)
        translations.Add((string)row.Cells[0].Value, (string)row.Cells[1].Value);
      if (MediaPortal)
        XmlHandler.Save(culture.Name, culture.EnglishName, translations);
      if (DeployTool)
        ResxHandler.Save(culture.Name, translations);
      MessageBox.Show("Your translations have been saved.", "MPLanguageTool -- Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    private void quitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (culture != null)
      {
        if (MessageBox.Show("Do you want to save before exiting?", "MPLanguageTool -- Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
          saveToolStripMenuItem_Click(null, new EventArgs());
      }
      Close();
    }
    #endregion

    public void ToolStripText(int lines)
    {
      string AddTxt;
      if (lines != 0)
      {
        toolStripStatusLabel1.ForeColor = System.Drawing.Color.Red;
        AddTxt = ". Double click a row to edit text.";
      }
      else
      {
        toolStripStatusLabel1.ForeColor = System.Drawing.Color.Black;
        AddTxt = null;
      }
      toolStripStatusLabel1.Text = "Missing translations: " + lines.ToString() + AddTxt;
    }
  }
}