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
  public class TransformMatrix
  {
    const float DEGREE_TO_RADIAN = 0.01745329f;
    private float[,] m = new float[2, 3];
    private float alpha;
    public TransformMatrix()
    {
      m[0, 0] = 1.0f; m[0, 1] = m[0, 2] = 0;
      m[1, 0] = m[1, 2] = 0; m[1, 1] = 1.0f;
      alpha = 1.0f;
    }

    public static TransformMatrix CreateScaler(float scaleX, float scaleY)
    {
      TransformMatrix scaler = new TransformMatrix();
      scaler.m[0, 0] = scaleX;
      scaler.m[1, 1] = scaleY;
      return scaler;
    }
    void SetScaler(float scaleX, float scaleY)
    {
      m[0, 0] = scaleX; m[0, 1] = m[0, 2] = 0;
      m[1, 1] = scaleY; m[1, 0] = m[1, 2] = 0;
      alpha = 1.0f;
    }
    public static TransformMatrix CreateTranslation(float transX, float transY)
    {
      TransformMatrix translation = new TransformMatrix();
      translation.m[0, 2] = transX;
      translation.m[1, 2] = transY;
      return translation;
    }
    public void SetTranslation(float transX, float transY)
    {
      m[0, 0] = 1.0f; m[0, 1] = 0.0f; m[0, 2] = transX;
      m[1, 0] = 0.0f; m[1, 1] = 1.0f; m[1, 2] = transY;
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
    public static TransformMatrix CreateRotation(float angle)
    {
      TransformMatrix rotation = new TransformMatrix();
      rotation.m[0, 0] = (float)Math.Cos(angle);
      rotation.m[1, 0] = (float)Math.Sin(angle);
      rotation.m[0, 1] = -rotation.m[1, 0];
      rotation.m[1, 1] = rotation.m[0, 0];
      return rotation;
    }
    void SetRotation(float angle)
    {
      m[0, 0] = (float)Math.Cos(angle); m[0, 1] = -(float)Math.Sin(angle); m[0, 2] = 0.0f;
      m[1, 0] = -m[0, 1]; m[1, 1] = m[0, 0]; m[1, 2] = 0.0f;
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
      m[0, 0] = 1.0f; m[0, 1] = 0.0f; m[0, 2] = 0.0f;
      m[1, 0] = 0.0f; m[1, 1] = 1.0f; m[1, 2] = 0.0f;
      alpha = a;
    }
    // assignment operator
    TransformMatrix assign(TransformMatrix right)
    {
      m[0,0]=right.m[0,0];
      m[0,1]=right.m[0,1];
      m[0,2]=right.m[0,2];
      m[1,0]=right.m[1,0];
      m[1,1]=right.m[1,1];
      m[1,2]=right.m[1,2];
      alpha = right.alpha;
      return this;
    }

    // multiplication operators
    public TransformMatrix multiplyAssign(TransformMatrix right)
    {
      float t00 = m[0, 0] * right.m[0, 0] + m[0, 1] * right.m[1, 0];
      float t01 = m[0, 0] * right.m[0, 1] + m[0, 1] * right.m[1, 1];
      m[0, 2] = m[0, 0] * right.m[0, 2] + m[0, 1] * right.m[1, 2] + m[0, 2];
      m[0, 0] = t00; m[0, 1] = t01;
      t00 = m[1, 0] * right.m[0, 0] + m[1, 1] * right.m[1, 0];
      t01 = m[1, 0] * right.m[0, 1] + m[1, 1] * right.m[1, 1];
      m[1, 2] = m[1, 0] * right.m[0, 2] + m[1, 1] * right.m[1, 2] + m[1, 2];
      m[1, 0] = t00; m[1, 1] = t01;
      alpha *= right.alpha;
      return this;
    }

    public TransformMatrix multiply(TransformMatrix right)
    {
      TransformMatrix result = new TransformMatrix();
      result.m[0, 0] = m[0, 0] * right.m[0, 0] + m[0, 1] * right.m[1, 0];
      result.m[0, 1] = m[0, 0] * right.m[0, 1] + m[0, 1] * right.m[1, 1];
      result.m[0, 2] = m[0, 0] * right.m[0, 2] + m[0, 1] * right.m[1, 2] + m[0, 2];
      result.m[1, 0] = m[1, 0] * right.m[0, 0] + m[1, 1] * right.m[1, 0];
      result.m[1, 1] = m[1, 0] * right.m[0, 1] + m[1, 1] * right.m[1, 1];
      result.m[1, 2] = m[1, 0] * right.m[0, 2] + m[1, 1] * right.m[1, 2] + m[1, 2];
      result.alpha = alpha * right.alpha;
      return result;
    }

    public void TransformPosition(ref float x, ref float y)
    {
      float newX = m[0, 0] * x + m[0, 1] * y + m[0, 2];
      y = m[1, 0] * x + m[1, 1] * y + m[1, 2];
      x = newX;
    }

    public float TransformXCoord(float x, float y)
    {
      return m[0, 0] * x + m[0, 1] * y + m[0, 2];
    }

    public float TransformYCoord(float x, float y)
    {
      return m[1, 0] * x + m[1, 1] * y + m[1, 2];
    }

    public uint TransformAlpha(uint colour)
    {
      return (uint)(colour * alpha);
    }
    public void GetScaling(out  float m00, out  float m01, out  float m02, out  float m10, out  float m11, out  float m12)
    {
      m00 = m[0, 0];
      m01 = m[0, 1];
      m02 = m[0, 2];

      m10 = m[1, 0];
      m11 = m[1, 1];
      m12 = m[1, 2];
    }
  }
}
