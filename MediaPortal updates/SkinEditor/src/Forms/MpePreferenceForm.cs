using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Resources;
using System.Windows.Forms;

namespace Mpe.Forms
{
  /// <summary>
  /// Summary description for PreferencesForm.
  /// </summary>
  public class MpePreferenceForm : Form
  {
    private Button okButton;
    private Button cancelButton;
    private FolderBrowserDialog folderBrowser;
    private PropertyGrid grid;
    private Container components = null;

    private MpePreferences preferences;

    public MpePreferenceForm()
    {
      InitializeComponent();
      preferences = MediaPortalEditor.Global.Preferences;
    }

    #region Windows Form Designer Generated Code

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

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      ResourceManager resources = new ResourceManager(typeof(MpePreferenceForm));
      okButton = new Button();
      cancelButton = new Button();
      folderBrowser = new FolderBrowserDialog();
      grid = new PropertyGrid();
      SuspendLayout();
      // 
      // okButton
      // 
      okButton.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      okButton.FlatStyle = FlatStyle.System;
      okButton.Location = new Point(282, 278);
      okButton.Name = "okButton";
      okButton.TabIndex = 0;
      okButton.Text = "OK";
      okButton.Click += new EventHandler(okButton_Click);
      // 
      // cancelButton
      // 
      cancelButton.Anchor = ((AnchorStyles) ((AnchorStyles.Bottom | AnchorStyles.Right)));
      cancelButton.FlatStyle = FlatStyle.System;
      cancelButton.Location = new Point(362, 278);
      cancelButton.Name = "cancelButton";
      cancelButton.TabIndex = 1;
      cancelButton.Text = "Cancel";
      cancelButton.Click += new EventHandler(cancelButton_Click);
      // 
      // folderBrowser
      // 
      folderBrowser.ShowNewFolderButton = false;
      // 
      // grid
      // 
      grid.Anchor = ((AnchorStyles) ((((AnchorStyles.Top | AnchorStyles.Bottom)
                                       | AnchorStyles.Left)
                                      | AnchorStyles.Right)));
      grid.CommandsVisibleIfAvailable = true;
      grid.LargeButtons = false;
      grid.LineColor = SystemColors.ScrollBar;
      grid.Location = new Point(8, 8);
      grid.Name = "grid";
      grid.Size = new Size(432, 264);
      grid.TabIndex = 2;
      grid.Text = "PropertyGrid";
      grid.ToolbarVisible = false;
      grid.ViewBackColor = SystemColors.Window;
      grid.ViewForeColor = SystemColors.WindowText;
      // 
      // MpePreferenceForm
      // 
      AutoScaleBaseSize = new Size(5, 13);
      ClientSize = new Size(450, 312);
      Controls.Add(grid);
      Controls.Add(cancelButton);
      Controls.Add(okButton);
      FormBorderStyle = FormBorderStyle.FixedDialog;
      Icon = ((Icon) (resources.GetObject("$this.Icon")));
      MaximizeBox = false;
      MinimizeBox = false;
      Name = "MpePreferenceForm";
      Text = "Preferences";
      Load += new EventHandler(PreferencesForm_Load);
      ResumeLayout(false);
    }

    #endregion

    private void cancelButton_Click(object sender, EventArgs e)
    {
      DialogResult = DialogResult.Cancel;
      Close();
    }

    private void PreferencesForm_Load(object sender, EventArgs e)
    {
      CenterToScreen();
      grid.SelectedObject = preferences;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
      try
      {
        preferences.Save();
      }
      catch (Exception ee)
      {
        MpeLog.Error(ee);
        return;
      }
      DialogResult = DialogResult.OK;
      Close();
    }
  }


  public class DirectoryEditor : UITypeEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.Modal;
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      if (context != null && context.Instance != null)
      {
        MpeLog.Info("Context: " + context.Instance.GetType().ToString());
      }
      if (value != null)
      {
        MpeLog.Info("Value: " + value.ToString());
      }
      FolderBrowserDialog folderBrowser = new FolderBrowserDialog();
      folderBrowser.ShowDialog();
      return folderBrowser.SelectedPath;
    }
  }
}