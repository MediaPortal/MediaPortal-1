#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using MpeCore;
using MpeCore.Classes;

namespace MpeMaker.Sections
{
  public partial class ToolsUpdateXml : UserControl, ISectionControl
  {
    public PackageClass Package { get; set; }
    private bool loading = false;

    public ToolsUpdateXml()
    {
      Package = new PackageClass();
      InitializeComponent();
    }

    private void button1_Click(object sender, EventArgs e)
    {
      saveFileDialog1.FileName = textBox1.Text;
      if (saveFileDialog1.ShowDialog() == DialogResult.OK)
      {
        textBox1.Text = saveFileDialog1.FileName;
      }
    }

    /// <summary>
    /// Handles the Click event of the btn_gen control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void btn_gen_Click(object sender, EventArgs e)
    {
      string xmlFile = textBox1.Text;
      ExtensionCollection list = new ExtensionCollection();
      if (File.Exists(xmlFile))
        list = ExtensionCollection.Load(xmlFile);
      list.Add(Package);
      list.Save(xmlFile);
    }

    public void Set(PackageClass pak)
    {
      Package = pak;
      loading = true;
      textBox1.Text = Package.ProjectSettings.UpdatePath1;
      txt_list1.Text = Package.ProjectSettings.UpdatePath2;
      txt_list2.Text = Package.ProjectSettings.UpdatePath3;
      loading = false;
    }

    public PackageClass Get()
    {
      throw new NotImplementedException();
    }

    private void btn_browse1_Click(object sender, EventArgs e)
    {
      saveFileDialog1.FileName = txt_list1.Text;
      if (saveFileDialog1.ShowDialog() == DialogResult.OK)
      {
        txt_list1.Text = saveFileDialog1.FileName;
      }
    }

    private void btn_browse2_Click(object sender, EventArgs e)
    {
      saveFileDialog1.FileName = txt_list2.Text;
      if (saveFileDialog1.ShowDialog() == DialogResult.OK)
      {
        txt_list2.Text = saveFileDialog1.FileName;
      }
    }

    private void add_list_Click(object sender, EventArgs e)
    {
      string xmlFile = txt_list1.Text;
      ExtensionCollection list = new ExtensionCollection();
      ExtensionCollection list2 = new ExtensionCollection();
      if (File.Exists(xmlFile))
        list = ExtensionCollection.Load(xmlFile);
      if (File.Exists(txt_list2.Text))
        list2 = ExtensionCollection.Load(txt_list2.Text);
      list.Add(list2);
      list.Save(xmlFile);
    }

    private void textBox1_TextChanged(object sender, EventArgs e)
    {
      if (loading)
        return;
      Package.ProjectSettings.UpdatePath1 = textBox1.Text;
      Package.ProjectSettings.UpdatePath2 = txt_list1.Text;
      Package.ProjectSettings.UpdatePath3 = txt_list2.Text;
    }
  }
}