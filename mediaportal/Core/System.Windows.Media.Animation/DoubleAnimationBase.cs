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

namespace System.Windows.Media.Animation
{
  public abstract class DoubleAnimationBase : AnimationTimeline
  {
    #region Constructors

    protected DoubleAnimationBase()
    {
    }

    #endregion Constructors

    #region Methods

    public new DoubleAnimationBase Copy()
    {
      return (DoubleAnimationBase) base.Copy();
    }

    public double GetCurrentValue(double defaultOriginValue, double defaultDestinationValue,
                                  AnimationClock animationClock)
    {
      return GetCurrentValueCore(defaultOriginValue, defaultDestinationValue, animationClock);
    }

    protected abstract double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue,
                                                  AnimationClock animationClock);

    #endregion Methods

    #region Properties

    public override sealed Type TargetPropertyType
    {
      get { return typeof (double); }
    }

    #endregion Properties
  }
}