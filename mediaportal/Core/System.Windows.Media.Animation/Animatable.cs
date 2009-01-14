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

namespace System.Windows.Media.Animation
{
  public abstract class Animatable : Freezable, IAnimatable
  {
    #region Constructors

    protected Animatable()
    {
    }

    #endregion Constructors

    #region Methods

    public void ApplyAnimationClock(DependencyProperty property, AnimationClock clock)
    {
      ApplyAnimationClock(property, clock, HandoffBehavior.SnapshotAndReplace);
    }

    public void ApplyAnimationClock(DependencyProperty property, AnimationClock clock, HandoffBehavior handoffBehavior)
    {
    }

    public void BeginAnimation(DependencyProperty property, AnimationTimeline animation)
    {
      BeginAnimation(property, animation, HandoffBehavior.SnapshotAndReplace);
    }

    public void BeginAnimation(DependencyProperty property, AnimationTimeline animation, HandoffBehavior handoffBehavior)
    {
    }

    public new Animatable Copy()
    {
      return (Animatable) base.Copy();
    }

    protected virtual void CopyCurrentValueCore(Animatable sourceAnimatable)
    {
      throw new NotImplementedException();
    }

    protected override bool FreezeCore(bool isChecking)
    {
      // An Animatable will return false from this method if there are any Clocks 
      // animating any of its properties. If the Animatable has persistent animations specified,
      // but all of the Clocks have been removed, it may still return true from this method if the
      // Timelines themselves can be frozen

      throw new NotImplementedException();
    }

    public object GetAnimationBaseValue(DependencyProperty property)
    {
      throw new NotImplementedException();
    }

    public Animatable GetCurrentValue()
    {
      throw new NotImplementedException();
    }

    protected override object GetValueCore(DependencyProperty property, object baseValue, PropertyMetadata metadata)
    {
      throw new NotImplementedException();
    }

    #endregion Methods

    #region Properties

    public bool HasAnimatedProperties
    {
      get { throw new NotImplementedException(); }
    }

    #endregion Properties
  }
}