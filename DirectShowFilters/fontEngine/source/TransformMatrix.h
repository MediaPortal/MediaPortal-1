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

#pragma once

#include <math.h>
#include <memory>

class TransformMatrix
{
public:
  TransformMatrix()
  {
    Reset();
  };
  TransformMatrix(float morg[3][4])
  {
    m[0][0] =morg[0][0];
    m[0][1] =morg[0][1];
    m[0][2] =morg[0][2];
    m[0][3] =morg[0][3];

    m[1][0] =morg[1][0];
    m[1][1] =morg[1][1];
    m[1][2] =morg[1][2];
    m[1][3] =morg[1][3];

    m[2][0] =morg[2][0];
    m[2][1] =morg[2][1];
    m[2][2] =morg[2][2];
    m[2][3] =morg[2][3];
  }
  void Reset()
  {
    m[0][0] = 1.0f; m[0][1] = m[0][2] = m[0][3] = 0;
    m[1][0] = m[1][2] = m[1][3] = 0; m[1][1] = 1.0f;
    m[2][0] = m[2][1] = m[2][3] = 0; m[2][2] = 1.0f;
    alpha = 1.0f;
  };
  static TransformMatrix CreateTranslation(float transX, float transY, float transZ = 0)
  {
    TransformMatrix translation;
    translation.m[0][3] = transX;
    translation.m[1][3] = transY;
    translation.m[2][3] = transZ;
    return translation;
  }
  void SetTranslation(float transX, float transY, float transZ)
  {
    m[0][1] = m[0][2] = 0.0f; m[0][0] = 1.0f; m[0][3] = transX;
    m[1][0] = m[1][2] = 0.0f; m[1][1] = 1.0f; m[1][3] = transY;
    m[2][0] = m[2][1] = 0.0f; m[2][2] = 1.0f; m[2][3] = transZ;
    alpha = 1.0f;
  }
  static TransformMatrix CreateScaler(float scaleX, float scaleY, float scaleZ = 1.0f)
  {
    TransformMatrix scaler;
    scaler.m[0][0] = scaleX;
    scaler.m[1][1] = scaleY;
    scaler.m[2][2] = scaleZ;
    return scaler;
  };
  void SetScaler(float scaleX, float scaleY, float centerX, float centerY)
  {
    // Trans(centerX,centerY,centerZ)*Scale(scaleX,scaleY,scaleZ)*Trans(-centerX,-centerY,-centerZ)
    float centerZ = 0, scaleZ = 1;
    m[0][0] = scaleX; m[0][1] = 0;      m[0][2] = 0;      m[0][3] = centerX*(1-scaleX);
    m[1][0] = 0;      m[1][1] = scaleY; m[1][2] = 0;      m[1][3] = centerY*(1-scaleY);
    m[2][0] = 0;      m[2][1] = 0;      m[2][2] = scaleZ; m[2][3] = centerZ*(1-scaleZ);
    alpha = 1.0f;
  };
  void SetXRotation(float angle, float y, float z, float ar = 1.0f)
  { // angle about the X axis, centered at y,z where our coordinate system has aspect ratio ar.
    // Trans(0,y,z)*Scale(1,1/ar,1)*RotateX(angle)*Scale(ar,1,1)*Trans(0,-y,-z);
    float c = cos(angle); float s = sin(angle);
    m[0][0] = ar; m[0][1] = 0;    m[0][2] = 0;     m[0][3] = 0;
    m[1][0] = 0;  m[1][1] = c/ar; m[1][2] = -s/ar; m[1][3] = (-y*c+s*z)/ar + y;
    m[2][0] = 0;  m[2][1] = s;    m[2][2] = c;     m[2][3] = (-y*s-c*z) + z;
    angle = 1.0f;
  }
  void SetYRotation(float angle, float x, float z, float ar = 1.0f)
  { // angle about the Y axis, centered at x,z where our coordinate system has aspect ratio ar.
    // Trans(x,0,z)*Scale(1/ar,1,1)*RotateY(angle)*Scale(ar,1,1)*Trans(-x,0,-z);
    float c = cos(angle); float s = sin(angle);
    m[0][0] = c;    m[0][1] = 0; m[0][2] = -s/ar; m[0][3] = -x*c + s*z/ar + x;
    m[1][0] = 0;    m[1][1] = 1; m[1][2] = 0;     m[1][3] = 0;
    m[2][0] = ar*s; m[2][1] = 0; m[2][2] = c;     m[2][3] = -ar*x*s - c*z + z;
    angle = 1.0f;
  }
  static TransformMatrix CreateZRotation(float angle, float x, float y, float ar = 1.0f)
  { // angle about the Z axis, centered at x,y where our coordinate system has aspect ratio ar.
    // Trans(x,y,0)*Scale(1/ar,1,1)*RotateZ(angle)*Scale(ar,1,1)*Trans(-x,-y,0)
    float c = cos(angle); float s = sin(angle);
    TransformMatrix rot;
    rot.m[0][0] = c;    rot.m[0][1] = -s/ar; rot.m[0][3] = -x*c + s*y/ar + x;
    rot.m[1][0] = s*ar; rot.m[1][1] = c;     rot.m[1][3] = -ar*x*s - c*y + y;
    return rot;
  }
  void SetZRotation(float angle, float x, float y, float ar = 1.0f)
  { // angle about the Z axis, centered at x,y where our coordinate system has aspect ratio ar.
    // Trans(x,y,0)*Scale(1/ar,1,1)*RotateZ(angle)*Scale(ar,1,1)*Trans(-x,-y,0)
    float c = cos(angle); float s = sin(angle);
    m[0][0] = c;    m[0][1] = -s/ar; m[0][2] = 0; m[0][3] = -x*c + s*y/ar + x;
    m[1][0] = s*ar; m[1][1] = c;     m[1][2] = 0; m[1][3] = -ar*x*s - c*y + y;
    m[2][0] = 0;    m[2][1] = 0;     m[2][2] = 1; m[2][3] = 0;
    angle = 1.0f;
  }
  static TransformMatrix CreateFader(float a)
  {
    TransformMatrix fader;
    fader.alpha = a;
    return fader;
  }
  void SetFader(float a)
  {
    m[0][0] = 1.0f; m[0][1] = 0.0f; m[0][2] = 0.0f; m[0][3] = 0.0f;
    m[1][0] = 0.0f; m[1][1] = 1.0f; m[1][2] = 0.0f; m[1][3] = 0.0f;
    m[2][0] = 0.0f; m[2][1] = 0.0f; m[2][2] = 1.0f; m[2][3] = 0.0f;
    alpha = a;
  }
  // assignment operator
  const TransformMatrix &operator =(const TransformMatrix &right)
  {
    if (this != &right)
    {
      memcpy(m, right.m, 12*sizeof(float));
      alpha = right.alpha;
    }
    return *this;
  }

  // multiplication operators
  const TransformMatrix &operator *=(const TransformMatrix &right)
  {
    float t00 = m[0][0] * right.m[0][0] + m[0][1] * right.m[1][0] + m[0][2] * right.m[2][0];
    float t01 = m[0][0] * right.m[0][1] + m[0][1] * right.m[1][1] + m[0][2] * right.m[2][1];
    float t02 = m[0][0] * right.m[0][2] + m[0][1] * right.m[1][2] + m[0][2] * right.m[2][2];
    m[0][3] = m[0][0] * right.m[0][3] + m[0][1] * right.m[1][3] + m[0][2] * right.m[2][3] + m[0][3];
    m[0][0] = t00; m[0][1] = t01; m[0][2] = t02;
    t00 = m[1][0] * right.m[0][0] + m[1][1] * right.m[1][0] + m[1][2] * right.m[2][0];
    t01 = m[1][0] * right.m[0][1] + m[1][1] * right.m[1][1] + m[1][2] * right.m[2][1];
    t02 = m[1][0] * right.m[0][2] + m[1][1] * right.m[1][2] + m[1][2] * right.m[2][2];
    m[1][3] = m[1][0] * right.m[0][3] + m[1][1] * right.m[1][3] + m[1][2] * right.m[2][3] + m[1][3];
    m[1][0] = t00; m[1][1] = t01; m[1][2] = t02;
    t00 = m[2][0] * right.m[0][0] + m[2][1] * right.m[1][0] + m[2][2] * right.m[2][0];
    t01 = m[2][0] * right.m[0][1] + m[2][1] * right.m[1][1] + m[2][2] * right.m[2][1];
    t02 = m[2][0] * right.m[0][2] + m[2][1] * right.m[1][2] + m[2][2] * right.m[2][2];
    m[2][3] = m[2][0] * right.m[0][3] + m[2][1] * right.m[1][3] + m[2][2] * right.m[2][3] + m[2][3];
    m[2][0] = t00; m[2][1] = t01; m[2][2] = t02;
    alpha *= right.alpha;
    return *this;
  }

  TransformMatrix operator *(const TransformMatrix &right) const
  {
    TransformMatrix result;
    result.m[0][0] = m[0][0] * right.m[0][0] + m[0][1] * right.m[1][0] + m[0][2] * right.m[2][0];
    result.m[0][1] = m[0][0] * right.m[0][1] + m[0][1] * right.m[1][1] + m[0][2] * right.m[2][1];
    result.m[0][2] = m[0][0] * right.m[0][2] + m[0][1] * right.m[1][2] + m[0][2] * right.m[2][2];
    result.m[0][3] = m[0][0] * right.m[0][3] + m[0][1] * right.m[1][3] + m[0][2] * right.m[2][3] + m[0][3];
    result.m[1][0] = m[1][0] * right.m[0][0] + m[1][1] * right.m[1][0] + m[1][2] * right.m[2][0];
    result.m[1][1] = m[1][0] * right.m[0][1] + m[1][1] * right.m[1][1] + m[1][2] * right.m[2][1];
    result.m[1][2] = m[1][0] * right.m[0][2] + m[1][1] * right.m[1][2] + m[1][2] * right.m[2][2];
    result.m[1][3] = m[1][0] * right.m[0][3] + m[1][1] * right.m[1][3] + m[1][2] * right.m[2][3] + m[1][3];
    result.m[2][0] = m[2][0] * right.m[0][0] + m[2][1] * right.m[1][0] + m[2][2] * right.m[2][0];
    result.m[2][1] = m[2][0] * right.m[0][1] + m[2][1] * right.m[1][1] + m[2][2] * right.m[2][1];
    result.m[2][2] = m[2][0] * right.m[0][2] + m[2][1] * right.m[1][2] + m[2][2] * right.m[2][2];
    result.m[2][3] = m[2][0] * right.m[0][3] + m[2][1] * right.m[1][3] + m[2][2] * right.m[2][3] + m[2][3];
    result.alpha = alpha * right.alpha;
    return result;
  }

  inline void TransformPosition(float &x, float &y, float &z) const
  {
    float newX = m[0][0] * x + m[0][1] * y + m[0][2] * z + m[0][3];
    float newY = m[1][0] * x + m[1][1] * y + m[1][2] * z + m[1][3];
    z = m[2][0] * x + m[2][1] * y + m[2][2] * z + m[2][3];
    y = newY;
    x = newX;
  }

  inline void TransformPositionUnscaled(float &x, float &y, float &z) const
  {
    float n;
    // calculate the norm of the transformed (but not translated) vectors involved
    n = sqrt(m[0][0]*m[0][0] + m[0][1]*m[0][1] + m[0][2]*m[0][2]);
    float newX = (m[0][0] * x + m[0][1] * y + m[0][2] * z)/n + m[0][3];
    n = sqrt(m[1][0]*m[1][0] + m[1][1]*m[1][1] + m[1][2]*m[1][2]);
    float newY = (m[1][0] * x + m[1][1] * y + m[1][2] * z)/n + m[1][3];
    n = sqrt(m[2][0]*m[2][0] + m[2][1]*m[2][1] + m[2][2]*m[2][2]);
    float newZ = (m[2][0] * x + m[2][1] * y + m[2][2] * z)/n + m[2][3];
    z = newZ;
    y = newY;
    x = newX;
  }

  inline void InverseTransformPosition(float &x, float &y) const
  { // used for mouse - no way to find z
    x -= m[0][3]; y -= m[1][3];
    float detM = m[0][0]*m[1][1] - m[0][1]*m[1][0];
    float newX = (m[1][1] * x - m[0][1] * y)/detM;
    y = (-m[1][0] * x + m[0][0] * y)/detM;
    x = newX;
  }

  inline float TransformXCoord(float x, float y, float z) const
  {
    return m[0][0] * x + m[0][1] * y + m[0][2] * z + m[0][3];
  }

  inline float TransformYCoord(float x, float y, float z) const
  {
    return m[1][0] * x + m[1][1] * y + m[1][2] * z + m[1][3];
  }

  inline float TransformZCoord(float x, float y, float z) const
  {
    return m[2][0] * x + m[2][1] * y + m[2][2] * z + m[2][3];
  }

  inline unsigned int TransformAlpha(unsigned int colour) const
  {
    return (unsigned int)(colour * alpha);
  }
  inline float ScaleFinalXCoord(float x, float y) const { return TransformXCoord(x, y, 0); }
  inline float ScaleFinalYCoord(float x, float y) const { return TransformYCoord(x, y, 0); }
  inline float ScaleFinalZCoord(float x, float y) const { return TransformZCoord(x, y, 0); }

private:
  float m[3][4];
  float alpha;
};
