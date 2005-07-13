using System;
using System.IO;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Xml;

namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for SectionSettings.
	/// </summary>
	public class SectionSettings : System.Windows.Forms.UserControl
	{
		public SectionSettings(string text)
		{
			this.AutoScroll=true;
			Text = text;
		}

		public virtual void SaveSettings()
		{
		}

		public virtual void LoadSettings()
		{
		}

    public virtual void LoadWizardSettings(XmlNode node)
    {

    }

    /// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SectionSettings()
		{
			// This call is required by the Windows.Forms Form Designer.
			this.AutoScroll=true;
			InitializeComponent();
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

		/// <summary>
		/// Returns the current setting for the given setting name
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public virtual object GetSetting(string name)
		{
			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static SectionSettings GetSection(string name)
		{
			SectionSettings sectionSettings = null;
			SectionTreeNode sectionTreeNode = SettingsForm.SettingSections[name] as SectionTreeNode;

			if(sectionTreeNode != null)
			{
				sectionSettings = sectionTreeNode.Section;
			}
			else
			{
				//
				// Failed to locate the specified section, loop through and try to match
				// a section against the type name instead, as this is the way the wizard names
				// its sections.
				//
				IDictionaryEnumerator enumerator = SettingsForm.SettingSections.GetEnumerator();

				while(enumerator.MoveNext())
				{
					SectionTreeNode treeNode = enumerator.Value as SectionTreeNode;

					if(treeNode != null)
					{
						Type sectionType = treeNode.Section.GetType();

						if(sectionType.Name.Equals(name))
						{
							sectionSettings = treeNode.Section;
							break;
						}
					}
				}

        //
        // If we didn't find what we were looking for it might be due to the fact that
        // we're running in wizard mode. Check with the loaded wizard pages too.
        //
        if(sectionSettings == null)
        {
          foreach(WizardForm.SectionHolder holder in WizardForm.WizardPages)
          {
            Type sectionType = holder.Section.GetType();

            if(sectionType.Name.Equals(name))
            {
              sectionSettings = holder.Section;
              break;
            }
          }
        }
			}

			return sectionSettings;
		}

		public virtual void OnSectionActivated()
		{
		}

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
