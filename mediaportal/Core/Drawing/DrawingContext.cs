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
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MediaPortal.Drawing.Transforms;

namespace MediaPortal.Drawing
{
  public abstract class DrawingContext : DispatcherObject, IDisposable
  {
    #region Methods

    public abstract void Close();

    void IDisposable.Dispose()
    {
      DisposeCore();
    }

    protected abstract void DisposeCore();

//		public abstract void DrawDrawing(Drawing drawing);

    public abstract void DrawEllipse(Brush brush, Pen pen, Point center, double radiusX, double radiusY);

    public abstract void DrawEllipse(Brush brush, Pen pen, Point center, AnimationClock centerAnimations, double radiusX,
                                     AnimationClock radiusXAnimations, double radiusY, AnimationClock radiusYAnimations);

    public abstract void DrawGeometry(Brush brush, Pen pen, Geometry geometry);
    public abstract void DrawGlyphRun(Brush foregroundBrush, GlyphRun glyphRun);

    public abstract void DrawImage(ImageSource imageSource, Rect rectangle);
    public abstract void DrawImage(ImageSource imageSource, Rect rectangle, AnimationClock rectangleAnimations);

    public abstract void DrawLine(Pen pen, Point point0, Point point1);

    public abstract void DrawLine(Pen pen, Point point0, AnimationClock point0Animations, Point point1,
                                  AnimationClock point1Animations);

    public abstract void DrawRectangle(Brush brush, Pen pen, Rect rectangle);
    public abstract void DrawRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations);

    public abstract void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, double radiusX, double radiusY);

    public abstract void DrawRoundedRectangle(Brush brush, Pen pen, Rect rectangle, AnimationClock rectangleAnimations,
                                              double radiusX, AnimationClock radiusXAnimations, double radiusY,
                                              AnimationClock radiusYAnimations);

    public abstract void DrawText(FormattedText formattedText, Point origin);

//		public abstract void DrawVideo(MediaClock clock, Rect rectangle);
//		public abstract void DrawVideo(MediaClock clock, Rect rectangle, AnimationClock rectangleAnimations);

    public abstract void Pop();
    public abstract void PushClip(Geometry clipGeometry);

    public abstract void PushOpacity(double opacity);
    public abstract void PushOpacity(double opacity, AnimationClock opacityAnimations);

    public abstract void PushTransform(Transform transform);

    protected virtual void VerifyApiNonstructuralChange() {}

    #endregion Methods
  }
}