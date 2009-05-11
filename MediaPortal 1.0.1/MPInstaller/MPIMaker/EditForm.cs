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

using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using MediaPortal.Configuration;

namespace MediaPortal.MPInstaller
{
  public partial class EditForm : MPInstallerForm
  {
    private MPinstallerStruct _struct = new MPinstallerStruct();
    private string proiect_file_name = "Untitled";
    private bool _loading = false;
    private ScriptEditorForm scriptEditor = new ScriptEditorForm();
    public int sortColumn;
    public FilePropertiesClass fpc = new FilePropertiesClass();

    public EditForm()
    {
      InitializeComponent();
    }

    public EditForm(string fil)
    {
      proiect_file_name = fil;
      InitializeComponent();
      proiectt_textBox6.Items.AddRange(MPinstallerStruct.CategoryListing);

      OpenProjectFile(fil);
    }

    private void sToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!IsGoodToSave())
      {
        return;
      }

      saveFileDialog1.Filter = "Project files (*.xmp)|*.xmp|All files (*.*)|*.*";
      saveFileDialog1.DefaultExt = "*.xmp";
      _struct.AddFileList(bossview);
      _struct.Script = scriptEditor.textBox_code.Text;
      if (Path.GetFileName(proiect_file_name) == "Untitled" || String.IsNullOrEmpty(proiect_file_name.Trim()))
      {
        if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
        {
          proiect_file_name = saveFileDialog1.FileName;
          _struct.SaveToFile(proiect_file_name);
        }
      }
      else
      {
        _struct.SaveToFile(proiect_file_name);
      }
    }

    private void windowToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addplugin(1);
    }

    private void addplugin(int type)
    {
      /*
       1 - 10 PLUGINS
       1 - window
       2 - process
       3 - subtitle
       4 - tagreader
       5 - external player
      */
      string fil;
      openFileDialog1.Filter = "dll files (*.dll)|*.dll|exe files (*.exe)|*.exe|All files (*.*)|*.*";
      openFileDialog1.FileName = "";
      openFileDialog1.DefaultExt = "*.dll";
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        fil = openFileDialog1.FileName;
        switch (type)
        {
          case 1:
            addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_WINDOW_SUBTYPE,
                   Path.GetFullPath(fil), "01010");
            break;
          case 2:
            addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_PROCESS_SUBTYPE,
                   Path.GetFullPath(fil), "01020");
            break;
          case 3:
            addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_SUBTITLE_SUBTYPE,
                   Path.GetFullPath(fil), "01030");
            break;
          case 4:
            addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_TAGREADER_SUBTYPE,
                   Path.GetFullPath(fil), "01040");
            break;
          case 5:
            addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_PLAYER_SUBTYPE,
                   Path.GetFullPath(fil), "01050");
            break;
          default:
            break;
        }
      }
    }

    private void addtext()
    {
      openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
      openFileDialog1.FileName = "";
      openFileDialog1.DefaultExt = "*.txt";
      openFileDialog1.Multiselect = true;
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        foreach (string fil in openFileDialog1.FileNames)
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.TEXT_TYPE, MPinstallerStruct.TEXT_LOG_TYPE,
                 Path.GetFullPath(fil), "03010", "");
        }
      }
    }

    private void addskin(int type)
    {
      //  string fil;
      string subtype;
      openFileDialog1.FileName = "";
      openFileDialog1.Multiselect = true;
      switch (type)
      {
        case 1:
          openFileDialog1.Filter =
            "xml files (*.xml)|*.xml|png files (*.png)|*.png|bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
          openFileDialog1.DefaultExt = "*.xml";
          break;
        case 2:
          openFileDialog1.Filter =
            "png files (*.png)|*.png|bmp files (*.bmp)|*.bmp|xml files (*.xml)|*.xml|All files (*.*)|*.*";
          openFileDialog1.DefaultExt = "*.png";
          break;
        case 3:
          openFileDialog1.Filter = "wav files (*.wav)|*.wav|All files (*.*)|*.*";
          openFileDialog1.DefaultExt = "*.wav";
          break;
        case 4:
          openFileDialog1.Filter =
            "png files (*.png)|*.png|bmp files (*.bmp)|*.bmp|xml files (*.xml)|*.xml|All files (*.*)|*.*";
          openFileDialog1.DefaultExt = "*.png";
          break;
        case 5:
          openFileDialog1.Filter =
            "png files (*.png)|*.png|bmp files (*.bmp)|*.bmp|xml files (*.xml)|*.xml|All files (*.*)|*.*";
          openFileDialog1.DefaultExt = "*.png";
          break;
        case 6:
          openFileDialog1.Filter = "font files (*.ttf)|*.ttf|All files (*.*)|*.*";
          openFileDialog1.DefaultExt = "*.ttf";
          break;
      }
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        foreach (string fil in openFileDialog1.FileNames)
        {
          if (Path.GetFullPath(fil).ToLower().IndexOf("skin\\") == 0)
          {
            subtype = "Blue3";
          }
          else
          {
            subtype = Path.GetFullPath(fil).Substring(Path.GetFullPath(fil).ToLower().IndexOf("skin\\") + 5);
            subtype = subtype.Substring(0, subtype.IndexOf("\\"));
          }
          switch (type)
          {
            case 1:
              addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_TYPE, subtype, Path.GetFullPath(fil), "02010", "");
              break;
            case 2:
              addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_MEDIA_TYPE, subtype, Path.GetFullPath(fil), "02020",
                     "");
              break;
            case 3:
              addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_SOUNDS_TYPE, subtype, Path.GetFullPath(fil), "02030",
                     "");
              break;
            case 4:
              addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_ANIMATIONS_TYPE, subtype, Path.GetFullPath(fil),
                     "02040", "");
              break;
            case 5:
              addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_TETRIS_TYPE, subtype, Path.GetFullPath(fil), "02050",
                     "");
              break;
            case 6:
              addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_SYSTEMFONT_TYPE, subtype, Path.GetFullPath(fil),
                     "02060", "");
              break;
            default:
              break;
          }
        }
      }
    }

    private void addother(string ty, string sty)
    {
      //      string fil;
      openFileDialog1.Filter =
        "All files (*.*)|*.*|xml files (*.xml)|*.xml|png files (*.png)|*.png|bmp files (*.bmp)|*.bmp|jpeg files (*.jpg)|*.jpg";
      openFileDialog1.FileName = "";
      openFileDialog1.DefaultExt = "*.*";
      openFileDialog1.Multiselect = true;
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        foreach (string fil in openFileDialog1.FileNames)
        {
          addrow(Path.GetFileName(fil), ty, sty, Path.GetFullPath(fil), "04010", "");
        }
      }
    }

    private void subtitleToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addplugin(3);
    }

    private void procesToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addplugin(2);
    }

    private void tagReadersToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addplugin(4);
    }

    private void externalPlayersToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addplugin(5);
    }

    private void addrow(MPIFileList list)
    {
      ListViewItem item1 = new ListViewItem(Path.GetFileName(list.FileName), 0);
      item1.SubItems.Add(list.Type);
      item1.SubItems.Add(list.SubType);
      item1.SubItems.Add(list.FileName);
      item1.SubItems.Add(list.ID);
      item1.SubItems.Add(list.Option);
      bossview.Items.AddRange(new ListViewItem[] {item1});
    }

    private void addrow(string file, string type, string subtype, string path, string id)
    {
      ListViewItem item1 = new ListViewItem(file, 0);
      item1.SubItems.Add(type);
      item1.SubItems.Add(subtype);
      item1.SubItems.Add(path);
      item1.SubItems.Add(id);
      item1.SubItems.Add("");
      bossview.Items.AddRange(new ListViewItem[] {item1});
    }

    private void addrow(string file, string type, string subtype, string path, string id, string op)
    {
      ListViewItem item1 = new ListViewItem(file, 0);
      ListViewItem item = new ListViewItem(file, 0);
      foreach (ListViewItem it in bossview.Items)
      {
        if (it.SubItems[3].Text == path && it.SubItems[0].Text == file)
        {
          item = it;
        }
      }
      bossview.Items.Remove(item);
      item1.SubItems.Add(type);
      item1.SubItems.Add(subtype);
      item1.SubItems.Add(path);
      item1.SubItems.Add(id);
      item1.SubItems.Add(op);
      bossview.Items.AddRange(new ListViewItem[] {item1});
    }

    private void OpenProjectFile(string projectFile)
    {
      _struct.LoadFromFile(projectFile);
      _struct.ProiectFileName = projectFile;
      loadProperties();
      this.Text = projectFile;
      for (int i = 0; i < _struct.FileList.Count; i++)
      {
        addrow((MPIFileList) _struct.FileList[i]);
      }
      scriptEditor.textBox_code.Text = _struct.Script;
      openFileDialog1.InitialDirectory = Path.GetDirectoryName(_struct.ProiectFileName);
      folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(_struct.ProiectFileName);
    }

    private void openProiectToolStripMenuItem_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "Project files (*.xmp)|*.xmp|All files (*.*)|*.*";
      openFileDialog1.FileName = "";
      openFileDialog1.DefaultExt = "*.xmp";
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        bossview.Items.Clear();

        proiect_file_name = openFileDialog1.FileName;
        OpenProjectFile(proiect_file_name);
      }
    }

    private void loadProperties()
    {
      _loading = true;
      proiectt_textBox1.Text = _struct.Name;
      proiectt_textBox2.Text = _struct.Author;
      proiectt_textBox3.Text = _struct.UpdateURL;
      proiectt_textBox4.Text = _struct.Version;
      proiectt_textBox5.Text = _struct.Description;
      proiectt_textBox6.Text = _struct.Group;
      proiectt_comboBox1.Text = _struct.Release;
      pictureBox1.Image = _struct.Logo;
      _loading = false;
    }

    /// <summary>
    /// Show the language editor form
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void languageToolStripMenuItem_Click(object sender, EventArgs e)
    {
      Form2 StrForm = new Form2();
      StrForm.Language = _struct.Language;
      StrForm.ShowDialog();
      LanguageStringComparer lsc = new LanguageStringComparer();
      StrForm.Language.Sort(lsc);
      _struct.Language = StrForm.Language;
    }

    private void optionsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      OptionForm opForm = new OptionForm();
      opForm.ShowDialog();
    }

    private void buildToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (IsGoodToSave())
      {
        sToolStripMenuItem_Click(sender, e);
        Build_dialog buildfrm = new Build_dialog(this._struct);
        buildfrm.ShowDialog();
        this._struct = buildfrm._struct;
      }
    }

    private void newToolStripMenuItem_Click(object sender, EventArgs e)
    {
      bossview.Items.Clear();
      _struct.Clear();
      scriptEditor.Reset();
      loadProperties();
    }

    private void componentToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addskin(1);
    }

    private void mediaToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addskin(2);
    }

    private void toolStripComboBox1_Click(object sender, EventArgs e)
    {
      //for (int i = 0; i < bossview.Items.Count; i++)
      //  if (bossview.Items[i].Selected) bossview.Items.RemoveAt(i);
      foreach (ListViewItem li in bossview.SelectedItems)
      {
        bossview.Items.Remove(li);
      }
    }


    private void saveProiectAsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (proiectt_textBox1.Text.Length == 0)
      {
        MessageBox.Show("Name is mandatory !", "Stop");
        proiectt_textBox1.Focus();
        return;
      }
      saveFileDialog1.Filter = "Project files (*.xmp)|*.xmp|All files (*.*)|*.*";
      saveFileDialog1.DefaultExt = "*.xmp";
      if (saveFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        proiect_file_name = saveFileDialog1.FileName;
        _struct.ProiectFileName = proiect_file_name;
        _struct.AddFileList(bossview);
        _struct.Script = scriptEditor.textBox_code.Text;
        _struct.SaveToFile(proiect_file_name);
        this.Text = proiect_file_name;
      }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      tabControl1.Controls.Clear();
      tabControl1.Controls.Add(tabPage_Proiect);
      tabControl1.Controls.Add(tabPage_Advanced);
      propertyGrid1.SelectedObject = _struct.ProjectProperties;
      textt_comboBox1.Items.Clear();
      textt_comboBox1.Items.Add(MPinstallerStruct.TEXT_LOG_TYPE);
      textt_comboBox1.Items.Add(MPinstallerStruct.TEXT_README_TYPE);
      textt_comboBox1.Items.Add(MPinstallerStruct.TEXT_EULA_TYPE);
      skint_comboBox1.Items.Clear();
      string SkinDirectory = Config.GetFolder(Config.Dir.Skin);
      if (Directory.Exists(SkinDirectory))
      {
        string[] skinFolders = Directory.GetDirectories(SkinDirectory, "*.*");

        foreach (string skinFolder in skinFolders)
        {
          bool isInvalidDirectory = false;
          string[] invalidDirectoryNames = new string[] {"cvs"};

          string directoryName = skinFolder.Substring(SkinDirectory.Length + 1);

          if (directoryName != null && directoryName.Length > 0)
          {
            foreach (string invalidDirectory in invalidDirectoryNames)
            {
              if (invalidDirectory.Equals(directoryName.ToLower()))
              {
                isInvalidDirectory = true;
                break;
              }
            }

            if (isInvalidDirectory == false)
            {
              //
              // Check if we have a home.xml located in the directory, if so we consider it as a
              // valid skin directory
              //
              string filename = Path.Combine(SkinDirectory, Path.Combine(directoryName, "references.xml"));
              if (File.Exists(filename))
              {
                skint_comboBox1.Items.Add(directoryName);
              }
            }
          }
        }
      }
    }

    private void bossview_SelectedIndexChanged(object sender, EventArgs e)
    {
      if (bossview.SelectedItems.Count > 0)
      {
        tabControl1.Controls.Clear();
        if (bossview.SelectedItems[0].SubItems[1].Text == MPinstallerStruct.PLUGIN_TYPE)
        {
          tabControl1.Controls.Add(tabPage_Plugin);
        }
        if (bossview.SelectedItems[0].SubItems[1].Text == MPinstallerStruct.SKIN_TYPE ||
            bossview.SelectedItems[0].SubItems[1].Text == MPinstallerStruct.SKIN_MEDIA_TYPE ||
            bossview.SelectedItems[0].SubItems[1].Text == MPinstallerStruct.SKIN_SYSTEMFONT_TYPE)
        {
          tabControl1.Controls.Add(tabPage_Skin);
          skint_comboBox1.Text = bossview.SelectedItems[0].SubItems[2].Text;
        }
        if (bossview.SelectedItems[0].SubItems[1].Text == MPinstallerStruct.TEXT_TYPE)
        {
          tabControl1.Controls.Add(tabPage_Text);
          textt_comboBox1.Text = bossview.SelectedItems[0].SubItems[2].Text;
        }
        if (bossview.SelectedItems[0].SubItems[1].Text == MPinstallerStruct.THUMBS_TYPE)
        {
          tabControl1.Controls.Add(tabPage_Thumbs);
          thumbst_comboBox1.Text = bossview.SelectedItems[0].SubItems[2].Text;
          thumbs_pictureBox.LoadAsync(bossview.SelectedItems[0].SubItems[3].Text);
        }
        if (bossview.SelectedItems[0].SubItems[1].Text == MPinstallerStruct.OTHER_TYPE)
        {
          tabControl1.Controls.Add(tabPage_Other);
          othert_comboBox1.Text = bossview.SelectedItems[0].SubItems[2].Text;
        }
        tabControl1.Controls.Add(tabPage_Proiect);
        tabControl1.Controls.Add(tabPage_Advanced);
        propertyGrid1.SelectedObject = _struct.ProjectProperties;
        propertyGrid2.SelectedObject = fpc.Parse(bossview.SelectedItems[0].SubItems[5].Text);
        propertyGrid1.Update();
        propertyGrid2.Update();
      }
      else
      {
        tabControl1.Controls.Clear();
        tabControl1.Controls.Add(tabPage_Proiect);
        tabControl1.Controls.Add(tabPage_Advanced);
      }
      bossview.Focus();
    }

    private void bossview_ColumnClick(object sender, ColumnClickEventArgs e)
    {
      // Determine whether the column is the same as the last column clicked.
      if (e.Column != sortColumn)
      {
        // Set the sort column to the new column.
        sortColumn = e.Column;
        // Set the sort order to ascending by default.
        bossview.Sorting = SortOrder.Ascending;
      }
      else
      {
        // Determine what the last sort order was and change it.
        if (bossview.Sorting == SortOrder.Ascending)
        {
          bossview.Sorting = SortOrder.Descending;
        }
        else
        {
          bossview.Sorting = SortOrder.Ascending;
        }
      }

      // Call the sort method to manually sort.
      bossview.Sort();
      // Set the ListViewItemSorter property to a new ListViewItemComparer
      // object.
      this.bossview.ListViewItemSorter = new ListViewItemComparer(e.Column, bossview.Sorting, true);
    }

    private void textToolStripMenuItem1_Click(object sender, EventArgs e)
    {
      addother(MPinstallerStruct.THUMBS_TYPE, thumbst_comboBox1.Text);
    }

    private void textToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addtext();
    }

    private void tab_text_change(object sender, EventArgs e)
    {
      foreach (ListViewItem item in bossview.SelectedItems)
      {
        if (item.SubItems[1].Text == MPinstallerStruct.TEXT_TYPE)
        {
          item.SubItems[2].Text = textt_comboBox1.Text;
        }
      }
    }

    private void tab_skin_change(object sender, EventArgs e)
    {
      foreach (ListViewItem item in bossview.SelectedItems)
      {
        if (item.SubItems[1].Text == MPinstallerStruct.SKIN_TYPE)
        {
          item.SubItems[2].Text = skint_comboBox1.Text;
        }
      }
    }

    private void tab_thumbs_change(object sender, EventArgs e)
    {
      foreach (ListViewItem item in bossview.SelectedItems)
      {
        if (item.SubItems[1].Text == MPinstallerStruct.THUMBS_TYPE)
        {
          item.SubItems[2].Text = thumbst_comboBox1.Text;
        }
      }
    }

    private void tab_other_change(object sender, EventArgs e)
    {
      foreach (ListViewItem item in bossview.SelectedItems)
      {
        if (item.SubItems[1].Text == MPinstallerStruct.OTHER_TYPE)
        {
          item.SubItems[2].Text = othert_comboBox1.Text;
        }
      }
    }

    private void proiectt_textBox1_TextChanged(object sender, EventArgs e)
    {
      if (!_loading)
      {
        _struct.Name = proiectt_textBox1.Text;
        _struct.Author = proiectt_textBox2.Text;
        _struct.UpdateURL = proiectt_textBox3.Text;
        _struct.Version = proiectt_textBox4.Text;
        _struct.Description = proiectt_textBox5.Text;
        _struct.Group = proiectt_textBox6.Text;
        _struct.Release = proiectt_comboBox1.Text;
      }
    }

    private void otherToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addother(MPinstallerStruct.OTHER_TYPE, "");
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
      this.Close();
    }

    private void button_browse_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter =
        "All files (*.*)|*.*|png files (*.png)|*.png|bmp files (*.bmp)|*.bmp|jpeg files (*.jpg)|*.jpg";
      openFileDialog1.FileName = "";
      openFileDialog1.DefaultExt = "*.*";
      openFileDialog1.Multiselect = false;
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        pictureBox1.Load(openFileDialog1.FileName);
        _struct.Logo = pictureBox1.Image;
      }
    }

    private void pictureBox1_LoadCompleted(object sender, AsyncCompletedEventArgs e)
    {
      _struct.Logo = pictureBox1.Image;
    }

    private void postSetupToolStripMenuItem_Click(object sender, EventArgs e)
    {
      post_setup dlg = new post_setup();
      dlg._struct = this._struct;
      dlg.ShowDialog();
    }

    private void soundsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addskin(3);
    }

    private void animationsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addskin(4);
    }

    private void tetrisToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addskin(5);
    }

    private void propertyGrid2_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
    {
      bossview.SelectedItems[0].SubItems[5].Text = ((FilePropertiesClass) propertyGrid2.SelectedObject).ToString();
    }

    private void setupGroupsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      _struct.AddFileList(bossview);
      GroupForm dlg = new GroupForm();
      dlg._struct = this._struct;
      dlg.ShowDialog();
    }

    private void fileAutomatedDiscoverTypeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter =
        "All files (*.*)|*.*|png files (*.png)|*.png|bmp files (*.bmp)|*.bmp|jpeg files (*.jpg)|*.jpg|Skin files (*.xml)|*.xml|Plugin files (*.dll)|*.dll";
      openFileDialog1.FileName = "";
      openFileDialog1.DefaultExt = "*.*";
      openFileDialog1.Multiselect = true;
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        foreach (string f in openFileDialog1.FileNames)
        {
          addFile(f);
        }
      }
    }

    private void addFile(string fil)
    {
      if (Path.GetExtension(fil).ToUpper() == ".DLL")
      {
        if (fil.ToUpper().Contains("PLUGINS\\WINDOWS"))
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_WINDOW_SUBTYPE,
                 Path.GetFullPath(fil), "01010");
        }
        if (fil.ToUpper().Contains("PLUGINS\\TAGREADERS"))
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_TAGREADER_SUBTYPE,
                 Path.GetFullPath(fil), "01010");
        }
        if (fil.ToUpper().Contains("PLUGINS\\SUBTITLE"))
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_SUBTITLE_SUBTYPE,
                 Path.GetFullPath(fil), "01010");
        }
        if (fil.ToUpper().Contains("PLUGINS\\PROCESS"))
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_PROCESS_SUBTYPE,
                 Path.GetFullPath(fil), "01010");
        }
        if (fil.ToUpper().Contains("PLUGINS\\EXTERNALPLAYERS"))
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.PLUGIN_TYPE, MPinstallerStruct.PLUGIN_PLAYER_SUBTYPE,
                 Path.GetFullPath(fil), "01010");
        }
        return;
      }
      if (fil.ToUpper().Contains("SKIN"))
      {
        string subtype = Path.GetFullPath(fil).Substring(Path.GetFullPath(fil).ToLower().IndexOf("skin\\") + 5);
        subtype = subtype.Substring(0, subtype.IndexOf("\\"));
        if (fil.ToUpper().Contains("SOUNDS"))
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_SOUNDS_TYPE, subtype, Path.GetFullPath(fil), "02010", "");
        }
        else if (fil.ToUpper().Contains("MEDIA\\ANIMATIONS"))
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_ANIMATIONS_TYPE, subtype, Path.GetFullPath(fil), "02010",
                 "");
        }
        else if (fil.ToUpper().Contains("MEDIA\\TETRIS"))
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_TETRIS_TYPE, subtype, Path.GetFullPath(fil), "02010", "");
        }
        else if (fil.ToUpper().Contains("\\MEDIA\\"))
        {
          string st = Path.GetFullPath(fil).Substring(Path.GetFullPath(fil).ToLower().IndexOf("\\media\\") + 6);
          addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_MEDIA_TYPE, subtype, Path.GetFullPath(fil), "02010",
                 "OutputFileName=" + st + "|");
        }
        else
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.SKIN_TYPE, subtype, Path.GetFullPath(fil), "02010", "");
        }
        return;
      }
     
      if (fil.ToUpper().Contains("THUMBS"))
      {
        foreach (string subdir in thumbst_comboBox1.Items)
        {
          if (!string.IsNullOrEmpty(subdir.Trim()) && fil.ToUpper().Contains(subdir.ToUpper()))
            addrow(Path.GetFileName(fil), MPinstallerStruct.THUMBS_TYPE, subdir, Path.GetFullPath(fil), "04010", "");
        }
        return;
      }

      if (Path.GetExtension(fil).ToUpper() == ".TXT")
      {
        addrow(Path.GetFileName(fil), MPinstallerStruct.TEXT_TYPE, MPinstallerStruct.TEXT_README_TYPE,
               Path.GetFullPath(fil), "02010", "");
        return;
      }
      addrow(Path.GetFileName(fil), MPinstallerStruct.OTHER_TYPE, "", Path.GetFullPath(fil), "02010", "");
    }

    private void directoryAutomatedDiscoverTypeToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
      {
        DirectoryInfo di = new DirectoryInfo(folderBrowserDialog1.SelectedPath);
        FileInfo[] fileList = di.GetFiles("*.*", SearchOption.AllDirectories);
        foreach (FileInfo f in fileList)
        {
          if (!f.DirectoryName.Contains(".svn"))
          {
            addFile(f.FullName);
          }
        }
      }
    }

    private bool IsGoodToSave()
    {
      if (proiectt_textBox1.Text.Length == 0)
      {
        MessageBox.Show("Name is mandatory !", "Stop");
        proiectt_textBox1.Focus();
        return false;
      }
      if (proiectt_textBox6.Text.Length == 0)
      {
        MessageBox.Show("Group is mandatory !", "Stop");
        proiectt_textBox6.Focus();
        return false;
      }
      if (proiectt_comboBox1.Text.Length == 0)
      {
        MessageBox.Show("Release is mandatory !", "Stop");
        proiectt_comboBox1.Focus();
        return false;
      }
      return true;
    }

    private void bossview_KeyDown(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Delete)
      {
        foreach (ListViewItem li in bossview.SelectedItems)
        {
          bossview.Items.Remove(li);
        }
      }
    }

    /// <summary>
    /// Skin tab Set to defaul button
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
    private void button1_Click(object sender, EventArgs e)
    {
      _struct.AddFileList(bossview);
      bossview.Items.Clear();
      for (int i = 0; i < _struct.FileList.Count; i++)
      {
        if (((MPIFileList) _struct.FileList[i]).SkinType &&
            ((MPIFileList) _struct.FileList[i]).FileName.Contains(@"\" + skint_comboBox1.Text + @"\"))
        {
          ((MPIFileList) _struct.FileList[i]).FileProperties.DefaultFile = true;
          ((MPIFileList) _struct.FileList[i]).Option = ((MPIFileList) _struct.FileList[i]).FileProperties.ToString();
        }
        addrow((MPIFileList) _struct.FileList[i]);
      }
    }

    private void button2_Click(object sender, EventArgs e)
    {
      _struct.AddFileList(bossview);
      bossview.Items.Clear();
      for (int i = 0; i < _struct.FileList.Count; i++)
      {
        if (((MPIFileList) _struct.FileList[i]).SkinType &&
            ((MPIFileList) _struct.FileList[i]).FileName.Contains(@"\" + skint_comboBox1.Text + @"\"))
        {
          ((MPIFileList) _struct.FileList[i]).FileProperties.DefaultFile = false;
          ((MPIFileList) _struct.FileList[i]).Option = ((MPIFileList) _struct.FileList[i]).FileProperties.ToString();
        }
        addrow((MPIFileList) _struct.FileList[i]);
      }
    }

    private void internalPluginToolStripMenuItem_Click(object sender, EventArgs e)
    {
      openFileDialog1.Filter = "dll files (*.dll)|*.dll|All files (*.*)|*.*";
      openFileDialog1.FileName = "";
      openFileDialog1.DefaultExt = "*.dll";
      openFileDialog1.Multiselect = false;
      if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
      {
        foreach (string fil in openFileDialog1.FileNames)
        {
          addrow(Path.GetFileName(fil), MPinstallerStruct.INTERNAL_TYPE, MPinstallerStruct.INTERNAL_PLUGIN_SUBTYPE,
                 Path.GetFullPath(fil), "07010", "");
        }
      }
    }

    private void systemFontToolStripMenuItem_Click(object sender, EventArgs e)
    {
      addskin(6);
    }

    private void installScriptToolStripMenuItem_Click(object sender, EventArgs e)
    {
      if (!scriptEditor.Visible)
      {
        scriptEditor.Show();
      }
      else
      {
        scriptEditor.BringToFront();
      }
    }
  }
}