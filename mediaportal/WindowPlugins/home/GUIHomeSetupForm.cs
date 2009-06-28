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
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.GUI.Home
{
  [PluginIcons("WindowPlugins.Home.Homemenu.gif", "WindowPlugins.Home.Homemenu_disabled.gif")]
  public partial class GUIHomeSetupForm : MPConfigForm, ISetupForm, IComparer
  {
    public GUIHomeSetupForm()
    {
      InitializeComponent();
      LoadSettings();
      UpdateTestBox();
    }

    private void LoadSettings()
    {
      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        chkboxFixScrollbar.Checked = xmlreader.GetValueAsBool("home", "scrollfixed", false);
        chkBoxUseMyPlugins.Checked = xmlreader.GetValueAsBool("home", "usemyplugins", true);
        chkBoxAnimation.Checked = xmlreader.GetValueAsBool("home", "enableanimation", true);
        checkBoxShowSeconds.Checked = xmlreader.GetValueAsBool("home", "LongTimeFormat", false);
        string text = xmlreader.GetValueAsString("home", "dateformat", "<Day> <DD>.<Month>");
        cboxFormat.Items.Add(text);
        if (!text.Equals("<Day> <DD>.<Month>"))
        {
          cboxFormat.Items.Add("<Day> <DD>.<Month>");
        }
        if (!text.Equals("<Day> <DD> <Month>"))
        {
          cboxFormat.Items.Add("<Day> <DD> <Month>");
        }
        if (!text.Equals("<Day> <Month> <DD>"))
        {
          cboxFormat.Items.Add("<Day> <Month> <DD>");
        }
        cboxFormat.Text = text;
      }
    }


    private void SaveSettings()
    {
      using (Profile.Settings xmlWriter = new Profile.MPSettings())
      {
        xmlWriter.SetValueAsBool("home", "scrollfixed", chkboxFixScrollbar.Checked);
        xmlWriter.SetValueAsBool("home", "usemyplugins", chkBoxUseMyPlugins.Checked);
        xmlWriter.SetValueAsBool("home", "enableanimation", chkBoxAnimation.Checked);
        xmlWriter.SetValueAsBool("home", "LongTimeFormat", checkBoxShowSeconds.Checked);
        xmlWriter.SetValue("home", "dateformat", cboxFormat.Text);
      }
      SaveMenuSorting();
    }

    private void SaveMenuSorting()
    {
      using (Profile.Settings xmlWriter = new Profile.MPSettings())
      {
        foreach (TreeNode node in tvMenu.Nodes)
        {
          xmlWriter.SetValue("pluginSorting", node.Text, node.Index);
          if (node.Nodes != null)
          {
            foreach (TreeNode subNode in node.Nodes)
            {
              xmlWriter.SetValue("pluginSorting", subNode.Text, subNode.Index);
            }
          }
        }
      }
    }

    #region Tab: Settings

    private void btnOK_Click(object sender, EventArgs e)
    {
      SaveSettings();
      Close();
    }

    private void btnCancel_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void btnDayText_Click(object sender, EventArgs e)
    {
      cboxFormat.Text += "<Day>";
      UpdateTestBox();
    }

    private void btnDayNumber_Click(object sender, EventArgs e)
    {
      cboxFormat.Text += "<DD>";
      UpdateTestBox();
    }

    private void btnMonthText_Click(object sender, EventArgs e)
    {
      cboxFormat.Text += "<Month>";
      UpdateTestBox();
    }

    private void btnMonthNumber_Click(object sender, EventArgs e)
    {
      cboxFormat.Text += "<MM>";
      UpdateTestBox();
    }

    private void btnYearText_Click(object sender, EventArgs e)
    {
      cboxFormat.Text += "<Year>";
      UpdateTestBox();
    }

    private void btnYearNumber_Click(object sender, EventArgs e)
    {
      cboxFormat.Text += "<YY>";
      UpdateTestBox();
    }

    private void cboxFormat_TextUpdate(object sender, EventArgs e)
    {
      UpdateTestBox();
    }

    private void UpdateTestBox()
    {
      tboxTest.Text = "";
      string dateString = cboxFormat.Text;
      if (String.IsNullOrEmpty(dateString))
      {
        return;
      }

      DateTime cur = DateTime.Now;
      string day;
      switch (cur.DayOfWeek)
      {
        case DayOfWeek.Monday:
          day = "Monday";
          break;
        case DayOfWeek.Tuesday:
          day = "Tuesday";
          break;
        case DayOfWeek.Wednesday:
          day = "Wednesday";
          break;
        case DayOfWeek.Thursday:
          day = "Thursday";
          break;
        case DayOfWeek.Friday:
          day = "Friday";
          break;
        case DayOfWeek.Saturday:
          day = "Saturday";
          break;
        default:
          day = "Sunday";
          break;
      }

      string month;
      switch (cur.Month)
      {
        case 1:
          month = "January";
          break;
        case 2:
          month = "February";
          break;
        case 3:
          month = "March";
          break;
        case 4:
          month = "April";
          break;
        case 5:
          month = "May";
          break;
        case 6:
          month = "June";
          break;
        case 7:
          month = "July";
          break;
        case 8:
          month = "August";
          break;
        case 9:
          month = "September";
          break;
        case 10:
          month = "October";
          break;
        case 11:
          month = "November";
          break;
        default:
          month = "December";
          break;
      }

      dateString = Util.Utils.ReplaceTag(dateString, "<Day>", day, "unknown");
      dateString = Util.Utils.ReplaceTag(dateString, "<DD>", cur.Day.ToString(), "unknown");

      dateString = Util.Utils.ReplaceTag(dateString, "<Month>", month, "unknown");
      dateString = Util.Utils.ReplaceTag(dateString, "<MM>", cur.Month.ToString(), "unknown");

      dateString = Util.Utils.ReplaceTag(dateString, "<Year>", cur.Year.ToString(), "unknown");
      dateString = Util.Utils.ReplaceTag(dateString, "<YY>", (cur.Year - 2000).ToString("00"), "unknown");

      tboxTest.Text = dateString;
    }

    private void cboxFormat_SelectedIndexChanged(object sender, EventArgs e)
    {
      UpdateTestBox();
    }

    private void cboxFormat_KeyPress(object sender, KeyPressEventArgs e)
    {
      UpdateTestBox();
    }

    #endregion

    #region Tab: Menu SetUp

    private void LoadPlugins()
    {
      tvMenu.Nodes.Clear();
      string directory = Config.GetSubFolder(Config.Dir.Plugins, "windows");
      if (!Directory.Exists(directory))
      {
        return;
      }

      using (Profile.Settings xmlreader = new Profile.MPSettings())
      {
        TreeNode tnMyPlugIns = null;
        bool useMyPlugins = xmlreader.GetValueAsBool("home", "usemyplugins", true);
        if (useMyPlugins)
        {
          tnMyPlugIns = new TreeNode("my Plugins");
          tnMyPlugIns.Tag = new PluginInfo("my Plugins");
          tvMenu.Nodes.Add(tnMyPlugIns);
        }

        string[] files = Directory.GetFiles(directory, "*.dll");
        foreach (string pluginFile in files)
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
                // Try to locate the interface we're interested in
                if (type.GetInterface("MediaPortal.GUI.Library.ISetupForm") != null)
                {
                  try
                  {
                    // Create instance of the current type
                    object pluginObject = Activator.CreateInstance(type);
                    ISetupForm pluginForm = pluginObject as ISetupForm;

                    if (pluginForm != null)
                    {
                      if (pluginForm.PluginName().Equals("Home"))
                      {
                        continue;
                      }
                      if (pluginForm.PluginName().Equals("my Plugins"))
                      {
                        if (tnMyPlugIns != null)
                        {
                          tnMyPlugIns.Tag = new PluginInfo(pluginForm);
                          tvMenu.Nodes.Add(tnMyPlugIns);
                        }
                        continue;
                      }
                      string enabled = xmlreader.GetValue("plugins", pluginForm.PluginName());
                      if (enabled.CompareTo("yes") != 0)
                      {
                        continue;
                      }

                      string showInHome = xmlreader.GetValue("home", pluginForm.PluginName());

                      TreeNode node;
                      if ((useMyPlugins) && (showInHome.CompareTo("no") == 0))
                      {
                        node = tnMyPlugIns.Nodes.Add(pluginForm.PluginName());
                      }
                      else
                      {
                        node = tvMenu.Nodes.Add(pluginForm.PluginName());
                      }

                      if (node != null)
                      {
                        node.Tag = new PluginInfo(pluginForm);
                      }
                    }
                  }
                  catch (Exception setupFormException)
                  {
                    Log.Info("Exception in plugin SetupForm loading :{0}", setupFormException.Message);
                    Log.Info("Current class is :{0}", type.FullName);
                    Log.Info(setupFormException.StackTrace);
                  }
                }
              }
            }
          }
          catch (Exception unknownException)
          {
            Log.Info("Exception in plugin loading :{0}", unknownException.Message);
            Log.Info(unknownException.StackTrace);
          }
        }
      }

      ValidateIndex();

      tvMenu.TreeViewNodeSorter = this;
      tvMenu.Sort();
    }

    private void ValidateIndex()
    {
      bool updated = false;
      foreach (TreeNode node in tvMenu.Nodes)
      {
        if (((PluginInfo) node.Tag).Index == Int32.MaxValue)
        {
          ((PluginInfo) node.Tag).Index = node.Index;
          updated = true;
        }
        if (node.Nodes != null)
        {
          foreach (TreeNode subNode in node.Nodes)
          {
            if (((PluginInfo) subNode.Tag).Index == Int32.MaxValue)
            {
              ((PluginInfo) subNode.Tag).Index = subNode.Index;
              updated = true;
            }
          }
        }
      }
      if (updated)
      {
        SaveMenuSorting();
      }
    }

    private void tvMenu_AfterSelect(object sender, TreeViewEventArgs e)
    {
      TreeNode tnSelected = tvMenu.SelectedNode;
      laName.Text = "Name: ";
      if (tnSelected != null)
      {
        laName.Text += tnSelected.Text;
      }
    }

    private void buUp_Click(object sender, EventArgs e)
    {
      TreeNode tnSelected = tvMenu.SelectedNode;
      if (tnSelected == null)
      {
        return;
      }

      tvMenu.BeginUpdate();
      if (((PluginInfo) tnSelected.Tag).Index > 0)
      {
        ((PluginInfo) tnSelected.Tag).Index--;
        ((PluginInfo) tnSelected.PrevNode.Tag).Index++;
        tvMenu.Sort();
        tvMenu.SelectedNode = tnSelected;
      }
      tvMenu.EndUpdate();
    }

    private void buDown_Click(object sender, EventArgs e)
    {
      TreeNode tnSelected = tvMenu.SelectedNode;
      if (tnSelected == null)
      {
        return;
      }

      tvMenu.BeginUpdate();
      TreeNodeCollection nodeColl = tvMenu.Nodes;
      if (tnSelected.Parent != null)
      {
        nodeColl = tnSelected.Parent.Nodes;
      }

      if (((PluginInfo) tnSelected.Tag).Index + 1 < nodeColl.Count)
      {
        ((PluginInfo) tnSelected.Tag).Index++;
        ((PluginInfo) tnSelected.NextNode.Tag).Index--;
        tvMenu.Sort();

        tvMenu.SelectedNode = tnSelected;
      }
      tvMenu.EndUpdate();
    }

    #endregion

    #region ISetupForm members

    public string PluginName()
    {
      return "Home";
    }

    public string Description()
    {
      return "Home Screen";
    }

    public string Author()
    {
      return "Bavarian";
    }

    public void ShowPlugin()
    {
      Form setup = new GUIHomeSetupForm();
      setup.ShowDialog();
    }

    //System.Reflection.TargetInvocationException
    public bool HasSetup()
    {
      return true;
    }

    public bool CanEnable()
    {
      return false;
    }

    public bool DefaultEnabled()
    {
      return true;
    }

    public int GetWindowId()
    {
      return (int) GUIWindow.Window.WINDOW_HOME;
    }

    public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus,
                        out string strPictureImage)
    {
      strButtonText = PluginName();
      strButtonImage = string.Empty;
      strButtonImageFocus = string.Empty;
      strPictureImage = string.Empty;
      return true;
    }

    #endregion

    #region IComparer<TreeNode>

    public int Compare(object x, object y)
    {
      TreeNode tx = x as TreeNode;
      TreeNode ty = y as TreeNode;
      return ((PluginInfo) tx.Tag).Index - ((PluginInfo) ty.Tag).Index;
    }

    #endregion

    private void GUIHomeSetupForm_Load(object sender, EventArgs e)
    {
      LoadPlugins();
    }
  }

  public class PluginInfo
  {
    protected ISetupForm _setup = null;
    protected string _name = string.Empty;
    protected int _index = -1;
    protected string _text = string.Empty;
    protected string _btnImage = string.Empty;
    protected string _focus = string.Empty;
    protected string _picImage = string.Empty;

    public PluginInfo(string Name)
    {
      _name = Name;
      LoadData();
    }

    public PluginInfo(ISetupForm setup)
    {
      _setup = setup;
      LoadData();
    }

    public string Name
    {
      get { return _name; }
    }

    public int Index
    {
      get { return _index; }
      set { _index = value; }
    }

    public string Text
    {
      get { return _text; }
    }

    private void LoadData()
    {
      if (_setup != null)
      {
        _name = _setup.PluginName();
        _setup.GetHome(out _text, out _btnImage, out _focus, out _picImage);
      }

      if (_name != string.Empty)
      {
        using (Profile.Settings xmlreader = new Profile.MPSettings())
        {
          _index = xmlreader.GetValueAsInt("pluginSorting", _name, Int32.MaxValue);
        }
      }
    }
  }
}