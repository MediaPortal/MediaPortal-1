using System;
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Runtime;
using System.Runtime.InteropServices;

using MediaPortal;
using MediaPortal.UserInterface.Controls;
using MediaPortal.GUI.Library;

namespace MediaPortal.Configuration
{
  /// <summary>
	/// Summary description for Settings.
	/// </summary>
	public class SettingsForm : System.Windows.Forms.Form
	{
    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr window, int message, int wparam, int lparam);
    
    private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private MPBeveledLine beveledLine1;
		private System.Windows.Forms.TreeView sectionTree;
		private System.Windows.Forms.Panel holderPanel;
		private MPGradientLabel headerLabel;

		//
		// Hashtable where we store each added tree node/section for faster access
		//
		public static Hashtable SettingSections
		{
			get { return settingSections; }
		}
		static Hashtable settingSections = new Hashtable();
		private System.Windows.Forms.Button applyButton;
    //private System.ComponentModel.IContainer components;

		public SettingsForm()
		{
      Log.Write("SettingsForm constructor");
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Set caption
			//
			this.Text = "Configuration - " + Application.ProductVersion;

			//
			// Build options tree
			//
      
      Log.Write("add general section");
			AddSection(new Sections.General());

      Log.Write("add keys section");
      AddSection(new Sections.Keys());

      Log.Write("add skins section");
      AddSection(new Sections.Skin());


      Log.Write("add DVD section");
      SectionSettings dvd = new Sections.DVD();
			AddSection(dvd);

      Log.Write("  add DVD codec section");
      AddChildSection(dvd, new Sections.DVDCodec());
      
      Log.Write("  add DVD player section");
      AddChildSection(dvd, new Sections.DVDPlayer());

      Log.Write("  add DVD postprocessing section");
      AddChildSection(dvd, new Sections.DVDPostProcessing());
			
      Log.Write("add movie section");
      SectionSettings movie = new Sections.Movies();
			AddSection(movie);

      Log.Write("  add movie extensions section");
      AddChildSection(movie, new Sections.MovieExtensions());
      Log.Write("  add movie shares section");
      AddChildSection(movie, new Sections.MovieShares());
      Log.Write("  add movie player section");
      AddChildSection(movie, new Sections.MoviePlayer());
      Log.Write("  add movie postprocessing section");
      AddChildSection(movie, new Sections.MoviePostProcessing());

      Log.Write("add music section");
      SectionSettings music = new Sections.Music();
			AddSection(music);
      Log.Write("  add music extension section");
      AddChildSection(music, new Sections.MusicExtensions());
      Log.Write("  add music shares section");
      AddChildSection(music, new Sections.MusicShares());
      Log.Write("  add music database section");
      AddChildSection(music, new Sections.MusicDatabase());

      Log.Write("add pictures section");
      SectionSettings picture = new Sections.Pictures();
			AddSection(picture);
      Log.Write("  add pictures extensions section");
      AddChildSection(picture, new Sections.PictureExtensions());
      Log.Write("  add pictures shares section");
      AddChildSection(picture, new Sections.PictureShares());

      Log.Write("add radio section");
      SectionSettings radio = new Sections.Radio();
			AddSection(radio);
      Log.Write("  add radio stations section");
      AddChildSection(radio, new Sections.RadioStations());

      Log.Write("add television section");
      SectionSettings television = new Sections.Television();
			AddSection(television);
      Log.Write("  add tv capture cards section");
      AddChildSection(television, new Sections.TVCaptureCards());
      Log.Write("  add tv channels section");
      AddChildSection(television, new Sections.TVChannels());
      Log.Write("  add tv program guide section");
      AddChildSection(television, new Sections.TVProgramGuide());
      Log.Write("  add tv recording section");
      AddChildSection(television, new Sections.TVRecording());
      Log.Write("  add tv postprocessing section");
      AddChildSection(television, new Sections.TVPostProcessing());

      Log.Write("add USBUIRT section");
      AddSection(new Sections.USBUIRT());
			Log.Write("add WINLIRC section");//sd00//
			AddSection(new Sections.WINLIRC());//sd00//
      Log.Write("add weather section");
      AddSection(new Sections.Weather());
      Log.Write("add plugins section");
      AddSection(new Sections.Plugins());
      Log.Write("add project section");
      AddSection(new Sections.Project());

			//
			// Select first item in the section tree
			//
			sectionTree.SelectedNode = sectionTree.Nodes[0];

      Log.Write("bring to front");
      // make sure window is in front of mediaportal
      this.BringToFront();
      Log.Write("settingsform constructor done");
    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="section"></param>
		public void AddSection(SectionSettings section)
		{
			AddChildSection(null, section);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parentSection"></param>
		/// <param name="section"></param>
		public void AddChildSection(SectionSettings parentSection, SectionSettings section)
		{
			//
			// Make sure this section doesn't already exist
			//
			if(settingSections.ContainsKey(section.Text))
				return;

			//
			// Add section to tree
			//
			SectionTreeNode treeNode = new SectionTreeNode(section);

			if(parentSection == null)
			{
				//
				// Add to the root
				//
				sectionTree.Nodes.Add(treeNode);
			}
			else
			{
				//
				// Add to the parent node
				//
				SectionTreeNode parentTreeNode = (SectionTreeNode)settingSections[parentSection.Text];
				parentTreeNode.Nodes.Add(treeNode);
			}

			settingSections.Add(section.Text, treeNode);

			//treeNode.EnsureVisible();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				/*if(components != null)
				{
					components.Dispose();
				}*/
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
      System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SettingsForm));
      this.sectionTree = new System.Windows.Forms.TreeView();
      this.cancelButton = new System.Windows.Forms.Button();
      this.okButton = new System.Windows.Forms.Button();
      this.headerLabel = new MediaPortal.UserInterface.Controls.MPGradientLabel();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.beveledLine1 = new MediaPortal.UserInterface.Controls.MPBeveledLine();
      this.applyButton = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // sectionTree
      // 
      this.sectionTree.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left)));
      this.sectionTree.FullRowSelect = true;
      this.sectionTree.HideSelection = false;
      this.sectionTree.HotTracking = true;
      this.sectionTree.ImageIndex = -1;
      this.sectionTree.Indent = 19;
      this.sectionTree.ItemHeight = 16;
      this.sectionTree.Location = new System.Drawing.Point(8, 16);
      this.sectionTree.Name = "sectionTree";
      this.sectionTree.SelectedImageIndex = -1;
      this.sectionTree.Size = new System.Drawing.Size(184, 446);
      this.sectionTree.TabIndex = 0;
      this.sectionTree.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.sectionTree_AfterSelect);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(609, 485);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.TabIndex = 2;
      this.cancelButton.Text = "Cancel";
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(530, 485);
      this.okButton.Name = "okButton";
      this.okButton.TabIndex = 3;
      this.okButton.Text = "OK";
      this.okButton.Click += new System.EventHandler(this.okButton_Click);
      // 
      // headerLabel
      // 
      this.headerLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.headerLabel.Caption = "";
      this.headerLabel.FirstColor = System.Drawing.SystemColors.InactiveCaption;
      this.headerLabel.Font = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      this.headerLabel.LastColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.Location = new System.Drawing.Point(200, 16);
      this.headerLabel.Name = "headerLabel";
      this.headerLabel.PaddingLeft = 2;
      this.headerLabel.Size = new System.Drawing.Size(484, 24);
      this.headerLabel.TabIndex = 0;
      this.headerLabel.TextColor = System.Drawing.Color.WhiteSmoke;
      this.headerLabel.TextFont = new System.Drawing.Font("Verdana", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
      // 
      // holderPanel
      // 
      this.holderPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.holderPanel.BackColor = System.Drawing.SystemColors.Control;
      this.holderPanel.Location = new System.Drawing.Point(200, 40);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(484, 422);
      this.holderPanel.TabIndex = 4;
      // 
      // beveledLine1
      // 
      this.beveledLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
      this.beveledLine1.Location = new System.Drawing.Point(8, 475);
      this.beveledLine1.Name = "beveledLine1";
      this.beveledLine1.Size = new System.Drawing.Size(676, 2);
      this.beveledLine1.TabIndex = 5;
      // 
      // applyButton
      // 
      this.applyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.applyButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.applyButton.Location = new System.Drawing.Point(450, 485);
      this.applyButton.Name = "applyButton";
      this.applyButton.TabIndex = 6;
      this.applyButton.Text = "Apply";
      this.applyButton.Visible = false;
      this.applyButton.Click += new System.EventHandler(this.applyButton_Click);
      // 
      // SettingsForm
      // 
      this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
      this.ClientSize = new System.Drawing.Size(692, 516);
      this.Controls.Add(this.applyButton);
      this.Controls.Add(this.beveledLine1);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.headerLabel);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.sectionTree);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MinimumSize = new System.Drawing.Size(700, 550);
      this.Name = "SettingsForm";
      this.Text = "Settings";
      this.Load += new System.EventHandler(this.SettingsForm_Load);
      this.ResumeLayout(false);

    }
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void sectionTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			SectionTreeNode treeNode = e.Node as SectionTreeNode;

			if(treeNode != null)
			{
				headerLabel.Caption = treeNode.Section.Text;
				ActivateSection(treeNode.Section);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="section"></param>
		private void ActivateSection(SectionSettings section)
		{
			section.Dock = DockStyle.Fill;

			section.OnSectionActivated();

			holderPanel.Controls.Clear();
			holderPanel.Controls.Add(section);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SettingsForm_Load(object sender, System.EventArgs e)
		{
      Log.Write("Load settings");
			foreach(TreeNode treeNode in sectionTree.Nodes)
			{
				//
				// Load settings for all sections
				//
        
        Log.Write("  Load settings:{0}",treeNode.Text);
				LoadSectionSettings(treeNode);
			}
      Log.Write("Load settings done");
    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentNode"></param>
		private void LoadSectionSettings(TreeNode currentNode)
    {
      Log.Write("LoadSectionSettings()");
      if(currentNode != null)
			{
				//
				// Load settings for current node
				//
				SectionTreeNode treeNode = currentNode as SectionTreeNode;

				if(treeNode != null)
				{
					treeNode.Section.LoadSettings();
				}

				//
				// Load settings for all child nodes
				//
				foreach(TreeNode childNode in treeNode.Nodes)
				{
          Log.Write("  Load settings:{0}",childNode.Text);
          LoadSectionSettings(childNode);
				}
			}
      Log.Write("LoadSectionSettings() done");
    }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentNode"></param>
		private void SaveSectionSettings(TreeNode currentNode)
		{
      Log.Write("SaveSectionSettings()");
      if(currentNode != null)
			{
				//
				// Save settings for current node
				//
				SectionTreeNode treeNode = currentNode as SectionTreeNode;

				if(treeNode != null)
				{
					treeNode.Section.SaveSettings();
				}

				//
				// Load settings for all child nodes
				//
				foreach(TreeNode childNode in treeNode.Nodes)
        {
          Log.Write("SaveSectionSettings:{0}",childNode.Text);		
					SaveSectionSettings(childNode);
				}
      }
      Log.Write("SaveSectionSettings done()");		
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void okButton_Click(object sender, System.EventArgs e)
		{
			applyButton_Click(sender, e);

			this.Close();
		}

		private void applyButton_Click(object sender, System.EventArgs e)
		{
			foreach(TreeNode treeNode in sectionTree.Nodes)
			{
				//
				// Save settings for all sections
				//
				SaveSectionSettings(treeNode);
			}		

      //
      // Check if MediaPortal is running, if so inform user that it needs to be restarted
      // for the changes to take effect.
      //
      string processName = "MediaPortal";

      foreach(Process process in Process.GetProcesses())
      {
        if(process.ProcessName.Equals(processName))
        {
          DialogResult dialogResult = MessageBox.Show("For the changes to take effect you need to restart MediaPortal, restart now?", "MediaPortal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

          if(dialogResult == DialogResult.Yes)
          {

            try
            {              
              //
              // Kill the MediaPortal process by finding window and sending ALT+F4 to it.
              //
              process.CloseMainWindow();
  
              //
              // Wait for the process to die, we wait for a maximum of 10 seconds
              //
              if(process.WaitForExit(10000) == true)
              {
                //
                // Start the MediaPortal process
                // 
                Process.Start(processName + ".exe");
              }
            }
            catch
            {
              // Ignore
            }

            break;
          }
        }
      }
		}
	}
}
