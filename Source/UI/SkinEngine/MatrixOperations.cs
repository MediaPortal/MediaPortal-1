#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com
 
    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2.  If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System;
using System.Drawing;
using SlimDX;

namespace MediaPortal.UI.SkinEngine
{
  /// <summary>
  /// Extended functionality for the <see cref="Matrix"/> (transformations, inverting, ...).
  /// </summary>
  public static class MatrixOperations
  {
    public static Matrix Clone(Matrix matrix)
    {
      return new Matrix
        {
          M11 = matrix.M11, M12 = matrix.M12, M13 = matrix.M13, M14 = matrix.M14,
          M21 = matrix.M21, M22 = matrix.M22, M23 = matrix.M23, M24 = matrix.M24,
          M31 = matrix.M31, M32 = matrix.M32, M33 = matrix.M33, M34 = matrix.M34,
          M41 = matrix.M41, M42 = matrix.M42, M43 = matrix.M43, M44 = matrix.M44,
        };
    }

    public static Matrix Scale(this Matrix matrix, float x, float y)
    {
      return matrix * new Matrix { M11 = x, M22 = y };
    }

    /// <summary>
    /// Returns a matrix with the translation part removed from the given <paramref name="matrix"/>.
    /// </summary>
    /// <param name="matrix">The matrix to remove the translation part.</param>
    /// <returns>Matrix with removed translation part.</returns>
    public static Matrix RemoveTranslation(this Matrix matrix)
    {
      return new Matrix
        {
          M11 = matrix.M11,
          M12 = matrix.M12,

          M21 = matrix.M21,
          M22 = matrix.M22,

          M33 = 1,
          M44 = 1
        };
    }

    /// <summary>
    /// Transforms the given point <paramref name="p"/> by the given matrix.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="p">Point to transform.</param>
    public static void Transform(this Matrix matrix, ref PointF p)
    {
      float w = p.X * matrix.M11 + p.Y * matrix.M21;
      float h = p.X * matrix.M12 + p.Y * matrix.M22;
      p.X = w;
      p.Y = h;
    }

    /// <summary>
    /// Transforms the given point <paramref name="p"/> by the given matrix.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="p">Point to transform. Will contain the transformed point after this method returns.</param>
    public static void Transform(this Matrix matrix, ref Point p)
    {
      float w = p.X * matrix.M11 + p.Y * matrix.M21;
      float h = p.X * matrix.M12 + p.Y * matrix.M22;
      p.X = (int) w;
      p.Y = (int) h;
    }

    /// <summary>
    /// Transforms the point given by the coordinates <paramref name="x"/> and <paramref name="y"/> by the given matrix.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="x">X coordinate of the point to transform. Will contain the transformed coordinate after
    /// this method returns.</param>
    /// <param name="y">Y coordinate of the point to transform. Will contain the transformed coordinate after
    /// this method returns.</param>
    public static void Transform(this Matrix matrix, ref float x, ref float y)
    {
      float w = x * matrix.M11 + y * matrix.M21;
      float h = x * matrix.M12 + y * matrix.M22;
      x = w;
      y = h;
    }

    /// <summary>
    /// Transforms the given two-dimensional vector <paramref name="v"/> by the inverse of the given matrix.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="v">Vector to transform. Will contain the transformed vector after this method returns.</param>
    public static void Transform(this Matrix matrix, ref Vector2 v)
    {
      float w = v.X * matrix.M11 + v.Y * matrix.M21;
      float h = v.X * matrix.M12 + v.Y * matrix.M22;
      v.X = w;
      v.Y = h;
    }

    /// <summary>
    /// Transforms a rect which is parallel to the coordinate axes, which has the given <paramref name="size"/>,
    /// by this matrix and returns the size of the transformed rectangle.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="size">Size of the rectangle to transform. Will contain the size of the transformed rectangle after
    /// this method returns.</param>
    public static void TransformSize(this Matrix matrix, ref SizeF size)
    {
      PointF p0 = new PointF(0, 0);
      PointF p1 = new PointF(size.Width, 0);
      PointF p2 = new PointF(size.Width, size.Height);
      PointF p3 = new PointF(0, size.Height);
      matrix.Transform(ref p0);
      matrix.Transform(ref p1);
      matrix.Transform(ref p2);
      matrix.Transform(ref p3);
      size.Width = Math.Max(p0.X - p2.X, p1.X-p3.X);
      size.Height = Math.Max(p0.Y - p2.Y, p1.Y-p3.Y);
    }

    /// <summary>
    /// Transforms the given point <paramref name="p"/> by the inverse of the given matrix.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="p">Point to transform. Will contain the transformed point after this method returns.</param>
    public static void Invert(this Matrix matrix, ref PointF p)
    {
      Matrix inverse = Matrix.Invert(matrix);
      float w = p.X * inverse.M11 + p.Y * inverse.M21;
      float h = p.X * inverse.M12 + p.Y * inverse.M22;
      p.X = w;
      p.Y = h;
    }

    /// <summary>
    /// Transforms the given point <paramref name="p"/> by the inverse of the given matrix.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="p">Point to transform. Will contain the transformed point after this method returns.</param>
    public static void Invert(this Matrix matrix, ref Point p)
    {
      Matrix inverse = Matrix.Invert(matrix);
      float w = p.X * inverse.M11 + p.Y * inverse.M21;
      float h = p.X * inverse.M12 + p.Y * inverse.M22;
      p.X = (int) w;
      p.Y = (int) h;
    }

    /// <summary>
    /// Transforms the point given by the coordinates <paramref name="x"/> and <paramref name="y"/> by the inverse
    /// of the given matrix.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="x">X coordinate of the point to transform. Will contain the transformed coordinate after
    /// this method returns.</param>
    /// <param name="y">Y coordinate of the point to transform. Will contain the transformed coordinate after
    /// this method returns.</param>
    public static void Invert(this Matrix matrix, ref float x, ref float y)
    {
      Matrix inverse = Matrix.Invert(matrix);
      float w = x * inverse.M11 + y * inverse.M21;
      float h = x * inverse.M12 + y * inverse.M22;
      x = w;
      y = h;
    }

    /// <summary>
    /// Transforms the given two-dimensional vector <paramref name="v"/> by the inverse of the given matrix.
    /// </summary>
    /// <param name="matrix">Transformation matrix.</param>
    /// <param name="v">Vector to transform. Will contain the transformed vector after this method returns.</param>
    public static void Invert(this Matrix matrix, ref Vector2 v)
    {
      Matrix inverse = Matrix.Invert(matrix);
      float w = v.X * inverse.M11 + v.Y * inverse.M21;
      float h = v.X * inverse.M12 + v.Y * inverse.M22;
      v.X = w;
      v.Y = h;
    }

    /// <summary>
    /// Gets a matrix of the <see cref="System.Drawing.Drawing2D"/> namespace out of the given matrix.
    /// </summary>
    /// <param name="matrix">Matrix to convert.</param>
    /// <returns>Matrix of the 2D-namespace.</returns>
    public static System.Drawing.Drawing2D.Matrix Get2dMatrix(this Matrix matrix)
    {
      return new System.Drawing.Drawing2D.Matrix(matrix.M11, matrix.M12, matrix.M21, matrix.M22, 0, 0);
    }
  }
}
