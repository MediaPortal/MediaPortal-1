#region Copyright (C) 2005 Team MediaPortal

/* 
 *	Copyright (C) 2005 Team MediaPortal
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
using System.Collections;
using System.Drawing;
using Keys = System.Windows.Forms.Keys;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Drawing.Scenegraph
{
	public sealed class Camera : IScenegraphCamera, IScenegraphGroup
	{
		#region Constructors

		public Camera()
		{
		}

		public Camera(float x, float y)
		{
			_x = x;
			_y = y;
		}

		public Camera(float x, float y, float z)
		{
			_x = x;
			_y = y;
			_z = z;
		}

		#endregion Constructors

		#region Methods

		public void Add(object child)
		{
			if(_children == null)
				_children = new ScenegraphCollection(this);

			_children.Add(child);
		}

		public override bool Equals(object other) 
		{
			IScenegraphCamera camera = other as IScenegraphCamera;

			if(camera == null)
				return false;

			if(_projection.Equals(camera.Projection) == false)
				return false;

			if(_view.Equals(camera.View) == false)
				return false;

			if(_world.Equals(camera.World) == false)
				return false;

			return true;
		}
		
		public override int GetHashCode() 
		{
			return _projection.GetHashCode() ^ _world.GetHashCode() ^ _view.GetHashCode();
		}

		Matrix PrepareProjection()
		{
			return Matrix.OrthoOffCenterLH(0, 720, 576, 0, 0, 1);
		}

		Matrix PrepareView()
		{
			return new Matrix(Microsoft.DirectX.Matrix.LookAtLH(new Vector3(_x, _y, -0.1f), new Vector3(_x, _y, 0), new Vector3(0.0f, 1.0f, 0.0f)));
		}

		Matrix PrepareWorld()
		{
			Matrix matrix = Matrix.Translation(-_center.X, -_center.Y, 0);

			matrix *= Matrix.Scaling(_z, _z, 1);
			matrix *= Matrix.Translation(_center.X, _center.Y, 0);

			return matrix;
		}

		public bool ProcessKey(Keys keyData)
		{
			// check for special key combinations (Pan + Zoom etc)
			if((keyData & (Keys.Control | Keys.Alt)) == (Keys.Control | Keys.Alt))
			{
				switch(keyData & ~(Keys.Control | Keys.Alt | Keys.Shift))
				{
					case Keys.Add:
					case Keys.Oemplus:
						_z += (keyData & Keys.Shift) != 0 ? 0.001f : 0.004f;
						_isWorldDirty = true;
						return true;
					case Keys.Subtract:
					case Keys.OemMinus:
						_z -= (keyData & Keys.Shift) != 0 ? 0.001f : 0.004f;
						_isWorldDirty = true;
						return true;
					case Keys.Left:
						_x += (keyData & Keys.Shift) != 0 ? 1 : 4;
						return true;
					case Keys.Right:
						_x -= (keyData & Keys.Shift) != 0 ? 1 : 4;
						return true;
					case Keys.Up:
						_y += (keyData & Keys.Shift) != 0 ? 1 : 4;
						return true;
					case Keys.Down:
						_y -= (keyData & Keys.Shift) != 0 ? 1 : 4;
						return true;
					case Keys.Home:

						if((keyData & Keys.Shift) != 0)
						{
							_x = 0;
							_y = 0;
							_z = 1;
						}

						return true;
				}
			}

			return false;
		}

		void RaiseChanged()
		{
			RaiseChanged(EventArgs.Empty);
		}

		void RaiseChanged(EventArgs e)
		{
		}

		#endregion Methods

		#region Properties

		ICollection IScenegraphGroup.Children
		{
			get { if(_children == null) _children = new ScenegraphCollection(this); return _children; }
		}

		bool IScenegraphGroup.HasChildren
		{
			get { return _children != null && _children.Count != 0; }
		}

		bool IScenegraphElement.HasParents
		{
			get { return _parents != null && _parents.Count != 0; }
		}

		ICollection IScenegraphElement.Parents
		{
			get { if(_parents == null) _parents = new ScenegraphCollection(this); return _parents; }
		}

		public Matrix Projection
		{
			get { if(_isProjectionDirty) { _projection = PrepareProjection(); _isProjectionDirty = false; } return _projection; }
		}

		public Matrix World
		{
			get { if(_isWorldDirty) { _world = PrepareWorld(); _isWorldDirty = false; } return _world; }
		}

		public Matrix View
		{
			get { if(_isViewDirty) { _view = PrepareView(); _isViewDirty = false; } return _view; }
		}

		public float X
		{
			get { return _x; }
			set { _x = value; _isViewDirty = true; RaiseChanged(); }
		}

		public float Y
		{
			get { return _y; }
			set { _y = value; _isViewDirty = true; RaiseChanged(); }
		}

		public float Zoom
		{
			get { return _z; }
			set { _z = value; _isWorldDirty = true; }
		}

		#endregion Properties

		#region Fields

		PointF						_center = PointF.Empty;
		bool						_isProjectionDirty = true;
		bool						_isViewDirty = true;
		bool						_isWorldDirty = true;
		Matrix						_projection;
		Matrix						_view;
		Matrix						_world;
		float						_x = 0;
		float						_y = 0;
		float						_z = 1;
		ScenegraphCollection		_children;
		ScenegraphCollection		_parents;

		#endregion Fields
	}
}
