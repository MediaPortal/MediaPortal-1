#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

namespace MediaPortal.Drawing
{
  public struct Matrix
  {
    #region Constructors

    internal Matrix(Microsoft.DirectX.Matrix matrix)
    {
      _matrix = matrix;
    }

    #endregion Constructors

    #region Methods

    public static Matrix Add(Matrix l, Matrix r)
    {
      return new Matrix(Microsoft.DirectX.Matrix.Add(l._matrix, r._matrix));
    }

    public static Matrix AffineTransformation2D(double scaling, Point rotationCenter, double rotation, Point translation)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.AffineTransformation2D((float) scaling, rotationCenter.ToVector2(),
                                                                   (float) rotation, translation.ToVector2()));
    }

    public override bool Equals(object compare)
    {
      return _matrix.Equals(compare);
    }

    public override int GetHashCode()
    {
      return _matrix.GetHashCode();
    }

    public void Invert()
    {
      _matrix.Invert();
    }

    public static Matrix Invert(Matrix source)
    {
      return new Matrix(Microsoft.DirectX.Matrix.Invert(source._matrix));
    }

    public static Matrix Invert(ref double determinant, Matrix source)
    {
      float innerDeterminant = (float) determinant;
      Matrix matrix = new Matrix(Microsoft.DirectX.Matrix.Invert(ref innerDeterminant, source._matrix));

      determinant = innerDeterminant;

      return matrix;
    }

    public static Matrix LookAtLH(Point cameraPosition, Point cameraTarget, Point cameraUpVector)
    {
      return new Matrix(Microsoft.DirectX.Matrix.LookAtLH(cameraPosition, cameraTarget, cameraUpVector));
    }

    public static Matrix LookAtRH(Point cameraPosition, Point cameraTarget, Point cameraUpVector)
    {
      return new Matrix(Microsoft.DirectX.Matrix.LookAtRH(cameraPosition, cameraTarget, cameraUpVector));
    }

    public void Multiply(Matrix source)
    {
      _matrix.Multiply(source);
    }

    public static Matrix Multiply(Matrix l, Matrix r)
    {
      return new Matrix(Multiply(l, r));
    }

    public void MultiplyTranspose(Matrix source)
    {
      _matrix.MultiplyTranspose(source);
    }

    public static Matrix MultiplyTranspose(Matrix l, Matrix r)
    {
      return MultiplyTranspose(l, r);
    }

    public static Matrix OrthoLH(double width, double height, double znearPlane, double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.OrthoLH((float) width, (float) height, (float) znearPlane, (float) zfarPlane));
    }

    public static Matrix OrthoOffCenterLH(double left, double right, double bottom, double top, double znearPlane,
                                          double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.OrthoOffCenterLH((float) left, (float) right, (float) bottom, (float) top,
                                                             (float) znearPlane, (float) zfarPlane));
    }

    public static Matrix OrthoOffCenterRH(double left, double right, double bottom, double top, double znearPlane,
                                          double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.OrthoOffCenterRH((float) left, (float) right, (float) bottom, (float) top,
                                                             (float) znearPlane, (float) zfarPlane));
    }

    public static Matrix OrthoRH(double width, double height, double znearPlane, double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.OrthoRH((float) width, (float) height, (float) znearPlane, (float) zfarPlane));
    }

    public static Matrix PerspectiveFovLH(float fieldOfViewY, float aspectRatio, float znearPlane, float zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.PerspectiveFovLH((float) fieldOfViewY, (float) aspectRatio,
                                                             (float) znearPlane, (float) zfarPlane));
    }

    public static Matrix PerspectiveFovRH(double fieldOfViewY, double aspectRatio, double znearPlane, double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.PerspectiveFovRH((float) fieldOfViewY, (float) aspectRatio,
                                                             (float) znearPlane, (float) zfarPlane));
    }

    public static Matrix PerspectiveLH(double width, double height, double znearPlane, double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.PerspectiveLH((float) width, (float) height, (float) znearPlane,
                                                          (float) zfarPlane));
    }

    public static Matrix PerspectiveOffCenterLH(double left, double right, double bottom, double top, double znearPlane,
                                                double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.PerspectiveOffCenterLH((float) left, (float) right, (float) bottom,
                                                                   (float) top, (float) znearPlane, (float) zfarPlane));
    }

    public static Matrix PerspectiveOffCenterRH(double left, double right, double bottom, double top, double znearPlane,
                                                double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.PerspectiveOffCenterRH((float) left, (float) right, (float) bottom,
                                                                   (float) top, (float) znearPlane, (float) zfarPlane));
    }

    public static Matrix PerspectiveRH(double width, double height, double znearPlane, double zfarPlane)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.PerspectiveFovRH((float) width, (float) height, (float) znearPlane,
                                                             (float) zfarPlane));
    }

//		public void Reflect(Plane plane)
//		{
//		}

    public void RotateAxis(Point axisRotation, double angle)
    {
      _matrix.RotateAxis(axisRotation, (float) angle);
    }

//		public void RotateQuaternion(Quaternion quat)
//		{
//		}

    public void RotateX(double angle)
    {
      _matrix.RotateX((float) angle);
    }

    public void RotateY(double angle)
    {
      _matrix.RotateY((float) angle);
    }

    public void RotateYawPitchRoll(double yaw, double pitch, double roll)
    {
      _matrix.RotateYawPitchRoll((float) yaw, (float) pitch, (float) roll);
    }

    public void RotateZ(double angle)
    {
      _matrix.RotateZ((float) angle);
    }

    public static Matrix RotationAxis(Point axisRotation, double angle)
    {
      return new Matrix(Microsoft.DirectX.Matrix.RotationAxis(axisRotation, (float) angle));
    }

//		public static Matrix RotationQuaternion(Quaternion quat)
//		{
//		}

    public static Matrix RotationX(double angle)
    {
      return new Matrix(Microsoft.DirectX.Matrix.RotationX((float) angle));
    }

    public static Matrix RotationY(double angle)
    {
      return new Matrix(Microsoft.DirectX.Matrix.RotationY((float) angle));
    }

    public static Matrix RotationYawPitchRoll(double yaw, double pitch, double roll)
    {
      return new Matrix(Microsoft.DirectX.Matrix.RotationYawPitchRoll((float) yaw, (float) pitch, (float) roll));
    }

    public static Matrix RotationZ(double angle)
    {
      return new Matrix(Microsoft.DirectX.Matrix.RotationZ((float) angle));
    }

    public void Scale(double x, double y)
    {
      _matrix.Scale((float) x, (float) y, 1f);
    }

    public void Scale(double x, double y, double z)
    {
      _matrix.Scale((float) x, (float) y, (float) z);
    }

    public static Matrix Scaling(Point p)
    {
      return new Matrix(Microsoft.DirectX.Matrix.Scaling((float) p.X, (float) p.Y, 1f));
    }

    public static Matrix Scaling(double x, double y)
    {
      return new Matrix(Microsoft.DirectX.Matrix.Scaling((float) x, (float) y, 1f));
    }

    public static Matrix Scaling(double x, double y, double z)
    {
      return new Matrix(Microsoft.DirectX.Matrix.Scaling((float) x, (float) y, (float) z));
    }

//		public void Shadow(Vector4 light, Plane plane)
//		{
//		}

    public static Matrix Subtract(Matrix l, Matrix r)
    {
      return new Matrix(l._matrix - r._matrix);
    }

    public override string ToString()
    {
      return _matrix.ToString();
    }

//		public void Transform(Vector3 scalingCenter, Quaternion scalingRotation, Vector3 scalingFactor, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
//		{
//		}

//		public void Transform(Vector3 scalingCenter, Quaternion scalingRotation, Vector3 scalingFactor, Vector3 rotationCenter, Quaternion rotation, Vector3 translation)
//		{
//		}

    public static Matrix Transformation2D(Point scalingCenter, double scalingRotation, Point scaling,
                                          Point rotationCenter, double rotation, Point translation)
    {
      return
        new Matrix(Microsoft.DirectX.Matrix.Transformation2D(scalingCenter, (float) scalingRotation, scaling,
                                                             rotationCenter, (float) rotation, translation));
    }

    public void Translate(Point p)
    {
      _matrix.Translate((float) p.X, (float) p.Y, 0f);
    }

    public void Translate(double x, double y)
    {
      _matrix.Translate((float) x, (float) y, 0f);
    }

    public void Translate(double x, double y, double z)
    {
      _matrix.Translate((float) x, (float) y, (float) z);
    }

    public static Matrix Translation(Point p)
    {
      return new Matrix(Microsoft.DirectX.Matrix.Translation((float) p.X, (float) p.Y, 0f));
    }

    public static Matrix Translation(double x, double y, double z)
    {
      return new Matrix(Microsoft.DirectX.Matrix.Translation((float) x, (float) y, 0f));
    }

    public void Transpose(Matrix source)
    {
      _matrix.Transpose(source);
    }

    public static Matrix TransposeMatrix(Matrix source)
    {
      return new Matrix(Microsoft.DirectX.Matrix.TransposeMatrix(source));
    }

    #endregion Methods

    #region Operators

    public static implicit operator Microsoft.DirectX.Matrix(Matrix matrix)
    {
      return matrix._matrix;
    }

    public static Matrix operator +(Matrix l, Matrix r)
    {
      return new Matrix(Microsoft.DirectX.Matrix.Add(l, r));
    }

    public static bool operator ==(Matrix l, Matrix r)
    {
      return l._matrix == r._matrix;
    }

    public static bool operator !=(Matrix l, Matrix r)
    {
      return l._matrix != r._matrix;
    }

    public static Matrix operator *(Matrix l, Matrix r)
    {
      return new Matrix(l._matrix*r._matrix);
    }

    public static Matrix operator -(Matrix l, Matrix r)
    {
      return new Matrix(l._matrix - r._matrix);
    }

    #endregion Operators

    #region Properties

    public float Determinant
    {
      get { return _matrix.Determinant; }
    }

    public double M11
    {
      get { return _matrix.M11; }
      set { _matrix.M11 = (float) value; }
    }

    public double M12
    {
      get { return _matrix.M12; }
      set { _matrix.M12 = (float) value; }
    }

    public double M13
    {
      get { return _matrix.M13; }
      set { _matrix.M13 = (float) value; }
    }

    public double M14
    {
      get { return _matrix.M14; }
      set { _matrix.M14 = (float) value; }
    }

    public double M21
    {
      get { return _matrix.M21; }
      set { _matrix.M21 = (float) value; }
    }

    public double M22
    {
      get { return _matrix.M22; }
      set { _matrix.M22 = (float) value; }
    }

    public double M23
    {
      get { return _matrix.M23; }
      set { _matrix.M23 = (float) value; }
    }

    public double M24
    {
      get { return _matrix.M24; }
      set { _matrix.M24 = (float) value; }
    }

    public double M31
    {
      get { return _matrix.M31; }
      set { _matrix.M31 = (float) value; }
    }

    public double M32
    {
      get { return _matrix.M32; }
      set { _matrix.M32 = (float) value; }
    }

    public double M33
    {
      get { return _matrix.M33; }
      set { _matrix.M33 = (float) value; }
    }

    public double M34
    {
      get { return _matrix.M34; }
      set { _matrix.M34 = (float) value; }
    }

    public double M41
    {
      get { return _matrix.M41; }
      set { _matrix.M41 = (float) value; }
    }

    public double M42
    {
      get { return _matrix.M42; }
      set { _matrix.M42 = (float) value; }
    }

    public double M43
    {
      get { return _matrix.M43; }
      set { _matrix.M43 = (float) value; }
    }

    public double M44
    {
      get { return _matrix.M44; }
      set { _matrix.M44 = (float) value; }
    }

    #endregion Properties

    #region Fields

    private Microsoft.DirectX.Matrix _matrix;

    public static readonly Matrix Identity = new Matrix(Microsoft.DirectX.Matrix.Identity);
    public static readonly Matrix Zero = new Matrix(Microsoft.DirectX.Matrix.Zero);

    #endregion Fields
  }
}