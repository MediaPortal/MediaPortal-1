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
using MediaPortal.GUI.Library;
using MediaPortal.Picture.Database;
using MediaPortal.Util;
using SharpDX.Direct3D9;

#region SlidePicture class

internal class SlidePicture : IDisposable
{
  private const int MAX_PICTURE_WIDTH = 2040;
  private const int MAX_PICTURE_HEIGHT = 2040;

  private Texture _texture;
  private int _width = 0;
  private int _height = 0;
  private int _rotation = 0;

  public string _filePath;
  private bool _useActualSizeTexture;

  private object _syncRoot = new Object();

  public Texture Texture
  {
    get { return _texture; }
  }

  public string FilePath
  {
    get { return _filePath; }
  }

  public bool TrueSizeTexture
  {
    get { return _useActualSizeTexture; }
  }

  public int Width
  {
    get { return _width; }
  }

  public int Height
  {
    get { return _height; }
  }

  public int Rotation
  {
    get { return _rotation; }
  }

  public SlidePicture(string strFilePath, bool useActualSizeTexture)
  {
    lock (_syncRoot)
    {
      _filePath = strFilePath;

      _rotation = PictureDatabase.GetRotation(_filePath);

      int iMaxWidth = GUIGraphicsContext.OverScanWidth;
      int iMaxHeight = GUIGraphicsContext.OverScanHeight;

      _useActualSizeTexture = useActualSizeTexture;
      if (_useActualSizeTexture)
      {
        iMaxWidth = MAX_PICTURE_WIDTH;
        iMaxHeight = MAX_PICTURE_HEIGHT;
      }

      if (!Utils.IsPicture(strFilePath))
      {
        return;
      }
      _texture = Picture.Load(strFilePath, _rotation, iMaxWidth, iMaxHeight, true, false, true, out _width, out _height);
    }
  }

  ~SlidePicture()
  {
    if (_texture != null && !_texture.IsDisposed)
    {
      Log.Warn("[SlidePicture][dtor] Texture dispose call from dtor.");
      this.Dispose();
    }
  }

  public void Dispose()
  {
    if (this._texture != null && !this._texture.IsDisposed)
    {
      this._texture.Dispose();
      this._texture = null;
    }
  }
}

#endregion