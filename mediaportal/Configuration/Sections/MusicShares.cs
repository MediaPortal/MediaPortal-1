using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class MusicShares : MediaPortal.Configuration.Sections.Shares
	{
		private System.ComponentModel.IContainer components = null;

		public MusicShares() : this("Music Shares")
		{
		}

		public MusicShares(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		public override void LoadSettings()
		{
			using (AMS.Profile.Xml xmlreader = new AMS.Profile.Xml("MediaPortal.xml"))
			{
				string defaultShare = xmlreader.GetValueAsString("music", "default", "");

				for(int index = 0; index < MaximumShares; index++)
				{
					string shareName = String.Format("sharename{0}", index);
					string sharePath = String.Format("sharepath{0}", index);
          string sharePin  = String.Format("pincode{0}", index);

					string shareNameData = xmlreader.GetValueAsString("music", shareName, "");
					string sharePathData = xmlreader.GetValueAsString("music", sharePath, "");
          string sharePinData = xmlreader.GetValueAsString("pictures", sharePin, "");

					if(shareNameData != null && shareNameData.Length > 0)
						AddShare(new ShareData(shareNameData, sharePathData, sharePinData), shareNameData.Equals(defaultShare));
				}
			}				

      //
      // Add static shares
      //
      AddStaticShares(DriveType.CD, "CD");
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

					xmlwriter.SetValue("music", shareName, shareNameData);
					xmlwriter.SetValue("music", sharePath, sharePathData);
          xmlwriter.SetValue("music", sharePin, sharePinData);
        }

				xmlwriter.SetValue("music", "default", defaultShare);
			}
		}

    public override object GetSetting(string name)
    {
      switch(name.ToLower())
      {
        case "shares.available":
          return CurrentShares.Count > 0;
        
        case "shares":
          ArrayList shares = new ArrayList();

          foreach(ListViewItem listItem in CurrentShares)
          {
            shares.Add(listItem.SubItems[2].Text);
          }
          return shares;
      }

      return null;
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

