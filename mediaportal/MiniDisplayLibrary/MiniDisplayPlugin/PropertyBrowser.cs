using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using MediaPortal.UserInterface.Controls;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public class PropertyBrowser : MPForm
  {
    private IContainer components = null;
    private DataGrid dataGrid1;
    private bool DoDebug = Settings.Instance.ExtensiveLogging;
    private MPLabel label1;
    private MPLabel label2;
    private Panel panel1;
    private DataTable properties;
    private MPTextBox txtActiveWindow;
    private MPTextBox txtStatus;

    public PropertyBrowser()
    {
      this.InitializeComponent();
    }

    protected override void Dispose(bool disposing)
    {
      Log.Info("PropertyBrowser.Dispose(): called.", new object[0]);
      lock (MiniDisplayHelper.PropertyBrowserMutex)
      {
        MiniDisplayHelper.DisablePropertyBrowser();
      }
      if (disposing && (this.components != null))
      {
        this.components.Dispose();
      }
      base.Dispose(disposing);
    }

    private void GUIPropertyManager_OnPropertyChanged(string tag, string value)
    {
      if (this.properties != null)
      {
        DataRow row = this.properties.Rows.Find(tag);
        if (row == null)
        {
          this.properties.Rows.Add(new object[] {tag, value});
        }
        else
        {
          row["Value"] = value;
        }
      }
    }

    private void InitializeComponent()
    {
      this.dataGrid1 = new DataGrid();
      this.panel1 = new Panel();
      this.txtStatus = new MPTextBox();
      this.label2 = new MPLabel();
      this.txtActiveWindow = new MPTextBox();
      this.label1 = new MPLabel();
      ((ISupportInitialize) (this.dataGrid1)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // dataGrid1
      // 
      this.dataGrid1.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                                 | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.dataGrid1.DataMember = "";
      this.dataGrid1.HeaderForeColor = SystemColors.ControlText;
      this.dataGrid1.Location = new Point(0, 0);
      this.dataGrid1.Name = "dataGrid1";
      this.dataGrid1.ReadOnly = true;
      this.dataGrid1.Size = new Size(336, 254);
      this.dataGrid1.TabIndex = 0;
      // 
      // panel1
      // 
      this.panel1.Controls.Add(this.txtStatus);
      this.panel1.Controls.Add(this.label2);
      this.panel1.Controls.Add(this.txtActiveWindow);
      this.panel1.Controls.Add(this.label1);
      this.panel1.Dock = DockStyle.Bottom;
      this.panel1.Location = new Point(0, 254);
      this.panel1.Name = "panel1";
      this.panel1.Size = new Size(336, 64);
      this.panel1.TabIndex = 1;
      // 
      // txtStatus
      // 
      this.txtStatus.Anchor = ((AnchorStyles) (((AnchorStyles.Bottom | AnchorStyles.Left)
                                                | AnchorStyles.Right)));
      this.txtStatus.BorderColor = Color.Empty;
      this.txtStatus.Location = new Point(96, 32);
      this.txtStatus.Name = "txtStatus";
      this.txtStatus.ReadOnly = true;
      this.txtStatus.Size = new Size(222, 20);
      this.txtStatus.TabIndex = 3;
      // 
      // label2
      // 
      this.label2.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.label2.Location = new Point(8, 32);
      this.label2.Name = "label2";
      this.label2.Size = new Size(80, 23);
      this.label2.TabIndex = 2;
      this.label2.Text = "Status";
      this.label2.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // txtActiveWindow
      // 
      this.txtActiveWindow.Anchor = ((AnchorStyles) (((AnchorStyles.Bottom | AnchorStyles.Left)
                                                      | AnchorStyles.Right)));
      this.txtActiveWindow.BorderColor = Color.Empty;
      this.txtActiveWindow.Location = new Point(96, 8);
      this.txtActiveWindow.Name = "txtActiveWindow";
      this.txtActiveWindow.ReadOnly = true;
      this.txtActiveWindow.Size = new Size(222, 20);
      this.txtActiveWindow.TabIndex = 1;
      // 
      // label1
      // 
      this.label1.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Left)));
      this.label1.Location = new Point(8, 8);
      this.label1.Name = "label1";
      this.label1.Size = new Size(80, 23);
      this.label1.TabIndex = 0;
      this.label1.Text = "Active Window";
      this.label1.TextAlign = ContentAlignment.MiddleLeft;
      // 
      // PropertyBrowser
      // 
      this.AutoScaleDimensions = new SizeF(6F, 13F);
      this.ClientSize = new Size(336, 318);
      this.Controls.Add(this.dataGrid1);
      this.Controls.Add(this.panel1);
      this.Name = "PropertyBrowser";
      this.Text = "MiniDisplay - Property Browser";
      this.TopMost = true;
      this.Load += new EventHandler(this.PropertyBrowser_Load);
      ((ISupportInitialize) (this.dataGrid1)).EndInit();
      this.panel1.ResumeLayout(false);
      this.panel1.PerformLayout();
      this.ResumeLayout(false);
    }

    private void PropertyBrowser_Load(object sender, EventArgs e)
    {
      this.properties = new DataTable("Properties");
      DataColumn column = this.properties.Columns.Add("Key", typeof (string));
      this.properties.Columns.Add("Value", typeof (string));
      this.properties.PrimaryKey = new DataColumn[] {column};
      this.dataGrid1.DataSource = this.properties;
      GUIPropertyManager.OnPropertyChanged +=
        new GUIPropertyManager.OnPropertyChangedHandler(this.GUIPropertyManager_OnPropertyChanged);
      this.GUIPropertyManager_OnPropertyChanged("#currentmodule", GUIPropertyManager.GetProperty("#currentmodule"));
      if (this.DoDebug)
      {
        Log.Info("MiniDisplay.PropertyBrowser_Load(): PropertyBrowser loaded.", new object[0]);
      }
    }

    public void SetActiveWindow(GUIWindow.Window _window)
    {
      if (base.InvokeRequired)
      {
        base.Invoke(new SetActiveWindowDelegate(this.SetActiveWindow), new object[] {_window});
      }
      this.txtActiveWindow.Text = _window.ToString();
    }

    public void SetStatus(Status _status)
    {
      if (base.InvokeRequired)
      {
        base.Invoke(new SetStatusDelegate(this.SetStatus), new object[] {_status});
      }
      this.txtStatus.Text = _status.ToString();
    }

    private delegate void SetActiveWindowDelegate(GUIWindow.Window _window);

    private delegate void SetStatusDelegate(Status status);
  }
}