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

using MediaPortal.Drawing;
using MediaPortal.Drawing.Transforms;

namespace System.Windows
{
  public sealed class VisualOperations
  {
    #region Constructors

    private VisualOperations()
    {
    }

    #endregion Constructors

    #region Methods

    public static DependencyObject FindCommonVisualAncestor(DependencyObject reference, DependencyObject otherVisual)
    {
      throw new NotImplementedException();
    }

//		public static BitmapEffect GetBitmapEffect(Visual visual)
//		{
//		}

//		public static BitmapEffectInput GetBitmapEffectInput(Visual visual)
//		{
//		}

    public static VisualCollection GetChildren(Visual visual)
    {
      throw new NotImplementedException();
    }

    public static Geometry GetClip(Visual visual)
    {
      throw new NotImplementedException();
    }

    public static Rect GetContentBounds(Visual visual)
    {
      throw new NotImplementedException();
    }

//		public static Rect3D GetContentBounds(Visual3D reference)
//		{
//			throw new NotImplementedException();
//		}

    public static Rect GetDescendantBounds(Visual visual)
    {
      throw new NotImplementedException();
    }

//		public static Rect3D GetDescendantBounds(Visual3D reference)
//		{
//			throw new NotImplementedException();
//		}			

//		public static DrawingGroup GetDrawing(Visual visual)
//		{
//		}

//		public static Vector GetOffset(Visual visual)
//		{
//			throw new NotImplementedException();
//		}

    public static double GetOpacity(Visual visual)
    {
      if (visual is UIElement)
      {
        return ((UIElement) visual).Opacity;
      }

      return 1;
    }

    public static Brush GetOpacityMask(Visual visual)
    {
      if (visual is UIElement)
      {
        return ((UIElement) visual).OpacityMask;
      }

      return null;
    }

    public static DependencyObject GetParent(DependencyObject reference)
    {
      if (reference is Visual)
      {
        return ((Visual) reference).VisualParent;
      }

      throw new NotImplementedException();
    }

    public static Transform GetTransform(Visual visual)
    {
      throw new NotImplementedException();
    }

    public static DoubleCollection GetXSnappingGuidelines(Visual visual)
    {
      throw new NotImplementedException();
    }

    public static DoubleCollection GetYSnappingGuidelines(Visual visual)
    {
      throw new NotImplementedException();
    }

//		public static HitTestResult HitTest(Visual visual, Point point)
//		{
//			throw new NotImplementedException();
//		}

//		public static void HitTest(Visual visual, HitTestFilter filterHitDelegate, HitTestResultDelegate resultHitDelegate, HitTestParameters hitTestParameters)
//		{
//			throw new NotImplementedException();
//		}

//		public static void HitTest(Visual3D reference, HitTestFilter filterHitDelegate, HitTestResultDelegate resultHitDelegate, HitTestParameters3D hitTestParameters)
//		{
//			throw new NotImplementedException();
//		}

    public static bool IsAncestorOf(DependencyObject reference, DependencyObject descendant)
    {
      throw new NotImplementedException();
    }

    public static bool IsDescendantOf(DependencyObject reference, DependencyObject ancestor)
    {
      throw new NotImplementedException();
    }

//		public static void SetBitmapEffect(Visual visual, BitmapEffect bitmapEffect)
//		{
//			throw new NotImplementedException();
//		}

//		public static void SetBitmapEffectInput(Visual visual, BitmapEffectInput bitmapEffectInput)
//		{
//			throw new NotImplementedException();
//		}

    public static void SetClip(Visual visual, Geometry clip)
    {
      throw new NotImplementedException();
    }

//		public static void SetOffset(Visual visual, Vector offset)
//		{
//			throw new NotImplementedException();
//		}

    public static void SetOpacity(Visual visual, double opacity)
    {
      throw new NotImplementedException();
    }

    public static void SetOpacityMask(Visual visual, Brush opacityMask)
    {
      throw new NotImplementedException();
    }

    public static void SetTransform(Visual visual, Transform transform)
    {
      throw new NotImplementedException();
    }

    public static void SetYSnappingGuidelines(Visual visual, DoubleCollection guidelines)
    {
      throw new NotImplementedException();
    }

//		public static GeneralTransform TransformToAncestor(Visual visual, Visual ancestor)
//		{
//			throw new NotImplementedException();
//		}

//		public static GeneralTransform TransformToDescendant(Visual visual, Visual descendant)
//		{
//			throw new NotImplementedException();
//		}

//		public static GeneralTransform TransformToVisual(Visual visual, Visual target)
//		{
//			throw new NotImplementedException();
//		}

    #endregion Methods
  }
}