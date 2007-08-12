#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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

namespace MediaPortal.GUI.Library
{
  public class TransformMatrix : ICloneable
  {
    const float DEGREE_TO_RADIAN = 0.01745329f;
    private float[,] m = new float[3, 4];
    private float alpha;
    public TransformMatrix()
    {
      Reset();
    }
    public void Reset()
    {
      m[0,0] = 1.0f; m[0,1] = m[0,2] = m[0,3] = 0;
      m[1,0] = m[1,2] = m[1,3] = 0; m[1,1] = 1.0f;
      m[2,0] = m[2,1] = m[2,3] = 0; m[2,2] = 1.0f;
      alpha = 1.0f;
    }
    public static TransformMatrix CreateScaler(float scaleX, float scaleY, float scaleZ)
    {
      TransformMatrix scaler = new TransformMatrix();
      scaler.m[0, 0] = scaleX;
      scaler.m[1, 1] = scaleY;
      scaler.m[2, 2] = scaleZ;
      return scaler;
    }
   
    void SetScaler(float scaleX, float scaleY, float centerX, float centerY)
    {
      // Trans(centerX,centerY,centerZ)*Scale(scaleX,scaleY,scaleZ)*Trans(-centerX,-centerY,-centerZ)
      float centerZ = 0, scaleZ = 1;
      m[0, 0] = scaleX;  m[0, 1] = 0;        m[0, 2] = 0;       m[0, 3] = centerX*(1 - scaleX); 
      m[1, 0] = 0;       m[1, 1] = scaleY;   m[1, 2] = 0;       m[1, 3] = centerY*(1 - scaleY);
      m[2, 0] = 0;       m[2, 1] = 0;        m[2, 2] = scaleZ;  m[2, 3] = centerZ*(1 - scaleZ);
      alpha = 1.0f;
    }


    public static TransformMatrix CreateTranslation(float transX, float transY, float transZ)
    {
      TransformMatrix translation = new TransformMatrix();
      translation.m[0, 3] = transX;
      translation.m[1, 3] = transY;
      translation.m[2, 3] = transZ;
      return translation;
    }
    public void SetTranslation(float transX, float transY, float transZ)
    {
      m[0, 0] = 1.0f; m[0, 1] = 0.0f; m[0, 2] = 0.0f;  m[0, 3] = transX;
      m[1, 0] = 0.0f; m[1, 1] = 1.0f; m[1, 2] = 0.0f;  m[1, 3] = transY;
      m[2, 0] = 0.0f; m[2, 1] = 0.0f; m[2, 2] = 1.0f;  m[2, 3] = transZ;
      alpha = 1.0f;
    }
    public static TransformMatrix CreateRotationDegrees(float angle)
    {
      angle *= DEGREE_TO_RADIAN;
      TransformMatrix rotation = new TransformMatrix();
      rotation.m[0, 0] = (float)Math.Cos(angle);
      rotation.m[1, 0] = (float)Math.Sin(angle);
      rotation.m[0, 1] = -rotation.m[1, 0];
      rotation.m[1, 1] = rotation.m[0, 0];
      return rotation;
    }
    public static TransformMatrix CreateXRotation(float angle, float y, float z)
    {
      TransformMatrix rot = new TransformMatrix();
      // angle about the X axis, centered at y,z where our coordinate system has aspect ratio ar.
      // Trans(0,y,z)*Scale(1,1/ar,1)*RotateX(angle)*Scale(ar,1,1)*Trans(0,-y,-z);
      float ar = 1.0f;
      float c = (float)Math.Cos(angle); float s = (float)Math.Sin(angle);
      rot.m[0, 0] = ar;  rot.m[0, 1] = 0;       rot.m[0, 2] = 0;        rot.m[0, 3] = 0;
      rot.m[1, 0] = 0;   rot.m[1, 1] = c / ar;  rot.m[1, 2] = -s / ar;  rot.m[1, 3] = (-y * c + s * z) / ar + y;
      rot.m[2, 0] = 0;   rot.m[2, 1] = s;       rot.m[2, 2] = c;        rot.m[2, 3] = (-y * s - c * z) + z;   
      return rot;
    }
    public static TransformMatrix CreateYRotation(float angle, float x, float z)
    {
      TransformMatrix rot = new TransformMatrix();
      float ar = 1.0f;
      float c = (float)Math.Cos(angle); float s = (float)Math.Sin(angle);
      rot.m[0, 0] = c;        rot.m[0, 1] = 0;   rot.m[0, 2] = -s / ar;   rot.m[0, 3] = -x * c + s * z / ar + x;
      rot.m[1, 0] = 0;        rot.m[1, 1] = 1;   rot.m[1, 2] = 0;         rot.m[1, 3] = 0;
      rot.m[2, 0] = ar * s;   rot.m[2, 1] = 0;   rot.m[2, 2] = c;         rot.m[2, 3] = -ar * x * s - c * z + z;
      return rot;
    }
    public static TransformMatrix CreateZRotation(float angle, float x, float y)
    {// angle about the Z axis, centered at x,y where our coordinate system has aspect ratio ar.
     // Trans(x,y,0)*Scale(1/ar,1,1)*RotateZ(angle)*Scale(ar,1,1)*Trans(-x,-y,0)
      TransformMatrix rot = new TransformMatrix();
      float c = (float)Math.Cos(angle); float s = (float)Math.Sin(angle);
      float ar = 1.0f;
      rot.m[0,0] = c;    rot.m[0,1] = -s/ar; rot.m[0,3] = -x*c + s*y/ar + x;
      rot.m[1,0] = s*ar; rot.m[1,1] = c;     rot.m[1,3] = -ar*x*s - c*y + y;
      return rot;
    }
    public void SetXRotation(float angle, float y, float z)
    {// angle about the X axis, centered at y,z where our coordinate system has aspect ratio ar.
      // Trans(0,y,z)*Scale(1,1/ar,1)*RotateX(angle)*Scale(ar,1,1)*Trans(0,-y,-z);
      float ar = 1.0f;
      float c = (float)Math.Cos(angle); float s = (float)Math.Sin(angle);
      m[0,0] = ar;  m[0,1] = 0;        m[0,2] = 0;         m[0,3] = 0;
      m[1,0] = 0;   m[1,1] = c / ar;   m[1,2] = -s / ar;   m[1,3] = (-y * c + s * z) / ar + y;
      m[2,0] = 0;   m[2,1] = s;        m[2,2] = c;         m[2,3] = (-y * s - c * z) + z; 
      alpha = 1.0f;
    }
    public void SetYRotation(float angle, float x, float z)
    { // angle about the Y axis, centered at x,z where our coordinate system has aspect ratio ar.
      // Trans(x,0,z)*Scale(1/ar,1,1)*RotateY(angle)*Scale(ar,1,1)*Trans(-x,0,-z);
      float ar = 1.0f;
      float c = (float)Math.Cos(angle); float s = (float)Math.Sin(angle);
      m[0,0] = c;      m[0,1] = 0;   m[0,2] = -s / ar;    m[0,3] = -x * c + s * z / ar + x;
      m[1,0] = 0;      m[1,1] = 1;   m[1,2] = 0;          m[1,3] = 0;
      m[2,0] = ar * s; m[2,1] = 0;   m[2,2] = c;          m[2,3] = -ar * x * s - c * z + z;
      alpha = 1.0f;
    }
    public void SetZRotation(float angle, float x, float y)
    {
      float ar = 1.0f;
      float c = (float)Math.Cos(angle); float s = (float)Math.Sin(angle);
      m[0,0] = c;    m[0,1] = -s/ar; m[0,2] = 0; m[0,3] = -x*c + s*y/ar + x;
      m[1,0] = s*ar; m[1,1] = c;     m[1,2] = 0; m[1,3] = -ar*x*s - c*y + y;
      m[2,0] = 0;    m[2,1] = 0;     m[2,2] = 1; m[2,3] = 0;
      alpha = 1.0f;
    }

    static TransformMatrix CreateFader(float a)
    {
      TransformMatrix fader = new TransformMatrix();
      fader.alpha = a;
      return fader;
    }
    public void SetFader(float a)
    {
      m[0,0] = 1.0f; m[0,1] = 0.0f; m[0,2] = 0.0f; m[0,3] = 0.0f;
      m[1,0] = 0.0f; m[1,1] = 1.0f; m[1,2] = 0.0f; m[1,3] = 0.0f;
      m[2,0] = 0.0f; m[2,1] = 0.0f; m[2,2] = 1.0f; m[2,3] = 0.0f;
      alpha = a;
    }
    // assignment operator
    TransformMatrix assign(TransformMatrix right)
    {
      m[0,0]=right.m[0,0];  m[0,1]=right.m[0,1];  m[0,2]=right.m[0,2];  m[0,3]=right.m[0,3];
      m[1,0]=right.m[1,0];  m[1,1]=right.m[1,1];  m[1,2]=right.m[1,2];  m[1,3]=right.m[1,3];
      m[2,0]=right.m[2,0];  m[2,1]=right.m[2,1];  m[2,2]=right.m[2,2];  m[2,3]=right.m[2,3];
      alpha = right.alpha;
      return this;
    }

    // multiplication operators
    public TransformMatrix multiplyAssign(TransformMatrix right)
    {
      float t00 = m[0,0] * right.m[0,0] + m[0,1] * right.m[1,0] + m[0,2] * right.m[2,0];
      float t01 = m[0,0] * right.m[0,1] + m[0,1] * right.m[1,1] + m[0,2] * right.m[2,1];
      float t02 = m[0,0] * right.m[0,2] + m[0,1] * right.m[1,2] + m[0,2] * right.m[2,2];
      m[0,3] = m[0,0] * right.m[0,3] + m[0,1] * right.m[1,3] + m[0,2] * right.m[2,3] + m[0,3];
      m[0,0] = t00; m[0,1] = t01; m[0,2] = t02;
      
      t00 = m[1,0] * right.m[0,0] + m[1,1] * right.m[1,0] + m[1,2] * right.m[2,0];
      t01 = m[1,0] * right.m[0,1] + m[1,1] * right.m[1,1] + m[1,2] * right.m[2,1];
      t02 = m[1,0] * right.m[0,2] + m[1,1] * right.m[1,2] + m[1,2] * right.m[2,2];
      m[1,3] = m[1,0] * right.m[0,3] + m[1,1] * right.m[1,3] + m[1,2] * right.m[2,3] + m[1,3];
      m[1,0] = t00; m[1,1] = t01; m[1,2] = t02;
      
      t00 = m[2,0] * right.m[0,0] + m[2,1] * right.m[1,0] + m[2,2] * right.m[2,0];
      t01 = m[2,0] * right.m[0,1] + m[2,1] * right.m[1,1] + m[2,2] * right.m[2,1];
      t02 = m[2,0] * right.m[0,2] + m[2,1] * right.m[1,2] + m[2,2] * right.m[2,2];
      m[2,3] = m[2,0] * right.m[0,3] + m[2,1] * right.m[1,3] + m[2,2] * right.m[2,3] + m[2,3];
      m[2,0] = t00; m[2,1] = t01; m[2,2] = t02;
    
      alpha *= right.alpha;
      return this;
    }

    public TransformMatrix multiply(TransformMatrix right)
    {
      TransformMatrix result = new TransformMatrix();
      result.m[0,0] = m[0,0] * right.m[0,0] + m[0,1] * right.m[1,0] + m[0,2] * right.m[2,0];
      result.m[0,1] = m[0,0] * right.m[0,1] + m[0,1] * right.m[1,1] + m[0,2] * right.m[2,1];
      result.m[0,2] = m[0,0] * right.m[0,2] + m[0,1] * right.m[1,2] + m[0,2] * right.m[2,2];
      result.m[0,3] = m[0,0] * right.m[0,3] + m[0,1] * right.m[1,3] + m[0,2] * right.m[2,3] + m[0,3];
      result.m[1,0] = m[1,0] * right.m[0,0] + m[1,1] * right.m[1,0] + m[1,2] * right.m[2,0];
      result.m[1,1] = m[1,0] * right.m[0,1] + m[1,1] * right.m[1,1] + m[1,2] * right.m[2,1];
      result.m[1,2] = m[1,0] * right.m[0,2] + m[1,1] * right.m[1,2] + m[1,2] * right.m[2,2];
      result.m[1,3] = m[1,0] * right.m[0,3] + m[1,1] * right.m[1,3] + m[1,2] * right.m[2,3] + m[1,3];
      result.m[2,0] = m[2,0] * right.m[0,0] + m[2,1] * right.m[1,0] + m[2,2] * right.m[2,0];
      result.m[2,1] = m[2,0] * right.m[0,1] + m[2,1] * right.m[1,1] + m[2,2] * right.m[2,1];
      result.m[2,2] = m[2,0] * right.m[0,2] + m[2,1] * right.m[1,2] + m[2,2] * right.m[2,2];
      result.m[2,3] = m[2,0] * right.m[0,3] + m[2,1] * right.m[1,3] + m[2,2] * right.m[2,3] + m[2,3];
      result.alpha = alpha * right.alpha;
      return result;
    }

    public void TransformPosition(ref float x, ref float y, ref float z)
    {
      float newX = m[0,0] * x + m[0,1] * y + m[0,2] * z + m[0,3];
      float newY = m[1,0] * x + m[1,1] * y + m[1,2] * z + m[1,3];
      z = m[2,0] * x + m[2,1] * y + m[2,2] * z + m[2,3];
      y = newY;
      x = newX;
    }

    public float TransformXCoord(float x, float y, float z)
    {
      return m[0,0] * x + m[0,1] * y + m[0,2] * z + m[0,3];
    }

    public float TransformYCoord(float x, float y, float z)
    {
      return m[1,0] * x + m[1,1] * y + m[1,2] * z + m[1,3];
    }

    public float TransformZCoord(float x, float y, float z)
    {
      return m[2,0] * x + m[2,1] * y + m[2,2] * z + m[2,3];
    }

    public uint TransformAlpha(uint colour)
    {
      return (uint)(colour * alpha);
    }
    public void GetScaling(out  float m00, out  float m01, out  float m02, out  float m03,
                           out  float m10, out  float m11, out  float m12, out  float m13,
                           out  float m20, out  float m21, out  float m22, out  float m23 )
    {
      m00 = m[0, 0];  m01 = m[0, 1];  m02 = m[0, 2];  m03 = m[0, 3];
      m10 = m[1, 0];  m11 = m[1, 1];  m12 = m[1, 2];  m13 = m[1, 3];
      m20 = m[2, 0];  m21 = m[2, 1];  m22 = m[2, 2];  m23 = m[2, 3];
    }

    #region ICloneable Members

    public object Clone()
    {
      TransformMatrix matrix = new TransformMatrix();
      matrix.m[0,0]=m[0,0];  matrix.m[0,1]=m[0,1];  matrix.m[0,2]=m[0,2];  matrix.m[0,3]=m[0,3];
      matrix.m[1,0]=m[1,0];  matrix.m[1,1]=m[1,1];  matrix.m[1,2]=m[1,2];  matrix.m[1,3]=m[1,3];
      matrix.m[2,0]=m[2,0];  matrix.m[2,1]=m[2,1];  matrix.m[2,2]=m[2,2];  matrix.m[2,3]=m[2,3];
      matrix.alpha = alpha;
      return matrix;
    }

    #endregion
  }
}
