#region Copyright (C) 2005-2006 Team MediaPortal

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

#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.TV.Recording;

namespace MediaPortal.TV.Scanning
{
  /// <summary>
  /// Summary description for FormCountry.
  /// </summary>
  public class FormCountry : System.Windows.Forms.Form
  {
    private MediaPortal.UserInterface.Controls.MPLabel label1;
    private MediaPortal.UserInterface.Controls.MPComboBox comboBox1;
    private MediaPortal.UserInterface.Controls.MPButton button1;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public FormCountry()
    {
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      //
      // TODO: Add any constructor code after InitializeComponent call
      //
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (components != null)
        {
          components.Dispose();
        }
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.label1 = new MediaPortal.UserInterface.Controls.MPLabel();
      this.comboBox1 = new MediaPortal.UserInterface.Controls.MPComboBox();
      this.button1 = new MediaPortal.UserInterface.Controls.MPButton();
      this.SuspendLayout();
      // 
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(24, 16);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(144, 16);
      this.label1.TabIndex = 0;
      this.label1.Text = "Select Country/Region:";
      // 
      // comboBox1
      // 
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.Location = new System.Drawing.Point(32, 40);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(208, 21);
      this.comboBox1.TabIndex = 0;
      // 
      // button1
      // 
      this.button1.Location = new System.Drawing.Point(200, 72);
      this.button1.Name = "button1";
      this.button1.Size = new System.Drawing.Size(40, 23);
      this.button1.TabIndex = 1;
      this.button1.Text = "Ok";
      this.button1.Click += new System.EventHandler(this.button1_Click);
      // 
      // FormCountry
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(264, 118);
      this.Controls.Add(this.button1);
      this.Controls.Add(this.comboBox1);
      this.Controls.Add(this.label1);
      this.Name = "FormCountry";
      this.Text = "Country / Region selection";
      this.Load += new System.EventHandler(this.FormCountry_Load);
      this.ResumeLayout(false);

    }
    #endregion

    private void FormCountry_Load(object sender, System.EventArgs e)
    {
      if (comboBox1.Items.Count > 0)
        comboBox1.SelectedIndex = 0;
    }

    private void button1_Click(object sender, System.EventArgs e)
    {
      if (comboBox1.SelectedIndex < 0) return;
      this.Close();
    }
    public string countryName
    {
      get
      {
        if (comboBox1.SelectedIndex < 0) return "";
        return (string)comboBox1.SelectedItem;
      }
    }
    public void AddCountry(string country)
    {
      comboBox1.Items.Add(country);
    }
    public void Reset()
    {
      comboBox1.Items.Clear();
    }
  }
}
