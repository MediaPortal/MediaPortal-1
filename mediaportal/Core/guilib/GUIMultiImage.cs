#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using MediaPortal.Util;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A GUIControl for displaying MultiImages.
  /// </summary>
  public class GUIMultiImage : GUIControl
  {
    #region variables

    [XMLSkinElement("imagepath")] private string _texturePath = "";
    [XMLSkinElement("timeperimage")] private uint _timePerImage;
    [XMLSkinElement("fadetime")] private uint _fadeTime;
    [XMLSkinElement("randomize")] private bool _randomized;
    [XMLSkinElement("loop")] private bool _loop;
    [XMLSkinElement("keepaspectratio")] private bool _keepAspectRatio = false;

    private List<GUIImage> _imageList = new List<GUIImage>();
    private List<string> _fileList = new List<string>();
    private bool _directoryLoaded;
    private bool _isAllocated;
    private int _Info;
    private int _currentImage;
    private StopWatch _imageTimer = new StopWatch();
    private StopWatch _fadeTimer = new StopWatch();
    private bool _registeredForEvent = false;
    private bool _containsProperty = false;
    private bool _propertyChanged = false;
    private string _cachedPath = "";
    private string _newPath;

    #endregion

    #region ctor

    private GUIMultiImage() {}

    public GUIMultiImage(int dwParentID)
      : base(dwParentID) {}


    /// <summary>
    /// The constructor of the GUIMultiImage class.
    /// </summary>
    /// Remember to insert params here
    /// <param name="dwParentID">The parent of this GUIImage control.</param>
    /// <param name="dwControlId">The ID of this GUIImage control.</param>
    /// <param name="iPosX">The X position of this GUIImage control.</param>
    /// <param name="iPosY">The Y position of this GUIImage control.</param>
    /// <param name="dwWidth">The width of this GUIImage control.</param>
    /// <param name="dwHeight">The height of this GUIImage control.</param>
    /// <param name="strTexturePath">The filename of the texture (folder or file) of this GUIMultiImage control.</param>
    /// <param name="keepAspectRatio">keep the aspect ratio of the pictures</param>
    /// <param name="timePerImage">time in miliseconds that each image should be shown.</param>
    /// <param name="fadeTime">fadetime in miliseconds between visiblilitystates.</param>
    /// <param name="randomized">indicates if the images should be randomized.</param>
    /// <param name="loop">indicates if the image rotation should be looped.</param>
    public GUIMultiImage(int dwParentID, int dwControlId, int iPosX, int iPosY, int dwWidth, int dwHeight,
                         string strTexturePath, bool keepAspectRatio, uint timePerImage, uint fadeTime, bool randomized,
                         bool loop)
      : base(dwParentID, dwControlId, iPosX, iPosY, dwWidth, dwHeight)
    {
      _keepAspectRatio = keepAspectRatio;
      _timePerImage = timePerImage;
      _fadeTime = fadeTime;
      _randomized = randomized;
      _loop = loop;
      _newPath = strTexturePath;
      FinalizeConstruction();
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      if (_texturePath == null)
      {
        _texturePath = string.Empty;
      }

      if (_texturePath.IndexOf("#") >= 0)
      {
        _containsProperty = true;
      }

      _cachedPath = _texturePath;
      _propertyChanged = true;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/Set the TexturePath
    /// </summary>
    public string TexturePath
    {
      get { return _texturePath; }
      set { _texturePath = value; }
    }

    public uint TimePerImage
    {
      get { return _timePerImage; }
      set { _timePerImage = value; }
    }

    public uint FadeTime
    {
      get { return _fadeTime; }
      set { _fadeTime = value; }
    }

    public bool Randomized
    {
      get { return _randomized; }
      set { _randomized = value; }
    }

    public bool Loop
    {
      get { return _loop; }
      set { _loop = value; }
    }

    public new int Info
    {
      get { return _Info; }
      set { _Info = value; }
    }

    #endregion

    public override void ScaleToScreenResolution()
    {
      foreach (GUIImage image in _imageList)
      {
        image.ScaleToScreenResolution();
      }

      base.ScaleToScreenResolution();
    }

    public override void Render(float timePassed)
    {
      if (!IsVisible)
      {
        base.Render(timePassed);
        return;
      }

      if (_containsProperty && _propertyChanged)
      {
        _propertyChanged = false;
        _newPath = GUIPropertyManager.Parse(_texturePath);

        if (_cachedPath != _newPath)
        {
          if (_newPath == null)
          {
            _newPath = "";
          }
          _cachedPath = _newPath;
        }
        if (_cachedPath == null)
        {
          base.Render(timePassed);
          return;
        }

        if (_cachedPath.IndexOf("#") >= 0)
        {
          base.Render(timePassed);
          return;
        }
        Dispose();
        LoadDirectory();
      }

      if (!_isAllocated)
      {
        AllocResources();
      }

      if (_imageList.Count != 0)
      {
        _imageList[_currentImage].Render(timePassed);

        int nextImage = _currentImage + 1;
        if (nextImage >= _imageList.Count)
        {
          nextImage = _loop ? 0 : _currentImage; // stay on the last image if <loop>no</loop>
        }

        if (nextImage != _currentImage)
        {
          // check if we should be loading a new image yet
          if (_imageTimer.IsRunning && _imageTimer.ElapsedMilliseconds > _timePerImage)
          {
            _imageTimer.Stop();
            // grab a new image
            LoadImage(nextImage);
            // start the fade timer
            _fadeTimer.StartZero();
          }

          // check if we are still fading
          if (_fadeTimer.IsRunning)
          {
            // check if the fade timer has run out
            float timeFading = _fadeTimer.ElapsedMilliseconds;
            if (timeFading > _fadeTime)
            {
              _fadeTimer.Stop();
              // swap images
              _imageList[_currentImage].SafeDispose();
              _imageList[nextImage].ColourDiffuse = (_imageList[nextImage].ColourDiffuse | 0xff000000);
              _currentImage = nextImage;
              // start the load timer
              _imageTimer.StartZero();
            }
            else
            {
              // perform the fade
              float fadeAmount = timeFading / _fadeTime;
              long alpha = (int)(255 * fadeAmount);
              alpha <<= 24;
              alpha += (_imageList[nextImage].ColourDiffuse & 0x00ffffff);
              _imageList[nextImage].ColourDiffuse = alpha;
            }
            _imageList[nextImage].Render(timePassed);
          }
        }
      }
      base.Render(timePassed);
    }

    public override void PreAllocResources()
    {
      Dispose();
    }

    public override void AllocResources()
    {
      try
      {
        if (GUIGraphicsContext.DX9Device == null)
        {
          return;
        }
        if (GUIGraphicsContext.DX9Device.Disposed)
        {
          return;
        }
        if (_registeredForEvent == false)
        {
          GUIPropertyManager.OnPropertyChanged += GUIPropertyManager_OnPropertyChanged;
          _registeredForEvent = true;
        }
        _currentImage = 0;
        _Info = 0;
        _directoryLoaded = false;
        _isAllocated = false;
        _newPath = _texturePath;
        Dispose();
        base.AllocResources();
      }
      catch (Exception e)
      {
        Log.Error(e);
      }
      finally
      {
        _isAllocated = true;
      }

      if (!_directoryLoaded)
      {
        LoadDirectory();
      }

      // Randomize or sort our images if necessary
      if (_randomized && _fileList.Count > 1)
      {
        Random_Shuffle(ref _fileList);
      }

      for (int i = 0; i < _fileList.Count; i++)
      {
        GUIImage pImage = new GUIImage(ParentID, GetID, _positionX, _positionY, _width, _height, _fileList[i], 0);
        pImage.KeepAspectRatio = _keepAspectRatio;
        _imageList.Add(pImage);
      }

      // Load in the current image, and reset our timer
      _imageTimer.StartZero();
      _fadeTimer.Stop();
      _currentImage = 0;
      if (_imageList.Count == 0)
      {
        return;
      }

      LoadImage(_currentImage);
      _isAllocated = true;
    }

    public void LoadImage(int image)
    {
      if (_imageList != null)
      {
        if (image < 0 || image >= _imageList.Count)
        {
          return;
        }

        _imageList[image].AllocResources();
        _imageList[image].ColourDiffuse = ColourDiffuse;
      }
    }

    private void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      if (!_containsProperty)
      {
        return;
      }
      if (_texturePath.IndexOf(tag) >= 0)
      {
        _propertyChanged = true;
      }
    }

    public override void Dispose()
    {          
      _imageList.DisposeAndClear();
      //_currentImage = 0;      

      if (_registeredForEvent)
      {
        GUIPropertyManager.OnPropertyChanged -=
          new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
        _registeredForEvent = false;
      }

      _currentImage = 0;
      base.Dispose();
    }

    public override bool CanFocus()
    {
      return false;
    }

    public bool KeepAspectRatio
    {
      get { return _keepAspectRatio; }
      set
      {
        if (_keepAspectRatio != value)
        {
          _keepAspectRatio = value;
        }
      }
    }

    public void LoadDirectory()
    {
      if (_directoryLoaded)
      {
        return;
      }

      // Unload any images from our texture bundle first
      _fileList.Clear();

      // Don't load any images if our path is empty
      if (_newPath.Length == 0)
      {
        return;
      }

      // Check to see if we have a single image or a folder of images
      if (MediaPortal.Util.Utils.IsPicture(_newPath))
      {
        _fileList.Add(_newPath);
      }
      else
      {
        // Folder of images
        try
        {
          string imageFolder = string.Empty;
          // Try to use the provided folder as an absolute path
          if (Directory.Exists(_newPath) && Path.IsPathRooted(_newPath))
          {
            imageFolder = _newPath;
          }

          // If that didnt work, try to use relative pathing into the skin\Media folder
          if (imageFolder.Trim().Length == 0)
          {
            //Log.Debug("GUIMultiImage - try relative pathing: " + GUIGraphicsContext.Skin + @"\Media\animations\" + _newPath);
            imageFolder = GUIGraphicsContext.Skin + @"\Media\animations\" + _newPath;

            // If the folder doesnt exist, we have an invalid field, exit
            if (!Directory.Exists(imageFolder))
            {
              return;
            }
          }

          // Load the image files
          string[] files = Directory.GetFiles(imageFolder);
          for (int i = 0; i < files.Length; ++i)
          {
            if (MediaPortal.Util.Utils.IsPicture(files[i]))
            {
              _fileList.Add(files[i]);
            }
          }
        }
          // If there was some other error accessing the folder, exit
        catch (UnauthorizedAccessException)
        {
          return;
        }
        catch (IOException)
        {
          return;
        }
      }

      // Randomize or sort our images if necessary
      if (_randomized && _fileList.Count > 1)
      {
        Random_Shuffle(ref _fileList);
      }

      // flag as loaded - no point in antly reloading them
      _directoryLoaded = true;

      for (int i = 0; i < _fileList.Count; i++)
      {
        GUIImage pImage = new GUIImage(ParentID, GetID, _positionX, _positionY, _width, _height, _fileList[i], 0);
        pImage.KeepAspectRatio = _keepAspectRatio;
        _imageList.Add(pImage);
      }

      // Load in the current image, and reset our timer
      _imageTimer.StartZero();
      _fadeTimer.Stop();
      _currentImage = 0;
      if (_imageList.Count == 0)
      {
        return;
      }
      //LoadImage(_currentImage);
      _isAllocated = true;
    }

    // Shuffles inplace
    public void Random_Shuffle(ref List<string> listToShuffle)
    {
      Random r = new Random();
      for (int i = listToShuffle.Count - 1; i > 1; --i)
      {
        int randIndx = r.Next(i);
        string temp = listToShuffle[i];
        listToShuffle[i] = listToShuffle[randIndx]; // move random num to end of list
        listToShuffle[randIndx] = temp;
      }
    }
  }
}