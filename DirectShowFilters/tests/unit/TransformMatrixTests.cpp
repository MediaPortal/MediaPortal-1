// TransformMatrix of fontEngine: the 3x4 matrix and alpha that the skin engine uses to place textures (from XBMC).
#include <gtest/gtest.h>
#include <cmath>
#include "../../fontEngine/source/TransformMatrix.h"

namespace
{
  const float kPi = 3.14159265358979f;

  struct P { float x, y, z; };

  P Apply(const TransformMatrix& m, P p)
  {
    m.TransformPosition(p.x, p.y, p.z);
    return p;
  }

  void ExpectNear(P a, P b, float eps = 1e-4f)
  {
    EXPECT_NEAR(a.x, b.x, eps);
    EXPECT_NEAR(a.y, b.y, eps);
    EXPECT_NEAR(a.z, b.z, eps);
  }
}

TEST(TransformMatrix, NewMatrixIsTheIdentity)
{
  TransformMatrix m;
  ExpectNear(Apply(m, { 3, -4, 5 }), { 3, -4, 5 });
  EXPECT_EQ(m.TransformAlpha(1000), 1000u);
}

TEST(TransformMatrix, CopiesTheThreeByFourArray)
{
  float raw[3][4] = { { 1, 0, 0, 10 }, { 0, 2, 0, 20 }, { 0, 0, 3, 30 } };
  TransformMatrix m(raw);
  ExpectNear(Apply(m, { 1, 1, 1 }), { 11, 22, 33 });
}

TEST(TransformMatrix, Translation)
{
  ExpectNear(Apply(TransformMatrix::CreateTranslation(5, -6, 7), { 1, 1, 1 }), { 6, -5, 8 });
  ExpectNear(Apply(TransformMatrix::CreateTranslation(5, -6), { 1, 1, 1 }), { 6, -5, 1 });
  TransformMatrix m;
  m.SetTranslation(1, 2, 3);
  ExpectNear(Apply(m, { 0, 0, 0 }), { 1, 2, 3 });
}

TEST(TransformMatrix, Scaler)
{
  ExpectNear(Apply(TransformMatrix::CreateScaler(2, 3), { 1, 1, 1 }), { 2, 3, 1 });
  ExpectNear(Apply(TransformMatrix::CreateScaler(2, 3, 4), { 1, 1, 1 }), { 2, 3, 4 });
}

TEST(TransformMatrix, ScalerAroundACenterLeavesTheCenterAlone)
{
  TransformMatrix m;
  m.SetScaler(2, 0.5f, 100, 50);
  ExpectNear(Apply(m, { 100, 50, 0 }), { 100, 50, 0 });
  ExpectNear(Apply(m, { 110, 60, 0 }), { 120, 55, 0 });
}

TEST(TransformMatrix, ZRotationQuarterTurn)
{
  TransformMatrix m = TransformMatrix::CreateZRotation(kPi / 2, 0, 0);
  ExpectNear(Apply(m, { 1, 0, 0 }), { 0, 1, 0 });
  ExpectNear(Apply(m, { 0, 1, 0 }), { -1, 0, 0 });
}

TEST(TransformMatrix, ZRotationAroundACenter)
{
  TransformMatrix m;
  m.SetZRotation(kPi / 2, 10, 20);
  ExpectNear(Apply(m, { 10, 20, 0 }), { 10, 20, 0 });
  ExpectNear(Apply(m, { 11, 20, 0 }), { 10, 21, 0 });
}

TEST(TransformMatrix, ZRotationWithAnAspectRatio)
{
  // the x axis is ar times as long as the y axis on screen: a quarter turn of (1, 0) lands on (0, ar)
  const float ar = 2.0f;
  TransformMatrix m = TransformMatrix::CreateZRotation(kPi / 2, 0, 0, ar);
  P p = Apply(m, { 1, 0, 0 });
  EXPECT_NEAR(p.x, 0.0f, 1e-4f);
  EXPECT_NEAR(p.y, ar, 1e-4f);
}

TEST(TransformMatrix, RotationAndBackIsTheIdentity)
{
  TransformMatrix a = TransformMatrix::CreateZRotation(0.7f, 3, 4, 1.5f);
  TransformMatrix b = TransformMatrix::CreateZRotation(-0.7f, 3, 4, 1.5f);
  ExpectNear(Apply(a * b, { 9, -2, 0 }), { 9, -2, 0 });
}

TEST(TransformMatrix, XAndYRotation)
{
  TransformMatrix x;
  x.SetXRotation(kPi / 2, 0, 0);
  ExpectNear(Apply(x, { 0, 1, 0 }), { 0, 0, 1 });
  TransformMatrix y;
  y.SetYRotation(kPi / 2, 0, 0);
  ExpectNear(Apply(y, { 1, 0, 0 }), { 0, 0, 1 });
  // the center stays where it is
  x.SetXRotation(1.0f, 7, 9);
  ExpectNear(Apply(x, { 5, 7, 9 }), { 5, 7, 9 });
  y.SetYRotation(1.0f, 7, 9);
  ExpectNear(Apply(y, { 7, 5, 9 }), { 7, 5, 9 });
}

TEST(TransformMatrix, ProductAppliesTheRightHandMatrixFirst)
{
  TransformMatrix scale = TransformMatrix::CreateScaler(2, 2);
  TransformMatrix move = TransformMatrix::CreateTranslation(10, 0);
  ExpectNear(Apply(move * scale, { 1, 1, 0 }), { 12, 2, 0 });          // scale, then move
  ExpectNear(Apply(scale * move, { 1, 1, 0 }), { 22, 2, 0 });          // move, then scale
}

TEST(TransformMatrix, ProductOperatorsAgree)
{
  TransformMatrix a = TransformMatrix::CreateZRotation(0.3f, 1, 2);
  TransformMatrix b = TransformMatrix::CreateTranslation(4, 5);
  TransformMatrix c = a * b;
  TransformMatrix d = a;
  d *= b;
  ExpectNear(Apply(c, { 3, 4, 0 }), Apply(d, { 3, 4, 0 }));
  ExpectNear(Apply(c, { 3, 4, 0 }), Apply(a, Apply(b, { 3, 4, 0 })));
}

TEST(TransformMatrix, AlphaMultipliesInAProduct)
{
  TransformMatrix half = TransformMatrix::CreateFader(0.5f);
  TransformMatrix other = TransformMatrix::CreateFader(0.5f);
  EXPECT_EQ((half * other).TransformAlpha(1000), 250u);
  half *= other;
  EXPECT_EQ(half.TransformAlpha(1000), 250u);
  TransformMatrix f;
  f.SetFader(0.1f);
  EXPECT_EQ(f.TransformAlpha(1000), 100u);
}

TEST(TransformMatrix, AssignmentCopiesTheMatrixAndTheAlpha)
{
  TransformMatrix a = TransformMatrix::CreateTranslation(1, 2, 3);
  a *= TransformMatrix::CreateFader(0.5f);
  TransformMatrix b;
  b = a;
  ExpectNear(Apply(b, { 0, 0, 0 }), { 1, 2, 3 });
  EXPECT_EQ(b.TransformAlpha(100), 50u);
  b = b;
  ExpectNear(Apply(b, { 0, 0, 0 }), { 1, 2, 3 });
}

TEST(TransformMatrix, SettersThatBuildAMatrixResetTheAlpha)
{
  TransformMatrix m;
  m.SetFader(0.5f);
  m.SetTranslation(1, 2, 3);
  EXPECT_EQ(m.TransformAlpha(100), 100u);
  m.SetFader(0.5f);
  m.SetScaler(2, 2, 0, 0);
  EXPECT_EQ(m.TransformAlpha(100), 100u);
}

TEST(TransformMatrix, RotationSettersResetTheAlpha)
{
  TransformMatrix m;
  m.SetFader(0.5f);
  m.SetXRotation(1, 0, 0);
  EXPECT_EQ(m.TransformAlpha(100), 100u);
  m.SetFader(0.5f);
  m.SetYRotation(1, 0, 0);
  EXPECT_EQ(m.TransformAlpha(100), 100u);
  m.SetFader(0.5f);
  m.SetZRotation(1, 0, 0);
  EXPECT_EQ(m.TransformAlpha(100), 100u);
}

TEST(TransformMatrix, InverseOfTranslationScaleAndRotation)
{
  // InverseTransformPosition() undoes the 2D part (it is used for mouse positions)
  TransformMatrix m = TransformMatrix::CreateTranslation(30, -10) * TransformMatrix::CreateZRotation(0.4f, 5, 5) * TransformMatrix::CreateScaler(2, 3);
  float x = 7, y = 11, z = 0;
  m.TransformPosition(x, y, z);
  m.InverseTransformPosition(x, y);
  EXPECT_NEAR(x, 7.0f, 1e-3f);
  EXPECT_NEAR(y, 11.0f, 1e-3f);
}

TEST(TransformMatrix, UnscaledTransformKeepsTheSizeButMovesTheOrigin)
{
  TransformMatrix m = TransformMatrix::CreateTranslation(10, 20) * TransformMatrix::CreateScaler(4, 4);
  float x = 1, y = 1, z = 0;
  m.TransformPositionUnscaled(x, y, z);
  EXPECT_NEAR(x, 11.0f, 1e-4f);
  EXPECT_NEAR(y, 21.0f, 1e-4f);
}

TEST(TransformMatrix, CoordinateHelpersMatchTransformPosition)
{
  TransformMatrix m = TransformMatrix::CreateZRotation(0.9f, 2, 3, 1.3f) * TransformMatrix::CreateTranslation(1, 2, 3);
  const P p = Apply(m, { 4, 5, 6 });
  EXPECT_NEAR(m.TransformXCoord(4, 5, 6), p.x, 1e-4f);
  EXPECT_NEAR(m.TransformYCoord(4, 5, 6), p.y, 1e-4f);
  EXPECT_NEAR(m.TransformZCoord(4, 5, 6), p.z, 1e-4f);
  EXPECT_NEAR(m.ScaleFinalXCoord(4, 5), m.TransformXCoord(4, 5, 0), 1e-6f);
  EXPECT_NEAR(m.ScaleFinalYCoord(4, 5), m.TransformYCoord(4, 5, 0), 1e-6f);
  EXPECT_NEAR(m.ScaleFinalZCoord(4, 5), m.TransformZCoord(4, 5, 0), 1e-6f);
}

TEST(TransformMatrix, ResetGoesBackToTheIdentity)
{
  TransformMatrix m = TransformMatrix::CreateTranslation(9, 9, 9);
  m *= TransformMatrix::CreateFader(0.1f);
  m.Reset();
  ExpectNear(Apply(m, { 1, 2, 3 }), { 1, 2, 3 });
  EXPECT_EQ(m.TransformAlpha(100), 100u);
}
