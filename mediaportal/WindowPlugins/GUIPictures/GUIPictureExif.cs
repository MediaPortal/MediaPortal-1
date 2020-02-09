#region Copyright (C) 2005-2020 Team MediaPortal

// Copyright (C) 2005-2020 Team MediaPortal
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
using System.IO;
using System.Reflection;

using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.Picture.Database;
using MediaPortal.Util;

namespace MediaPortal.GUI.Pictures
{
  /// <summary>
  /// 
  /// </summary>
  public class GUIPicureExif : GUIInternalWindow, IRenderLayer
  {
    #region Skin controls

    [SkinControl(2)] protected GUIImage imgPicture = null;
    [SkinControl(3)] protected GUIListControl listExifProperties = null;
    [SkinControl(5)] protected GUIImage imgExif = null;

    #endregion

    #region Variables

    private string _currentPicture;
    private ExifMetadata.Metadata _currentMetaData ;
    private int _currentSelectedItem = -1;

    #endregion

    public GUIPicureExif()
    {
      GetID = (int)Window.WINDOW_PICTURE_EXIF;
    }

    #region Overrides

    public override bool Init()
    {
      return Load(GUIGraphicsContext.GetThemedSkinFile(@"\PictureExifInfo.xml"));
    }

    public override void PreInit() { }

    private void ReturnToPreviousWindow()
    {
      if (GUIWindowManager.HasPreviousWindow())
      {
        GUIWindowManager.ShowPreviousWindow();
      }
      else
      {
        GUIWindowManager.CloseCurrentWindow();
      }
    }

    protected override void OnPageLoad()
    {
      base.OnPageLoad();

      if (string.IsNullOrEmpty(_currentPicture) || !File.Exists(_currentPicture))
      {
        ReturnToPreviousWindow();
        return;
      }

      _currentMetaData = PictureDatabase.GetExifFromDB(_currentPicture);
      if (_currentMetaData.IsEmpty())
      {
        _currentMetaData = PictureDatabase.GetExifFromFile(_currentPicture);
      }
      if (_currentMetaData.IsEmpty())
      {
        ReturnToPreviousWindow();
        return;
      }

      GUIImageAllocator.ClearCachedAllocatorImages();
      GUITextureManager.CleanupThumbs();

      GUIPropertyManager.SetProperty("#pictures.exif.images", string.Empty);

      SetExifGUIListItems();
      Update();
      Refresh();
    }

    protected override void OnPageDestroy(int newWindowId)
    {
      GUIImageAllocator.ClearCachedAllocatorImages();
      GUITextureManager.CleanupThumbs();

      ReleaseResources();
      base.OnPageDestroy(newWindowId);
    }
    
    protected override void OnShowContextMenu()
    {
      GUIDialogMenu dlg = (GUIDialogMenu)GUIWindowManager.GetWindow((int)Window.WINDOW_DIALOG_MENU);
      
      if (dlg == null)
      {
        return;
      }
      
      dlg.Reset();
      dlg.SetHeading(498); // Menu
      
      // Dialog items
      dlg.AddLocalizedString(2168); // Update Exif

      // Show dialog menu
      dlg.DoModal(GetID);

      if (dlg.SelectedId== -1)
      {
        return;
      }
      
      switch (dlg.SelectedId)
      {
        case 2168: // Update Exif
          Log.Debug("GUIPicturesExif: Update Exif {0}: {1}", PictureDatabase.UpdatePicture(_currentPicture, -1), _currentPicture);
          _currentMetaData = PictureDatabase.GetExifFromDB(_currentPicture);
          SetExifGUIListItems();
          Update();
          Refresh();
          break;
      }
    }

    #endregion

    public string Picture
    {
      get { return _currentPicture; }
      set { _currentPicture = value; }
    }
    
    private void Update()
    {
      try
      {
        if (listExifProperties != null && !listExifProperties.IsVisible)
        {
          listExifProperties.IsVisible = true;

          if (!listExifProperties.IsEnabled)
          {
            GUIControl.EnableControl(GetID, listExifProperties.GetID);
          }

          GUIControl.SelectControl(GetID, listExifProperties.GetID);
          GUIControl.FocusControl(GetID, listExifProperties.GetID);
          GUIPropertyManager.SetProperty("#itemcount", listExifProperties.Count.ToString());
          listExifProperties.SelectedListItemIndex = _currentSelectedItem;
          SelectItem();
        }

        if (imgPicture != null)
        {
          imgPicture.Dispose();
          imgPicture.AllocResources();
          imgPicture.FileName = _currentPicture;
        }
        if (imgExif != null)
        {
          imgExif.Dispose();
          imgExif.AllocResources();
          imgExif.FileName = "#pictures.exif.images";
        }

        GUIPropertyManager.SetProperty("#currentpicture", _currentPicture);
      }
      catch (Exception ex)
      {
        Log.Error("GUIPictureExif Update controls Error: {1}", ex.Message);
      }
    }

    private void SelectItem()
    {
      if (_currentSelectedItem >= 0 && listExifProperties != null)
      {
        GUIControl.SelectItemControl(GetID, listExifProperties.GetID, _currentSelectedItem);
      }
    }

    private void Refresh()
    {
      SetProperties();
    }

    private void SetProperties()
    {
      _currentMetaData.SetExifProperties();

      int width = imgExif != null ? imgExif.Width < imgExif.Height ? 96 : 0 : 96;
      int height = imgExif != null ? imgExif.Width < imgExif.Height ? 0 : 96 : 0;

      List<GUIOverlayImage> exifIconImages = _currentMetaData.GetExifInfoOverlayImage(ref width, ref height);
      if (exifIconImages != null && exifIconImages.Count > 0)
      {
        GUIPropertyManager.SetProperty("#pictures.exif.images", GUIImageAllocator.BuildConcatImage("Exif:Details", string.Empty, width, height, exifIconImages));
      }
      else
      {
        GUIPropertyManager.SetProperty("#pictures.exif.images", string.Empty);
      }

      if (imgExif != null)
      {
        imgExif.Refresh();
      }
    }

    private void OnItemSelected(GUIListItem item, GUIControl parent)
    {
      try 
      {
        if (item != null)
        {
          GUIPropertyManager.SetProperty("#selecteditem", item.Label2 + ": " + item.Label);
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPicturesExif OnItemSelected exception: {0}", ex.Message);
      }
    }

    private void SetExifGUIListItems()
    {
      try 
      {
        if (listExifProperties != null)
        {
          listExifProperties.Clear();
        }
        else
        {
          return;
        }
        
        GUIListItem fileitem = new GUIListItem();
        fileitem.Label = Path.GetFileNameWithoutExtension(_currentPicture);
        fileitem.Label2 = GUILocalizeStrings.Get(863);
        fileitem.IconImage = Thumbs.Pictures + @"\exif\data\file.png";
        fileitem.ThumbnailImage = fileitem.IconImage;
        fileitem.OnItemSelected += OnItemSelected;
        listExifProperties.Add(fileitem);

        Type type = typeof(ExifMetadata.Metadata);
        foreach (FieldInfo prop in type.GetFields())
        {
          string value = string.Empty;
          string caption = prop.Name.ToCaption() ?? prop.Name;
          switch (prop.Name)
          {
            case "ImageDimensions": 
              value = _currentMetaData.ImageDimensionsAsString(); 
              break;
            case "Resolution": 
              value = _currentMetaData.ResolutionAsString(); 
              break;
            case "Location":
              if (!_currentMetaData.Location.IsZero)
              {
                string latitude = _currentMetaData.Location.Latitude.ToLatitudeString() ?? string.Empty;
                string longitude = _currentMetaData.Location.Longitude.ToLongitudeString() ?? string.Empty;
                if (!string.IsNullOrEmpty(latitude) && !string.IsNullOrEmpty(longitude))
                {
                  value = latitude + " / " + longitude;
                }
              }
              break;
            case "Altitude":
              if (_currentMetaData.Altitude != 0 || !_currentMetaData.Location.IsZero)
              {
                value = _currentMetaData.Altitude.ToAltitudeString();
              }
              break;
            default:
              value = ((ExifMetadata.MetadataItem)prop.GetValue(_currentMetaData)).DisplayValue;
              break;
          }
          if (!string.IsNullOrEmpty(value))
          {
             GUIListItem item = new GUIListItem();
             item.Label = value.ToValue() ?? value;
             item.Label2 = caption;
             item.IconImage = Thumbs.Pictures + @"\exif\data\" + prop.Name + ".png";
             item.ThumbnailImage = item.IconImage;
             item.OnItemSelected += OnItemSelected;
             listExifProperties.Add(item);
          }
        }

        if (listExifProperties.Count > 0)
        {
          listExifProperties.SelectedListItemIndex = 0;
          _currentSelectedItem = 0;
        }
      }
      catch (Exception ex)
      {
        Log.Error("GUIPicturesExif exception SetExifGUIListItems: {0}", ex.Message);
      }
    }

    #region IRenderLayer

    public bool ShouldRenderLayer()
    {
      return true;
    }

    public void RenderLayer(float timePassed)
    {
      Render(timePassed);
    }

    public override void Render(float timePassed)
    {
      base.Render(timePassed);
    }

    #endregion

    private void ReleaseResources()
    {
      if (imgPicture != null)
      {
        imgPicture.Dispose();
      }
      if (imgExif != null)
      {
        imgExif.Dispose();
      }
    }

  }
}