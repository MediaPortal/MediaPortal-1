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
	private	XPBurn.XPBurnCD burnClass; 
	private int selIndx=0; 
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
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox checkBox4;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.CheckBox checkBox5;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.CheckBox checkBox6;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Label label12;
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
		this.label7 = new System.Windows.Forms.Label();
		this.checkBox4 = new System.Windows.Forms.CheckBox();
		this.label8 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.checkBox5 = new System.Windows.Forms.CheckBox();
		this.button4 = new System.Windows.Forms.Button();
		this.label10 = new System.Windows.Forms.Label();
		this.label11 = new System.Windows.Forms.Label();
		this.checkBox6 = new System.Windows.Forms.CheckBox();
		this.label6 = new System.Windows.Forms.Label();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.button3 = new System.Windows.Forms.Button();
		this.label12 = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// comboBox1
		// 
		this.comboBox1.Enabled = false;
		this.comboBox1.Location = new System.Drawing.Point(216, 48);
		this.comboBox1.Name = "comboBox1";
		this.comboBox1.Size = new System.Drawing.Size(272, 21);
		this.comboBox1.TabIndex = 0;
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(8, 48);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(80, 24);
		this.label1.TabIndex = 1;
		this.label1.Text = "Select Drive";
		// 
		// button1
		// 
		this.button1.Location = new System.Drawing.Point(496, 296);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(88, 24);
		this.button1.TabIndex = 2;
		this.button1.Text = "OK";
		this.button1.Click += new System.EventHandler(this.button1_Click);
		// 
		// textBox1
		// 
		this.textBox1.Enabled = false;
		this.textBox1.Location = new System.Drawing.Point(216, 80);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(272, 20);
		this.textBox1.TabIndex = 3;
		this.textBox1.Text = "";
		// 
		// label2
		// 
		this.label2.Location = new System.Drawing.Point(8, 80);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(96, 24);
		this.label2.TabIndex = 4;
		this.label2.Text = "Select Temp Path";
		// 
		// button2
		// 
		this.button2.Enabled = false;
		this.button2.Location = new System.Drawing.Point(504, 80);
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
		this.checkBox1.Enabled = false;
		this.checkBox1.Location = new System.Drawing.Point(216, 112);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(16, 16);
		this.checkBox1.TabIndex = 6;
		this.checkBox1.Text = "checkBox1";
		// 
		// label3
		// 
		this.label3.Location = new System.Drawing.Point(8, 112);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(112, 16);
		this.label3.TabIndex = 7;
		this.label3.Text = "CD/RW Fast Format";
		// 
		// groupBox1
		// 
		this.groupBox1.Location = new System.Drawing.Point(8, 136);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(584, 8);
		this.groupBox1.TabIndex = 8;
		this.groupBox1.TabStop = false;
		// 
		// checkBox2
		// 
		this.checkBox2.Location = new System.Drawing.Point(216, 184);
		this.checkBox2.Name = "checkBox2";
		this.checkBox2.Size = new System.Drawing.Size(16, 16);
		this.checkBox2.TabIndex = 9;
		// 
		// label4
		// 
		this.label4.Location = new System.Drawing.Point(8, 184);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(184, 24);
		this.label4.TabIndex = 10;
		this.label4.Text = "Delete DVR-MS File after Convert";
		// 
		// label5
		// 
		this.label5.Location = new System.Drawing.Point(8, 232);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(184, 24);
		this.label5.TabIndex = 12;
		this.label5.Text = "Automatic convert DVR-MS Files";
		// 
		// checkBox3
		// 
		this.checkBox3.Location = new System.Drawing.Point(216, 232);
		this.checkBox3.Name = "checkBox3";
		this.checkBox3.Size = new System.Drawing.Size(16, 16);
		this.checkBox3.TabIndex = 11;
		// 
		// label7
		// 
		this.label7.Location = new System.Drawing.Point(8, 160);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(184, 24);
		this.label7.TabIndex = 17;
		this.label7.Text = "Convert  DVR-MS ";
		// 
		// checkBox4
		// 
		this.checkBox4.Location = new System.Drawing.Point(216, 152);
		this.checkBox4.Name = "checkBox4";
		this.checkBox4.Size = new System.Drawing.Size(24, 24);
		this.checkBox4.TabIndex = 16;
		// 
		// label8
		// 
		this.label8.Location = new System.Drawing.Point(8, 296);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(384, 40);
		this.label8.TabIndex = 18;
		this.label8.Text = "If you want to convert DVR-MS in MPEG Files you must instal the Cyberlink Filters" +
			".  Push the Help Button for more Infos";
		// 
		// label9
		// 
		this.label9.Location = new System.Drawing.Point(8, 16);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(184, 24);
		this.label9.TabIndex = 20;
		this.label9.Text = "Burn CD/DVD";
		// 
		// checkBox5
		// 
		this.checkBox5.Location = new System.Drawing.Point(216, 8);
		this.checkBox5.Name = "checkBox5";
		this.checkBox5.Size = new System.Drawing.Size(24, 24);
		this.checkBox5.TabIndex = 19;
		this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
		// 
		// button4
		// 
		this.button4.Location = new System.Drawing.Point(400, 296);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(88, 24);
		this.button4.TabIndex = 21;
		this.button4.Text = "Help DVR-MS";
		this.button4.Click += new System.EventHandler(this.button4_Click);
		// 
		// label10
		// 
		this.label10.Location = new System.Drawing.Point(240, 16);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(328, 16);
		this.label10.TabIndex = 22;
		this.label10.Text = "If you want to Burn you must  select the Burn CD/DVD Button";
		// 
		// label11
		// 
		this.label11.Location = new System.Drawing.Point(8, 208);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(208, 24);
		this.label11.TabIndex = 24;
		this.label11.Text = "Change TVDatabase entry after Convert";
		// 
		// checkBox6
		// 
		this.checkBox6.Location = new System.Drawing.Point(216, 208);
		this.checkBox6.Name = "checkBox6";
		this.checkBox6.Size = new System.Drawing.Size(16, 16);
		this.checkBox6.TabIndex = 23;
		// 
		// label6
		// 
		this.label6.Location = new System.Drawing.Point(232, 232);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(288, 16);
		this.label6.TabIndex = 25;
		this.label6.Text = "This Option converts automatic all TV-Record Files";
		// 
		// textBox2
		// 
		this.textBox2.Location = new System.Drawing.Point(216, 256);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(272, 20);
		this.textBox2.TabIndex = 26;
		this.textBox2.Text = "";
		// 
		// button3
		// 
		this.button3.Location = new System.Drawing.Point(504, 256);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(32, 24);
		this.button3.TabIndex = 27;
		this.button3.Text = "...";
		this.button3.Click += new System.EventHandler(this.button3_Click);
		// 
		// label12
		// 
		this.label12.Location = new System.Drawing.Point(8, 264);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(184, 24);
		this.label12.TabIndex = 28;
		this.label12.Text = "TV Recording Path";
		// 
		// SetupForm
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(600, 342);
		this.Controls.Add(this.label12);
		this.Controls.Add(this.textBox2);
		this.Controls.Add(this.checkBox1);
		this.Controls.Add(this.textBox1);
		this.Controls.Add(this.button3);
		this.Controls.Add(this.label6);
		this.Controls.Add(this.label11);
		this.Controls.Add(this.checkBox6);
		this.Controls.Add(this.label10);
		this.Controls.Add(this.button4);
		this.Controls.Add(this.label9);
		this.Controls.Add(this.checkBox5);
		this.Controls.Add(this.label8);
		this.Controls.Add(this.label7);
		this.Controls.Add(this.checkBox4);
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
		this.Load += new System.EventHandler(this.SetupForm_Load);
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
	  return 760;
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
			selIndx=xmlreader.GetValueAsInt("burner","recorder",0);
			checkBox1.Checked=xmlreader.GetValueAsBool("burner","fastformat",true);
			checkBox4.Checked=xmlreader.GetValueAsBool("burner","convertdvr",true);
			checkBox2.Checked=xmlreader.GetValueAsBool("burner","deletedvrsource",false);
			checkBox3.Checked=xmlreader.GetValueAsBool("burner","convertautomatic",false);
			checkBox6.Checked=xmlreader.GetValueAsBool("burner","changetvdatabase",false);
			textBox2.Text=xmlreader.GetValueAsString("burner","dvrms_folder","");
		}
	}

	private void SaveSettings() 
	{
	  using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml")) 
	  {
			xmlwriter.SetValue("burner","temp_folder",textBox1.Text);
			if(checkBox5.Checked==true) 
			{
				xmlwriter.SetValue("burner","recorder",comboBox1.SelectedIndex);
			}
			xmlwriter.SetValueAsBool("burner","burn",checkBox5.Checked);
			xmlwriter.SetValueAsBool("burner","fastformat",checkBox1.Checked);
			xmlwriter.SetValueAsBool("burner","convertdvr",checkBox4.Checked);
			xmlwriter.SetValueAsBool("burner","deletedvrsource",checkBox2.Checked);
			xmlwriter.SetValueAsBool("burner","convertautomatic",checkBox3.Checked);
			xmlwriter.SetValueAsBool("burner","changetvdatabase",checkBox6.Checked);
			xmlwriter.SetValue("burner","dvrms_folder",textBox2.Text);
		}
	}

		private void button4_Click(object sender, System.EventArgs e)
		{
			string s="1. Load from this site ftp://ftp.lifeview.com.tw/TV/LR301/ the file: LR301-Ver.1.02.0.600.zip\n";
			s=s+"2. Do not install this package, we need only two files from the Zip archive.\n";
			s=s+"    In the archive is a Data1.cab file. You can open it with the unzip program. Copy the\n";
			s=s+"    following files in the MP Main Folder: CLDump.ax    MpgMux.ax\n";
			s=s+"3. Start the Batch Program RecCodecs.cmd. You found it in MP Main folder.\n\n";
			s=s+"    That´s all.\n";
			MessageBox.Show( s );
		}

		private void checkBox5_CheckedChanged(object sender, System.EventArgs e)
		{	
			checkBox5.Checked=true;
			comboBox1.Enabled=true;
			textBox1.Enabled=true;
			button2.Enabled=true;
			checkBox1.Enabled=true;
			burnClass= new XPBurn.XPBurnCD();
			GetRecorder();
			comboBox1.SelectedIndex=selIndx;
		}

		private void SetupForm_Load(object sender, System.EventArgs e)
		{
		
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog1 = new FolderBrowserDialog()) 
			{
				folderBrowserDialog1.Description = "Select the folder where TV record files will be stored";
				folderBrowserDialog1.ShowNewFolderButton = true;
				folderBrowserDialog1.SelectedPath = textBox2.Text;
				DialogResult dialogResult = folderBrowserDialog1.ShowDialog(this);

				if(dialogResult == DialogResult.OK) 
				{
					textBox2.Text = folderBrowserDialog1.SelectedPath;
				}
			}		
		}
 }
}
