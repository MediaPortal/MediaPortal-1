#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.ComponentModel;
using Microsoft.DirectX.Direct3D;

namespace MediaPortal.Drawing
{
  [TypeConverter(typeof (GeometryConverter))]
  public class Geometry
  {
    #region Constructors

    public Geometry()
    {
    }

    internal Geometry(Type vertexType, int vertexCount, int primitiveCount)
    {
      _vertexType = vertexType;
      _vertexArray = Array.CreateInstance(vertexType, vertexCount);
      _primitiveCount = primitiveCount;
    }

    internal Geometry(Type vertexType, Array vertexArray, int primitiveCount)
    {
      _vertexType = vertexType;
      _vertexArray = Array.CreateInstance(vertexType, vertexArray.Length);

      for (int index = 0; index < vertexArray.Length; index++)
      {
        _vertexArray.SetValue(vertexArray.GetValue(index), index);
      }

      _primitiveCount = primitiveCount;
    }

    #endregion Constructors

    #region Properties

//		public abstract Rect Bounds
    public virtual Rect Bounds
    {
      get { return Rect.Empty; }
    }

    public VertexBuffer VertexBuffer
    {
      get { return _vertexBuffer; }
    }

    public int VertexCount
    {
      get { return _vertexArray == null ? 0 : _vertexArray.Length; }
    }

    public VertexFormats VertexFormat
    {
      get { return _vertexFormat; }
      set { _vertexFormat = value; }
    }

    public PrimitiveType PrimitiveType
    {
      get { return _primitiveType; }
      set { _primitiveType = value; }
    }

    public int PrimitiveCount
    {
      get { return _primitiveCount; }
    }

    public object this[int index]
    {
      get { return _vertexArray.GetValue(index); }
      set { _vertexArray.SetValue(value, index); }
    }

    #endregion Properties

    #region Fields

    private int _primitiveCount;
    private PrimitiveType _primitiveType;
    private Array _vertexArray;
    private VertexBuffer _vertexBuffer = null;
    private VertexFormats _vertexFormat;
    private Type _vertexType;

    #endregion Fields
  }
}