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
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using MediaPortal.Util;
using Microsoft.DirectX.Direct3D;


namespace MediaPortal.GUI.Library
{
  public class GUIMultiImage : GUIControl
  {
    #region variables
    [XMLSkinElement("imagepath")]
    string m_texturePath;
    [XMLSkinElement("timeperimage")]
    uint m_timePerImage;
    [XMLSkinElement("fadetime")]
    uint m_fadeTime;
    [XMLSkinElement("randomize")]
    bool m_randomized;
    [XMLSkinElement("loop")]
    bool m_loop;
    [XMLSkinElement("keepaspectratio")]
    private bool _keepAspectRatio = false;

    List<GUIImage> m_images = new List<GUIImage>();
    List<string> m_files = new List<string>();
    bool m_directoryLoaded;
    bool _isAllocated;
    int m_Info;
    string m_currentPath;
    int m_currentImage;
    StopWatch m_imageTimer = new StopWatch();
    StopWatch m_fadeTimer = new StopWatch();
    #endregion

    #region ctor

    public GUIMultiImage(int dwParentID)
      : base(dwParentID)
    {
    }

    public GUIMultiImage(int dwParentID, int dwControlId, int iPosX, int iPosY, int dwWidth, int dwHeight, string strTexturePath, uint timePerImage, uint fadeTime, bool randomized, bool loop)
      : base(dwParentID, dwControlId, iPosX, iPosY, dwWidth, dwHeight)
    {
      FinalizeConstruction();
      m_timePerImage = timePerImage;
      m_fadeTime = fadeTime;
      m_randomized = randomized;
      m_loop = loop;
      _keepAspectRatio = false;
      m_currentPath = m_texturePath = strTexturePath;
    }
    /// <summary>
    /// This method gets called when the control is created and all properties has been set
    /// It allows the control todo any initialization
    /// </summary>
    public override void FinalizeConstruction()
    {
      base.FinalizeConstruction();

    }
    #endregion

    #region properties
    public string TexturePath
    {
      get
      {
        return m_texturePath;
      }
      set
      {
        m_texturePath = value;
      }
    }
    public uint TimePerImage
    {
      get
      {
        return m_timePerImage;
      }
      set
      {
        m_timePerImage = value;
      }
    }
    public uint FadeTime
    {
      get
      {
        return m_fadeTime;
      }
      set
      {
        m_fadeTime = value;
      }
    }
    public bool Randomized
    {
      get
      {
        return m_randomized;
      }
      set
      {
        m_randomized = value;
      }
    }
    public bool Loop
    {
      get
      {
        return m_loop;
      }
      set
      {
        m_loop = value;
      }
    }

    public int Info
    {
      get
      {
        return m_Info;
      }
      set
      {
        m_Info = value;
      }
    }
    #endregion

    public override void Render(float timePassed)
    {
      if (!IsVisible)
      {
        base.Render(timePassed);
        return;
      }

      // check for conditional information before we
      // alloc as this can free our resources
      if (m_Info != 0)
      {
        string texturePath = "";// g_infoManager.GetImage(m_Info, WINDOW_INVALID);
        if (texturePath != m_currentPath && texturePath.Length != 0)
        {
          m_currentPath = texturePath;
          FreeResources();
          LoadDirectory();
        }
        else if (texturePath.Length == 0 && m_currentPath != m_texturePath)
        {
          m_currentPath = m_texturePath;
          FreeResources();
          LoadDirectory();
        }
      }

      if (!_isAllocated)
        AllocResources();

      if (m_images.Count != 0)
      {
        // Set a viewport so that we don't render outside the defined area
        Viewport oldview = GUIGraphicsContext.DX9Device.Viewport;
        Viewport view = new Viewport();
        view.X = _positionX;
        view.Y = _positionY;
        view.Width = _width;
        view.Height = _height;
        view.MinZ = 0.0f;
        view.MaxZ = 1.0f;
        GUIGraphicsContext.DX9Device.Viewport = view;
        m_images[m_currentImage].Render(timePassed);

        int nextImage = m_currentImage + 1;
        if (nextImage >= m_images.Count)
          nextImage = m_loop ? 0 : m_currentImage;  // stay on the last image if <loop>no</loop>

        //Log.WriteFile(MediaPortal.Services.LogType.Log, "next:{0} current:{1}", nextImage, m_currentImage);
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
            //Log.WriteFile(MediaPortal.Services.LogType.Log, "start fade");
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
              //Log.WriteFile(MediaPortal.Services.LogType.Log, "fade end. current:{0}", m_currentImage);
            }
            else
            { // perform the fade
              float fadeAmount = timeFading / ((float)m_fadeTime);
              long alpha = (int)(255 * fadeAmount);
              alpha <<= 24;
              alpha += (m_images[nextImage].ColourDiffuse & 0x00ffffff);
              m_images[nextImage].ColourDiffuse = alpha;
              //Log.WriteFile(MediaPortal.Services.LogType.Log, "fade :{0:X}", alpha);
            }
            m_images[nextImage].Render(timePassed);
          }
        }
        GUIGraphicsContext.DX9Device.Viewport = oldview;
      }
      base.Render(timePassed);
    }

    public override bool OnMessage(GUIMessage message)
    {
      /*if (message.Message == GUI_MSG_REFRESH_THUMBS)
      {
        if (GetInfo())
          FreeResources();
        return true;
      }*/
      return base.OnMessage(message);
    }

    public override void PreAllocResources()
    {
      FreeResources();
    }

    public override void AllocResources()
    {
      m_currentImage = 0;

      m_Info = 0;
      m_directoryLoaded = false;
      _isAllocated = false;
      m_currentPath = m_texturePath;
      FreeResources();
      base.AllocResources();

      if (!m_directoryLoaded)
        LoadDirectory();

      // Randomize or sort our images if necessary
      if (m_randomized && m_files.Count > 1)
        Random_Shuffle(ref m_files);

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
        return;

      LoadImage(m_currentImage);
      _isAllocated = true;
    }

    public void LoadImage(int image)
    {
      if (image < 0 || image >= (int)m_images.Count)
        return;

      m_images[image].AllocResources();
      m_images[image].ColourDiffuse = ColourDiffuse;

      // Scale image so that it will fill our render area
      if (_keepAspectRatio)
      {
        // image is scaled so that the aspect ratio is maintained (taking into account the TV pixel ratio)
        // and so that it fills the allocated space (so is zoomed then cropped)
        float sourceAspectRatio = (float)m_images[image].TextureWidth / m_images[image].TextureHeight;
        float aspectRatio = sourceAspectRatio / GUIGraphicsContext.PixelRatio;

        int newWidth = _width;
        int newHeight = (int)((float)newWidth / aspectRatio);
        if ((_keepAspectRatio == false && newHeight < _height) ||
            (_keepAspectRatio && newHeight > _height))
        {
          newHeight = _height;
          newWidth = (int)((float)newHeight * aspectRatio);
        }
        m_images[image].SetPosition(_positionX - (int)(newWidth - _width) / 2, _positionY - (int)(newHeight - _height) / 2);
        m_images[image].Width = newWidth;
        m_images[image].Height = newHeight;
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
      get
      {
        return _keepAspectRatio;
      }
      set
      {
        if (_keepAspectRatio != value)
        {
          _keepAspectRatio = value;
          //m_bInvalidated = true;
        }
      }
    }

    public void LoadDirectory()
    {
      // Load any images from our texture bundle first
      m_files.Clear();

      // don't load any images if our path is empty
      if (m_currentPath.Length == 0)
        return;

      // check to see if we have a single image or a folder of images
      if (MediaPortal.Util.Utils.IsPicture(m_currentPath))
      {
        m_files.Add(m_currentPath);
      }
      else
      { // folder of images
        try
        {
          string imageFolder = string.Empty;
          // try to use the provided folder as an absolute path
          if (Directory.Exists(m_currentPath) && Path.IsPathRooted(m_currentPath))
            imageFolder = m_currentPath;

          // if that didnt work, try to use relative pathing into the skin\media folder
          if (imageFolder.Trim().Length == 0)
          {
            imageFolder = GUIGraphicsContext.Skin + @"\media\animations\" + m_currentPath;
            // if the folder doesnt exist, we have an invalid field, exit
            if (!Directory.Exists(imageFolder))
              return;
          }

          // load the image files
          string[] files = Directory.GetFiles(imageFolder);
          for (int i = 0; i < files.Length; ++i)
          {
            if (MediaPortal.Util.Utils.IsPicture(files[i]))
            {
              m_files.Add(files[i]);
            }
          }
        }
        // if there was some other error accessing the folder, quit
        catch (UnauthorizedAccessException)
        {
          return;
        }
        catch (IOException)
        {
          return;
        }
      }

      // sort our images - they'll be randomized in AllocResources() if necessary
      //sort(m_files.begin(), m_files.end());

      // flag as loaded - no point in antly reloading them
      m_directoryLoaded = true;
    }

    // Shuffles inplace.
    public void Random_Shuffle(ref List<string> listToShuffle)
    {
      Random r = new Random();
      for (int i = listToShuffle.Count - 1; i > 1; --i)
      {
        int randIndx = r.Next(i);
        string temp = listToShuffle[i];
        listToShuffle[i] = listToShuffle[randIndx]; // move random num to end of list.
        listToShuffle[randIndx] = temp;
      }
    }

  }
}
