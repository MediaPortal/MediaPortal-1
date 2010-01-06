#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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

using System.Windows.Serialization;

namespace System.Windows.Media.Animation
{
  public abstract class TimelineGroup : Timeline, IAddChild
  {
    #region Constructors

    protected TimelineGroup() {}

    protected TimelineGroup(Nullable<TimeSpan> beginTime) : base(beginTime) {}

    protected TimelineGroup(Nullable<TimeSpan> beginTime, Duration duration) : base(beginTime, duration) {}

    protected TimelineGroup(Nullable<TimeSpan> beginTime, Duration duration, RepeatBehavior repeatBehavior)
      : base(beginTime, duration, repeatBehavior) {}

    #endregion Constructors

    #region Methods

    void IAddChild.AddChild(object child)
    {
      AddChild(child);
    }

    protected virtual void AddChild(object child) {}

    void IAddChild.AddText(string text)
    {
      AddText(text);
    }

    protected virtual void AddText(string text) {}

    protected internal override Clock AllocateClock()
    {
      return new ClockGroup(this);
    }

    public new TimelineGroup Copy()
    {
      return (TimelineGroup)base.Copy();
    }

    protected override void CopyCore(Freezable sourceFreezable)
    {
      base.CopyCore(sourceFreezable);

      if (_children == null)
      {
        return;
      }

      foreach (Timeline childTimeline in _children)
      {
        ((TimelineGroup)sourceFreezable).Children.Add(childTimeline.Copy());
      }
    }

    protected override void CopyCurrentValueCore(Animatable sourceAnimatable)
    {
      // The timeline to copy properties from. 
      // If this parameter is null, this timeline is constructed with default property values.

//			if(sourceAnimatable == null)
//				CopyCore(sourceFreezable);
    }

    public new ClockGroup CreateClock()
    {
      return new ClockGroup(this);
    }

    protected override bool FreezeCore(bool isChecking)
    {
      if (_children == null)
      {
        return base.FreezeCore(isChecking);
      }

      foreach (Freezable child in _children)
      {
        if (Freeze(child, isChecking) == false)
        {
          return false;
        }
      }

      return base.FreezeCore(isChecking);
    }

    protected override void PropagateChangedHandlersCore(EventHandler handler, bool isAdding) {}

    #endregion Methods

    #region Properties

    public TimelineCollection Children
    {
      get
      {
        if (_children == null)
        {
          _children = new TimelineCollection();
        }
        return _children;
      }
    }

    #endregion Properties

    #region Fields

    private TimelineCollection _children;

    #endregion Fields
  }
}