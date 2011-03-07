using System;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;


namespace MediaPortal
{
  public partial class IncompatiblePluginsForm : MPForm
  {
    private int _CloseInSeconds = 15;

    public IncompatiblePluginsForm()
    {
      InitializeComponent();
    }

    public bool CheckForIncompatiblePlugins()
    {
      Log.Debug("Checking for incompatible plugins");

      if (PluginManager.IncompatiblePlugins.Count == 0 &&
          PluginManager.IncompatiblePluginAssemblies.Count == 0)
      {
        return false;
      }

      PluginsList.BeginUpdate();
      try
      {
        PluginsList.Items.Clear();
        foreach (var plugin in PluginManager.IncompatiblePluginAssemblies)
        {
          int pos = PluginsList.Items.Add(plugin);
        }
        foreach (var plugin in PluginManager.IncompatiblePlugins)
        {
          PluginsList.Items.Add(plugin);
        }
      }
      finally
      {
        PluginsList.EndUpdate();
      }

      return true;
    }

    private void CloseTimerTick(object sender, EventArgs e)
    {
      _CloseInSeconds--;
      if (_CloseInSeconds > 0)
      {
        bClose.Text = string.Format("Continue ({0})", _CloseInSeconds);
      }
      else
      {
        CloseTimer.Enabled = false;
        bClose_Click(sender, e);
      }
    }

    private void bClose_Click(object sender, EventArgs e)
    {
      Close();
    }

    private void PluginsList_Format(object sender, ListControlConvertEventArgs e)
    {
      if (e.ListItem is Assembly)
      {
        e.Value = (e.ListItem as Assembly).GetName().Name;
      }
      else if (e.ListItem is Type)
      {
        var plugin = e.ListItem as Type;
        e.Value = string.Format("{0} (in {1})", plugin.Name, plugin.Assembly.GetName().Name);
      }
      else
      {
        e.Value = e.ListItem.ToString();
      }

    }

    private void PluginsList_Enter(object sender, EventArgs e)
    {
      bClose.Text = "Continue";
      CloseTimer.Enabled = false;
    }

  }
}
