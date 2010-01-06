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

using System.Collections.Specialized;

namespace System.Windows
{
  public class FrameworkPropertyMetadata : UIPropertyMetadata
  {
    #region Constructors

    public FrameworkPropertyMetadata() {}

    public FrameworkPropertyMetadata(GetValueOverride getValueOverride) : base(getValueOverride) {}

    public FrameworkPropertyMetadata(object defaultValue) : base(defaultValue) {}

    public FrameworkPropertyMetadata(PropertyInvalidatedCallback propertyInvalidatedCallback)
      : base(propertyInvalidatedCallback) {}

    public FrameworkPropertyMetadata(SetValueOverride setValueOverride) : base(setValueOverride) {}

    public FrameworkPropertyMetadata(GetValueOverride getValueOverride, SetValueOverride setValueOverride)
      : base(getValueOverride, setValueOverride) {}

    public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions options) : base(defaultValue)
    {
      _options = new BitVector32((int)options);
    }

    public FrameworkPropertyMetadata(object defaultValue, PropertyInvalidatedCallback propertyInvalidatedCallback)
      : base(defaultValue, propertyInvalidatedCallback) {}

    public FrameworkPropertyMetadata(PropertyInvalidatedCallback propertyInvalidatedCallback,
                                     GetValueOverride getValueOverride)
      : base(propertyInvalidatedCallback, getValueOverride, null) {}

    public FrameworkPropertyMetadata(object defaultValue, PropertyInvalidatedCallback propertyInvalidatedCallback,
                                     GetValueOverride getValueOverride)
      : base(defaultValue, propertyInvalidatedCallback, getValueOverride) {}

    public FrameworkPropertyMetadata(PropertyInvalidatedCallback propertyInvalidatedCallback,
                                     GetValueOverride getValueOverride, SetValueOverride setValueOverride)
      : base(propertyInvalidatedCallback, getValueOverride, setValueOverride) {}

    public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions options,
                                     PropertyInvalidatedCallback propertyInvalidatedCallback,
                                     GetValueOverride getValueOverride)
      : base(defaultValue, propertyInvalidatedCallback, getValueOverride)
    {
      _options = new BitVector32((int)options);
    }

    public FrameworkPropertyMetadata(object defaultValue, PropertyInvalidatedCallback propertyInvalidatedCallback,
                                     GetValueOverride getValueOverride, SetValueOverride setValueOverride)
      : base(defaultValue, propertyInvalidatedCallback, getValueOverride, setValueOverride) {}

    public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions options,
                                     PropertyInvalidatedCallback propertyInvalidatedCallback,
                                     GetValueOverride getValueOverride, SetValueOverride setValueOverride)
      : base(defaultValue, propertyInvalidatedCallback, getValueOverride, setValueOverride)
    {
      _options = new BitVector32((int)options);
    }

    public FrameworkPropertyMetadata(object defaultValue, FrameworkPropertyMetadataOptions options,
                                     PropertyInvalidatedCallback propertyInvalidatedCallback,
                                     GetValueOverride getValueOverride, SetValueOverride setValueOverride,
                                     bool isAnimationProhibited)
      : base(defaultValue, propertyInvalidatedCallback, getValueOverride, setValueOverride, isAnimationProhibited)
    {
      _options = new BitVector32((int)options);
    }

    #endregion Constructors

    #region Methods

    protected override PropertyMetadata CreateInstance()
    {
      return new FrameworkPropertyMetadata();
    }

    protected override void Merge(PropertyMetadata baseMetadata, DependencyProperty dp)
    {
      base.Merge(baseMetadata, dp);
    }

    protected override void OnApply(DependencyProperty dp, Type targetType)
    {
      base.OnApply(dp, targetType);
    }

    #endregion Methods

    #region Properties

    public bool AffectsArrange
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.AffectsArrange]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.AffectsArrange] = value; }
    }

    public bool AffectsMeasure
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.AffectsMeasure]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.AffectsMeasure] = value; }
    }

    public bool AffectsParentArrange
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.AffectsParentArrange]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.AffectsParentArrange] = value; }
    }

    public bool AffectsParentMeasure
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.AffectsParentMeasure]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.AffectsParentMeasure] = value; }
    }

    public bool AffectsRender
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.AffectsRender]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.AffectsRender] = value; }
    }

    public bool BindsTwoWayByDefault
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.BindsTwoWayByDefault]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.BindsTwoWayByDefault] = value; }
    }

    public bool Inherits
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.Inherits]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.Inherits] = value; }
    }

    public bool IsDataBindingAllowed
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.NotDataBindable] == false; }
    }

    public bool IsNotDataBindable
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.NotDataBindable]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.NotDataBindable] = value; }
    }

    public bool Journal
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.Journal]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.Journal] = value; }
    }

    public bool SpanSeparatedTrees
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.SpanSeparatedTrees]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.SpanSeparatedTrees] = value; }
    }

    public bool UpdatesSourceOnLostFocusByDefault
    {
      get { return _options[(int)FrameworkPropertyMetadataOptions.UpdatesSourceOnLostFocusByDefault]; }
      set { _options[(int)FrameworkPropertyMetadataOptions.UpdatesSourceOnLostFocusByDefault] = value; }
    }

    #endregion Properties

    #region Fields

    private BitVector32 _options = new BitVector32();

    #endregion Fields
  }
}