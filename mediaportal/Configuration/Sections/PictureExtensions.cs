using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class PictureExtensions : MediaPortal.Configuration.Sections.FileExtensions
	{
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 
		/// </summary>
		public PictureExtensions() : this("Picture Extensions")
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public PictureExtensions(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		/// <summary>
		/// 
		/// </summary>
		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				Extensions = xmlreader.GetValueAsString("pictures", "extensions", ".jpg,.jpeg,.gif,.bmp,.png");
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				//
				// Set language
				//
				xmlwriter.SetValue("pictures", "extensions", Extensions);
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
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

