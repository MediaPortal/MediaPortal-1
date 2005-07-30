/* 
 *	Copyright (C) 2005 Media Portal
 *  Author: Frodo
 *	http://mediaportal.sourceforge.net
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
#include "StdAfx.h"
#include ".\vector.h"

Vector::Vector(double x, double y, double z)
{
	this->x = x;
	this->y = y;
	this->z = z;
}

void Vector::Set(double x, double y, double z)
{
	this->x = x;
	this->y = y;
	this->z = z;
}

double Vector::Length()
{
	return(sqrt(x * x + y * y + z * z));
}

double Vector::Sum()
{
	return(x + y + z);
}

double Vector::CrossSum()
{
	return(x*y + x*z + y*z);
}

Vector Vector::Cross()
{
	return(Vector(x*y, x*z, y*z));
}

Vector Vector::Pow(double exp)
{
	return(exp == 0 ? Vector(1, 1, 1) : exp == 1 ? *this : Vector(pow(x, exp), pow(y, exp), pow(z, exp)));
}

Vector Vector::Unit()
{
	double l = Length();
	if(!l || l == 1) return(*this);
	return(*this * (1 / l));
}

Vector& Vector::Unitalize()
{
	return(*this = Unit());
}

Vector Vector::Normal(Vector& a, Vector& b)
{
	return((a - *this) % (b - a));
}

double Vector::Angle(Vector& a, Vector& b)
{
	return(((a - *this).Unit()).Angle((b - *this).Unit()));
}

double Vector::Angle(Vector& a)
{
	double angle = *this | a;
	return((angle > 1) ? 0 : (angle < -1) ? PI : acos(angle));
}

void Vector::Angle(double& u, double& v)
{
	Vector n = Unit();

	u = asin(n.y);

	if(IsZero(n.z)) v = PI/2 * Sgn(n.x);
	else if(n.z > 0) v = atan(n.x / n.z);
	else if(n.z < 0) v = IsZero(n.x) ? PI : (PI * Sgn(n.x) + atan(n.x / n.z));
}

Vector Vector::Angle()
{
	Vector ret;
	Angle(ret.x, ret.y);
	ret.z = 0;
	return(ret);
}

Vector& Vector::Min(Vector& a)
{
	x = (x < a.x) ? x : a.x;
	y = (y < a.y) ? y : a.y;
	z = (z < a.z) ? z : a.z;
	return(*this);
}

Vector& Vector::Max(Vector& a)
{
	x = (x > a.x) ? x : a.x;
	y = (y > a.y) ? y : a.y;
	z = (z > a.z) ? z : a.z;
	return(*this);
}

Vector Vector::Abs()
{
	return(Vector(fabs(x), fabs(y), fabs(z)));
}

Vector Vector::Reflect(Vector& n)
{
	return(n * ((-*this) | n) * 2 - (-*this));
}

Vector Vector::Refract(Vector& N, double nFront, double nBack, double* nOut)
{
	Vector D = -*this;

	double N_dot_D = (N | D);
	double n = N_dot_D >= 0 ? (nFront / nBack) : (nBack / nFront);

	Vector cos_D = N * N_dot_D;
	Vector sin_T = (cos_D - D) * n;

	double len_sin_T = sin_T | sin_T;

	if(len_sin_T > 1) 
	{
		if(nOut) {*nOut = N_dot_D >= 0 ? nFront : nBack;}
		return((*this).Reflect(N));
	}

	double N_dot_T = sqrt(1.0 - len_sin_T);
	if(N_dot_D < 0) N_dot_T = -N_dot_T;

	if(nOut) {*nOut = N_dot_D >= 0 ? nBack : nFront;}

	return(sin_T - (N * N_dot_T));
}

Vector Vector::Refract2(Vector& N, double nFrom, double nTo, double* nOut)
{
	Vector D = -*this;

	double N_dot_D = (N | D);
	double n = nFrom / nTo;

	Vector cos_D = N * N_dot_D;
	Vector sin_T = (cos_D - D) * n;

	double len_sin_T = sin_T | sin_T;

	if(len_sin_T > 1) 
	{
		if(nOut) {*nOut = nFrom;}
		return((*this).Reflect(N));
	}

	double N_dot_T = sqrt(1.0 - len_sin_T);
	if(N_dot_D < 0) N_dot_T = -N_dot_T;

	if(nOut) {*nOut = nTo;}

	return(sin_T - (N * N_dot_T));
}

double Vector::operator | (Vector& v)
{
	return(x * v.x + y * v.y + z * v.z);
}

Vector Vector::operator % (Vector& v)
{
	return(Vector(y * v.z - z * v.y, z * v.x - x * v.z, x * v.y - y * v.x));
}

double& Vector::operator [] (int i)
{
	return(!i ? x : (i == 1) ? y : z);
}

Vector Vector::operator - ()
{
	return(Vector(-x, -y, -z));
}

bool Vector::operator == (const Vector& v) const
{
	if(IsZero(x - v.x) && IsZero(y - v.y) && IsZero(z - v.z)) return(true);
	return(false);
}

bool Vector::operator != (const Vector& v) const
{
	return((*this == v) ? false : true);
}

Vector Vector::operator + (double d)
{
	return(Vector(x + d, y + d, z + d));
}

Vector Vector::operator + (Vector& v)
{
	return(Vector(x + v.x, y + v.y, z + v.z));
}

Vector Vector::operator - (double d)
{
	return(Vector(x - d, y - d, z - d));
}

Vector Vector::operator - (Vector& v)
{
	return(Vector(x - v.x, y - v.y, z - v.z));
}

Vector Vector::operator * (double d)
{
	return(Vector(x * d, y * d, z * d));
}

Vector Vector::operator * (Vector& v)
{
	return(Vector(x * v.x, y * v.y, z * v.z));
}

Vector Vector::operator / (double d)
{
	return(Vector(x / d, y / d, z / d));
}

Vector Vector::operator / (Vector& v)
{
	return(Vector(x / v.x, y / v.y, z / v.z));
}

Vector& Vector::operator += (double d)
{
	x += d; y += d; z += d;
	return(*this);
}

Vector& Vector::operator += (Vector& v)
{
	x += v.x; y += v.y; z += v.z;
	return(*this);
}

Vector& Vector::operator -= (double d)
{
	x -= d; y -= d; z -= d;
	return(*this);
}

Vector& Vector::operator -= (Vector& v)
{
	x -= v.x; y -= v.y; z -= v.z;
	return(*this);
}

Vector& Vector::operator *= (double d)
{
	x *= d; y *= d; z *= d;
	return(*this);
}

Vector& Vector::operator *= (Vector& v)
{
	x *= v.x; y *= v.y; z *= v.z;
	return(*this);
}

Vector& Vector::operator /= (double d)
{
	x /= d; y /= d; z /= d;
	return(*this);
}

Vector& Vector::operator /= (Vector& v)
{
	x /= v.x; y /= v.y; z /= v.z;
	return(*this);
}

