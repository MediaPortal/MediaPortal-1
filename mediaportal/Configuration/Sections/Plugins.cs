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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.Configuration.Controls;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

#pragma warning disable 108

namespace MediaPortal.Configuration.Sections
{
  public class Plugins : SectionSettings
  {
    private class ItemTag
    {
      public string DLLName;
      public ISetupForm SetupForm;
      public string strType = string.Empty;
      public int windowId = -1;
    } ;

    private GroupBox groupBox1;
    private Button setupButton;
    private IContainer components = null;

    private ArrayList availablePlugins = new ArrayList();
    private DataGrid dataGrid1;
    private ArrayList loadedPlugins = new ArrayList();
    private DataSet ds = new DataSet();
    private bool isLoaded = false;

    public Plugins()
      : this("Plugins")
    {
    }

    public Plugins(string name)
      : base(name)
    {
      // This call is required by the Windows Form Designer.
      InitializeComponent();

      DataGridTableStyle ts1 = new DataGridTableStyle();
      ts1.MappingName = "Customers";
      ts1.AlternatingBackColor = Color.LightBlue;


      DataGridColumnStyle boolCol = new FormattableBooleanColumn();
      boolCol.MappingName = "bool1";
      boolCol.HeaderText = "Enabled";
      boolCol.Width = 60;
      ts1.GridColumnStyles.Add(boolCol);

      boolCol = new FormattableBooleanColumn();
      boolCol.MappingName = "bool2";
      boolCol.HeaderText = "Home";
      boolCol.Width = 50;
      ts1.GridColumnStyles.Add(boolCol);


      boolCol = new FormattableBooleanColumn();
      boolCol.MappingName = "bool3";
      boolCol.HeaderText = "My Plugins";
      boolCol.Width = 80;
      ts1.GridColumnStyles.Add(boolCol);

      DataGridColumnStyle TextCol = new DataGridTextBoxColumn();
      TextCol.MappingName = "Name";
      TextCol.HeaderText = "Plugin Name";
      TextCol.Width = 100;
      TextCol.ReadOnly = true;
      ts1.GridColumnStyles.Add(TextCol);


      TextCol = new DataGridTextBoxColumn();
      TextCol.MappingName = "Author";
      TextCol.HeaderText = "Author";
      TextCol.Width = 80;
      TextCol.ReadOnly = true;
      ts1.GridColumnStyles.Add(TextCol);

      TextCol = new DataGridTextBoxColumn();
      TextCol.MappingName = "Description";
      TextCol.HeaderText = "Description";
      TextCol.Width = 250;
      TextCol.ReadOnly = true;
      ts1.GridColumnStyles.Add(TextCol);

      dataGrid1.TableStyles.Add(ts1);

      ds.Tables.Add("Customers");
      ds.Tables[0].Columns.Add("bool1", typeof (bool));
      ds.Tables[0].Columns.Add("bool2", typeof (bool));
      ds.Tables[0].Columns.Add("bool3", typeof (bool));
      ds.Tables[0].Columns.Add("Name", typeof (string));
      ds.Tables[0].Columns.Add("Author", typeof (string));
      ds.Tables[0].Columns.Add("Description", typeof (string));
      ds.Tables[0].Columns.Add("tag", typeof (ItemTag));
    }

    /// <summary>
    /// 
    /// </summary>
    private void PopulateDatagrid()
    {
      foreach (ItemTag tag in loadedPlugins)
      {
        ds.Tables[0].Rows.Add(new object[]
                                {
                                  true, true, false, tag.SetupForm.PluginName(), tag.SetupForm.Author(),
                                  tag.SetupForm.Description(), tag
                                });
      }
      ds.Tables[0].DefaultView.AllowNew = false;
      ds.Tables[0].DefaultView.AllowDelete = false;
      dataGrid1.DataSource = ds.Tables[0];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="directory"></param>
    private void EnumeratePluginDirectory(string directory)
    {
      if (Directory.Exists(directory))
      {
        //
        // Enumerate files
        //
        string[] files = Directory.GetFiles(directory, "*.dll");

        //
        // Add to list
        //
        foreach (string file in files)
        {
          availablePlugins.Add(file);
        }
      }
    }

    private void EnumeratePlugins()
    {
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "windows"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "subtitle"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "tagreaders"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "externalplayers"));
      EnumeratePluginDirectory(Config.GetSubFolder(Config.Dir.Plugins, "process"));
    }

    private void LoadPlugins()
    {
      foreach (string pluginFile in availablePlugins)
      {
        try
        {
          Assembly pluginAssembly = Assembly.LoadFrom(pluginFile);

          if (pluginAssembly != null)
          {
            Type[] exportedTypes = pluginAssembly.GetExportedTypes();

            foreach (Type type in exportedTypes)
            {
              // an abstract class cannot be instanciated
              if (type.IsAbstract)
              {
                continue;
              }
              //
              // Try to locate the interface we're interested in
              //
              if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
              {
                try
                {
                  //
                  // Create instance of the current type
                  //
                  object pluginObject = Activator.CreateInstance(type);
                  ISetupForm pluginForm = pluginObject as ISetupForm;

                  if (pluginForm != null)
                  {
                    ItemTag tag = new ItemTag();
                    tag.SetupForm = pluginForm;
                    tag.DLLName = pluginFile.Substring(pluginFile.LastIndexOf(@"\") + 1);
                    tag.windowId = pluginForm.GetWindowId();
                    loadedPlugins.Add(tag);
                  }
                }
                catch (Exception setupFormException)
                {
                  Log.Info("Exception in plugin SetupForm loading :{0}", setupFormException.Message);
                  Log.Info("Current class is :{0}", type.FullName);
#if DEBUG
                  Log.Info(setupFormException.StackTrace);
#endif
                }
              }
            }
            foreach (Type t in exportedTypes)
            {
              try
              {
                if (t.IsClass)
                {
                  if (t.IsSubclassOf(typeof (GUIWindow)))
                  {
                    object newObj = Activator.CreateInstance(t);
                    GUIWindow win = (GUIWindow) newObj;

                    foreach (ItemTag tag in loadedPlugins)
                    {
                      if (tag.windowId == win.GetID)
                      {
                        tag.strType = win.GetType().ToString();
                        break;
                      }
                    }
                  }
                }
              }
              catch (Exception guiWindowException)
              {
                Log.Info("Exception in plugin GUIWindows loading :{0}", guiWindowException.Message);
                Log.Info("Current class is :{0}", t.FullName);
#if DEBUG
                Log.Info(guiWindowException.StackTrace);
#endif
              }
            }
          }
        }
        catch (Exception unknownException)
        {
          Log.Info("Exception in plugin loading :{0}", unknownException.Message);
#if DEBUG
          Log.Info(unknownException.StackTrace);
#endif
        }
      }
    }

    public string GetPluginDescription(string name)
    {
      if (!isLoaded)
      {
        this.LoadAll();
      }
      foreach (ItemTag tag in loadedPlugins)
      {
        if (tag.SetupForm.PluginName() == name)
        {
          return tag.SetupForm.Description();
        }
      }
      return string.Empty;
    }

    public override void LoadSettings()
    {
      try
      {
        using (Settings xmlreader = new MPSettings())
        {
          foreach (DataRow row in ds.Tables[0].Rows)
          {
            ItemTag itemTag = row["tag"] as ItemTag;

            if (itemTag.SetupForm != null)
            {
              if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
              {
                row["bool1"] = xmlreader.GetValueAsBool("plugins", itemTag.SetupForm.PluginName(),
                                                        itemTag.SetupForm.DefaultEnabled());
              }
              else
              {
                row["bool1"] = itemTag.SetupForm.DefaultEnabled();
              }

              bool bHome = false;
              bool bPlugins = false;
              row["bool2"] = bHome;
              row["bool2"] = bPlugins;
              string buttontxt, buttonimage, buttonimagefocus, picture;
              if (itemTag.SetupForm.CanEnable() || itemTag.SetupForm.DefaultEnabled())
              {
                if (itemTag.SetupForm.GetHome(out buttontxt, out buttonimage, out buttonimagefocus, out picture))
                {
                  bHome = true;
                  row["bool2"] = xmlreader.GetValueAsBool("home", itemTag.SetupForm.PluginName(), bHome);
                  row["bool3"] = xmlreader.GetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), bPlugins);
                }
              }
            }
          }
        }
      }
      catch (Exception)
      {
      }
    }


    public override void SaveSettings()
    {
      LoadAll();
      try
      {
        using (Settings xmlwriter = new MPSettings())
        {
          foreach (DataRow row in ds.Tables[0].Rows)
          {
            ItemTag itemTag = row["tag"] as ItemTag;

            bool bEnabled = (bool) row["bool1"];
            bool bHome = (bool) row["bool2"];
            bool bPlugins = (bool) row["bool3"];
            if (itemTag.SetupForm != null)
            {
              if (itemTag.SetupForm.DefaultEnabled() && !itemTag.SetupForm.CanEnable())
              {
                bEnabled = true;
              }
            }
            else
            {
              bEnabled = true;
            }
            xmlwriter.SetValueAsBool("plugins", itemTag.SetupForm.PluginName(), bEnabled);
            xmlwriter.SetValueAsBool("home", itemTag.SetupForm.PluginName(), bHome);
            xmlwriter.SetValueAsBool("myplugins", itemTag.SetupForm.PluginName(), bPlugins);
            xmlwriter.SetValueAsBool("pluginsdlls", itemTag.DLLName, bEnabled);
            if (itemTag.strType != string.Empty)
            {
              xmlwriter.SetValueAsBool("pluginswindows", itemTag.strType, bEnabled);
            }
          }
        }
      }
      catch (Exception)
      {
      }
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

    #region Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.groupBox1 = new MediaPortal.UserInterface.Controls.MPGroupBox();
      this.dataGrid1 = new System.Windows.Forms.DataGrid();
      this.setupButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.groupBox1.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize) (this.dataGrid1)).BeginInit();
      this.SuspendLayout();
      // 
      // groupBox1
      // 
      this.groupBox1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.groupBox1.Controls.Add(this.dataGrid1);
      this.groupBox1.Controls.Add(this.setupButton);
      this.groupBox1.Location = new System.Drawing.Point(0, 0);
      this.groupBox1.Name = "groupBox1";
      this.groupBox1.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.TabIndex = 0;
      this.groupBox1.TabStop = false;
      // 
      // dataGrid1
      // 
      this.dataGrid1.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
           | System.Windows.Forms.AnchorStyles.Right)));
      this.dataGrid1.DataMember = "";
      this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
      this.dataGrid1.Location = new System.Drawing.Point(16, 24);
      this.dataGrid1.Name = "dataGrid1";
      this.dataGrid1.Size = new System.Drawing.Size(440, 344);
      this.dataGrid1.TabIndex = 0;
      // 
      // setupButton
      // 
      this.setupButton.Anchor =
        ((System.Windows.Forms.AnchorStyles)
         ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.setupButton.Location = new System.Drawing.Point(384, 376);
      this.setupButton.Name = "setupButton";
      this.setupButton.Size = new System.Drawing.Size(72, 22);
      this.setupButton.TabIndex = 1;
      this.setupButton.Text = "&Setup";
      this.setupButton.Click += new System.EventHandler(this.setupButton_Click);
      // 
      // Plugins
      // 
      this.Controls.Add(this.groupBox1);
      this.Name = "Plugins";
      this.Size = new System.Drawing.Size(472, 408);
      this.groupBox1.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize) (this.dataGrid1)).EndInit();
      this.ResumeLayout(false);
    }

    #endregion

    private void setupButton_Click(object sender, EventArgs e)
    {
      CurrencyManager cm = (CurrencyManager) this.BindingContext[dataGrid1.DataSource, dataGrid1.DataMember];
      DataRow row = ((DataRowView) cm.Current).Row;
      if (((bool) row["bool1"]) == false)
      {
        MessageBox.Show("Selected plugin is not enabled");
        return;
      }
      ItemTag tag = (ItemTag) row["tag"];
      if (tag.SetupForm != null)
      {
        if (tag.SetupForm.HasSetup())
        {
          tag.SetupForm.ShowPlugin();
          return;
        }
      }
      MessageBox.Show("Plugin has no setup");
    }

    public override void OnSectionActivated()
    {
      base.OnSectionActivated();
      LoadAll();
    }

    private void LoadAll()
    {
      if (!isLoaded)
      {
        isLoaded = true;
        //
        // Enumerate available plugins
        //
        EnumeratePlugins();

        //
        // Load plugins
        //
        LoadPlugins();


        //
        // Populate our list
        //
        PopulateDatagrid();
        LoadSettings();
      }
    }

  }
}