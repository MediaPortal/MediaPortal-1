using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using MediaPortal.GUI.Library;

namespace GUIExplorer
{
	/// <summary>
	/// Summary description for SetupForm.
	/// </summary>
  public class SetupForm : System.Windows.Forms.Form, ISetupForm 
  {
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.CheckBox checkBox2;
		private System.Windows.Forms.CheckBox checkBox3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.CheckBox checkBox4;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.CheckBox checkBox5;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.CheckBox checkBox6;
		private System.Windows.Forms.CheckBox checkBox7;
		private System.Windows.Forms.CheckBox checkBox1;
 
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
		this.label1 = new System.Windows.Forms.Label();
		this.checkBox1 = new System.Windows.Forms.CheckBox();
		this.label2 = new System.Windows.Forms.Label();
		this.checkBox2 = new System.Windows.Forms.CheckBox();
		this.checkBox3 = new System.Windows.Forms.CheckBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.checkBox4 = new System.Windows.Forms.CheckBox();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.button1 = new System.Windows.Forms.Button();
		this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
		this.button2 = new System.Windows.Forms.Button();
		this.label6 = new System.Windows.Forms.Label();
		this.label7 = new System.Windows.Forms.Label();
		this.textBox2 = new System.Windows.Forms.TextBox();
		this.label8 = new System.Windows.Forms.Label();
		this.checkBox5 = new System.Windows.Forms.CheckBox();
		this.label9 = new System.Windows.Forms.Label();
		this.label10 = new System.Windows.Forms.Label();
		this.checkBox6 = new System.Windows.Forms.CheckBox();
		this.checkBox7 = new System.Windows.Forms.CheckBox();
		this.SuspendLayout();
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(16, 16);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(168, 16);
		this.label1.TabIndex = 0;
		this.label1.Text = "Show only Shares (Destination)";
		// 
		// checkBox1
		// 
		this.checkBox1.Location = new System.Drawing.Point(264, 8);
		this.checkBox1.Name = "checkBox1";
		this.checkBox1.Size = new System.Drawing.Size(24, 24);
		this.checkBox1.TabIndex = 1;
		// 
		// label2
		// 
		this.label2.Location = new System.Drawing.Point(16, 40);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(152, 23);
		this.label2.TabIndex = 2;
		this.label2.Text = "Enable Delete Funktion";
		// 
		// checkBox2
		// 
		this.checkBox2.Location = new System.Drawing.Point(264, 32);
		this.checkBox2.Name = "checkBox2";
		this.checkBox2.Size = new System.Drawing.Size(24, 24);
		this.checkBox2.TabIndex = 3;
		// 
		// checkBox3
		// 
		this.checkBox3.Location = new System.Drawing.Point(264, 56);
		this.checkBox3.Name = "checkBox3";
		this.checkBox3.Size = new System.Drawing.Size(24, 24);
		this.checkBox3.TabIndex = 4;
		// 
		// label3
		// 
		this.label3.Location = new System.Drawing.Point(16, 64);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(160, 24);
		this.label3.TabIndex = 5;
		this.label3.Text = "Delete Files immediately ";
		// 
		// label4
		// 
		this.label4.Location = new System.Drawing.Point(16, 88);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(256, 23);
		this.label4.TabIndex = 6;
		this.label4.Text = "Delete moves files to Temp Folder (like Trashcan)";
		// 
		// checkBox4
		// 
		this.checkBox4.Location = new System.Drawing.Point(264, 80);
		this.checkBox4.Name = "checkBox4";
		this.checkBox4.Size = new System.Drawing.Size(24, 24);
		this.checkBox4.TabIndex = 7;
		// 
		// textBox1
		// 
		this.textBox1.Location = new System.Drawing.Point(264, 112);
		this.textBox1.Name = "textBox1";
		this.textBox1.Size = new System.Drawing.Size(216, 20);
		this.textBox1.TabIndex = 8;
		this.textBox1.Text = "";
		// 
		// label5
		// 
		this.label5.Location = new System.Drawing.Point(16, 112);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(176, 23);
		this.label5.TabIndex = 9;
		this.label5.Text = "Temp Folder";
		// 
		// button1
		// 
		this.button1.Location = new System.Drawing.Point(496, 112);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(32, 24);
		this.button1.TabIndex = 10;
		this.button1.Text = "...";
		this.button1.Click += new System.EventHandler(this.button1_Click);
		// 
		// button2
		// 
		this.button2.Location = new System.Drawing.Point(480, 264);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(56, 24);
		this.button2.TabIndex = 11;
		this.button2.Text = "OK";
		this.button2.Click += new System.EventHandler(this.button2_Click);
		// 
		// label6
		// 
		this.label6.Location = new System.Drawing.Point(16, 216);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(232, 23);
		this.label6.TabIndex = 12;
		this.label6.Text = "Additional File Extensions";
		// 
		// label7
		// 
		this.label7.Location = new System.Drawing.Point(16, 232);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(216, 23);
		this.label7.TabIndex = 13;
		this.label7.Text = " (Use this form:   .vob,.bin   etc.)   ";
		// 
		// textBox2
		// 
		this.textBox2.Location = new System.Drawing.Point(264, 224);
		this.textBox2.Name = "textBox2";
		this.textBox2.Size = new System.Drawing.Size(216, 20);
		this.textBox2.TabIndex = 14;
		this.textBox2.Text = "";
		// 
		// label8
		// 
		this.label8.Location = new System.Drawing.Point(16, 136);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(224, 23);
		this.label8.TabIndex = 15;
		this.label8.Text = "Use Video Extensions";
		// 
		// checkBox5
		// 
		this.checkBox5.Checked = true;
		this.checkBox5.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox5.Location = new System.Drawing.Point(264, 136);
		this.checkBox5.Name = "checkBox5";
		this.checkBox5.Size = new System.Drawing.Size(24, 24);
		this.checkBox5.TabIndex = 16;
		// 
		// label9
		// 
		this.label9.Location = new System.Drawing.Point(16, 160);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(224, 23);
		this.label9.TabIndex = 17;
		this.label9.Text = "Use Music Extensions";
		// 
		// label10
		// 
		this.label10.Location = new System.Drawing.Point(16, 184);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(224, 23);
		this.label10.TabIndex = 18;
		this.label10.Text = "Use Picture Extensions";
		// 
		// checkBox6
		// 
		this.checkBox6.Checked = true;
		this.checkBox6.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox6.Location = new System.Drawing.Point(264, 160);
		this.checkBox6.Name = "checkBox6";
		this.checkBox6.Size = new System.Drawing.Size(24, 24);
		this.checkBox6.TabIndex = 19;
		// 
		// checkBox7
		// 
		this.checkBox7.Checked = true;
		this.checkBox7.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBox7.Location = new System.Drawing.Point(264, 184);
		this.checkBox7.Name = "checkBox7";
		this.checkBox7.Size = new System.Drawing.Size(24, 24);
		this.checkBox7.TabIndex = 20;
		// 
		// SetupForm
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(552, 310);
		this.Controls.Add(this.checkBox7);
		this.Controls.Add(this.checkBox6);
		this.Controls.Add(this.label10);
		this.Controls.Add(this.label9);
		this.Controls.Add(this.checkBox5);
		this.Controls.Add(this.label8);
		this.Controls.Add(this.textBox2);
		this.Controls.Add(this.label7);
		this.Controls.Add(this.label6);
		this.Controls.Add(this.button2);
		this.Controls.Add(this.button1);
		this.Controls.Add(this.label5);
		this.Controls.Add(this.textBox1);
		this.Controls.Add(this.checkBox4);
		this.Controls.Add(this.label4);
		this.Controls.Add(this.label3);
		this.Controls.Add(this.checkBox3);
		this.Controls.Add(this.checkBox2);
		this.Controls.Add(this.label2);
		this.Controls.Add(this.checkBox1);
		this.Controls.Add(this.label1);
		this.Name = "SetupForm";
		this.Text = "SetupForm";
		this.ResumeLayout(false);

	}
	#endregion

	#region plugin vars

	public string PluginName() 
	{
	  return "My Explorer";
	}

	public string Description() 
	{
	  return "A File Explorer plugin for Media Portal";
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
	  return 770;
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
	  strButtonText = GUILocalizeStrings.Get(2200);
	  strButtonImage = "";
	  strButtonImageFocus = "";
	  strPictureImage = "";
	  return true;
	}
	#endregion

		private void button1_Click(object sender, System.EventArgs e)
		{
			using(folderBrowserDialog1 = new FolderBrowserDialog()) 
			{
				folderBrowserDialog1.Description = "Select the folder where delete file will be stored";
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
				textBox1.Text=xmlreader.GetValueAsString("myexplorer","temp_folder","");
				textBox2.Text=xmlreader.GetValueAsString("myexplorer","extensions","");
				checkBox1.Checked=xmlreader.GetValueAsBool("myexplorer","show_only_shares",false);
				checkBox2.Checked=xmlreader.GetValueAsBool("myexplorer","enable_delete",false);
				checkBox3.Checked=xmlreader.GetValueAsBool("myexplorer","delete_immediately",false);
				checkBox4.Checked=xmlreader.GetValueAsBool("myexplorer","delete_temp",false);
				checkBox5.Checked=xmlreader.GetValueAsBool("myexplorer","use_video",true);
				checkBox6.Checked=xmlreader.GetValueAsBool("myexplorer","use_music",true);
				checkBox7.Checked=xmlreader.GetValueAsBool("myexplorer","use_pic",true);
			}
		}

		private void SaveSettings() 
		{
			using(AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml")) 
			{
				xmlwriter.SetValue("myexplorer","temp_folder",textBox1.Text);
				xmlwriter.SetValue("myexplorer","extensions",textBox2.Text);
				xmlwriter.SetValueAsBool("myexplorer","show_only_shares",checkBox1.Checked);
				xmlwriter.SetValueAsBool("myexplorer","enable_delete",checkBox2.Checked);
				xmlwriter.SetValueAsBool("myexplorer","use_video",checkBox5.Checked);
				xmlwriter.SetValueAsBool("myexplorer","use_music",checkBox6.Checked);
				xmlwriter.SetValueAsBool("myexplorer","use_pic",checkBox7.Checked);
				if (checkBox2.Checked==true)
				{
					xmlwriter.SetValueAsBool("myexplorer","delete_immediately",checkBox3.Checked);
					if(checkBox3.Checked==true)
					{
						xmlwriter.SetValueAsBool("myexplorer","delete_temp",false);
					} 
					else 
					{
						xmlwriter.SetValueAsBool("myexplorer","delete_temp",checkBox4.Checked);
					}
				} 
				else 
				{
					xmlwriter.SetValueAsBool("myexplorer","delete_immediately",false);
					xmlwriter.SetValueAsBool("myexplorer","delete_temp",false);
				}
			}
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			if (checkBox4.Checked==true) 
			{
				if (textBox1.Text=="") 
				{
					MessageBox.Show( "Please select a temp path!" );
				} 
				else 
				{
					SaveSettings();
					this.Visible = false;
				}
			} 
			else 
			{
				if (checkBox2.Checked==true && checkBox3.Checked==false && checkBox4.Checked==false) 
				{
					MessageBox.Show( "Please a Option: \n(Delete Files immediately)\n or \n(Delete moves files to Temp Folder)" );
				} 
				else 
				{
					SaveSettings();
					this.Visible = false;
				}
			}
		}

  }
}
