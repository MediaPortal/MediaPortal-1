/*
 * (C) 2003-2006 Gabest
 * (C) 2006-2012 see Authors.txt
 *
 * This file is part of MPC-HC.
 *
 * MPC-HC is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 3 of the License, or
 * (at your option) any later version.
 *
 * MPC-HC is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 *
 */

#pragma once

#include <math.h>

#ifndef M_PI
#define M_PI 3.14159265358979323846
#define M_PI_2 1.57079632679489661923
#endif

#define EPSILON      (1e-7)
#define BIGNUMBER    (1e+9)
#define IsZero(d)    (fabs(d) < EPSILON)
#define Sgn(d)       (IsZero(d) ? 0 : (d) > 0 ? 1 : -1)
//#define SgnPow(d, p) (IsZero(d) ? 0 : (pow(fabs(d), p) * Sgn(d)))

class Vector
{
public:
    float x, y, z;

    Vector() { x = y = z = 0; }
    Vector(float x, float y, float z);
    void Set(float x, float y, float z);

    Vector Normal(Vector& a, Vector& b);
    float Angle(Vector& a, Vector& b);
    float Angle(Vector& a);
    void Angle(float& u, float& v); // returns spherical coords in radian, -M_PI_2 <= u <= M_PI_2, -M_PI <= v <= M_PI
    Vector Angle();                 // does like prev., returns 'u' in 'ret.x', and 'v' in 'ret.y'

    Vector Unit();
    Vector& Unitalize();
    float Length();
    float Sum();        // x + y + z
    float CrossSum();   // xy + xz + yz
    Vector Cross();     // xy, xz, yz
    Vector Pow(float exp);

    Vector& Min(Vector& a);
    Vector& Max(Vector& a);
    Vector Abs();

    Vector Reflect(Vector& n);
    Vector Refract(Vector& n, float nFront, float nBack, float* nOut = NULL);
    Vector Refract2(Vector& n, float nFrom, float nTo, float* nOut = NULL);

    Vector operator - ();
    float& operator [](size_t i);

    float operator | (Vector& v);   // dot
    Vector operator % (Vector& v);  // cross

    bool operator == (const Vector& v) const;
    bool operator != (const Vector& v) const;

    Vector operator + (float d);
    Vector operator + (Vector& v);
    Vector operator - (float d);
    Vector operator - (Vector& v);
    Vector operator * (float d);
    Vector operator * (Vector& v);
    Vector operator / (float d);
    Vector operator / (Vector& v);
    Vector& operator += (float d);
    Vector& operator += (Vector& v);
    Vector& operator -= (float d);
    Vector& operator -= (Vector& v);
    Vector& operator *= (float d);
    Vector& operator *= (Vector& v);
    Vector& operator /= (float d);
    Vector& operator /= (Vector& v);

    template<typename T> static float DegToRad(T angle) { return (float)(angle * M_PI / 180); }
};

class Ray
{
public:
    Vector p, d;

    Ray() {}
    Ray(Vector& p, Vector& d);
    void Set(Vector& p, Vector& d);

    float GetDistanceFrom(Ray& r);      // r = plane
    float GetDistanceFrom(Vector& v);   // v = point

    Vector operator [](float t);
};

class XForm
{
    class Matrix
    {
    public:
        float mat[4][4];

        Matrix();
        void Initalize();

        Matrix operator * (Matrix& m);
        Matrix& operator *= (Matrix& m);
    } m;

    bool m_isWorldToLocal;

public:
    XForm() : m_isWorldToLocal(true) {}
    XForm(Ray& r, Vector& s, bool isWorldToLocal = true);

    void Initalize();
    void Initalize(Ray& r, Vector& s, bool isWorldToLocal = true);

    void operator *= (Vector& s);   // scale
    void operator += (Vector& t);   // translate
    void operator <<= (Vector& r);  // rotate

    void operator /= (Vector& s);   // scale
    void operator -= (Vector& t);   // translate
    void operator >>= (Vector& r);  // rotate

    //  transformations
    Vector operator < (Vector& n);  // normal
    Vector operator << (Vector& v); // vector
    Ray operator << (Ray& r);       // ray
};
