using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
namespace home
{
  /// <summary>
  /// Summary description for SetupForm.
  /// </summary>
	public class SetupForm : System.Windows.Forms.Form , ISetupForm
	{
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.Button button1;
        private System.Windows.Forms.CheckBox chkBoxScrolling;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		
		public bool	CanEnable()		// Indicates whether plugin can be enabled/disabled
		{
			return false;
		}
		
		public int  GetWindowId()
		{
			return (int)GUIWindow.Window.WINDOW_HOME;
		}

		public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage)
		{
			strButtonText="";
			strButtonImage="";
			strButtonImageFocus="";
			strPictureImage="";
			return false;
		}
		public bool DefaultEnabled()
		{
			return true;
		}
    public string PluginName() 
    {
      return "Home";
    }
    public string Description()
    {
      return "Mediaportals home screen";
    }
    public string Author()
    {
      return "Frodo";
    }
    public void ShowPlugin()
    {
        ShowDialog();
    }

    public bool HasSetup()
    {
      return true;
    }

  public SetupForm()
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
  protected override void Dispose( bool disposing )
  {
    if( disposing )
    {
      if(components != null)
      {
        components.Dispose();
      }
    }
    base.Dispose( disposing );
  }

  #region Windows Form Designer generated code
  /// <summary>
  /// Required method for Designer support - do not modify
  /// the contents of this method with the code editor.
  /// </summary>
  private void InitializeComponent()
{
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SetupForm));
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.chkBoxScrolling = new System.Windows.Forms.CheckBox();
		this.radioButton2 = new System.Windows.Forms.RadioButton();
		this.radioButton1 = new System.Windows.Forms.RadioButton();
		this.button1 = new System.Windows.Forms.Button();
		this.groupBox1.SuspendLayout();
		this.SuspendLayout();
		// 
		// groupBox1
		// 
		this.groupBox1.Controls.Add(this.chkBoxScrolling);
		this.groupBox1.Controls.Add(this.radioButton2);
		this.groupBox1.Controls.Add(this.radioButton1);
		this.groupBox1.Location = new System.Drawing.Point(16, 16);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(344, 96);
		this.groupBox1.TabIndex = 0;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Home settings";
		// 
		// chkBoxScrolling
		// 
		this.chkBoxScrolling.Checked = true;
		this.chkBoxScrolling.CheckState = System.Windows.Forms.CheckState.Checked;
		this.chkBoxScrolling.Location = new System.Drawing.Point(160, 24);
		this.chkBoxScrolling.Name = "chkBoxScrolling";
		this.chkBoxScrolling.Size = new System.Drawing.Size(168, 24);
		this.chkBoxScrolling.TabIndex = 2;
		this.chkBoxScrolling.Text = "Scroll menu items";
		// 
		// radioButton2
		// 
		this.radioButton2.Location = new System.Drawing.Point(24, 48);
		this.radioButton2.Name = "radioButton2";
		this.radioButton2.TabIndex = 1;
		this.radioButton2.Text = "MM-DD-YYYY";
		// 
		// radioButton1
		// 
		this.radioButton1.Location = new System.Drawing.Point(24, 24);
		this.radioButton1.Name = "radioButton1";
		this.radioButton1.Size = new System.Drawing.Size(112, 24);
		this.radioButton1.TabIndex = 0;
		this.radioButton1.Text = "DD-MM-YYYY";
		// 
		// button1
		// 
		this.button1.Location = new System.Drawing.Point(288, 128);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(64, 23);
		this.button1.TabIndex = 1;
		this.button1.Text = "Save";
		this.button1.Click += new System.EventHandler(this.button1_Click);
		// 
		// SetupForm
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(368, 165);
		this.Controls.Add(this.button1);
		this.Controls.Add(this.groupBox1);
		this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
		this.Name = "SetupForm";
		this.Text = "Home Setup";
		this.Load += new System.EventHandler(this.SetupForm_Load);
		this.groupBox1.ResumeLayout(false);
		this.ResumeLayout(false);

	}
  #endregion

    private void button1_Click(object sender, System.EventArgs e)
    {
      using (AMS.Profile.Xml   xmlWriter=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        int iLayout=0;
        if (radioButton2.Checked) iLayout=1;
        xmlWriter.SetValue("home","datelayout",iLayout.ToString());
        xmlWriter.SetValueAsBool("home","scroll",chkBoxScrolling.Checked);
      }
      this.Close();
    }

    private void SetupForm_Load(object sender, System.EventArgs e)
    {
      using (AMS.Profile.Xml   xmlreader=new AMS.Profile.Xml("MediaPortal.xml"))
      {
        int iLayout = xmlreader.GetValueAsInt("home","datelayout",0);
        if (iLayout==0) radioButton1.Checked=true;
        else radioButton2.Checked=true;

        chkBoxScrolling.Checked=xmlreader.GetValueAsBool("home","scroll",true);
      }
    }
}
}
