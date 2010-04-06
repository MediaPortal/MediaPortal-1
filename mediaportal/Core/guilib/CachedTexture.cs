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
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using DShowNET.Helper;
using Microsoft.DirectX.Direct3D;
using System.Runtime.CompilerServices;

namespace MediaPortal.GUI.Library
{
  /// <summary>
  /// A datastructure for caching textures.
  /// This is used by the GUITextureManager which keeps a cache of all textures in use
  /// </summary>
  public class CachedTexture : IDisposable
  {
    #region imports

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineRemoveTexture(int textureNo);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe int FontEngineAddTexture(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe int FontEngineAddSurface(int hasCode, bool useAlphaBlend, void* fontTexture);

    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEngineDrawTexture(int textureNo, float x, float y, float nw, float nh,
                                                            float uoff, float voff, float umax, float vmax, int color,
                                                            float[,] matrix);


    [DllImport("fontEngine.dll", ExactSpelling = true, CharSet = CharSet.Auto, SetLastError = true)]
    private static extern unsafe void FontEnginePresentTextures();

    #endregion

    #region events / delegates

    public event EventHandler Disposed;

    #endregion

    #region Frame class

    /// <summary>
    /// Class which contains a single frame
    /// A cached texture can contain more then 1 frames for example when its an animated gif
    /// </summary>
    public class Frame : IDisposable
    {
      #region variables

      // Track whether Dispose has been called.
      private bool disposed = false;
      private Texture _image; //texture of current frame
      private int _duration; //duration of current frame
      private int _textureNumber = -1;
      public readonly bool UseNewTextureEngine = true;
      private string _imageName = string.Empty;
      private static bool logTextures = false;

      #endregion

      #region events

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
            IntPtr ptr = DirectShowUtil.GetUnmanagedTexture(_image);
            _textureNumber = FontEngineAddTexture(ptr.ToInt32(), true, (void*)ptr.ToPointer());
            if (logTextures)
            {
              Log.Info("Frame:ctor() fontengine: added texture:{0} {1}", _textureNumber.ToString(), _imageName);
            }
          }
        }
      }

      ~Frame()
      {      
        // call Dispose with false.  Since we're in the
        // destructor call, the managed resources will be
        // disposed of anyways.
        Dispose(false);
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

            try
            {
              if (logTextures)
              {
                Log.Info("Frame:Image fontengine: remove texture:{0} {1}", _textureNumber.ToString(), _imageName);
              }              
              Dispose();                            
            }
            catch (Exception)
            {
              //already disposed?
            }           
          }
          _image = value;

          if (_image != null)
          {
            _image.Disposing += new EventHandler(D3DTexture_Disposing);
            unsafe
            {
              IntPtr ptr = DirectShowUtil.GetUnmanagedTexture(_image);
              _textureNumber = FontEngineAddTexture(ptr.ToInt32(), true, (void*)ptr.ToPointer());
              if (logTextures)
              {
                Log.Info("Frame:Image fontengine: added texture:{0} {1}", _textureNumber.ToString(), _imageName);
              }
            }
          }
        }
      }

      private void D3DTexture_Disposing(object sender, EventArgs e)
      {
        // D3D has disposed of this texture! notify so that things are kept up to date
        if (GUIGraphicsContext.CurrentState == GUIGraphicsContext.State.RUNNING)
        {
          Dispose();
        }
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

      /// <summary>
      /// Releases the resources used by the texture.
      /// </summary>
      public void Dispose()
      {        
        Dispose(true);
        // This object will be cleaned up by the Dispose method.
        // Therefore, calling GC.SupressFinalize to take this object off
        // the finalization queue and prevent finalization code for this object
        // from executing a second time.
        GC.SuppressFinalize(this);
      }
      
      private void Dispose(bool disposeManagedResources)
      {              
        // process only if mananged and unmanaged resources have
        // not been disposed of.      
        if (!this.disposed)
        {
          lock (this)
          {
            DisposeUnmanagedResources();
          }                                      
          if (Disposed != null)
          {
            Disposed(this, new EventArgs());
          }
          disposed = true;
        }
      }

      private void DisposeUnmanagedResources()
      {
        //Log.Debug("Frame.DisposeUnmanagedResources mainthread: {0}", !(GUIGraphicsContext.form.InvokeRequired));
        if (_image != null)
        {
          if (logTextures)
          {
            Log.Info("Frame: dispose() fontengine: remove texture:" + _textureNumber.ToString());
          }
          try
          {
            if (_textureNumber >= 0)
            {
              FontEngineRemoveTexture(_textureNumber);
              _textureNumber = -1;
            }

            if (_image != null && !_image.Disposed)
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
        }
      }

      #endregion

      public int TextureNumber
      {
        get { return _textureNumber; }
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
          if (logTextures)
          {
            Log.Info("fontengine:Draw() ERROR. Texture is disposed:{0} {1}", _textureNumber.ToString(), _imageName);
          }
        }
      }

      public void Draw(float x, float y, float nw, float nh, float zrot, float uoff, float voff, float umax, float vmax,
                       int color)
      {
        if (_textureNumber >= 0)
        {
          // Rotate around the x,y point of the specified rectangle; maintain aspect ratio (1.0f)
          TransformMatrix localTransform = new TransformMatrix();
          localTransform.SetZRotation(zrot, x, y, 1.0f);
          TransformMatrix finalTransform = GUIGraphicsContext.GetFinalTransform();
          localTransform = finalTransform.multiply(localTransform);

          FontEngineDrawTexture(_textureNumber, x, y, nw, nh, uoff, voff, umax, vmax, color, localTransform.Matrix);
        }
        else
        {
          if (logTextures)
          {
            Log.Info("fontengine:Draw() ERROR. Texture is disposed:{0} {1}", _textureNumber.ToString(), _imageName);
          }
        }
      }
    }

    #endregion

    #region variables

    // Track whether Dispose has been called.
    private bool disposed = false;
    private string _fileName = ""; // filename of the texture
    private List<Frame> _listFrames = new List<Frame>(); // array to hold all frames
    private int _textureWidth = 0; // width of the texture
    private int _textureHeight = 0; // height of the texture
    private int _frameCount = 0; // number of frames in the animation
    private Image _gdiBitmap = null; // GDI image of the texture

    #endregion

    #region ctor/dtor

    /// <summary>
    /// The (emtpy) constructor of the CachedTexture class.
    /// </summary>
    public CachedTexture() {}

    ~CachedTexture()
    {      
      // call Dispose with false.  Since we're in the
      // destructor call, the managed resources will be
      // disposed of anyways.
      Dispose(false);
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
        if (_listFrames.Count == 0)
        {
          return null;
        }
        return _listFrames[0];
      }
      set
      {
        Dispose(); // cleanup..
        _listFrames.Clear();
        value.Disposed += new EventHandler(frame_Disposed);
        _listFrames.Add(value);        
      }
    }

    private void frame_Disposed(object sender, EventArgs e)
    {
      // D3D has released the texture in one Frame. In that case, just dispose the whole CachedTexture
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
        if (index < 0 || index >= _listFrames.Count)
        {
          return null;
        }
        return _listFrames[index];
      }
      set
      {
        if (index < 0)
        {
          return;
        }

        if (_listFrames.Count <= index)
        {
          value.Disposed += new EventHandler(frame_Disposed);
          _listFrames.Add(value);          
        }
        else
        {
          Frame frame = _listFrames[index];
          if (frame != value)
          {
            frame.Disposed -= new EventHandler(frame_Disposed);
            frame.Dispose();
            value.Disposed += new EventHandler(frame_Disposed);
            _listFrames[index] = value;            
          }
        }
      }
    }

    #endregion

    #region IDisposable Members

    private void Dispose(bool disposeManagedResources)
    {
      // process only if mananged and unmanaged resources have
      // not been disposed of.      
      lock (this)
      {
        if (!this.disposed)
        {
          DisposeUnmanagedResources();
        }
      }

      DisposeFrames();//somehow we need to call this always, regardless of state 'this.disposed', otherwise we leak resources.      

      if (Disposed != null)
      {
        Disposed(this, new EventArgs());
      }
      this.disposed = true;
    }

    private void DisposeFrames() {
      foreach (Frame tex in _listFrames)
      {
        if (tex != null)
        {
          tex.Disposed -= new EventHandler(frame_Disposed);
          tex.Dispose();
        }
      }
      _listFrames.Clear();
    }

    private void DisposeUnmanagedResources() {             
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
    }


    /// <summary>
    /// Releases the resources used by the texture.
    /// </summary>
    public void Dispose()
    {      
      Dispose(true);
      // This object will be cleaned up by the Dispose method.
      // Therefore, calling GC.SupressFinalize to take this object off
      // the finalization queue and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize(this);      
    }

    #endregion
  }
}