#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *      Copyright (C) 2005-2009 Team MediaPortal
 *      http://www.team-mediaportal.com
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
using System.Collections.Generic;
using System.IO;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A GUIControl for displaying MultiImages.
  /// </summary>
  public class GUIMultiImage : GUIControl
  {
    #region variables

    [XMLSkinElement("imagepath")] private string m_texturePath = "";
    [XMLSkinElement("timeperimage")] private uint m_timePerImage;
    [XMLSkinElement("fadetime")] private uint m_fadeTime;
    [XMLSkinElement("randomize")] private bool m_randomized;
    [XMLSkinElement("loop")] private bool m_loop;
    [XMLSkinElement("keepaspectratio")] private bool m_keepAspectRatio;
    [XMLSkinElement("posX")] private int m_posx = 0;
    [XMLSkinElement("posY")] private int m_posy = 0;
    [XMLSkinElement("width")] private int m_width = 0;
    [XMLSkinElement("height")] private int m_height = 0;

    private List<GUIImage> m_images = new List<GUIImage>();
    private List<string> m_files = new List<string>();
    private bool m_directoryLoaded;
    private bool _isAllocated;
    private int m_Info;
    private int m_currentImage;
    private StopWatch m_imageTimer = new StopWatch();
    private StopWatch m_fadeTimer = new StopWatch();
    private bool _registeredForEvent = false;
    private bool _containsProperty = false;
    private bool _propertyChanged = false;
    private string m_cachedPath = "";
    private string newPath;

    #endregion

    #region ctor

    private GUIMultiImage()
    {
    }

    public GUIMultiImage(int dwParentID)
      : base(dwParentID)
    {
    }


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
    /// <param name="timePerImage">time in miliseconds that each image should be shown.</param>
    /// <param name="fadeTime">fadetime in miliseconds between visiblilitystates.</param>
    /// <param name="randomized">indicates if the images should be randomized.</param>
    /// <param name="loop">indicates if the image rotation should be looped.</param>
    public GUIMultiImage(int dwParentID, int dwControlId, int iPosX, int iPosY, int dwWidth, int dwHeight,
                         string strTexturePath, uint timePerImage, uint fadeTime, bool randomized, bool loop)
      : base(dwParentID, dwControlId, iPosX, iPosY, dwWidth, dwHeight)
    {
      m_timePerImage = timePerImage;
      m_fadeTime = fadeTime;
      m_randomized = randomized;
      m_loop = loop;
      m_keepAspectRatio = false;
      m_posx = iPosX;
      m_posy = iPosY;
      m_width = dwWidth;
      m_height = dwHeight;
      //Log.Debug("GUIMultiImage - " + strTexturePath);
      newPath = strTexturePath;

      FinalizeConstruction();
    }

    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();
      if (m_texturePath == null)
      {
        m_texturePath = string.Empty;
      }

      if (m_texturePath.Length >= 0)
      {
        //Log.Debug("GUIMultiImage - contains property: " + m_texturePath);
        _containsProperty = true;
      }

      m_cachedPath = m_texturePath;
      _propertyChanged = true;
    }

    #endregion

    #region properties

    /// <summary>
    /// Get/Set the TexturePath
    /// </summary>
    public string TexturePath
    {
      get { return m_texturePath; }
      set { m_texturePath = value; }
    }

    public uint TimePerImage
    {
      get { return m_timePerImage; }
      set { m_timePerImage = value; }
    }

    public uint FadeTime
    {
      get { return m_fadeTime; }
      set { m_fadeTime = value; }
    }

    public bool Randomized
    {
      get { return m_randomized; }
      set { m_randomized = value; }
    }

    public bool Loop
    {
      get { return m_loop; }
      set { m_loop = value; }
    }

    public new int Info
    {
      get { return m_Info; }
      set { m_Info = value; }
    }

    #endregion

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
        newPath = GUIPropertyManager.Parse(m_texturePath);

        if (m_cachedPath != newPath)
        {
          if (newPath == null)
          {
            newPath = "";
          }
          m_cachedPath = newPath;

        }
        if (m_cachedPath == null)
        {
          base.Render(timePassed);
          return;
        }

        if (m_cachedPath.IndexOf("#") >= 0)
        {
          base.Render(timePassed);
          return;
        }
        //Log.Debug("GUIMultiImage - LoadDirectory(): " + newPath);
        FreeResources();
        LoadDirectory();
      }


      if (!_isAllocated)
      {
        //Log.Debug("GUIMultiImage - not allocated, AllocResources()");
        AllocResources();
      }

      if (m_images.Count != 0)
      {
        // Set a viewport so that we don't render outside the defined area
        Viewport oldview = GUIGraphicsContext.DX9Device.Viewport;
        Viewport view = new Viewport();
        view.X = m_posx;
        view.Y = m_posy;
        view.Width = m_posx + m_width;
        view.Height = m_posy + m_height;
        view.MinZ = 0.0f;
        view.MaxZ = 1.0f;
        GUIGraphicsContext.DX9Device.Viewport = view;
        m_images[m_currentImage].Render(timePassed);

        int nextImage = m_currentImage + 1;
        if (nextImage >= m_images.Count)
        {
          nextImage = m_loop ? 0 : m_currentImage; // stay on the last image if <loop>no</loop>
        }

        if (nextImage != m_currentImage)
        {
          // check if we should be loading a new image yet
          if (m_imageTimer.IsRunning && m_imageTimer.ElapsedMilliseconds > m_timePerImage)
          {
            m_imageTimer.Stop();
            // grab a new image
            LoadImage(nextImage);
            // start the fade timer
            m_fadeTimer.StartZero();
          }

          // check if we are still fading
          if (m_fadeTimer.IsRunning)
          {
            // check if the fade timer has run out
            float timeFading = m_fadeTimer.ElapsedMilliseconds;
            if (timeFading > m_fadeTime)
            {
              m_fadeTimer.Stop();
              // swap images
              m_images[m_currentImage].FreeResources();
              m_images[nextImage].ColourDiffuse = (m_images[nextImage].ColourDiffuse | (long)0xff000000);
              m_currentImage = nextImage;
              // start the load timer
              m_imageTimer.StartZero();
            }
            else
            {
              // perform the fade
              float fadeAmount = timeFading / ((float)m_fadeTime);
              long alpha = (int)(255 * fadeAmount);
              alpha <<= 24;
              alpha += (m_images[nextImage].ColourDiffuse & 0x00ffffff);
              m_images[nextImage].ColourDiffuse = alpha;
            }
            m_images[nextImage].Render(timePassed);
          }
        }
        GUIGraphicsContext.DX9Device.Viewport = oldview;
      }
      base.Render(timePassed);
    }

    public override void PreAllocResources()
    {
      FreeResources();
    }

    public override void AllocResources()
    {
      //Log.Debug("GUIMultiImage - AllocResources() run");
      _propertyChanged = true;

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
          GUIPropertyManager.OnPropertyChanged +=
            new GUIPropertyManager.OnPropertyChangedHandler(GUIPropertyManager_OnPropertyChanged);
          _registeredForEvent = true;
        }
        m_currentImage = 0;
        m_Info = 0;
        m_directoryLoaded = false;
        _isAllocated = false;
        FreeResources();
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

      if (newPath != null)
      {
        LoadDirectory();
      }

    }

    public void LoadImage(int image)
    {
      if (image < 0 || image >= (int)m_images.Count)
      {
        return;
      }

      m_images[image].AllocResources();
      m_images[image].ColourDiffuse = ColourDiffuse;
      float realHeight = ((float)GUIGraphicsContext.Height / GUIGraphicsContext.SkinSize.Height);
      float realWidth = ((float)GUIGraphicsContext.Width / GUIGraphicsContext.SkinSize.Width);
      int m_height2 = (int)(m_height * realHeight);
      int m_width2 = (int)(m_width * realWidth);
      int newHeight2 = (int)(m_height * realHeight);
      int newWidth2 = (int)(m_width * realWidth);
      int m_posx2 = m_posx;
      int m_posy2 = m_posy;
      int _positionX = 0;
      int _positionY = 0;

      if (m_posx != 0) // No need to try and recalculate positioning if it's 0 anyways
      {
        m_posx2 = (int)(m_posx * realHeight);
      }

      if (m_posy != 0) // No need to try and recalculate positioning if it's 0 anyways
      {
        m_posy2 = (int)(m_posy * realWidth);
      }

      if (m_keepAspectRatio)
      {
        // Scale image so that the aspect ratio is maintained (taking into account the TV pixel ratio)
        float sourceAspectRatio = (float)m_images[image].TextureWidth / m_images[image].TextureHeight;
        float aspectRatio = sourceAspectRatio / GUIGraphicsContext.PixelRatio;

        int aspH = (int)((float)newWidth2 / aspectRatio);
        if (aspH > m_height2)
        {
          newHeight2 = m_height2;
          newWidth2 = (int)((float)newHeight2 * aspectRatio);
          _positionX = m_posx2 - (int)(newWidth2 - m_width2) / 2;
          _positionY = m_posy2 - (int)(newHeight2 - m_height2) / 2;
        }
        else
        {
          newHeight2 = (int)((float)newWidth2 / aspectRatio);
          _positionX = m_posx2 - (int)(newWidth2 - m_width2) / 2;
          _positionY = m_posy2 - (int)(newHeight2 - m_height2) / 2;
        }
      }
      m_images[image].SetPosition(_positionX, _positionY);
      m_images[image].Width = newWidth2;
      m_images[image].Height = newHeight2;
    }

    private void GUIPropertyManager_OnPropertyChanged(string tag, string tagValue)
    {
      //Log.Debug("GUIMultiImage - GUIPropertyManager_OnPropertyChanged, tag: " + tag + ", value: " + tagValue + " texture path: " + m_texturePath);

      if (!_containsProperty)
      {
        return;
      }
      if (m_texturePath.IndexOf(tag) >= 0)
      {
        _propertyChanged = true;
      }
    }

    public override void FreeResources()
    {
      for (int i = 0; i < m_images.Count; ++i)
      {
        m_images[i].FreeResources();
      }

      m_images.Clear();
      m_currentImage = 0;
      base.FreeResources();
    }

    public override bool CanFocus()
    {
      return false;
    }

    public bool KeepAspectRatio
    {
      get { return m_keepAspectRatio; }
      set
      {
        if (m_keepAspectRatio != value)
        {
          m_keepAspectRatio = value;
        }
      }
    }

    public void LoadDirectory()
    {
      //Log.Debug("GUIMultiImage - LoadDirectory: " + newPath);

      // Unload any images from our texture bundle first
      m_files.Clear();

      // Don't load any images if our path is empty
      if (newPath.Length == 0)
      {
        return;
      }

      // Check to see if we have a single image or a folder of images
      if (MediaPortal.Util.Utils.IsPicture(newPath))
      {
        m_files.Add(newPath);
      }
      else
      {
        // Folder of images
        try
        {
          string imageFolder = string.Empty;
          // Try to use the provided folder as an absolute path
          if (Directory.Exists(newPath) && Path.IsPathRooted(newPath))
          {
            //Log.Debug("GUIMultiImage - absolute path found: " + newPath);
            imageFolder = newPath;
          }

          // If that didnt work, try to use relative pathing into the skin\Media folder
          if (imageFolder.Trim().Length == 0)
          {
            //Log.Debug("GUIMultiImage - try relative pathing: " + GUIGraphicsContext.Skin + @"\Media\animations\" + newPath);
            imageFolder = GUIGraphicsContext.Skin + @"\Media\animations\" + newPath;

            // If the folder doesnt exist, we have an invalid field, exit
            if (!Directory.Exists(imageFolder))
            {
              //Log.Debug("GUIMultiImage - location " + newPath + "doesn't exist");
              return;
            }
          }

          // Load the image files
          string[] files = Directory.GetFiles(imageFolder);
          for (int i = 0; i < files.Length; ++i)
          {
            if (MediaPortal.Util.Utils.IsPicture(files[i]))
            {
              m_files.Add(files[i]);
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
      if (m_randomized && m_files.Count > 1)
      {
        Random_Shuffle(ref m_files);
      }

      // flag as loaded - no point in antly reloading them
      m_directoryLoaded = true;

      for (int i = 0; i < m_files.Count; i++)
      {
        GUIImage pImage = new GUIImage(ParentID, GetID, _positionX, _positionY, _width, _height, m_files[i], 0);
        m_images.Add(pImage);
      }

      // Load in the current image, and reset our timer
      m_imageTimer.StartZero();
      m_fadeTimer.Stop();
      m_currentImage = 0;
      if (m_images.Count == 0)
      {
        return;
      }
      LoadImage(m_currentImage);
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