using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
using XPBurn;

namespace GUIBurner
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
  public class SetupForm : System.Windows.Forms.Form, ISetupForm 
  {
	private	XPBurn.XPBurnCD burnClass = new XPBurn.XPBurnCD(); 
 
	private System.Windows.Forms.ComboBox comboBox1;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.Button button1;
	private System.Windows.Forms.TextBox textBox1;
	private System.Windows.Forms.Label label2;
	private System.Windows.Forms.Button button2;
  private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
	private System.Windows.Forms.CheckBox checkBox1;
	private System.Windows.Forms.Label label3;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.CheckBox checkBox3;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox checkBox4;
		private System.Windows.Forms.Label label8;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public SetupForm()
	{
	  //
	  // Required for Windows Form Designer support
	  //
	  InitializeComponent();
	  GetRecorder();
		LoadSettings();
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
		this.comboBox1 = new System.Windows.Forms.ComboBox();
		this.label1 = new System.Windows.Forms.Label();
		this.button1 = new System.Windows.Forms.Button();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.button2 = new System.Windows.Forms.Button();
		this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		this.label3 = new System.Windows.Forms.Label();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.checkBox2 = new System.Windows.Forms.CheckBox();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.checkBox3 = new System.Windows.Forms.CheckBox();
		this.label6 = new System.Windows.Forms.Label();
		this.button3 = new System.Windows.Forms.Button();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.label7 = new System.Windows.Forms.Label();
		this.checkBox4 = new System.Windows.Forms.CheckBox();
		this.label8 = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// comboBox1
		// 
		this.comboBox1.Location = new System.Drawing.Point(128, 8);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(344, 21);
		this.comboBox1.TabIndex = 0;
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(8, 8);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(80, 24);
		this.label1.TabIndex = 1;
		this.label1.Text = "Select Drive";
		// 
		// button1
		// 
		this.button1.Location = new System.Drawing.Point(496, 256);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(88, 24);
		this.button1.TabIndex = 2;
		this.button1.Text = "OK";
		this.button1.Click += new System.EventHandler(this.button1_Click);
		// 
		// textBox1
		// 
		this.textBox1.Location = new System.Drawing.Point(128, 40);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(344, 20);
		this.textBox1.TabIndex = 3;
		this.textBox1.Text = "";
		// 
		// label2
		// 
		this.label2.Location = new System.Drawing.Point(8, 40);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(96, 24);
		this.label2.TabIndex = 4;
		this.label2.Text = "Select Temp Path";
		// 
		// button2
		// 
		this.button2.Location = new System.Drawing.Point(504, 40);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(32, 24);
		this.button2.TabIndex = 5;
		this.button2.Text = "...";
		this.button2.Click += new System.EventHandler(this.button2_Click);
		// 
		// checkBox1
		// 
		this.checkBox1.Checked = true;
		this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox1.Location = new System.Drawing.Point(128, 72);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(16, 16);
		this.checkBox1.TabIndex = 6;
		this.checkBox1.Text = "checkBox1";
		// 
		// label3
		// 
		this.label3.Location = new System.Drawing.Point(8, 72);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(112, 16);
		this.label3.TabIndex = 7;
		this.label3.Text = "CD/RW Fast Format";
		// 
		// groupBox1
		// 
		this.groupBox1.Location = new System.Drawing.Point(8, 104);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(584, 8);
		this.groupBox1.TabIndex = 8;
		this.groupBox1.TabStop = false;
		// 
		// checkBox2
		// 
		this.checkBox2.Location = new System.Drawing.Point(200, 144);
		this.checkBox2.Name = "checkBox2";
		this.checkBox2.Size = new System.Drawing.Size(24, 24);
		this.checkBox2.TabIndex = 9;
		// 
		// label4
		// 
		this.label4.Location = new System.Drawing.Point(8, 152);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(184, 24);
		this.label4.TabIndex = 10;
		this.label4.Text = "Delete DVR-MS File after Convert";
		// 
		// label5
		// 
		this.label5.Location = new System.Drawing.Point(8, 176);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(184, 24);
		this.label5.TabIndex = 12;
		this.label5.Text = "Automatic convert DVR-MS Files";
		// 
		// checkBox3
		// 
		this.checkBox3.Enabled = false;
		this.checkBox3.Location = new System.Drawing.Point(200, 168);
		this.checkBox3.Name = "checkBox3";
		this.checkBox3.Size = new System.Drawing.Size(24, 24);
		this.checkBox3.TabIndex = 11;
		// 
		// label6
		// 
		this.label6.Location = new System.Drawing.Point(8, 200);
		this.label6.Name = "label6";
		this.label6.TabIndex = 13;
		this.label6.Text = "DVR-MS File Path ";
		// 
		// button3
		// 
		this.button3.Location = new System.Drawing.Point(504, 192);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(32, 24);
		this.button3.TabIndex = 15;
		this.button3.Text = "...";
		// 
		// textBox2
		// 
		this.textBox2.Enabled = false;
		this.textBox2.Location = new System.Drawing.Point(200, 192);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(272, 20);
		this.textBox2.TabIndex = 14;
		this.textBox2.Text = "";
		// 
		// label7
		// 
		this.label7.Location = new System.Drawing.Point(8, 128);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(184, 24);
		this.label7.TabIndex = 17;
		this.label7.Text = "Convert  DVR-MS ";
		// 
		// checkBox4
		// 
		this.checkBox4.Location = new System.Drawing.Point(200, 120);
		this.checkBox4.Name = "checkBox4";
		this.checkBox4.Size = new System.Drawing.Size(24, 24);
		this.checkBox4.TabIndex = 16;
		// 
		// label8
		// 
		this.label8.Location = new System.Drawing.Point(8, 240);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(432, 40);
		this.label8.TabIndex = 18;
		this.label8.Text = "If you want to convert DVR-MS in MPEG Files you must instal the Cyberlink Filters" +
			". Read the \"Convert DVR-MS.txt\" in Docs folder.";
		// 
		// SetupForm
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(600, 294);
		this.Controls.Add(this.label8);
		this.Controls.Add(this.label7);
		this.Controls.Add(this.checkBox4);
		this.Controls.Add(this.button3);
		this.Controls.Add(this.textBox2);
		this.Controls.Add(this.checkBox1);
		this.Controls.Add(this.textBox1);
		this.Controls.Add(this.label6);
		this.Controls.Add(this.label5);
		this.Controls.Add(this.checkBox3);
		this.Controls.Add(this.label4);
		this.Controls.Add(this.checkBox2);
		this.Controls.Add(this.groupBox1);
		this.Controls.Add(this.label3);
		this.Controls.Add(this.button2);
		this.Controls.Add(this.label2);
		this.Controls.Add(this.button1);
		this.Controls.Add(this.label1);
		this.Controls.Add(this.comboBox1);
		this.Name = "SetupForm";
		this.Text = "SetupForm";
		this.ResumeLayout(false);

	}
	#endregion

	#region plugin vars

	public string PluginName() 
	{
	  return "My Burner";
	}

	public string Description() 
	{
	  return "A CD/DVD burner plugin for Media Portal";
	}

	public string Author() 
	{
	  return "Gucky62";
	}

	public void ShowPlugin() 
	{
	  ShowDialog();
	}

	public bool DefaultEnabled() 
	{
	  return false;
	}

	public bool CanEnable() 
	{
	  return true;
	}

	public bool HasSetup() 
	{
	  return true;
	}

	public int GetWindowId() 
	{
	  return (int)GUIWindow.Window.WINDOW_MY_BURNER;
	}

	/// <summary>
	/// If the plugin should have its own button on the home screen then it
	/// should return true to this method, otherwise if it should not be on home
	/// it should return false
	/// </summary>
	/// <param name="strButtonText">text the button should have</param>
	/// <param name="strButtonImage">image for the button, or empty for default</param>
	/// <param name="strButtonImageFocus">image for the button, or empty for default</param>
	/// <param name="strPictureImage">subpicture for the button or empty for none</param>
	/// <returns>true  : plugin needs its own button on home
	///          false : plugin does not need its own button on home</returns>
	public bool GetHome(out string strButtonText, out string strButtonImage, out string strButtonImageFocus, out string strPictureImage) 
	{
	  strButtonText = GUILocalizeStrings.Get(2100);
	  strButtonImage = "";
	  strButtonImageFocus = "";
	  strPictureImage = "";
	  return true;
	}
	#endregion

	private void GetRecorder()
	{
		//Fill The Combobox with available drives
		string name;
	
		for (int i=0; i<burnClass.NumberOfDrives; i++ )
		{
			burnClass.BurnerDrive = burnClass.RecorderDrives[i].ToString();
			name=burnClass.Vendor+" "+burnClass.ProductID+" "+burnClass.Revision;
			comboBox1.Items.Add(name); 
			comboBox1.SelectedIndex=0;
		}
	}

	private void button1_Click(object sender, System.EventArgs e)
	{
	  SaveSettings();
	  this.Visible = false;
	}

	private void button2_Click(object sender, System.EventArgs e)
	{
	  using(folderBrowserDialog1 = new FolderBrowserDialog()) 
	  {
			folderBrowserDialog1.Description = "Select the folder where recorder temp file will be stored";
			folderBrowserDialog1.ShowNewFolderButton = true;
			folderBrowserDialog1.SelectedPath = textBox1.Text;
			DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);

			if(dialogResult == DialogResult.OK) 
			{
				textBox1.Text = folderBrowserDialog1.SelectedPath;
			}
	  }		
	} 

	private void LoadSettings() 
	{
		using(AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml")) 
		{
			textBox1.Text=xmlreader.GetValueAsString("burner","temp_folder","");
			comboBox1.SelectedIndex=xmlreader.GetValueAsInt("burner","recorder",0);
			checkBox1.Checked=xmlreader.GetValueAsBool("burner","fastformat",true);
			checkBox4.Checked=xmlreader.GetValueAsBool("burner","convertdvr",true);
			checkBox2.Checked=xmlreader.GetValueAsBool("burner","deletedvrsource",false);
		}
	}

	private void SaveSettings() 
	{
	  using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml")) 
	  {
			xmlwriter.SetValue("burner","temp_folder",textBox1.Text);
			xmlwriter.SetValue("burner","recorder",comboBox1.SelectedIndex);
			xmlwriter.SetValueAsBool("burner","fastformat",checkBox1.Checked);
			xmlwriter.SetValueAsBool("burner","convertdvr",checkBox4.Checked);
			xmlwriter.SetValueAsBool("burner","deletedvrsource",checkBox2.Checked);
		}
	}

 }
}
