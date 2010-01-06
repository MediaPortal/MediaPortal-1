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

namespace System.Windows.Media.Imaging
{
  public abstract class BitmapSource : MediaPortal.Drawing.ImageSource
  {
    #region Constructors

    public BitmapSource() {}

    #endregion Constructors

    #region Events

    // TODO: MSDN docs show these are virtual
    public abstract event EventHandler DownloadCompleted;
    public abstract event DownloadProgressEventHandler DownloadProgress;

    #endregion Events

    #region Methods

    public new BitmapSource Copy()
    {
      return (BitmapSource)base.Copy();
    }

//		public virtual void CopyPixels(Array pixels, int stride, int offset)
//		{
//			throw new NotImplementedException();
//		}

//		public virtual void CopyPixels(Int32Rect sourceRect, Array pixels, int stride, int offset)
//		{
//			throw new NotImplementedException();
//		}

//		public virtual void CopyPixels(Int32Rect sourceRect, IntPtr buffer, int bufferSize, int stride)
//		{
//			throw new NotImplementedException();
//		}

//		public static BitmapSource Create(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette palette, Array pixels, int stride)
//		{
//			throw new NotImplementedException();
//		}

//		public static BitmapSource Create(int pixelWidth, int pixelHeight, double dpiX, double dpiY, PixelFormat pixelFormat, BitmapPalette palette, IntPtr buffer, int bufferSize, int stride)
//		{
//			throw new NotImplementedException();
//		}

    protected override bool FreezeCore(bool isChecking)
    {
      throw new NotImplementedException();
    }

    public new BitmapSource GetCurrentValue()
    {
      return this;
    }

    #endregion Methods

    #region Properties

//		public virtual ColorContext ColorContext
//		{
//			get { throw new NotImplementedException(); }
//			set { throw new NotImplementedException(); }
//		}

    public virtual double DpiX
    {
      get { return _dpiX; }
    }

    public virtual double DpiY
    {
      get { return _dpiY; }
    }

//		public virtual PixelFormat Format
//		{
//			get { throw new NotImplementedException(); }
//		}

    public override double Height
    {
      get { return _height; }
    }

    public virtual bool IsDownloading
    {
      get { return false; }
    }

//		public virtual BitmapPalette Palette
//		{
//			get { throw new NotImplementedException(); }
//		}

    public virtual int PixelHeight
    {
      get { return _pixelHeight; }
    }

    public virtual int PixelWidth
    {
      get { return _pixelWidth; }
    }

    public override double Width
    {
      get { return _width; }
    }

    #endregion Properties

    #region Fields

    private int _dpiX = 0;
    private int _dpiY = 0;
    private int _height = 0;
    private int _pixelHeight = 0;
    private int _pixelWidth = 0;
    private int _width = 0;

    #endregion Fields
  }
}