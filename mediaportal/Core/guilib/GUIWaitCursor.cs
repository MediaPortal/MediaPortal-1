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
	public sealed class GUIWaitCursor : GUIControl, IDisposable
	{
		#region Constructors

		private GUIWaitCursor()
		{
		}

		#endregion Constructors

		#region Methods

		GUIImage[] _images;

		public void Dispose()
		{
			if(_images == null)
				return;

			for(int index = 0; index < _images.Length; index++)
				_images[index].FreeResources();
		}

		public static void Init()
		{
			if(_instance != null)
				_instance.Dispose();

			_instance = new GUIWaitCursor();

			ArrayList array = new ArrayList();

			foreach(string filename in Directory.GetFiles(GUIGraphicsContext.Skin + @"\media\", "common.waiting.*.png"))
				array.Add(filename);

			int x = 0;
			int y = 0;
			int w = 0;
			int h = 0;

			_instance._images = new GUIImage[array.Count];

			for(int index = 0; index < _instance._images.Length; index++)
			{
				_instance._images[index] = new GUIImage(_instance.ParentID, 200001 + index, x, y, w, h, (string)array[index], Color.White);
				_instance._images[index].AllocResources();

				if(index != 0)
					continue;

				w = _instance._images[index].Width;
				h = _instance._images[index].Height;
				x = (GUIGraphicsContext.Width - w) / 2;
				y = (GUIGraphicsContext.Height - h) / 2;

				_instance._images[index].SetPosition(x, y);
			}
		}

		public override void Render(float timePassed)
		{
			if(_showCount <= 0)
				return;

			if(_images == null)
				return;

			if(_images.Length == 0)
				return;

			double x = (_images.Length * (Environment.TickCount - _tickCount)) / 800;

			_images[(int)x % _images.Length].Render(timePassed);
		}

		public void Show()
		{
			if(Interlocked.Increment(ref _showCount) == 0)
				Interlocked.Exchange(ref _tickCount, Environment.TickCount);
		}

		public void Hide()
		{
			Interlocked.Decrement(ref _showCount);
		}

		#endregion Methods

		#region Properties
		
		public static GUIWaitCursor Instance
		{
			get { return _instance; }
		}

		#endregion Properties

		#region Fields

		static GUIWaitCursor			_instance;
		int								_showCount;
		float							_tickCount = 0;

		#endregion Fields
	}
}
