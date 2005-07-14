using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class FiltersSection : MediaPortal.Configuration.SectionSettings
	{
		//private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog;
		//private System.Windows.Forms.Label label4;
		//private System.ComponentModel.IContainer components = null;

		public FiltersSection() : this("Filters")
		{
		}

    private void InitializeComponent()
    {
      // 
      // FiltersSection
      // 
      this.Name = "FiltersSection";
      this.Size = new System.Drawing.Size(472, 408);

    }

		public FiltersSection(string name) : base(name)
		{

		}
	}
}

