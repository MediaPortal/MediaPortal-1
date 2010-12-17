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
