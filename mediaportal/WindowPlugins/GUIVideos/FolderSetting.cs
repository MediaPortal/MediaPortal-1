#region Copyright (C) 2005-2017 Team MediaPortal

// Copyright (C) 2005-2017 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

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

    public void UpdateViews(int CurrentSortMethod, bool CurrentSortAsc)
    {
      string currentView = GUIVideoTitle.GetCurrentView;

      if (currentView == string.Empty)
      {
        return;
      }

      if (!OnResetFolderSettings())
      {
        return;
      }

      Log.Debug("UpdateViews: CurrentView {0}", currentView);

      ArrayList strPathList = new ArrayList();

      FolderSettings.GetPath(GUIVideoTitle.GetCurrentView, ref strPathList, "VideoViews");

      for (int iRow = 0; iRow < strPathList.Count; iRow++)
      {
        object o;
        FolderSettings.GetFolderSetting(strPathList[iRow] as string, "VideoViews", typeof(GUIVideoTitle.MapSettings), out o);
        Log.Debug("UpdateViews: GetViewSetting {0}", strPathList[iRow] as string);

        if (o != null)
        {
          GUIVideoTitle.MapSettings mapSettings = o as GUIVideoTitle.MapSettings;

          if (mapSettings == null)
          {
            mapSettings = new GUIVideoTitle.MapSettings();
          }

          if (CurrentSortMethod != -1)
          {
            Log.Debug("UpdateViews: Old SortBy {0}/{2}, new SortBy {1}/{3}", mapSettings.SortBy, CurrentSortMethod, mapSettings.SortBy, CurrentSortAsc);
            mapSettings.SortBy = CurrentSortMethod;
            mapSettings.SortAscending = CurrentSortAsc;

            FolderSettings.AddFolderSetting(strPathList[iRow] as string, "VideoViews", typeof(GUIVideoFiles.MapSettings), mapSettings);
          }
        }
      }
    }
  }
}
