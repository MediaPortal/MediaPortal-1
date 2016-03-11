using System;
using System.Collections;
using System.Linq;
using System.Text;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Database;
using Common.GUIPlugins;

namespace MediaPortal.GUI.Pictures
{
  public class FolderSetting : WindowPluginBase
  {
    public void UpdateFolders(int CurrentSortMethod, bool CurrentSortAsc, int CurrentLayout)
    {
      string currentFolder = GUIPictures.GetCurrentFolder;

      if (currentFolder == string.Empty)
      {
        currentFolder = "root";
      }

      if (!OnResetFolderSettings())
      {
        return;
      }
      Log.Debug("UpdateFolders: currentFolder {0}", currentFolder);

      ArrayList strPathList = new ArrayList();

      FolderSettings.GetPath(GUIPictures.GetCurrentFolder, ref strPathList, "Pictures");

      for (int iRow = 0; iRow < strPathList.Count; iRow++)
      {
        object o;
        FolderSettings.GetFolderSetting(strPathList[iRow] as string, "Pictures", typeof(GUIPictures.MapSettings), out o);
        Log.Debug("UpdateFolders: GetFolderSetting {0}", strPathList[iRow] as string);

        if (o != null)
        {
          GUIPictures.MapSettings mapSettings = o as GUIPictures.MapSettings;

          if (mapSettings == null)
          {
            mapSettings = new GUIPictures.MapSettings();
          }

          if (CurrentSortMethod != -1)
          {
            Log.Debug("UpdateFolders: old SortBy {0}, new SortBy {1}", mapSettings.SortBy, CurrentSortMethod);
            mapSettings.SortBy = CurrentSortMethod;
            mapSettings.SortAscending = CurrentSortAsc;
          }

          if (CurrentLayout != -1)
          {
            Log.Debug("UpdateFolders: old ViewAs {0}, new ViewAs {1}", mapSettings.ViewAs, CurrentLayout);
            mapSettings.ViewAs = CurrentLayout;
          }

          FolderSettings.AddFolderSetting(strPathList[iRow] as string, "Pictures", typeof(GUIPictures.MapSettings), mapSettings);
        }
      }
    }
  }
}
