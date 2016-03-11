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
using MediaPortal.ExtensionMethods;

namespace MediaPortal.guilib
{
  /// <summary>
  /// A datastructure for caching textures.
  /// This is used by the GUITextureManager which keeps a cache of all textures in use
  /// </summary>
  internal class CachedTexture : IDisposable
  {
    public event EventHandler Disposed;

    private readonly List<TextureFrame> _frames = new List<TextureFrame>(); // array to hold all frames
    
    private bool _disposed = false;           // track whether Dispose has been called.
    private string _fileName = "";            // filename of the texture
    private int _textureWidth = 0;            // width of the texture
    private int _textureHeight = 0;           // height of the texture
    private int _frameCount = 0;              // number of frames in the animation

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
    public TextureFrame Texture
    {
      get
      {
        if (_frames.Count == 0)
        {
          return null;
        }
        return _frames[0];
      }
      set
      {
        DisposeTextureFrames();
        value.Disposed += new EventHandler(TextureFrame_Disposed);
        _frames.Add(value);
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
    public TextureFrame this[int index]
    {
      get
      {
        if (index < 0 || index >= _frames.Count)
        {
          return null;
        }
        return _frames[index];
      }
      set
      {
        if (index < 0)
        {
          return;
        }

        if (_frames.Count <= index)
        {
          value.Disposed += new EventHandler(TextureFrame_Disposed);
          _frames.Add(value);
        }
        else
        {
          TextureFrame frame = _frames[index];
          if (frame != value)
          {
            frame.Disposed -= new EventHandler(TextureFrame_Disposed);
            frame.SafeDispose();
            value.Disposed += new EventHandler(TextureFrame_Disposed);
            _frames[index] = value;
          }
        }
      }
    }

    /// <summary>
    /// Releases the resources used by the texture.
    /// </summary>
    public void Dispose()
    {
      DisposeInternal();
      // This object will be cleaned up by the Dispose method.
      // Therefore, calling GC.SupressFinalize to take this object off
      // the finalization queue and prevent finalization code for this object
      // from executing a second time.
      GC.SuppressFinalize(this);
    }

    private void DisposeInternal()
    {
      if (_disposed)
      {
        return;
      }

      DisposeTextureFrames();

      _disposed = true;

      if (Disposed != null)
      {
        Disposed(this, new EventArgs());

        if (Disposed != null)
        {
          foreach (EventHandler eventDelegate in Disposed.GetInvocationList())
          {
            Disposed -= eventDelegate;
          }
        }
      }
    }

    private void DisposeTextureFrames()
    {
      foreach (TextureFrame frame in _frames)
      {
        if (frame != null)
        {
          frame.Disposed -= new EventHandler(TextureFrame_Disposed);
          frame.SafeDispose();
        }
      }
      _frames.Clear();
    }

    private void TextureFrame_Disposed(object sender, EventArgs e)
    {
      // D3D has released the texture in one Frame. In that case, just dispose the whole CachedTexture
      Dispose();
    }
  }
}