#pragma once
#ifndef PI
#define PI (3.141592654)
#endif
#define EPSILON (1e-7)
#define BIGNUMBER (1e+9)
#define IsZero(d) (fabs(d) < EPSILON)

#define DegToRad(d) ((d)*PI/180.0)
#define RadToDeg(r) ((r)*180.0/PI)
#define Sgn(d) (IsZero(d) ? 0 : (d) > 0 ? 1 : -1)
#define SgnPow(d, p) (IsZero(d) ? 0 : (pow(fabs(d), p) * Sgn(d)))



class Vector
{
public:
	double x, y, z;

	Vector() {x = y = z = 0;}
	Vector(double x, double y, double z);
	void Set(double x, double y, double z);

	Vector Normal(Vector& a, Vector& b);
	double Angle(Vector& a, Vector& b);
	double Angle(Vector& a);
	void Angle(double& u, double& v); // returns spherical coords in radian, -PI/2 <= u <= PI/2, -PI <= v <= PI
	Vector Angle(); // does like prev., returns 'u' in 'ret.x', and 'v' in 'ret.y'

	Vector Unit();
	Vector& Unitalize();
	double Length();
	double Sum(); // x + y + z
	double CrossSum(); // xy + xz + yz
	Vector Cross(); // xy, xz, yz
	Vector Pow(double exp);

	Vector& Min(Vector& a);
	Vector& Max(Vector& a);
	Vector Abs();

	Vector Reflect(Vector& n);
	Vector Refract(Vector& n, double nFront, double nBack, double* nOut = NULL);
	Vector Refract2(Vector& n, double nFrom, double nTo, double* nOut = NULL);

	Vector operator - ();
	double& operator [] (int i);

	double operator | (Vector& v); // dot
	Vector operator % (Vector& v); // cross

	bool operator == (const Vector& v) const;
	bool operator != (const Vector& v) const;

	Vector operator + (double d);
	Vector operator + (Vector& v);
	Vector operator - (double d);
	Vector operator - (Vector& v);
	Vector operator * (double d);
	Vector operator * (Vector& v);
	Vector operator / (double d);
	Vector operator / (Vector& v);
	Vector& operator += (double d);
	Vector& operator += (Vector& v);
	Vector& operator -= (double d);
	Vector& operator -= (Vector& v);
	Vector& operator *= (double d);
	Vector& operator *= (Vector& v);
	Vector& operator /= (double d);
	Vector& operator /= (Vector& v);
};

