using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace MediaPortal.Configuration.Sections
{
	public class MovieShares : MediaPortal.Configuration.Sections.Shares
	{

		private System.ComponentModel.IContainer components = null;

		public MovieShares() : this("Movie Folders")
		{
		}

		public MovieShares(string name) : base(name)
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call
		}

		public override void LoadSettings()
		{
			using (MediaPortal.Profile.Xml xmlreader = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string defaultShare = xmlreader.GetValueAsString("movies", "default", "");

				for(int index = 0; index < MaximumShares; index++)
				{
					string shareName = String.Format("sharename{0}", index);
					string sharePath = String.Format("sharepath{0}", index);
          string sharePin  = String.Format("pincode{0}", index);
          
          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd  = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);
					string shareViewPath = String.Format("shareview{0}", index);

					string shareNameData = xmlreader.GetValueAsString("movies", shareName, "");
					string sharePathData = xmlreader.GetValueAsString("movies", sharePath, "");
          string sharePinData = xmlreader.GetValueAsString("movies", sharePin, "");

          bool   shareTypeData = xmlreader.GetValueAsBool("movies", shareType, false);
          string shareServerData = xmlreader.GetValueAsString("movies", shareServer, "");
          string shareLoginData = xmlreader.GetValueAsString("movies", shareLogin, "");
          string sharePwdData = xmlreader.GetValueAsString("movies", sharePwd, "");
          int    sharePortData = xmlreader.GetValueAsInt("movies", sharePort, 21);
          string shareRemotePathData = xmlreader.GetValueAsString("movies", shareRemotePath, "/");
					int    shareView = xmlreader.GetValueAsInt("movies", shareViewPath, (int)ShareData.Views.List);

          if(shareNameData != null && shareNameData.Length > 0)
          {
            ShareData newShare= new ShareData(shareNameData, sharePathData, sharePinData);
            newShare.IsRemote=shareTypeData;
            newShare.Server=shareServerData;
            newShare.LoginName=shareLoginData;
            newShare.PassWord=sharePwdData;
            newShare.Port=sharePortData;
            newShare.RemoteFolder=shareRemotePathData;
						newShare.DefaultView=(ShareData.Views)shareView;

            AddShare(newShare, shareNameData.Equals(defaultShare));
          }
				}
			}				

      //
      // Add static shares
      //
      AddStaticShares(DriveType.DVD, "DVD");
		}

		public override void SaveSettings()
		{
			using (MediaPortal.Profile.Xml xmlwriter = new MediaPortal.Profile.Xml("MediaPortal.xml"))
			{
				string defaultShare = String.Empty;

				for(int index = 0; index < MaximumShares; index++)
				{
					string shareName = String.Format("sharename{0}", index);
					string sharePath = String.Format("sharepath{0}", index);
          string sharePin  = String.Format("pincode{0}", index);

          string shareType = String.Format("sharetype{0}", index);
          string shareServer = String.Format("shareserver{0}", index);
          string shareLogin = String.Format("sharelogin{0}", index);
          string sharePwd  = String.Format("sharepassword{0}", index);
          string sharePort = String.Format("shareport{0}", index);
          string shareRemotePath = String.Format("shareremotepath{0}", index);
					string shareViewPath = String.Format("shareview{0}", index);

					string shareNameData = String.Empty;
					string sharePathData = String.Empty;
          string sharePinData  = String.Empty;

          bool   shareTypeData = false;
          string shareServerData = String.Empty;
          string shareLoginData = String.Empty;
          string sharePwdData = String.Empty;
          int    sharePortData = 21;
          string shareRemotePathData = String.Empty;
					int    shareView =(int)ShareData.Views.List;

					if(CurrentShares != null && CurrentShares.Count > index)
					{
            ShareData shareData = CurrentShares[index].Tag as ShareData;

            if(shareData != null)
            {
              shareNameData = shareData.Name;
              sharePathData = shareData.Folder;
              sharePinData  = shareData.PinCode;

              shareTypeData = shareData.IsRemote;
              shareServerData = shareData.Server;
              shareLoginData = shareData.LoginName;
              sharePwdData = shareData.PassWord;
              sharePortData = shareData.Port;
              shareRemotePathData=shareData.RemoteFolder;
							shareView=(int)shareData.DefaultView;



              if(CurrentShares[index] == DefaultShare)
                defaultShare = shareNameData;
            }
					}

					xmlwriter.SetValue("movies", shareName, shareNameData);
					xmlwriter.SetValue("movies", sharePath, sharePathData);
          xmlwriter.SetValue("movies", sharePin, sharePinData);

          xmlwriter.SetValueAsBool("movies", shareType, shareTypeData);
          xmlwriter.SetValue("movies", shareServer, shareServerData);
          xmlwriter.SetValue("movies", shareLogin, shareLoginData);
          xmlwriter.SetValue("movies", sharePwd, sharePwdData);
          xmlwriter.SetValue("movies", sharePort, sharePortData.ToString());
          xmlwriter.SetValue("movies", shareRemotePath, shareRemotePathData);
					xmlwriter.SetValue("movies", shareViewPath, shareView);

				}

				xmlwriter.SetValue("movies", "default", defaultShare);
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

