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

using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Threading;

namespace MediaPortal.GUI.Library
{
	public sealed class GUIAnimation : GUIControl, IDisposable
	{
		#region Methods

		public void Dispose()
		{
			if(_images == null)
				return;

			for(int index = 0; index < _images.Length; index++)
				_images[index].FreeResources();
		}

		void PrepareImages()
		{
			int x = 0;
			int y = 0;
			int w = 0;
			int h = 0;

			_images = new GUIImage[_filenames.Count];

			for(int index = 0; index < _images.Length; index++)
			{
				_images[index] = new GUIImage(ParentID, _imageId + index, x, y, w, h, (string)_filenames[index], Color.White);
				_images[index].AllocResources();

				if(index != 0)
					continue;

				w = _images[index].Width;
				h = _images[index].Height;
				x = (GUIGraphicsContext.Width - w) / 2;
				y = (GUIGraphicsContext.Height - h) / 2;

				_images[index].SetPosition(x, y);
				_imageId++;
			}
		}

		public override void Render(float timePassed)
		{
			if(_filenames != null && _images == null)
				PrepareImages();

			if(_images == null)
				return;

			if(_images.Length == 0)
				return;

			double x = (_images.Length * (Environment.TickCount - _tickCount)) / 800;

			_images[(int)x % _images.Length].Render(timePassed);
		}

		#endregion Methods

		#region Properties

		public ArrayList Filenames
		{
			get { if(_filenames == null) _filenames = new ArrayList(); return _filenames; }
		}
		
		#endregion Properties

		#region Fields

		GUIImage[]						_images;
		ArrayList						_filenames;
		static int						_imageId = 200000;
		float							_tickCount;

		#endregion Fields
	}
}
