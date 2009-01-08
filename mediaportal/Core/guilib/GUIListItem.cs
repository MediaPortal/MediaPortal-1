#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using MediaPortal.Util;
namespace MediaPortal.GUI.Library
{

  /// <summary>
  /// An implementation of an item that is part of a collection. (E.g, a GUIThumbnailPanel).
  /// </summary>
  public class GUIListItem
  {
    public delegate void ItemSelectedHandler(GUIListItem item, GUIControl parent);
    public delegate void RetrieveCoverArtHandler(GUIListItem item);

    //event which gets fired when the user has selected the item in the
    //list,thumbnail or filmstrip view
    public event ItemSelectedHandler OnItemSelected = null;

    //even which gets fired if the list,thumbnail or filmstrip few needs the
    //coverart for the specified item
    public event RetrieveCoverArtHandler OnRetrieveArt = null;
    protected string _label = string.Empty;							// text of column1
    protected string _label2 = string.Empty;							// text of column2
    protected string _label3 = string.Empty;							// text of column3
    protected string _thumbNailName = string.Empty;			// filename of thumbnail 
    protected string _smallIconName = string.Empty;								// filename of icon
    protected string _bigIconName = string.Empty;						// filename of icon
    protected GUIImage _thumbnailImage = null;			// pointer to CImage containing the thumbnail
    protected GUIImage _imageIcon = null;					// pointer to CImage containing the icon
    protected GUIImage _imageBigPinIcon = null;				// pointer to CImage containing the icon
    protected bool _isSelected = false;					// item is selected or not
    protected bool _isFolder = false;						// indicated if the item is a folder or a path
    protected string _folder = string.Empty;								// path + filename of the item
    protected string _dvdLabel = string.Empty;						// indicates the disc number of movie
    protected int _duration = 0;							// duration (in seconds) of the movie or song
    FileInformation _fileInfo = null;								// file info (size, date/time etc.) of the file
    bool _shaded = false;						// indicates if the item needs to be rendered shaded
    float _rating = 0;								// rating of a movie
    int _year = 0;									// release year of the movie/song
    object _tagMusic;									// object containing the tag info of a music file (e.g., id3 tag)
    object _tagTv;										// object containing the tag info of a tv-recording
    object _tagAlbumInfo;							// object tag info of a music album
    bool _isCoverArtRetrieved = false;
    string _pinIconName = string.Empty;
    protected GUIImage _imagePinIcon = null;
    protected bool _isRemote = false;           // indicating if this is a local or remote file
    protected bool _isDownloading = false;            // indicating if this file is being downloaded
    protected bool _isPlayed = false;                // indicating if playcount > 1
    int _idItem = 0;                // General item id
    bool _retrieveCoverArtAllowed = true;
    int _dimColor = 0x60ffffff;

    /// <summary>
    /// The (empty) constructor of the GUIListItem.
    /// </summary>
    public GUIListItem()
    {
    }


    /// <summary>
    /// Creates a GUIListItem based on another GUIListItem.
    /// </summary>
    /// <param name="item">The item on which the new item is based.</param>
    public GUIListItem(GUIListItem item)
    {
      _label = item._label;
      _label2 = item._label2;
      _label3 = item._label3;
      _thumbNailName = item._thumbNailName;
      _smallIconName = item._smallIconName;
      _bigIconName = item._bigIconName;
      _pinIconName = item._pinIconName;
      _isSelected = item._isSelected;
      _isFolder = item._isFolder;
      _folder = item._folder;
      _dvdLabel = item._dvdLabel;
      _duration = item._duration;
      _fileInfo = item._fileInfo;
      _rating = item._rating;
      _year = item._year;
      _idItem = item._idItem;
      _tagMusic = item._tagMusic;
      _tagTv = item._tagTv;
      _tagAlbumInfo = item._tagAlbumInfo;
    }

    public GUIListItem(string aLabel, string aLabel2, string aPath, bool aIsFolder, FileInformation aFileInformation)
    {
      if (String.IsNullOrEmpty(aLabel))
        return;

      _label = aLabel;
      _label2 = aLabel2;
      _folder = aPath;
      _isFolder = aIsFolder;
      _fileInfo = aFileInformation;
    }

    /// <summary>
    /// Creates a GUIListItem.
    /// </summary>
    /// <param name="strLabel">The text of the first label of the item.</param>
    public GUIListItem(string strLabel)
    {
      if (strLabel == null)
        return;
      _label = strLabel;
    }

    /// <summary>
    /// Get/set the text of the first label of the item.
    /// </summary>
    public string Label
    {
      get { return _label; }
      set
      {
        if (value == null)
          return;
        _label = value;
      }
    }

    /// <summary>
    /// Get/set the text of the second label of the item.
    /// </summary>
    public string Label2
    {
      get { return _label2; }
      set
      {
        if (value == null)
          return;
        _label2 = value;
      }
    }

    /// <summary>
    /// Get/set the text of the 3th label of the item.
    /// </summary>
    public string Label3
    {
      get { return _label3; }
      set
      {
        if (value == null)
          return;
        _label3 = value;
      }
    }

    /// <summary>
    /// Get/set the filename of the ThumbnailImage of the item.
    /// </summary>
    public string ThumbnailImage
    {
      get
      {

        DoRetrieveArt();
        return _thumbNailName;
      }
      set
      {
        if (value == null)
          return;
        _thumbNailName = value;
      }
    }

    /// <summary>
    /// Get/set the filename of the IconImage of the item.
    /// </summary>
    public string IconImage
    {
      get
      {

        DoRetrieveArt();
        return _smallIconName;
      }
      set
      {
        if (value == null)
          return;
        _smallIconName = value;
      }
    }

    public string PinImage
    {
      get
      {

        DoRetrieveArt();
        return _pinIconName;
      }
      set
      {
        if (value == null)
          return;
        _pinIconName = value;
      }
    }


    /// <summary>
    /// Get/set the filename of the IconImageBig of the item.
    /// </summary>
    public string IconImageBig
    {
      get
      {

        DoRetrieveArt();
        return _bigIconName;
      }
      set
      {
        if (value == null)
          return;
        _bigIconName = value;
      }
    }

    /// <summary>
    /// Get/set if the current item is selected.
    /// </summary>
    public bool Selected
    {
      get { return _isSelected; }
      set { _isSelected = value; }
    }

    /// <summary>
    /// Returns if the item has a thumbnail.
    /// </summary>
    public bool HasThumbnail
    {
      get { return _thumbNailName.Length > 0; }
    }

    /// <summary>
    /// Returns if the item has an icon.
    /// </summary>
    public bool HasIcon
    {
      get
      {
        DoRetrieveArt();
        return _smallIconName.Length > 0;
      }
    }


    /// <summary>
    /// Returns if the item has an icon.
    /// </summary>
    public bool HasPinIcon
    {
      get
      {
        DoRetrieveArt();
        return _pinIconName.Length > 0;
      }
    }

    /// <summary>
    /// Returns if the item has a bigicon.
    /// </summary>
    public bool HasIconBig
    {
      get
      {

        DoRetrieveArt();
        return _bigIconName.Length > 0;
      }
    }

    /// <summary>
    /// Get/set the Thumbnail image.
    /// </summary>
    public GUIImage Thumbnail
    {
      get { return _thumbnailImage; }
      set
      {
        _thumbnailImage = value;
        if (_thumbnailImage != null)
          _thumbnailImage.DimColor = DimColor;
      }
    }

    /// <summary>
    /// Get/set the icon image.
    /// </summary>
    public GUIImage Icon
    {
      get { return _imageIcon; }
      set
      {
        _imageIcon = value;
        if (_imageIcon != null)
          _imageIcon.DimColor = DimColor;
      }
    }

    /// <summary>
    /// Get/set the pinicon image.
    /// </summary>
    public GUIImage PinIcon
    {
      get { return _imagePinIcon; }
      set
      {
        _imagePinIcon = value;
        if (_imagePinIcon != null)
          _imagePinIcon.DimColor = DimColor;
      }
    }

    /// <summary>
    /// Get/set the big icon image.
    /// </summary>
    public GUIImage IconBig
    {
      get { return _imageBigPinIcon; }
      set
      {
        _imageBigPinIcon = value;
        if (_imageBigPinIcon != null)
          _imageBigPinIcon.DimColor = DimColor;

      }
    }

    /// <summary>
    /// Get/set if the item is a folder.
    /// </summary>
    public bool IsFolder
    {
      get { return _isFolder; }
      set { _isFolder = value; }
    }

    /// <summary>
    /// Get/set the path + filename of the item.
    /// </summary>
    public string Path
    {
      get { return _folder; }
      set
      {
        if (value == null)
          return;
        _folder = value;
      }
    }

    /// <summary>
    /// Get/set the DVDLabel of the item. This indicates the disc number of movie.
    /// </summary>
    public string DVDLabel
    {
      get { return _dvdLabel; }
      set
      {
        if (value == null)
          return;
        _dvdLabel = value;
      }
    }

    /// <summary>
    /// Gets the file size of the item.
    /// </summary>
    public long Size
    {
      get
      {
        if (_fileInfo != null)
          return _fileInfo.Length;
        else
          return 0L;
      }
    }

    /// <summary>
    /// Get/set the file info of the item.
    /// </summary>
    public FileInformation FileInfo
    {
      get { return _fileInfo; }
      set
      {
        try
        {
          _fileInfo = value;
        }
        catch (Exception) { }
      }
    }

    /// <summary>
    /// Get/set the duration (in seconds) of the movie or song.
    /// </summary>
    public int Duration
    {
      get { return _duration; }
      set { _duration = value; }
    }

    /// <summary>
    /// Get/set the release year of the movie/song.
    /// </summary>
    public int Year
    {
      get { return _year; }
      set { _year = value; }
    }

    /// <summary>
    /// Get/set the general item id.
    /// </summary>
    public int ItemId
    {
      get { return _idItem; }
      set { _idItem = value; }
    }

    /// <summary>
    /// Get/set the rating of a movie.
    /// </summary>
    public float Rating
    {
      get { return _rating; }
      set { _rating = value; }
    }

    /// <summary>
    /// Get/set the object containing the tag info of a tv-recording.
    /// </summary>
    public object TVTag
    {
      get { return _tagTv; }
      set { _tagTv = value; }
    }

    /// <summary>
    /// Get/set the object tag info of a music album
    /// </summary>
    public object AlbumInfoTag
    {
      get { return _tagAlbumInfo; }
      set { _tagAlbumInfo = value; }
    }

    /// <summary>
    /// Get/set the object containing the tag info of a music file (e.g., id3 tag).
    /// </summary>
    public object MusicTag
    {
      get { return _tagMusic; }
      set { _tagMusic = value; }
    }

    /// <summary>
    /// Get/set if the control is shaded.
    /// </summary>
    public bool Shaded
    {
      get { return _shaded; }
      set { _shaded = value; }
    }

    /// <summary>
    /// Free the memory that is used.
    /// </summary>
    public void FreeMemory()
    {
      _isCoverArtRetrieved = false;

      if (null != _thumbnailImage)
      {
        _thumbnailImage.FreeResources();
        _thumbnailImage = null;
      }
      if (null != _imageIcon)
      {
        _imageIcon.FreeResources();
        _imageIcon = null;
      }
      if (null != _imagePinIcon)
      {
        _imagePinIcon.FreeResources();
        _imagePinIcon = null;
      }
      if (null != _imageBigPinIcon)
      {
        _imageBigPinIcon.FreeResources();
        _imageBigPinIcon = null;
      }
    }

    /// <summary>
    /// Free the memory that is used by the icons.
    /// </summary>
    public void FreeIcons()
    {
      FreeMemory();
      if (OnRetrieveArt != null)
      {
        _thumbNailName = string.Empty;
        _smallIconName = string.Empty;
        _pinIconName = string.Empty;
      }
    }

    /// <summary>
    /// This method will raise the OnRetrieveArt() event to
    /// ask the listener to supply the thumbnail(s) for this item
    /// </summary>
    void DoRetrieveArt()
    {
      if (!_retrieveCoverArtAllowed)
        return;
      if (_isCoverArtRetrieved)
        return;
      _isCoverArtRetrieved = true;
      if (OnRetrieveArt != null)
        OnRetrieveArt(this);
    }
    public void RefreshCoverArt()
    {
      FreeIcons();
      _isCoverArtRetrieved = false;
    }


    /// <summary>
    /// this method will raise the OnItemSelected() event to let any
    /// listener know that this item has been selected by the user in a
    /// list,thumbnail or filmstrip control
    /// </summary>
    /// <param name="parent"></param>
    public void ItemSelected(GUIControl parent)
    {
      if (OnItemSelected != null)
      {
        AsyncCallback callback = new AsyncCallback(itemSelectedCallback);
        OnItemSelected.BeginInvoke(this, parent, callback, this);
      }
    }

    private void itemSelectedCallback(IAsyncResult ar)
    {
      OnItemSelected.EndInvoke(ar);
    }

    public bool IsPlayed
    {
      get { return _isPlayed; }
      set { _isPlayed = value; }
    }

    public bool IsRemote
    {
      get { return _isRemote; }
      set { _isRemote = value; }
    }

    public bool IsDownloading
    {
      get { return _isDownloading; }
      set { _isDownloading = value; }
    }

    public bool RetrieveArt
    {
      get { return _retrieveCoverArtAllowed; }
      set { _retrieveCoverArtAllowed = value; }
    }

    public virtual int DimColor
    {
      get { return _dimColor; }
      set
      {
        _dimColor = value;
        if (_thumbnailImage != null)
          _thumbnailImage.DimColor = value;
        if (_imageIcon != null)
          _imageIcon.DimColor = value;
        if (_imageBigPinIcon != null)
          _imageBigPinIcon.DimColor = value;
        if (_imagePinIcon != null)
          _imagePinIcon.DimColor = value;
      }
    }

  }
}