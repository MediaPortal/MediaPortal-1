using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class PictureShares : MediaPortal.Configuration.Sections.Shares
	{
		private System.ComponentModel.IContainer components = null;

		public PictureShares() : this("Picture Shares")
		{
		}

		public PictureShares(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				string defaultShare = xmlreader.GetValueAsString("pictures", "default", "");

				for(int index = 0; index < MaximumShares; index++)
				{
					string shareName = String.Format("sharename{0}", index);
					string sharePath = String.Format("sharepath{0}", index);
          string sharePin  = String.Format("pincode{0}", index);

					string shareNameData = xmlreader.GetValueAsString("pictures", shareName, "");
					string sharePathData = xmlreader.GetValueAsString("pictures", sharePath, "");
          string sharePinData = xmlreader.GetValueAsString("pictures", sharePin, "");

					if(shareNameData != null && shareNameData.Length > 0)
						AddShare(new ShareData(shareNameData, sharePathData, sharePinData), shareNameData.Equals(defaultShare));
				}
			}				
		}

		public override void SaveSettings()
		{
			using (AMS.Profile.Xml xmlwriter = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				string defaultShare = String.Empty;

				for(int index = 0; index < MaximumShares; index++)
				{
          string shareName = String.Format("sharename{0}", index);
          string sharePath = String.Format("sharepath{0}", index);
          string sharePin  = String.Format("pincode{0}", index);

          string shareNameData = String.Empty;
          string sharePathData = String.Empty;
          string sharePinData  = String.Empty;

					if(CurrentShares != null && CurrentShares.Count > index)
					{
            ShareData shareData = CurrentShares[index].Tag as ShareData;

            if(shareData != null)
            {
              shareNameData = shareData.Name;
              sharePathData = shareData.Folder;
              sharePinData  = shareData.PinCode;

              if(CurrentShares[index] == DefaultShare)
                defaultShare = shareNameData;
            }
          }

					xmlwriter.SetValue("pictures", shareName, shareNameData);
					xmlwriter.SetValue("pictures", sharePath, sharePathData);
          xmlwriter.SetValue("pictures", sharePin, sharePinData);
        }

				xmlwriter.SetValue("pictures", "default", defaultShare);
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

