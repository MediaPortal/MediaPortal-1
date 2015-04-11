using System;
using System.Collections;
using System.Linq;
using System.Text;
using MediaPortal.Util;
using MediaPortal.GUI.Library;
using MediaPortal.Database;
using Common.GUIPlugins;

namespace MediaPortal.GUI.Music
{
  public class FolderSetting : WindowPluginBase
  {
    public void UpdateFolders(int CurrentSortMethod, bool CurrentSortAsc, int CurrentLayout)
    {
      string currentFolder = GUIMusicFiles.GetCurrentFolder;

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

      FolderSettings.GetPath(GUIMusicFiles.GetCurrentFolder, ref strPathList, "MusicFiles");

      for (int iRow = 0; iRow < strPathList.Count; iRow++)
      {
        object o;
        FolderSettings.GetFolderSetting(strPathList[iRow] as string, "MusicFiles", typeof(GUIMusicFiles.MapSettings), out o);
        Log.Debug("UpdateFolders: GetFolderSetting {0}", strPathList[iRow] as string);

        if (o != null)
        {
          GUIMusicFiles.MapSettings mapSettings = o as GUIMusicFiles.MapSettings;

          if (mapSettings == null)
          {
            mapSettings = new GUIMusicFiles.MapSettings();
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

          FolderSettings.AddFolderSetting(strPathList[iRow] as string, "MusicFiles", typeof(GUIMusicFiles.MapSettings), mapSettings);
        }
      }
    }
  }
}
