using System;
using System.ComponentModel;
using System.Windows.Forms;
using ProgramsDatabase;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for AppSettings.
  /// </summary>
  public class AppSettings: UserControl
  {
    private IContainer components;
    protected OpenFileDialog dialogFile;
    protected FolderBrowserDialog dialogFolder;
    public ToolTip toolTip;
    protected ProgramConditionChecker m_Checker = new ProgramConditionChecker();

    public event EventHandler OnUpClick;
    public event EventHandler OnDownClick;


    public AppSettings()
    {
      // This call is required by the Windows.Forms Form Designer.
      InitializeComponent();

      // TODO: Add any initialization after the InitializeComponent call

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

    #region Component Designer generated code
    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      this.dialogFile = new System.Windows.Forms.OpenFileDialog();
      this.dialogFolder = new System.Windows.Forms.FolderBrowserDialog();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      // 
      // AppSettings
      // 
      this.Name = "AppSettings";

    }
    #endregion 

    public virtual bool AppObj2Form(AppItem curApp)
    {
      // virtual!
      return true;
    }

    public virtual bool Applist2Form(Applist apps)
    {
      // virtual!
      return true;
    }


    public virtual void Form2AppObj(AppItem curApp)
    {
      // virtual!
    }

    public virtual bool EntriesOK(AppItem curApp)
    {
      return true;
    }

    public virtual void LoadFromAppItem(AppItem tempApp)
    {
      // virtual!
    }

    protected void UpButtonClicked()
    {
      if (this.OnUpClick != null)
      {
        OnUpClick(this, null);
      }
    }

    protected void DownButtonClicked()
    {
      if (this.OnDownClick != null)
      {
        OnDownClick(this, null);
      }
    }

  }
}
