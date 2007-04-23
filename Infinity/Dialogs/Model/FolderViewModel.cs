using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Diagnostics;
using ProjectInfinity;
using ProjectInfinity.Localisation;

namespace Dialogs
{
  public class Folder : INotifyPropertyChanged
  {
    #region variables
    public event PropertyChangedEventHandler PropertyChanged;
    bool _isSelected;
    string _path;
    List<Folder> _subFolders = new List<Folder>();
    #endregion

    /// <summary>
    /// Initializes a new instance of the <see cref="Folder"/> class.
    /// </summary>
    public Folder()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Folder"/> class.
    /// </summary>
    /// <param name="path">The path.</param>
    public Folder(string path)
    {
      _isSelected = false;
      _path = path;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Folder"/> class.
    /// </summary>
    /// <param name="path">The path.</param>
    /// <param name="selected">if set to <c>true</c> [selected].</param>
    public Folder(string path, bool selected)
    {
      _isSelected = selected;
      _path = path;
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is selected.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is selected; otherwise, <c>false</c>.
    /// </value>
    public bool IsSelected
    {
      get
      {
        return _isSelected;
      }
      set
      {
        _isSelected = value;
        foreach (Folder subFolder in _subFolders)
        {
          subFolder.IsSelected = value;
        }
        if (PropertyChanged != null)
        {
          PropertyChanged(this, new PropertyChangedEventArgs("IsSelected"));
        }
      }
    }

    /// <summary>
    /// Gets or sets the path.
    /// </summary>
    /// <value>The path.</value>
    public string Path
    {
      get
      {
        int pos = _path.LastIndexOf(@"\");
        if (pos > 0 && pos < _path.Length - 1)
        {
          return _path.Substring(pos);
        }
        return _path;
      }
      set
      {
        _path = value;
      }
    }

    /// <summary>
    /// Gets or sets the folders.
    /// </summary>
    /// <value>The folders.</value>
    public List<Folder> Folders
    {
      get
      {
        if (_subFolders.Count == 0)
        {
          string[] subFolders = System.IO.Directory.GetDirectories(_path);
          for (int i = 0; i < subFolders.Length; ++i)
          {
            string path = subFolders[i];
            if (path == "." || path == "..") continue;
            Folder subFolder = new Folder(path, IsSelected);
            _subFolders.Add(subFolder);
          }
        }
        return _subFolders;
      }
      set
      {
        _subFolders = value;
      }
    }
    public List<Folder> SelectedFolders
    {
      get
      {
        List<Folder> selected = new List<Folder>();
        if (IsSelected) 
        {
          selected.Add(this);
          return selected;
        }
        foreach (Folder subfolder in _subFolders)
        {
          List<Folder> selectedSubs = subfolder.SelectedFolders;
          if (selectedSubs.Count != 0)
          {
            selected.AddRange(selectedSubs);
          }
        }
        return selected;
      }
    }

  }
  public class FolderViewModel : DialogViewModel
  {
    List<Folder> _folders = new List<Folder>();
    public FolderViewModel(Window window)
      : base(window)
    {
      string[] drives=System.IO.Directory.GetLogicalDrives();
      for (int i = 0; i < drives.Length; ++i)
      {
        _folders.Add(new Folder(drives[i]));
      }
    }

    public List<Folder> Folders
    {
      get
      {
        return _folders;
      }
    }
    public List<Folder> SelectedFolders
    {
      get
      {
        List<Folder> selected = new List<Folder>();
        foreach (Folder subfolder in _folders)
        {
          List<Folder> selectedSubs = subfolder.SelectedFolders;
          if (selectedSubs.Count != 0)
          {
            selected.AddRange(selectedSubs);
          }
        }
        return selected;
      }
    }

  }
}
