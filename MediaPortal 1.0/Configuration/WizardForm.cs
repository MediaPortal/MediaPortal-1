#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;
using MediaPortal.GUI.Library;
using MediaPortal.TV.Recording;
using MediaPortal.Util;

namespace MediaPortal.Configuration
{
  /// <summary>
  /// Summary description for WizardForm.
  /// </summary>
  public class WizardForm : MediaPortal.UserInterface.Controls.MPConfigForm
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
    public static ArrayList WizardPages
    {
      get { return wizardPages; }
    }

    static ArrayList wizardPages = new ArrayList();

    public static WizardForm Form
    {
      get { return wizardForm; }
    }

    static WizardForm wizardForm;

    string wizardCaption = string.Empty;

    private MediaPortal.UserInterface.Controls.MPButton cancelButton;
    private MediaPortal.UserInterface.Controls.MPButton nextButton;
    private MediaPortal.UserInterface.Controls.MPButton backButton;
    private System.Windows.Forms.Panel topPanel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.Panel holderPanel;
    private MediaPortal.UserInterface.Controls.MPLabel topicLabel;
    private MediaPortal.UserInterface.Controls.MPLabel infoLabel;
    private System.Windows.Forms.PictureBox pictureBox1;
    int visiblePageIndex = -1;

    public void AddSection(SectionSettings settings, string topic, string information)
    {
      AddSection(settings, topic, information, string.Empty);
    }

    public void AddSection(SectionSettings settings, string topic, string information, string expression)
    {
      wizardPages.Add(new SectionHolder(settings, topic, information, expression));
    }

    public void DisableBack(bool disabled)
    {
      backButton.Enabled = !disabled;
    }

    public void DisableNext(bool disabled)
    {
      nextButton.Enabled = !disabled;
    }

    public WizardForm(string sectionConfiguration)
    {
      wizardForm = this;
      //
      // Required for Windows Form Designer support
      //
      InitializeComponent();

      // Stop MCE services
      MediaPortal.Util.Utils.StopMCEServices();

      //
      // Set caption
      //
      wizardCaption = "MediaPortal Settings Wizard";

      //
      // Check if we got a sections file to read from, or if we should specify
      // the default sections
      //
      if (sectionConfiguration != string.Empty && System.IO.File.Exists(sectionConfiguration))
      {
        LoadSections(sectionConfiguration);
      }
      else
      {
        Sections.TVCaptureCards sect = new Sections.TVCaptureCards();
        sect.AddAllCards();
        sect.LoadCaptureCards();
        bool analogCard = false;
        bool DVBTCard = false;
        bool DVBCCard = false;
        bool DVBSCard = false;
        bool ATSCCard = false;
        Log.Info("found {0} tv cards", sect.captureCards.Count);
        foreach (TVCaptureDevice dev in sect.captureCards)
        {
          /*
                    if (dev.VideoDevice == "B2C2 MPEG-2 Source" ||
                        dev.VideoDevice == "TechnoTrend SAA7146 Capture (WDM)")
                    {
                      dev.CreateGraph();
                    }
          */
          if (dev.Network == NetworkType.Analog)
          {
            Log.Info("Analog TV Card:{0}", dev.CommercialName);
            analogCard = true;
          }
          if (dev.Network == NetworkType.DVBT)
          {
            Log.Info("Digital DVB-T Card:{0}", dev.CommercialName);
            DVBTCard = true;
          }
          if (dev.Network == NetworkType.DVBC)
          {
            Log.Info("Digital DVB-C Card:{0}", dev.CommercialName);
            DVBCCard = true;
          }
          if (dev.Network == NetworkType.DVBS)
          {
            Log.Info("Digital DVB-S Card:{0}", dev.CommercialName);
            DVBSCard = true;
          }
          if (dev.Network == NetworkType.ATSC)
          {
            Log.Info("Digital ATSC Card:{0}", dev.CommercialName);
            ATSCCard = true;
          }
          /*
                    if (dev.VideoDevice == "B2C2 MPEG-2 Source" || 
                        dev.VideoDevice == "TechnoTrend SAA7146 Capture (WDM)")
                    {
                      dev.DeleteGraph();
                    }
          */
        }

        AddSection(new Sections.Wizard_Welcome(), "Welcome to MediaPortal", "");
        AddSection(new Sections.General(), "General", "General information...");
        AddSection(new Sections.GeneralSkin(), "Skin", "Skin settings...");
        AddSection(new Sections.Wizard_SelectPlugins(), "Media", "Let MediaPortal find your media (music, movies, pictures) on your harddisk");
        //if (analogCard)
        //{
        //  AddSection(new Sections.Wizard_AnalogTV(), "TV - Analog", "Analog TV configuration", "");
        //  AddSection(new Sections.Wizard_AnalogRadio(), "Radio - Analog", "Analog Radio configuration", "");
        //}
        //if (DVBTCard)
        //{
        //  AddSection(new Sections.Wizard_DVBTTV(), "TV - DVB-T", "Digital TV Terrestrial configuration", "");
        //}
        //if (DVBCCard)
        //{
        //  AddSection(new Sections.Wizard_DVBCTV(), "TV - DVB-C", "Digital TV Cable configuration", "");
        //}
        //if (DVBSCard)
        //{
        //  AddSection(new Sections.Wizard_DVBSTV(), "TV - DVB-S", "Digital TV Satellite configuration", "");
        //}
        //if (ATSCCard)
        //{
        //  AddSection(new Sections.Wizard_ATSCTV(), "TV - ATSC", "Digital TV ATSC configuration", "");
        //}
        
        // AddSection(new Sections.TVProgramGuide(), "Television Program Guide", "Configure the Electronic Program Guide using XMLTV listings", "");

        if (Sections.Remote.IsMceRemoteInstalled(this.Handle))
        {
          AddSection(new Sections.Remote(), "Remote Control", "Configure MCE Remote control", "");
        }
        AddSection(new Sections.Weather(), "Weather", "My weather setup", "");
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
        if (rootElement != null && rootElement.Name.Equals("wizard"))
        {
          //
          // Fetch wizard settings
          //
          XmlNode wizardTopicNode = rootElement.SelectSingleNode("/wizard/caption");
          if (wizardTopicNode != null)
          {
            wizardCaption = wizardTopicNode.InnerText;
          }

          //
          // Fetch sections
          //
          XmlNodeList nodeList = rootElement.SelectNodes("/wizard/sections/section");

          foreach (XmlNode node in nodeList)
          {
            //
            // Fetch section information
            //
            XmlNode nameNode = node.SelectSingleNode("name");
            XmlNode topicNode = node.SelectSingleNode("topic");
            XmlNode infoNode = node.SelectSingleNode("information");
            XmlNode dependencyNode = node.SelectSingleNode("dependency");

            if (nameNode != null && nameNode.InnerText.Length > 0)
            {
              //
              // Allocate new wizard page
              //
              SectionSettings section = CreateSection(nameNode.InnerText);

              if (section != null)
              {
                //
                // Load wizard specific settings
                //
                section.LoadWizardSettings(node);

                //
                // Add the section to the sections list
                //
                if (dependencyNode == null)
                {
                  AddSection(section, topicNode != null ? topicNode.InnerText : string.Empty, infoNode != null ? infoNode.InnerText : string.Empty);
                }
                else
                {
                  AddSection(section, topicNode != null ? topicNode.InnerText : string.Empty, infoNode != null ? infoNode.InnerText : string.Empty, dependencyNode.InnerText);
                }
              }
            }
          }
        }
      }
      catch (Exception e)
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

      if (sectionType != null)
      {
        //
        // Create the instance of the section settings class, pass the section name as argument
        // to the constructor. We do this to be able to use the same name on <name> as in the <dependency> tag.
        //
        SectionSettings section = (SectionSettings)Activator.CreateInstance(sectionType, new object[] { sectionName });
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

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.cancelButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.nextButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.backButton = new MediaPortal.UserInterface.Controls.MPButton();
      this.topPanel = new System.Windows.Forms.Panel();
      this.infoLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.topicLabel = new MediaPortal.UserInterface.Controls.MPLabel();
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.holderPanel = new System.Windows.Forms.Panel();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.topPanel.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.Location = new System.Drawing.Point(536, 520);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(72, 22);
      this.cancelButton.TabIndex = 1;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
      // 
      // nextButton
      // 
      this.nextButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.nextButton.Location = new System.Drawing.Point(456, 520);
      this.nextButton.Name = "nextButton";
      this.nextButton.Size = new System.Drawing.Size(72, 22);
      this.nextButton.TabIndex = 0;
      this.nextButton.Text = "&Next >";
      this.nextButton.UseVisualStyleBackColor = true;
      this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
      // 
      // backButton
      // 
      this.backButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.backButton.Location = new System.Drawing.Point(376, 520);
      this.backButton.Name = "backButton";
      this.backButton.Size = new System.Drawing.Size(72, 22);
      this.backButton.TabIndex = 5;
      this.backButton.Text = "< &Back";
      this.backButton.UseVisualStyleBackColor = true;
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
      this.topPanel.Size = new System.Drawing.Size(618, 72);
      this.topPanel.TabIndex = 2;
      // 
      // infoLabel
      // 
      this.infoLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.infoLabel.Location = new System.Drawing.Point(8, 26);
      this.infoLabel.Name = "infoLabel";
      this.infoLabel.Size = new System.Drawing.Size(512, 40);
      this.infoLabel.TabIndex = 1;
      this.infoLabel.Text = "Information information information information information";
      // 
      // topicLabel
      // 
      this.topicLabel.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
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
      this.panel1.Size = new System.Drawing.Size(618, 1);
      this.panel1.TabIndex = 7;
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.SystemColors.ControlLightLight;
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 73);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(618, 1);
      this.panel2.TabIndex = 3;
      // 
      // holderPanel
      // 
      this.holderPanel.Location = new System.Drawing.Point(16, 88);
      this.holderPanel.Name = "holderPanel";
      this.holderPanel.Size = new System.Drawing.Size(584, 408);
      this.holderPanel.TabIndex = 4;
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::MediaPortal.Configuration.Properties.Resources.wizard_header;
      this.pictureBox1.Location = new System.Drawing.Point(528, 14);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(70, 48);
      this.pictureBox1.TabIndex = 2;
      this.pictureBox1.TabStop = false;
      // 
      // WizardForm
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.ClientSize = new System.Drawing.Size(618, 552);
      this.Controls.Add(this.holderPanel);
      this.Controls.Add(this.panel2);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.topPanel);
      this.Controls.Add(this.backButton);
      this.Controls.Add(this.nextButton);
      this.Controls.Add(this.cancelButton);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.Name = "WizardForm";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "WizardForm";
      this.Load += new System.EventHandler(this.WizardForm_Load);
      this.topPanel.ResumeLayout(false);
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
      while (true)
      {
        if (visiblePageIndex + 1 < wizardPages.Count)
        {
          //
          // Move to next index, the index  that will be shown
          //
          visiblePageIndex++;

          //
          // Activate section
          //
          SectionHolder holder = wizardPages[visiblePageIndex] as SectionHolder;

          if (holder != null)
          {
            //
            // Evaluate if this section should be shown at all
            //
            if (EvaluateExpression(holder.Expression) == true)
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
      if (expression.Length > 0)
      {
        int dividerPosition = expression.IndexOf(".");

        string section = expression.Substring(0, dividerPosition);
        string property = expression.Substring(dividerPosition + 1);

        //
        // Fetch section
        //
        foreach (SectionHolder holder in wizardPages)
        {
          string sectionName = holder.Section.Text.ToLower();

          if (sectionName.Equals(section.ToLower()))
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
      while (true)
      {
        if (visiblePageIndex - 1 >= 0)
        {
          //
          // Move to previous index
          //
          visiblePageIndex--;

          //
          // Activate section
          //
          SectionHolder holder = wizardPages[visiblePageIndex] as SectionHolder;

          if (holder != null)
          {
            //
            // Evaluate if this section should be shown at all
            //
            if (EvaluateExpression(holder.Expression) == true)
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
      SectionHolder holder = wizardPages[visiblePageIndex] as SectionHolder;
      holder.Section.SaveSettings();
      if (visiblePageIndex == wizardPages.Count - 1)
      {
        //
        // This was the last page, finish off the wizard
        //
        SaveSectionSettings();

        // Restart MCE services
        MediaPortal.Util.Utils.RestartMCEServices();

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
      // Restart MCE services
      MediaPortal.Util.Utils.RestartMCEServices();

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

      if (visiblePageIndex == wizardPages.Count - 1)
      {
        nextButton.Text = "&Finish";
      }
      else
      {
        nextButton.Text = "&Next >";
      }

      //
      // Set caption
      //
      this.Text = String.Format("{0} [{1}/{2}]", wizardCaption, visiblePageIndex + 1, wizardPages.Count);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="currentNode"></param>
    private void LoadSectionSettings()
    {
      foreach (SectionHolder holder in wizardPages)
      {
        holder.Section.LoadSettings();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private void SaveSectionSettings()
    {
      foreach (SectionHolder holder in wizardPages)
      {
        holder.Section.SaveSettings();
      }

      //
      // Init general (not visual) settings
      //
      MediaPortal.Configuration.SectionSettings DVDClass;
      DVDClass = new Sections.DVD("DVD");
      DVDClass.LoadSettings();
      DVDClass.SaveSettings();
      DVDClass.Dispose();

      //
      // Make sure default enabled plugins are activated in MediaPortal.xml
      // (required since PluginManager only loads plugins which are listed in there)
      //
      Sections.PluginsNew pluginSettings = new MediaPortal.Configuration.Sections.PluginsNew();
      pluginSettings.OnSectionActivated();
      pluginSettings.SaveSettings();

      MediaPortal.Profile.Settings.SaveCache();
    }
  }
}