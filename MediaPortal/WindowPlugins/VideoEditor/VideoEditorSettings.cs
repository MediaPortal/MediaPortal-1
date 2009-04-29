using MediaPortal.GUI.Library;

namespace WindowPlugins.VideoEditor
{
  internal class VideoEditorSettings
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