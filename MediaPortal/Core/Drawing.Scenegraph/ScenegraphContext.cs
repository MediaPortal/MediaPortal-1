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
using System.ComponentModel;
using System.Drawing;

using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Drawing.Scenegraph
{
	public class ScenegraphContext : IDisposable
	{
		#region Constructors

		public ScenegraphContext(/* ScenegraphForm form */)
		{
//			if(form == null)
//				throw new ArgumentNullException("form");

/*			_device = form.Device;
			_device.DeviceLost += new EventHandler(DeviceLost);
			_device.DeviceReset += new EventHandler(DeviceReset);
			_device.DeviceResizing += new CancelEventHandler(DeviceResizing);
*/
			_opacityStack = new Stack(20);
			_opacityStack.Push(1.0);

			_transformStack = new MatrixStack();
			_transformStack.Push();
			_transformStack.MultiplyMatrixLocal(_device.Transform.World);

			_clipStack = new Stack(20);
			_clipStack.Push(_device.ClipPlanes);

			Caps caps = _device.DeviceCaps;

			_vertexBuffer = new VertexBuffer[caps.MaxSimultaneousTextures];
			_vertexFormat = new VertexFormats[caps.MaxSimultaneousTextures];
			_texture = new Texture[caps.MaxSimultaneousTextures];

			_material = new Material();
			_material.Ambient = Color.White;
			_material.Emissive = Color.RoyalBlue;
			_material.Diffuse = Color.White;

			_device.Material = _material;
		}

		#endregion Constructors

		#region Methods

		void DeviceReset(object sender, EventArgs e)
		{
			Dispose();
		}

		void DeviceResizing(object sender, CancelEventArgs e)
		{
			Dispose();
		}

		void DeviceLost(object sender, EventArgs e)
		{
			Dispose();
		}

		public void Dispose()
		{
			_device.DeviceLost -= new EventHandler(DeviceLost);
			_device.DeviceReset -= new EventHandler(DeviceReset);
			_device.DeviceResizing -= new CancelEventHandler(DeviceResizing);
			_device = null;
		}

		public void PushOpacity(double opacity)
		{
			// clamp the value within its allowed range
			double opacityClamped = Math.Max(0, Math.Min(1, opacity));

			// calculate the correct parametric value based on previous and requested opacity
			double nestedOpacity = (double)_opacityStack.Peek() * opacityClamped;

			// allow for repeated opacities of 1, may not be necessary to be so cautious but we'll see
			if(nestedOpacity != (double)_opacityStack.Peek())
			{
				_material.Diffuse = Color.FromArgb((int)(255 * nestedOpacity), Color.White);
				_device.Material = _material;
			}

			// irrespective of above, we still need to push or popping will get ugly
			_opacityStack.Push(nestedOpacity);
		}

		public void PushTransform(IScenegraphTransform transform)
		{
			_transformStack.Push();
			_transformStack.MultiplyMatrix(transform.Matrix);

			_device.Transform.World = _transformStack.Top;
		}

		public void Render(IScenegraphGeometry geometry)
		{
			if(_device == null)
				return;

//			_device.SetTexture(0, null);
			_device.VertexFormat = geometry.VertexFormat;
			_device.SetStreamSource(0, geometry.VertexBuffer, 0);
			_device.DrawPrimitives(geometry.PrimitiveType, 0, geometry.PrimitiveCount);
		}

		public ScenegraphState Save()
		{
			return new ScenegraphState(this);
		}

		public void SetCamera(IScenegraphCamera camera)
		{
			_device.Transform.World = camera.World;
			_device.Transform.View = camera.View;
			_device.Transform.Projection = camera.Projection;
		}

		#endregion Methods

		#region Properties

		public IScenegraphCamera Camera
		{
			get { return _camera; }
			set { if(_camera == null || _camera.Equals(value) == false) { _camera = value; SetCamera(_camera); } }
		}

		public Device Device
		{
			get { return _device; }
		}

		public double Opacity
		{
			get { return (double)_opacityStack.Peek(); }
		}

		public Matrix Projection
		{
			get { return _device == null ? Matrix.Identity : new Matrix(_device.Transform.World); }
			set { if(_device != null) _device.Transform.World = value; }
		}

		public Matrix View
		{
			get { return _device == null ? Matrix.Identity : new Matrix(_device.Transform.World); }
			set { if(_device != null) _device.Transform.World = value; }
		}

		public Matrix World
		{
			get { return _device == null ? Matrix.Identity : new Matrix(_device.Transform.World); }
			set { if(_device != null) _device.Transform.World = value; }
		}

		public Texture Texture
		{
			get { return _texture[_streamIndex]; }
			set { _texture[_streamIndex] = value; if(_device != null) { _device.SetTexture(_streamIndex, value); _streamIndex++; } }
		}

		public VertexBuffer VertexBuffer
		{
			get { return _vertexBuffer[_streamIndex]; }
			set { _vertexBuffer[_streamIndex] = value; if(_device != null) { _device.SetStreamSource(_streamIndex, value, 0, 0); ++_streamIndex; } }
		}

		public VertexFormats VertexFormat
		{
			get { return _vertexFormat[_streamIndex]; }
			set { _vertexFormat[_streamIndex++] = value; if(_device != null) _device.VertexFormat = value; }
		}

		#endregion Properties

		#region Fields

		IScenegraphCamera			_camera;
		Stack						_clipStack;
		Device						_device;
		Material					_material;
		Stack						_opacityStack;
		int							_streamIndex;
		Texture[]					_texture;
		VertexBuffer[]				_vertexBuffer;
		VertexFormats[]				_vertexFormat;
		MatrixStack					_transformStack;

		#endregion Fields
	}
}
