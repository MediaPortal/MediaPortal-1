using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for WizardForm.
	/// </summary>
	public class WizardForm : System.Windows.Forms.Form
	{
		internal class SectionHolder
		{
			public SectionSettings Section;
			public string Topic;
			public string Information;
			public string Expression;

			public SectionHolder(SectionSettings section, string topic, string information, string expression)
			{
				this.Section = section;
				this.Topic = topic;
				this.Information = information;
				this.Expression = expression;
			}
		}
	
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		//
		// Private members
		//
		ArrayList wizardPages = new ArrayList();
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button nextButton;
		private System.Windows.Forms.Button backButton;
		private System.Windows.Forms.Panel topPanel;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel holderPanel;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Label topicLabel;
		private System.Windows.Forms.Label infoLabel;
		private System.Windows.Forms.PictureBox pictureBox1;
		int visiblePageIndex = -1;

		public void AddSection(SectionSettings settings, string topic, string information)
		{
			AddSection(settings, topic, information, String.Empty);
		}

		public void AddSection(SectionSettings settings, string topic, string information, string expression)
		{
			wizardPages.Add(new SectionHolder(settings, topic, information, expression));
		}

		public WizardForm(string sectionConfiguration)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// Set caption
			//
			this.Text = "MediaPortal Settings Wizard - " + Application.ProductVersion;

			//
			// Check if we got a sections file to read from, or if we should specify
			// the default sections
			//
			if(sectionConfiguration != String.Empty && System.IO.File.Exists(sectionConfiguration))
			{
				LoadSections(sectionConfiguration);
			}
			else
			{
				//
				// Build default wizard pages
				//
				AddSection(new Sections.Wizard_Welcome(), "", "");
				AddSection(new Sections.General(), "General", "General information...");
				AddSection(new Sections.Skin(), "Skin", "Skin settings...");
				AddSection(new Sections.Wizard_SelectPlugins(), "Plugins", "Please check which plugins you would like to be loaded when MediaPortal starts.");
				AddSection(new Sections.MusicShares(), "Music Shares", "Music share information, yada yada. By checking one of the shares you will make that share the default share, this share will be automatically shown when you enter My Music.", "Plugin Selection.Plugin.MyMusic");
				AddSection(new Sections.MovieShares(), "Movie Shares", "Movie share information, yada yada, By checking one of the shares you will make that share the default share, this share will be automatically shown when you enter My Movies.", "Plugin Selection.Plugin.MyMovies");
				AddSection(new Sections.PictureShares(), "Picture Shares", "Picture share information, yada yada. By checking one of the shares you will make that share the default share, this share will be automatically shown when you enter My Pictures.", "Plugin Selection.Plugin.MyPictures");
				AddSection(new Sections.Wizard_Finished(), "Congratulations", "You have now finished the setup wizard.");
			}
		}

		/// <summary>
		/// Loads, parses and creates the defined sections in the section xml.
		/// </summary>
		/// <param name="xmlFile"></param>
		private void LoadSections(string xmlFile)
		{
			XmlDocument document = new XmlDocument();
	
			try
			{
				//
				// Load the xml document
				//
				document.Load(xmlFile);

				XmlElement rootElement = document.DocumentElement;

				//
				// Make sure we're loading a wizard file
				//
				if(rootElement != null && rootElement.Name.Equals("wizard"))
				{
					//
					// Fetch wizard settings
					//
					XmlNode	wizardTopicNode = rootElement.SelectSingleNode("/wizard/caption");
					if(wizardTopicNode != null)
					{
						this.Text = wizardTopicNode.InnerText;
					}

					//
					// Fetch sections
					//
					XmlNodeList nodeList = rootElement.SelectNodes("/wizard/sections/section");

					foreach(XmlNode node in nodeList)
					{
						//
						// Fetch section information
						//
						XmlNode nameNode = node.SelectSingleNode("name");
						XmlNode topicNode = node.SelectSingleNode("topic");
						XmlNode infoNode = node.SelectSingleNode("information");
						XmlNode dependencyNode = node.SelectSingleNode("dependency");

						if(nameNode != null && nameNode.InnerText.Length > 0)
						{
							//
							// Allocate new wizard page
							//
							SectionSettings section = CreateSection(nameNode.InnerText);

							if(section != null)
							{
								if(dependencyNode == null)
								{
									AddSection(section, topicNode != null ? topicNode.InnerText : String.Empty, infoNode != null ? infoNode.InnerText : String.Empty);
								}
								else
								{
									AddSection(section, topicNode != null ? topicNode.InnerText : String.Empty, infoNode != null ? infoNode.InnerText : String.Empty, dependencyNode.InnerText);
								}
							}
						}
					}
				}
			}
			catch(Exception e)
			{
				System.Diagnostics.Debug.WriteLine(e.Message);
			}
		}

		/// <summary>
		/// Creates a section class from the specified name
		/// </summary>
		/// <param name="sectionName"></param>
		/// <returns></returns>
		private SectionSettings CreateSection(string sectionName)
		{
			Type sectionType = Type.GetType("MediaPortal.Configuration.Sections." + sectionName);

			if(sectionType != null)
			{
				//
				// Create the instance of the section settings class, pass the section name as argument
				// to the constructor. We do this to be able to use the same name on <name> as in the <dependency> tag.
				//
				SectionSettings section = (SectionSettings)Activator.CreateInstance(sectionType, new object[] { sectionName } );
				return section;
			}

			//
			// Section was not found
			//
			return null;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(WizardForm));
			this.cancelButton = new System.Windows.Forms.Button();
			this.nextButton = new System.Windows.Forms.Button();
			this.backButton = new System.Windows.Forms.Button();
			this.topPanel = new System.Windows.Forms.Panel();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.infoLabel = new System.Windows.Forms.Label();
			this.topicLabel = new System.Windows.Forms.Label();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel2 = new System.Windows.Forms.Panel();
			this.holderPanel = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.topPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.cancelButton.Location = new System.Drawing.Point(526, 414);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.TabIndex = 1;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// nextButton
			// 
			this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.nextButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.nextButton.Location = new System.Drawing.Point(441, 414);
			this.nextButton.Name = "nextButton";
			this.nextButton.TabIndex = 4;
			this.nextButton.Text = "Next >";
			this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
			// 
			// backButton
			// 
			this.backButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.backButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.backButton.Location = new System.Drawing.Point(363, 414);
			this.backButton.Name = "backButton";
			this.backButton.TabIndex = 5;
			this.backButton.Text = "< Back";
			this.backButton.Click += new System.EventHandler(this.backButton_Click);
			// 
			// topPanel
			// 
			this.topPanel.BackColor = System.Drawing.SystemColors.Window;
			this.topPanel.Controls.Add(this.pictureBox1);
			this.topPanel.Controls.Add(this.infoLabel);
			this.topPanel.Controls.Add(this.topicLabel);
			this.topPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.topPanel.Location = new System.Drawing.Point(0, 0);
			this.topPanel.Name = "topPanel";
			this.topPanel.Size = new System.Drawing.Size(610, 72);
			this.topPanel.TabIndex = 6;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(528, 14);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(70, 48);
			this.pictureBox1.TabIndex = 2;
			this.pictureBox1.TabStop = false;
			// 
			// infoLabel
			// 
			this.infoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.infoLabel.Location = new System.Drawing.Point(8, 26);
			this.infoLabel.Name = "infoLabel";
			this.infoLabel.Size = new System.Drawing.Size(512, 40);
			this.infoLabel.TabIndex = 1;
			this.infoLabel.Text = "Information information information information information";
			// 
			// topicLabel
			// 
			this.topicLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.topicLabel.Location = new System.Drawing.Point(8, 8);
			this.topicLabel.Name = "topicLabel";
			this.topicLabel.Size = new System.Drawing.Size(272, 23);
			this.topicLabel.TabIndex = 0;
			this.topicLabel.Text = "Topic";
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 72);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(610, 1);
			this.panel1.TabIndex = 7;
			// 
			// panel2
			// 
			this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel2.Location = new System.Drawing.Point(0, 73);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(610, 1);
			this.panel2.TabIndex = 9;
			// 
			// holderPanel
			// 
			this.holderPanel.Location = new System.Drawing.Point(24, 74);
			this.holderPanel.Name = "holderPanel";
			this.holderPanel.Size = new System.Drawing.Size(560, 326);
			this.holderPanel.TabIndex = 10;
			// 
			// panel3
			// 
			this.panel3.BackColor = System.Drawing.SystemColors.ControlDark;
			this.panel3.Location = new System.Drawing.Point(0, 400);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(616, 1);
			this.panel3.TabIndex = 11;
			// 
			// panel4
			// 
			this.panel4.BackColor = System.Drawing.SystemColors.ControlLightLight;
			this.panel4.Location = new System.Drawing.Point(0, 401);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(616, 1);
			this.panel4.TabIndex = 12;
			// 
			// WizardForm
			// 
			this.AcceptButton = this.nextButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(610, 448);
			this.Controls.Add(this.panel4);
			this.Controls.Add(this.panel3);
			this.Controls.Add(this.holderPanel);
			this.Controls.Add(this.panel2);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.topPanel);
			this.Controls.Add(this.backButton);
			this.Controls.Add(this.nextButton);
			this.Controls.Add(this.cancelButton);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "WizardForm";
			this.Text = "WizardForm";
			this.Load += new System.EventHandler(this.WizardForm_Load);
			this.topPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WizardForm_Load(object sender, System.EventArgs e)
		{
			//
			// Load settings
			//
			LoadSectionSettings();
					
			//
			// Load first page
			//
			ShowNextPage();
		}

		/// <summary>
		/// 
		/// </summary>
		private void ShowNextPage()
		{
			//
			// Make sure we have something to show
			//
			while(true)
			{
				if(visiblePageIndex + 1 < wizardPages.Count)
				{
					//
					// Move to next index, the index  that will be shown
					//
					visiblePageIndex++;

					//
					// Activate section
					//
					SectionHolder holder = wizardPages[visiblePageIndex] as SectionHolder;

					if(holder != null)
					{					
						//
						// Evaluate if this section should be shown at all
						//
						if(EvaluateExpression(holder.Expression) == true)
						{
							ActivateSection(holder.Section);

							//
							// Set topic and information
							//
							SetTopic(holder.Topic);
							SetInformation(holder.Information);

							break;
						}
						else
						{
							//
							// Fetch next section
							//
						}
					}
				}
				else
				{
					//
					// No more sections to show
					//
					break;
				}
			}

			//
			// Update control status
			//
			UpdateControlStatus();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		private bool EvaluateExpression(string expression)
		{
			if(expression.Length > 0)
			{
				int dividerPosition = expression.IndexOf(".");

				string section = expression.Substring(0, dividerPosition);
				string property = expression.Substring(dividerPosition + 1);

				//
				// Fetch section
				//
				foreach(SectionHolder holder in wizardPages)
				{
					string sectionName = holder.Section.Text.ToLower();

					if(sectionName.Equals(section.ToLower()))
					{
						//
						// Return property
						//
						return (bool)holder.Section.GetSetting(property);
					}
				}

				return false;
			}

			return true;
		}

		private void SetTopic(string topic)
		{
			topicLabel.Text = topic;
		}

		private void SetInformation(string information)
		{
			infoLabel.Text = information;
		}

		private void ShowPreviousPage()
		{
			//
			// Make sure we have something to show
			//
			while(true)
			{
				if(visiblePageIndex - 1 >= 0)
				{
					//
					// Move to previous index
					//
					visiblePageIndex--;

					//
					// Activate section
					//
					SectionHolder holder = wizardPages[visiblePageIndex] as SectionHolder;

					if(holder != null)
					{
						//
						// Evaluate if this section should be shown at all
						//
						if(EvaluateExpression(holder.Expression) == true)
						{
							ActivateSection(holder.Section);

							//
							// Set topic and information
							//
							SetTopic(holder.Topic);
							SetInformation(holder.Information);

							break;
						}
						else
						{
							//
							// Fetch next section
							//
						}
					}
				}
				else
				{
					//
					// No more pages to show
					//
					break;
				}
			}

			//
			// Update control status
			//
			UpdateControlStatus();			
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

		private void nextButton_Click(object sender, System.EventArgs e)
		{
			if(visiblePageIndex == wizardPages.Count - 1)
			{
				//
				// This was the last page, finish off the wizard
				//
				SaveSectionSettings();
				this.Close();
			}
			else
			{
				//
				// Show the next page of the wizard
				//
				ShowNextPage();		
			}
		}

		private void cancelButton_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void backButton_Click(object sender, System.EventArgs e)
		{
			ShowPreviousPage();
		}

		private void UpdateControlStatus()
		{
			backButton.Enabled = visiblePageIndex > 0;
			nextButton.Enabled = true;

			if(visiblePageIndex == wizardPages.Count - 1)
			{
				nextButton.Text = "Finish";
			}
			else
			{
				nextButton.Text = "Next >";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="currentNode"></param>
		private void LoadSectionSettings()
		{
			foreach(SectionHolder holder in wizardPages)
			{
				holder.Section.LoadSettings();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private void SaveSectionSettings()
		{
			foreach(SectionHolder holder in wizardPages)
			{
				holder.Section.SaveSettings();
			}
		}
	}
}
