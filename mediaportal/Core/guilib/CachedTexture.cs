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
using System.Diagnostics;
using System.Drawing;
using System.Collections.Generic;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Direct3D = Microsoft.DirectX.Direct3D;
using System.Runtime.InteropServices;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A datastructure for caching textures.
  /// This is used by the GUITextureManager which keeps a cache of all textures in use
  /// </summary>
  public class CachedTexture
  {
    #region imports
    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineRemoveTexture(int textureNo);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern int FontEngineAddTexture(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern int FontEngineAddSurface(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEngineDrawTexture(int textureNo, float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color, float[,] matrix);


    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    unsafe private static extern void FontEnginePresentTextures();
    #endregion

    #region events
    public event EventHandler Disposed;
    #endregion

    #region Frame class
    /// <summary>
    /// Class which contains a single frame
    /// A cached texture can contain more then 1 frames for example when its an animated gif
    /// </summary>
    public class Frame
    {
      #region variables
      Texture _image;			//texture of current frame
      int _duration;	//duration of current frame
      int _textureNumber=-1;
      public readonly bool UseNewTextureEngine = true;
      string _imageName = string.Empty;
      private static bool logTextures = false;
      #endregion

      #region events
      public event EventHandler Disposing;
      public event EventHandler Disposed;
      #endregion

      public Frame(string name, Texture image, int duration)
      {

        _imageName = name;
        _image = image;
        _duration = duration;
        if (image != null)
        {
          unsafe
          {
              _image.Disposing += new EventHandler(D3DTexture_Disposing);
              IntPtr ptr = DShowNET.Helper.DirectShowUtil.GetUnmanagedTexture(_image);
            _textureNumber = FontEngineAddTexture(ptr.ToInt32(), true, (void*)ptr.ToPointer());
            if (logTextures) Log.Info("Frame:ctor() fontengine: added texture:{0} {1}", _textureNumber.ToString(), _imageName);
          }
        }
      }
      public string ImageName
      {
        get { return _imageName; }
        set { _imageName = value; }
      }

      /// <summary>
      /// Property to get/set the texture
      /// </summary>
      public Texture Image
      {
        get { return _image; }
        set
        {
          if (_image != null)
          {
            if (Disposing != null) Disposing(this, new EventArgs());
            try
            {
              if (logTextures) Log.Info("Frame:Image fontengine: remove texture:{0} {1}", _textureNumber.ToString(), _imageName);
              FontEngineRemoveTexture(_textureNumber);
              if (!_image.Disposed)
              {
                  _image.Dispose();
              }
              _textureNumber = -1;
            }
            catch (Exception)
            {
              //already disposed?
            }
            if (Disposed != null) Disposed(this, new EventArgs());
          }
          _image = value;

          if (_image != null)
          {
              _image.Disposing += new EventHandler(D3DTexture_Disposing);
            unsafe
            {
              IntPtr ptr = DShowNET.Helper.DirectShowUtil.GetUnmanagedTexture(_image);
              _textureNumber = FontEngineAddTexture(ptr.ToInt32(), true, (void*)ptr.ToPointer());
              if (logTextures) Log.Info("Frame:Image fontengine: added texture:{0} {1}", _textureNumber.ToString(), _imageName);
            }
          }
        }
      }

        void D3DTexture_Disposing(object sender, EventArgs e)
        {
            // D3D has disposed of this texture! notify so that things are kept up to date
            Dispose();
        }

      /// <summary>
      /// property to get/set the duration for this frame
      /// (only useful if the frame belongs to an animation, like an animated gif)
      /// </summary>
      public int Duration
      {
        get { return _duration; }
        set { _duration = value; }
      }
      #region IDisposable Members

      public void Dispose()
      {
        if (_image != null)
        {
          if (Disposing != null) Disposing(this, new EventArgs());
          if (logTextures) Log.Info("Frame: dispose() fontengine: remove texture:" + _textureNumber.ToString());
          try
          {
            if (!_image.Disposed)
            {
                _image.Disposing -= new EventHandler(D3DTexture_Disposing);
                _image.Dispose();
            }
          }
          catch (Exception)
          {
            //image already disposed?
          }
          _image = null;
          if (_textureNumber >= 0)
          {
            FontEngineRemoveTexture(_textureNumber);
            _textureNumber = -1;
          }
          if (Disposed != null) Disposed(this, new EventArgs());
        }
      }

      #endregion

      public int TextureNumber
      {
        get
        {
          return _textureNumber;
        }
      }
      public void Draw(float x, float y, float nw, float nh, float uoff, float voff, float umax, float vmax, int color)
      {
        //string logline=String.Format("draw:#{0} {1} {2} {3} {4}",_textureNumber,x,y,nw,nh);
        //Trace.WriteLine(logline);
        if (_textureNumber >= 0)
        {
          float[,] matrix = GUIGraphicsContext.GetFinalMatrix();
          FontEngineDrawTexture(_textureNumber, x, y, nw, nh, uoff, voff, umax, vmax, color, matrix);
        }
        else
        {
          if (logTextures) Log.Info("fontengine:Draw() ERROR. Texture is disposed:{0} {1}", _textureNumber.ToString(), _imageName);
        }
      }
    }
    #endregion

    #region variables
    string _fileName = "";								// filename of the texture
    List<Frame> _listFrames = new List<Frame>();	  // array to hold all frames
    int _textureWidth = 0;									// width of the texture
    int _textureHeight = 0;								// height of the texture
    int _frameCount = 0;								// number of frames in the animation
    Image _gdiBitmap = null;								// GDI image of the texture
    #endregion

    #region ctor/dtor
    /// <summary>
    /// The (emtpy) constructor of the CachedTexture class.
    /// </summary>
    public CachedTexture()
    {
    }
    #endregion


    #region properties
    /// <summary>
    /// Get/set the filename/location of the texture.
    /// </summary>
    public string Name
    {
      get { return _fileName; }
      set { _fileName = value; }
    }

    /// <summary>
    /// Get/set the DirectX texture for the 1st frame
    /// </summary>
    public Frame texture
    {
      get
      {
        if (_listFrames.Count == 0) return null;
        return _listFrames[0];
      }
      set
      {
        Dispose();      // cleanup..
        _listFrames.Add(value);
        value.Disposed += new EventHandler(frame_Disposed);
      }
    }

      void frame_Disposed(object sender, EventArgs e)
      {
          // AB - if for some reason we get this event, it means D3D has released the texture in one Frame. In that case, just dispose the whole CachedTexture
          Dispose();
      }

    /// <summary>
    /// Get/set the GDI Image 
    /// </summary>
    public Image image
    {
      get { return _gdiBitmap; }
      set
      {
        if (_gdiBitmap != null)
        {
          try
          {
            _gdiBitmap.Dispose();
          }
          catch (Exception)
          {
            //already disposed?
          }
        }
        _gdiBitmap = value;
      }
    }

    /// <summary>
    /// Get/set the width of the texture.
    /// </summary>
    public int Width
    {
      get { return _textureWidth; }
      set { _textureWidth = value; }
    }

    /// <summary>
    /// Get/set the height of the texture.
    /// </summary>
    public int Height
    {
      get { return _textureHeight; }
      set { _textureHeight = value; }
    }

    /// <summary>
    /// Get/set the number of frames out of which the texture exists.
    /// </summary>
    public int Frames
    {
      get { return _frameCount; }
      set { _frameCount = value; }
    }

    /// <summary>
    /// indexer to get a Frame or to set a Frame
    /// </summary>
    public Frame this[int index]
    {
      get
      {
        if (index < 0 || index >= _listFrames.Count) return null;
        return _listFrames[index];
      }
      set
      {
        if (index < 0) return;

        if (_listFrames.Count <= index)
        {
            _listFrames.Add(value);
            value.Disposed += new EventHandler(frame_Disposed);
        }
        else
        {
            Frame frame = _listFrames[index];
            if (frame != value)
            {
                frame.Disposed -= new EventHandler(frame_Disposed);
                frame.Dispose();
                _listFrames[index] = value;
                value.Disposed += new EventHandler(frame_Disposed);
            }
        }
      }
    }
    #endregion

    #region IDisposable Members
    /// <summary>
    /// Releases the resources used by the texture.
    /// </summary>
    public void Dispose()
    {

      foreach (Frame tex in _listFrames)
      {
        if (tex != null)
        {
          tex.Disposed -= new EventHandler(frame_Disposed);
          tex.Dispose();
        }
      }
      _listFrames.Clear();
      if (_gdiBitmap != null)
      {
        try
        {
          _gdiBitmap.Dispose();
        }
        catch (Exception)
        {
          //already disposed?
        }
        _gdiBitmap = null;
      }
      if (Disposed != null) Disposed(this, new EventArgs());
    }
    #endregion
  }
}
