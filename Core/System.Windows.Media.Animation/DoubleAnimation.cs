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
  public class DoubleAnimation : DoubleAnimationBase
  {
    #region Constructors

    static DoubleAnimation()
    {
      ByProperty = DependencyProperty.Register("By", typeof (double), typeof (DoubleAnimation));
      FromProperty = DependencyProperty.Register("From", typeof (double), typeof (DoubleAnimation));
      ToProperty = DependencyProperty.Register("To", typeof (double), typeof (DoubleAnimation));
    }

    public DoubleAnimation()
    {
    }

    public DoubleAnimation(double to, Duration duration)
    {
      this.To = to;
      this.Duration = duration;
    }

    public DoubleAnimation(double from, double to, Duration duration)
    {
      this.From = from;
      this.To = to;
      this.Duration = duration;
    }

    public DoubleAnimation(double to, Duration duration, FillBehavior fillBehavior)
    {
      this.To = to;
      this.Duration = duration;
      this.FillBehavior = fillBehavior;
    }

    public DoubleAnimation(double from, double to, Duration duration, FillBehavior fillBehavior)
    {
      this.From = from;
      this.To = to;
      this.Duration = duration;
      this.FillBehavior = fillBehavior;
    }

    #endregion Constructors

    #region Methods

    public new DoubleAnimation Copy()
    {
      return (DoubleAnimation) base.Copy();
    }

    protected override void CopyCore(Freezable sourceFreezable)
    {
      base.CopyCore(sourceFreezable);

      sourceFreezable.SetValue(ByProperty, GetValue(ByProperty));
      sourceFreezable.SetValue(FromProperty, GetValue(FromProperty));
      sourceFreezable.SetValue(ToProperty, GetValue(ToProperty));
    }

    protected override void CopyCurrentValueCore(Animatable sourceAnimatable)
    {
      throw new NotImplementedException();
    }

    protected override Freezable CreateInstanceCore()
    {
      return new DoubleAnimation();
    }

    protected override double GetCurrentValueCore(double defaultOriginValue, double defaultDestinationValue,
                                                  AnimationClock animationClock)
    {
      throw new NotImplementedException();
    }

    // By
    // animation progresses from the base value, the previous animation's output value,
    // or a zero value (depending on how the animation is configured) to the sum of that
    // value and the value specified by the By property

    // AnimationType.From
    // The animation progresses from the value specified by the From property to the base value, 
    // the previous animation's output value, or a zero value (depending upon how the animation is configured).

    // FromBy
    // animation progresses from the value specified by the From property to the value 
    // specified by the sum of the From and By properties

    // FromTo
    // The animation progresses from the value specified by the From property to the value 
    // specified by the To property.

    // To
    // The animation progresses from the base value, the previous animation's output value,
    // or a zero value (depending on how the animation is configured) to the value 
    // specified by the To property.

    #endregion Methods

    #region Properties

    public double By
    {
      get { return (double) GetValue(ByProperty); }
      set { SetValue(ByProperty, value); }
    }

    public double From
    {
      get { return (double) GetValue(FromProperty); }
      set { SetValue(FromProperty, value); }
    }

    public bool IsAdditive
    {
      get { return (bool) GetValue(IsAdditiveProperty); }
      set { SetValue(IsAdditiveProperty, value); }
    }

    public bool IsCumulative
    {
      get { return (bool) GetValue(IsCumulativeProperty); }
      set { SetValue(IsCumulativeProperty, value); }
    }

    public double To
    {
      get { return (double) GetValue(ToProperty); }
      set { SetValue(ToProperty, value); }
    }

    #endregion Properties

    #region Properties (Dependency)

    public static readonly DependencyProperty ByProperty;
    public static readonly DependencyProperty FromProperty;
    public static readonly DependencyProperty ToProperty;

    #endregion Properties (Dependency)
  }
}