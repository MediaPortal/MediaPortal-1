#region Copyright (C) 2005 Media Portal

/* 
 *	Copyright (C) 2005 Media Portal
 *	http://mediaportal.sourceforge.net
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
using System.Collections;
using System.ComponentModel;
using System.Drawing;

using MediaPortal.Drawing.Scenegraph;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Drawing
{
	[TypeConverter(typeof(ImageSourceConverter))]
	public class ImageSource : IScenegraphTexture, IScenegraphResource
	{
		#region Constructors

		public ImageSource()
		{
			_filename = string.Empty;
		}

		public ImageSource(Bitmap bitmap)
		{
			_bitmap = bitmap;
			_isLoaded = true;
			_filename = string.Empty;
		}

		public ImageSource(string filename)
		{
			_filename = filename;
		}

		#endregion Constructors

		#region Methods

		void IScenegraphResource.PrepareResource(ScenegraphContext context)
		{
 			ImageSource cached = (ImageSource)_cache[_filename];

			if(_isLoaded == false)
			{
				if(cached == null)
				{
					_bitmap = (Bitmap)Image.FromFile(_filename);
					_pixelWidth = _bitmap.Width;
					_pixelHeight = _bitmap.Height;

					_aspectRatio = (float)_pixelWidth / _pixelHeight;

					_dpiX = _bitmap.HorizontalResolution;
					_dpiY = _bitmap.VerticalResolution;

					_texture = Texture.FromBitmap(context.Device, _bitmap, Usage.None, Pool.Managed);
					_isLoaded = true;
					_cache[_filename] = this;
				}
				else
				{
					_pixelWidth = cached._pixelWidth;
					_pixelHeight = cached._pixelHeight;

					_aspectRatio = cached._aspectRatio;

					_dpiX = cached._dpiX;
					_dpiY = cached._dpiX;

					_texture = cached._texture;
					_isLoaded = cached._isLoaded;
				}
			}
			
			if(_texture == null)
				return;

			context.Device.SetTexture(0, _texture);
		}

		void IScenegraphResource.ReleaseResource(ScenegraphContext context)
		{
			if(_texture == null)
				return;

			_texture.Dispose();
			_texture = null;

			ImageSource cached = _cache[_filename] as ImageSource;

			if(cached == null)
				return;

			_cache.Remove(_filename);
		}

		#endregion Methods

		#region Properties

		public double AspectRatio
		{
			get { return _aspectRatio; }
		}

		public string Filename
		{
			get { return _filename; }
		}

		bool IScenegraphElement.HasParents
		{
			get { return _parents != null && _parents.Count != 0; }
		}

		public bool IsLoaded
		{
			get { return _isLoaded; }
		}

		ICollection IScenegraphElement.Parents
		{
			get { if(_parents == null) _parents = new ScenegraphCollection(this); return _parents; }
		}

		public int PixelWidth
		{
			get { return _pixelWidth; }
		}

		public int PixelHeight
		{
			get { return _pixelHeight; }
		}

		Texture IScenegraphTexture.Texture
		{
			get { return _texture; }
		}

		#endregion Properties

		#region Fields

		float						_aspectRatio;
		Bitmap						_bitmap;
		static Hashtable			_cache = new Hashtable();
		float						_dpiX;
		float						_dpiY;
		string						_filename;
		bool						_isLoaded;
		int							_pixelWidth;
		int							_pixelHeight;
		Texture						_texture;
		ScenegraphCollection		_parents;

		#endregion Fields
	}
}
