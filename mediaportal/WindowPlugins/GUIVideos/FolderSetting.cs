using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using MediaPortal.Util;
using MediaPortal.Video.Database;
using MediaPortal.GUI.Library;
using MediaPortal.Database;
using Common.GUIPlugins;

namespace MediaPortal.GUI.Video
{
  public class FolderSetting : WindowPluginBase
  {
    public void UpdateFolders(int CurrentSortMethod, bool CurrentSortAsc, int CurrentLayout)
    {
      string currentFolder = GUIVideoFiles.GetCurrentFolder;

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

      FolderSettings.GetPath(GUIVideoFiles.GetCurrentFolder, ref strPathList, "VideoFiles");

      for (int iRow = 0; iRow < strPathList.Count; iRow++)
      {
        object o;
        FolderSettings.GetFolderSetting(strPathList[iRow] as string, "VideoFiles", typeof(GUIVideoFiles.MapSettings), out o);
        Log.Debug("UpdateFolders: GetFolderSetting {0}", strPathList[iRow] as string);

        if (o != null)
        {
          GUIVideoFiles.MapSettings mapSettings = o as GUIVideoFiles.MapSettings;

          if (mapSettings == null)
          {
            mapSettings = new GUIVideoFiles.MapSettings();
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

          FolderSettings.AddFolderSetting(strPathList[iRow] as string, "VideoFiles", typeof(GUIVideoFiles.MapSettings), mapSettings);
        }
      }
    }
  }
}
