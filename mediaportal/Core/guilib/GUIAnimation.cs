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
using System.Drawing;
using System.Collections;
using System.IO;
using System.Threading;

using MediaPortal.Animation;

namespace MediaPortal.GUI.Library
{
	public class GUIAnimation : GUIControl
	{
		#region Constructors

		public GUIAnimation()
		{
		}

		public GUIAnimation(int parentId) : base(parentId)
		{
		}

		#endregion Constructors

		#region Methods

		public void Begin()
		{
			_startTick = AnimationTimer.TickCount;
			_isAnimating = true;
		}

		public override void AllocResources()
		{
			if(_filenames == null)
			{
				_filenames = new ArrayList();

				foreach(string filename in _textureNames.Split(';'))
					_filenames.Add(filename.Trim());
			}

			_images = new GUIImage[_filenames.Count];

			int w = 0;
			int h = 0;

			for(int index = 0; index < _images.Length; index++)
			{
				_imageId++;
				_images[index] = new GUIImage(ParentID, _imageId + index, 0, 0, 0, 0, (string)_filenames[index], Color.White);
				_images[index].AllocResources();

				w = Math.Max(w, _images[index].Width);
				h = Math.Max(h, _images[index].Height);
			}

			for(int index = 0; index < _images.Length; index++)
			{
				int x = m_dwPosX;
				int y = m_dwPosY;

				if(_horizontalAlignment == HorizontalAlignment.Center)
				{
					x = x - (w / 2);
				}
				else if(_horizontalAlignment == HorizontalAlignment.Right)
				{
					x = x - w;
				}

				if(_verticalAlignment == VerticalAlignment.Center)
				{
					y = y - (h / 2);
				}
				else if(_verticalAlignment == VerticalAlignment.Bottom)
				{
					y = y - h;
				}

				_images[index].SetPosition(x, y);
			}
		}

		public override void FreeResources()
		{
			if(_images == null)
				return;

			for(int index = 0; index < _images.Length; index++)
				_images[index].FreeResources();

			_images = null;
		}

		public override void Render(float timePassed)
		{
			if(_images == null)
				return;

			if(_images.Length == 0)
				return;

			if(_isAnimating == false)
				return;

			double elapsedTicks = AnimationTimer.TickCount - _startTick;
			double progress = Math.Min(1, TweenHelper.Interpolate(_easing, 0, 1, _startTick, _duration));

			// determine whether we are repeating
			if(_duration < elapsedTicks)
			{
				// keep track of iterations regardless of the repeat behaviour
				_iterationCount++;

				if(_repeatBehavior.IsIterationCount && _repeatBehavior.IterationCount <= _iterationCount)
				{
					_isAnimating = false;

					// XAML: fire event
				}
				else if(_repeatBehavior.IsRepeatDuration && _repeatBehavior.RepeatDuration <= elapsedTicks)
				{
					_isAnimating = false;

					// XAML: fire event
				}

				if(_isAnimating)
					_startTick = AnimationTimer.TickCount;
			}

			int index = 0;

			if(_isAnimating && progress < 1)
				index = (int)(progress * _images.Length);
			
			_images[index].Render(timePassed);
		}

		#endregion Methods

		#region Properties

		public Duration Duration
		{
			get { return _duration; }
			set { _duration = value; }
		}

		public Easing Easing
		{
			get { return _easing; }
			set { _easing = value; }
		}

		public ArrayList Filenames
		{
			get { if(_filenames == null) _filenames = new ArrayList(); return _filenames; }
		}

		public HorizontalAlignment HorizontalAlignment
		{
			get { return _horizontalAlignment; }
			set { _horizontalAlignment = value; }
		}

		public RepeatBehavior RepeatBehavior
		{
			get { return _repeatBehavior; }
			set { _repeatBehavior = value; }
		}
		
		public VerticalAlignment VerticalAlignment
		{
			get { return _verticalAlignment; }
			set { _verticalAlignment = value; }
		}

		#endregion Properties

		#region Properties (Skin)

		[XMLSkinElement("Easing")]
		protected Easing						_easing = Easing.Linear;

		[XMLSkinElement("HorizontalAlignment")]
		protected HorizontalAlignment			_horizontalAlignment = HorizontalAlignment.Left;

		[XMLSkinElement("textures")]
		protected string						_textureNames = string.Empty;

		[XMLSkinElement("rate")]
		protected double						_rate = 1;

		[XMLSkinElement("Duration")]
		protected Duration						_duration = Duration.Automatic;

		[XMLSkinElement("RepeatBehavior")]	
		protected RepeatBehavior				_repeatBehavior = RepeatBehavior.Forever;

		[XMLSkinElement("VerticalAlignment")]
		protected VerticalAlignment				_verticalAlignment = VerticalAlignment.Top;

		#endregion Properties (Skin)

		#region Fields

		ArrayList						_filenames;
		GUIImage[]						_images;
		bool							_isAnimating = false;
		int								_iterationCount = 0;
		static int						_imageId = 200000;
		double							_startTick = 0;

		#endregion Fields
	}
}
