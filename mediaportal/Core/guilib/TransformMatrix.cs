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

using System;

namespace MediaPortal.GUI.Library
{
  public class TransformMatrix : ICloneable
  {
    private const float DEGREE_TO_RADIAN = 0.01745329f;
    private float[,] m = new float[3,4];
    private float alpha;

    public TransformMatrix()
    {
      Reset();
    }

    public void Reset()
    {
      m[0, 0] = 1.0f;
      m[0, 1] = m[0, 2] = m[0, 3] = 0;
      m[1, 0] = m[1, 2] = m[1, 3] = 0;
      m[1, 1] = 1.0f;
      m[2, 0] = m[2, 1] = m[2, 3] = 0;
      m[2, 2] = 1.0f;
      alpha = 1.0f;
    }

    public static TransformMatrix CreateTranslation(float transX, float transY, float transZ /*=0.0f*/)
    {
      TransformMatrix translation = new TransformMatrix();
      translation.m[0, 3] = transX;
      translation.m[1, 3] = transY;
      translation.m[2, 3] = transZ;
      return translation;
    }

    public void SetTranslation(float transX, float transY, float transZ)
    {
      m[0, 1] = m[0, 2] = 0.0f;
      m[0, 0] = 1.0f;
      m[0, 3] = transX;
      m[1, 0] = m[1, 2] = 0.0f;
      m[1, 1] = 1.0f;
      m[1, 3] = transY;
      m[2, 0] = m[2, 1] = 0.0f;
      m[2, 2] = 1.0f;
      m[2, 3] = transZ;
      alpha = 1.0f;
    }

    public static TransformMatrix CreateScaler(float scaleX, float scaleY, float scaleZ /*=1.0f*/)
    {
      TransformMatrix scaler = new TransformMatrix();
      scaler.m[0, 0] = scaleX;
      scaler.m[1, 1] = scaleY;
      scaler.m[2, 2] = scaleZ;
      return scaler;
    }

    public void SetScaler(float scaleX, float scaleY, float centerX, float centerY)
    {
      // Trans(centerX,centerY,centerZ)*Scale(scaleX,scaleY,scaleZ)*Trans(-centerX,-centerY,-centerZ)
      float centerZ = 0, scaleZ = 1;
      m[0, 0] = scaleX;
      m[0, 1] = 0;
      m[0, 2] = 0;
      m[0, 3] = centerX * (1 - scaleX);
      m[1, 0] = 0;
      m[1, 1] = scaleY;
      m[1, 2] = 0;
      m[1, 3] = centerY * (1 - scaleY);
      m[2, 0] = 0;
      m[2, 1] = 0;
      m[2, 2] = scaleZ;
      m[2, 3] = centerZ * (1 - scaleZ);
      alpha = 1.0f;
    }

    public void SetXRotation(float angle, float y, float z, float ar /*=1.0f*/)
    {
      // angle about the X axis, centered at y,z where our coordinate system has aspect ratio ar.
      // Trans(0,y,z)*Scale(1,1/ar,1)*RotateX(angle)*Scale(ar,1,1)*Trans(0,-y,-z);
      float c = (float)Math.Cos(angle);
      float s = (float)Math.Sin(angle);
      m[0, 0] = ar;
      m[0, 1] = 0;
      m[0, 2] = 0;
      m[0, 3] = 0;
      m[1, 0] = 0;
      m[1, 1] = c / ar;
      m[1, 2] = -s / ar;
      m[1, 3] = (-y * c + s * z) / ar + y;
      m[2, 0] = 0;
      m[2, 1] = s;
      m[2, 2] = c;
      m[2, 3] = (-y * s - c * z) + z;
      angle = 1.0f;
    }

    public void SetYRotation(float angle, float x, float z, float ar /*=1.0f*/)
    {
      // angle about the Y axis, centered at x,z where our coordinate system has aspect ratio ar.
      // Trans(x,0,z)*Scale(1/ar,1,1)*RotateY(angle)*Scale(ar,1,1)*Trans(-x,0,-z);
      float c = (float)Math.Cos(angle);
      float s = (float)Math.Sin(angle);
      m[0, 0] = c;
      m[0, 1] = 0;
      m[0, 2] = -s / ar;
      m[0, 3] = -x * c + s * z / ar + x;
      m[1, 0] = 0;
      m[1, 1] = 1;
      m[1, 2] = 0;
      m[1, 3] = 0;
      m[2, 0] = ar * s;
      m[2, 1] = 0;
      m[2, 2] = c;
      m[2, 3] = -ar * x * s - c * z + z;
      angle = 1.0f;
    }

    public static TransformMatrix CreateZRotation(float angle, float x, float y, float ar /*=1.0f*/)
    {
      // angle about the Z axis, centered at x,y where our coordinate system has aspect ratio ar.
      // Trans(x,y,0)*Scale(1/ar,1,1)*RotateZ(angle)*Scale(ar,1,1)*Trans(-x,-y,0)
      float c = (float)Math.Cos(angle);
      float s = (float)Math.Sin(angle);
      TransformMatrix rot = new TransformMatrix();
      rot.m[0, 0] = c;
      rot.m[0, 1] = -s / ar;
      rot.m[0, 3] = -x * c + s * y / ar + x;
      rot.m[1, 0] = s * ar;
      rot.m[1, 1] = c;
      rot.m[1, 3] = -ar * x * s - c * y + y;
      return rot;
    }

    public void SetZRotation(float angle, float x, float y, float ar /*=1.0f*/)
    {
      // angle about the Z axis, centered at x,y where our coordinate system has aspect ratio ar.
      // Trans(x,y,0)*Scale(1/ar,1,1)*RotateZ(angle)*Scale(ar,1,1)*Trans(-x,-y,0)
      float c = (float)Math.Cos(angle);
      float s = (float)Math.Sin(angle);
      m[0, 0] = c;
      m[0, 1] = -s / ar;
      m[0, 2] = 0;
      m[0, 3] = -x * c + s * y / ar + x;
      m[1, 0] = s * ar;
      m[1, 1] = c;
      m[1, 2] = 0;
      m[1, 3] = -ar * x * s - c * y + y;
      m[2, 0] = 0;
      m[2, 1] = 0;
      m[2, 2] = 1;
      m[2, 3] = 0;
      angle = 1.0f;
    }

    public static TransformMatrix CreateFader(float a)
    {
      TransformMatrix fader = new TransformMatrix();
      fader.alpha = a;
      return fader;
    }

    public void SetFader(float a)
    {
      m[0, 0] = 1.0f;
      m[0, 1] = 0.0f;
      m[0, 2] = 0.0f;
      m[0, 3] = 0.0f;
      m[1, 0] = 0.0f;
      m[1, 1] = 1.0f;
      m[1, 2] = 0.0f;
      m[1, 3] = 0.0f;
      m[2, 0] = 0.0f;
      m[2, 1] = 0.0f;
      m[2, 2] = 1.0f;
      m[2, 3] = 0.0f;
      alpha = a;
    }

    // assignment operator
    public TransformMatrix assign(TransformMatrix right)
    {
      m[0, 0] = right.m[0, 0];
      m[0, 1] = right.m[0, 1];
      m[0, 2] = right.m[0, 2];
      m[0, 3] = right.m[0, 3];

      m[1, 0] = right.m[1, 0];
      m[1, 1] = right.m[1, 1];
      m[1, 2] = right.m[1, 2];
      m[1, 3] = right.m[1, 3];

      m[2, 0] = right.m[2, 0];
      m[2, 1] = right.m[2, 1];
      m[2, 2] = right.m[2, 2];
      m[2, 3] = right.m[2, 3];
      alpha = right.alpha;
      return this;
    }

    // multiplication operators
    public TransformMatrix multiplyAssign(TransformMatrix right)
    {
      float t00 = m[0, 0] * right.m[0, 0] + m[0, 1] * right.m[1, 0] + m[0, 2] * right.m[2, 0];
      float t01 = m[0, 0] * right.m[0, 1] + m[0, 1] * right.m[1, 1] + m[0, 2] * right.m[2, 1];
      float t02 = m[0, 0] * right.m[0, 2] + m[0, 1] * right.m[1, 2] + m[0, 2] * right.m[2, 2];
      m[0, 3] = m[0, 0] * right.m[0, 3] + m[0, 1] * right.m[1, 3] + m[0, 2] * right.m[2, 3] + m[0, 3];
      m[0, 0] = t00;
      m[0, 1] = t01;
      m[0, 2] = t02;
      t00 = m[1, 0] * right.m[0, 0] + m[1, 1] * right.m[1, 0] + m[1, 2] * right.m[2, 0];
      t01 = m[1, 0] * right.m[0, 1] + m[1, 1] * right.m[1, 1] + m[1, 2] * right.m[2, 1];
      t02 = m[1, 0] * right.m[0, 2] + m[1, 1] * right.m[1, 2] + m[1, 2] * right.m[2, 2];
      m[1, 3] = m[1, 0] * right.m[0, 3] + m[1, 1] * right.m[1, 3] + m[1, 2] * right.m[2, 3] + m[1, 3];
      m[1, 0] = t00;
      m[1, 1] = t01;
      m[1, 2] = t02;
      t00 = m[2, 0] * right.m[0, 0] + m[2, 1] * right.m[1, 0] + m[2, 2] * right.m[2, 0];
      t01 = m[2, 0] * right.m[0, 1] + m[2, 1] * right.m[1, 1] + m[2, 2] * right.m[2, 1];
      t02 = m[2, 0] * right.m[0, 2] + m[2, 1] * right.m[1, 2] + m[2, 2] * right.m[2, 2];
      m[2, 3] = m[2, 0] * right.m[0, 3] + m[2, 1] * right.m[1, 3] + m[2, 2] * right.m[2, 3] + m[2, 3];
      m[2, 0] = t00;
      m[2, 1] = t01;
      m[2, 2] = t02;
      alpha *= right.alpha;

      return this;
    }

    public TransformMatrix multiply(TransformMatrix right)
    {
      TransformMatrix result = new TransformMatrix();
      result.m[0, 0] = m[0, 0] * right.m[0, 0] + m[0, 1] * right.m[1, 0] + m[0, 2] * right.m[2, 0];
      result.m[0, 1] = m[0, 0] * right.m[0, 1] + m[0, 1] * right.m[1, 1] + m[0, 2] * right.m[2, 1];
      result.m[0, 2] = m[0, 0] * right.m[0, 2] + m[0, 1] * right.m[1, 2] + m[0, 2] * right.m[2, 2];
      result.m[0, 3] = m[0, 0] * right.m[0, 3] + m[0, 1] * right.m[1, 3] + m[0, 2] * right.m[2, 3] + m[0, 3];
      result.m[1, 0] = m[1, 0] * right.m[0, 0] + m[1, 1] * right.m[1, 0] + m[1, 2] * right.m[2, 0];
      result.m[1, 1] = m[1, 0] * right.m[0, 1] + m[1, 1] * right.m[1, 1] + m[1, 2] * right.m[2, 1];
      result.m[1, 2] = m[1, 0] * right.m[0, 2] + m[1, 1] * right.m[1, 2] + m[1, 2] * right.m[2, 2];
      result.m[1, 3] = m[1, 0] * right.m[0, 3] + m[1, 1] * right.m[1, 3] + m[1, 2] * right.m[2, 3] + m[1, 3];
      result.m[2, 0] = m[2, 0] * right.m[0, 0] + m[2, 1] * right.m[1, 0] + m[2, 2] * right.m[2, 0];
      result.m[2, 1] = m[2, 0] * right.m[0, 1] + m[2, 1] * right.m[1, 1] + m[2, 2] * right.m[2, 1];
      result.m[2, 2] = m[2, 0] * right.m[0, 2] + m[2, 1] * right.m[1, 2] + m[2, 2] * right.m[2, 2];
      result.m[2, 3] = m[2, 0] * right.m[0, 3] + m[2, 1] * right.m[1, 3] + m[2, 2] * right.m[2, 3] + m[2, 3];
      result.alpha = alpha * right.alpha;
      return result;
    }

    public void TransformPosition(ref float x, ref float y, ref float z)
    {
      float newX = m[0, 0] * x + m[0, 1] * y + m[0, 2] * z + m[0, 3];
      float newY = m[1, 0] * x + m[1, 1] * y + m[1, 2] * z + m[1, 3];
      z = m[2, 0] * x + m[2, 1] * y + m[2, 2] * z + m[2, 3];
      y = newY;
      x = newX;
    }

    private void TransformPositionUnscaled(ref float x, ref float y, ref float z)
    {
      float n;
      // calculate the norm of the transformed (but not translated) vectors involved
      n = (float)Math.Sqrt(m[0, 0] * m[0, 0] + m[0, 1] * m[0, 1] + m[0, 2] * m[0, 2]);
      float newX = (m[0, 0] * x + m[0, 1] * y + m[0, 2] * z) / n + m[0, 3];
      n = (float)Math.Sqrt(m[1, 0] * m[1, 0] + m[1, 1] * m[1, 1] + m[1, 2] * m[1, 2]);
      float newY = (m[1, 0] * x + m[1, 1] * y + m[1, 2] * z) / n + m[1, 3];
      n = (float)Math.Sqrt(m[2, 0] * m[2, 0] + m[2, 1] * m[2, 1] + m[2, 2] * m[2, 2]);
      float newZ = (m[2, 0] * x + m[2, 1] * y + m[2, 2] * z) / n + m[2, 3];
      z = newZ;
      y = newY;
      x = newX;
    }

    private void InverseTransformPosition(ref float x, ref float y)
    {
      // used for mouse - no way to find z
      x -= m[0, 3];
      y -= m[1, 3];
      float detM = m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];
      float newX = (m[1, 1] * x - m[0, 1] * y) / detM;
      y = (-m[1, 0] * x + m[0, 0] * y) / detM;
      x = newX;
    }

    public float TransformXCoord(float x, float y, float z)
    {
      return m[0, 0] * x + m[0, 1] * y + m[0, 2] * z + m[0, 3];
    }

    public float TransformYCoord(float x, float y, float z)
    {
      return m[1, 0] * x + m[1, 1] * y + m[1, 2] * z + m[1, 3];
    }

    public float TransformZCoord(float x, float y, float z)
    {
      return m[2, 0] * x + m[2, 1] * y + m[2, 2] * z + m[2, 3];
    }


    public uint TransformAlpha(uint colour)
    {
      return (uint)(colour * alpha);
    }

    #region ICloneable Members

    public object Clone()
    {
      TransformMatrix matrix = new TransformMatrix();
      matrix.assign(this);
      return matrix;
    }

    public float[,] Matrix
    {
      get { return m; }
    }

    #endregion
  }
}