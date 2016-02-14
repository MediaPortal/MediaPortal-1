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

    protected DatabaseViewDefinition _currentView;
    protected int _currentLevel = 0;
    protected List<DatabaseViewDefinition> views = new List<DatabaseViewDefinition>();
    private bool _foundView;

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
      get { return _currentView.Levels.Count; }
    }

    public virtual int CurrentLevel
    {
      get { return _currentLevel; }
      set
      {
        if (value < 0 || value >= _currentView.Levels.Count)
        {
          return;
        }
        _currentLevel = value;
      }
    }

    public virtual DatabaseViewDefinition View
    {
      get { return _currentView; }
      set { _currentView = value; }
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
        if (_currentView == null)
        {
          return string.Empty;
        }
        return _currentView.LocalizedName;
      }
    }

    public virtual string CurrentViewName
    {
      get
      {
        if (_currentView == null)
        {
          return string.Empty;
        }
        return _currentView.Name;
      }
    }

    public virtual string CurrentViewParentName
    {
      get
      {
        if (_currentView == null)
        {
          return string.Empty;
        }
        var parentName = string.Empty;
        foreach (DatabaseViewDefinition view in views)
        {
          // Do we have a Main View
          if (view.Id == new Guid(_currentView.Parent))
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
        if (_currentView == null)
        {
          return string.Empty;
        }

        if (_currentView.Levels.Count == 0)
        {
          return _currentView.LocalizedName;
        }

        var def = _currentView.Levels[_currentLevel];

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
        if (_currentView == null)
        {
          return Guid.Empty;
        }
        return _currentView.Id;
      }
      set
      {
        _foundView = false;
        foreach (DatabaseViewDefinition view in views)
        {
          // Do we have a Main View
          if (view.Id == value)
          {
            _currentView = view;
            _currentLevel = 0;
            return;
          }
          SearchSubViews(view, value);
          if (_foundView)
          {
            return;
          }
        }

        if (views.Count > 0 && !_foundView)
        {
          _currentView = views[0];
          _currentLevel = 0;
        }
      }
    }

    private void SearchSubViews(DatabaseViewDefinition view, Guid id)
    {
      foreach (var subview in view.SubViews)
      {
        if (subview.Id == id)
        {
          _currentView = subview;
          _currentLevel = 0;
          _foundView = true;
          return;
        }
        SearchSubViews(subview, id);
      }
    }

    public virtual int CurrentViewIndex
    {
      get { return views.IndexOf(_currentView); }
    }

    public virtual string CurrentLevelWhere
    {
      get
      {
        var level = _currentView.Levels[CurrentLevel];
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