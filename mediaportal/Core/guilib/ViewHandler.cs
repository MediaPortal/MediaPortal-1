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
using System.Linq;
using System.Text;
using MediaPortal.Configuration;
using MediaPortal.GUI.View;

namespace MediaPortal.GUI.Library
{
  public class ViewHandler
  {
    #region Fields

    protected ViewDefinition currentView;
    protected int currentLevel = 0;
    protected List<ViewDefinition> views = new List<ViewDefinition>();

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
      get { return currentView.Filters.Count; }
    }

    public virtual int CurrentLevel
    {
      get { return currentLevel; }
      set
      {
        if (value < 0 || value >= currentView.Filters.Count)
        {
          return;
        }
        currentLevel = value;
      }
    }

    public virtual ViewDefinition View
    {
      get { return currentView; }
      set { currentView = value; }
    }

    public virtual List<ViewDefinition> Views
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
        
        FilterDefinition def = (FilterDefinition)currentView.Filters[currentLevel];

        if (def.SqlOperator == "group")
        {
          return GUILocalizeStrings.Get(1222);
        }
        
        return(GetLocalizedViewLevel(def.Where));
      }
    }

    public virtual string LocalizedCurrentViewPath
    {
      get
      {
        string viewPath = string.Empty;

        if (currentView == null)
        {
          return viewPath;
        }

        viewPath = LocalizedCurrentView;

        for (int i = 0; i < currentLevel; ++i)
        {
          FilterDefinition def = (FilterDefinition)currentView.Filters[i];

          if (def.SqlOperator == "group")
          {
            viewPath = viewPath + ":" + GUILocalizeStrings.Get(1222);
          }
          else
          {
            viewPath = viewPath + ":" + GetLocalizedViewLevel(def.Where);
          }
        }
        return viewPath;
      }
    }

    protected virtual string GetLocalizedViewLevel(String lvlName)
    {
      return lvlName;
    }

    public virtual string CurrentView
    {
      get
      {
        if (currentView == null)
        {
          return string.Empty;
        }
        return currentView.Name;
      }
      set
      {
        bool done = false;
        foreach (ViewDefinition definition in views)
        {
          if (definition.Name == value)
          {
            currentView = definition;
            CurrentLevel = 0;
            done = true;
            break;
          }
        }
        if (!done)
        {
          if (views.Count > 0)
          {
            currentView = (ViewDefinition)views[0];
          }
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
        FilterDefinition definition = (FilterDefinition)currentView.Filters[CurrentLevel];
        if (definition == null)
        {
          return string.Empty;
        }
        return definition.Where;
      }
    }

    #endregion
  }
}