using System;
using System.Collections.Generic;
using System.Text;
using MediaPortal.TV.Database;
using MediaPortal.GUI.Library;

namespace WindowPlugins.VideoEditor
{
	class VideoEditorSettings
	{
		public CompressionSettings settings = new CompressionSettings();
		//public TVRecorded recInfo;
		//public DvrMsModifier dvrmsMod;
		public string currentFolder = "";
		//public List<System.IO.FileInfo> joiningList;
		//public List<TimeDomain> cutPointList;
		public GUISpinControl operationSpinCtrl = null;
		//public List<GUIListItem> commandListCtrl;
	}
}
