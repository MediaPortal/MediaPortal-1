#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using MediaPortal.Configuration;

namespace MediaPortal.GUI.DatabaseViews
{
  public class DatabaseViewHandler
  {
    #region Fields

    protected DatabaseViewDefinition currentView;
    protected int currentLevel = 0;
    protected List<DatabaseViewDefinition> views = new List<DatabaseViewDefinition>();

    #endregion

    #region Public Static Properties

    /// <summary>
    /// Returns the path to the directory, which contains all available default view definition files.
    /// </summary>
    public static string DefaultsDirectory
    {
      get { return Config.GetSubFolder(Config.Dir.Base, @"Defaults"); }
    }

    #endregion

    #region Public Virtual Properties

    public virtual int MaxLevels
    {
      get { return currentView.Levels.Count; }
    }

    public virtual int CurrentLevel
    {
      get { return currentLevel; }
      set
      {
        if (value < 0 || value >= currentView.Levels.Count)
        {
          return;
        }
        currentLevel = value;
      }
    }

    public virtual DatabaseViewDefinition View
    {
      get { return currentView; }
      set { currentView = value; }
    }

    public virtual List<DatabaseViewDefinition> Views
    {
      get { return views; }
      set { views = value; }
    }

    public virtual string LocalizedCurrentView
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }
        return currentView.LocalizedName;
      }
    }

    public virtual string CurrentViewName
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }
        return currentView.Name;
      }
    }

    public virtual string CurrentViewParentName
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }
        var parentName = string.Empty;
        foreach (DatabaseViewDefinition view in views)
        {
          // Do we have a Main View
          if (view.Id == new Guid(currentView.Parent))
          {
            parentName = view.Name;
          }
        }
        return parentName;
      }
    }

    /// <summary>
    /// Property for the view level name as localized string
    /// This will return the view level (ie. the where in view
    /// definition so artist, genre, actor etc)
    /// </summary>
    public virtual string LocalizedCurrentViewLevel
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }

        if (currentView.Levels.Count == 0)
        {
          return currentView.LocalizedName;
        }

        var def = currentView.Levels[currentLevel];

        return (GetLocalizedViewLevel(def.Selection));
      }
    }

    protected virtual string GetLocalizedViewLevel(String lvlName)
    {
      return lvlName;
    }

    public virtual Guid CurrentView
    {
      get
      {
        if (currentView == null)
        {
          return Guid.Empty;
        }
        return currentView.Id;
      }
      set
      {
        var searchViews = new List<DatabaseViewDefinition>();
        
        foreach (DatabaseViewDefinition view in views)
        {
          // Do we have a Main View
          if (view.Id == value)
          {
            currentView = view;
            CurrentLevel = 0;
            return;
          }
          foreach (var subview in view.SubViews)
          {
            if (subview.Id == value)
            {
              currentView = subview;
              CurrentLevel = 0;
              return;
            }
          }
        }

        if (views.Count > 0)
        {
          currentView = views[0];
        }
      }
    }

    public virtual int CurrentViewIndex
    {
      get { return views.IndexOf(currentView); }
    }

    public virtual string CurrentLevelWhere
    {
      get
      {
        var level = currentView.Levels[CurrentLevel];
        if (level == null)
        {
          return string.Empty;
        }
        return level.Selection;
      }
    }

    #endregion
  }
}