using System;
using System.IO;
using MediaPortal.GUI.Library;

namespace WindowPlugins.GUISettings.Epg
{
	/// <summary>
	/// Summary description for GUIWizardEpgSelect.
	/// </summary>
	public class GUIWizardEpgSelect : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIListControl listGrabbers=null;
		public GUIWizardEpgSelect()
		{
			
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_EPG_SELECT;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_epg_select.xml");
		}

		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadGrabbers();
		}
		void LoadGrabbers()
		{
			listGrabbers.Clear();
			string[] folders = System.IO.Directory.GetDirectories(@"webepg\grabbers");			
			foreach (string folder in folders)
			{
				Log.Write("{0}", folder);
				if (folder.IndexOf("..")>=0) continue;
				string[] files = System.IO.Directory.GetFiles(folder);
				foreach (string file in files)
				{
					string ext=System.IO.Path.GetExtension(file).ToLower();
					if (ext==".xml")
					{
						GUIListItem item = new GUIListItem();
						item.Label=System.IO.Path.GetFileNameWithoutExtension(folder);
						item.Path=String.Format(@"{0}\{1}",folder,file);
						listGrabbers.Add( item );
					}
				}
			}
		}
	}
}
