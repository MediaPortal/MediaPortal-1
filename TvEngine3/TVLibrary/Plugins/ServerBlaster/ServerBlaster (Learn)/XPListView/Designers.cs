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
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;

namespace XPListview
{
  internal class XPListViewItemCollectionEditor : System.ComponentModel.Design.CollectionEditor
  {
    public XPListViewItemCollectionEditor() : base(typeof (XPListViewItemCollection)) {}

    protected override object CreateInstance(System.Type itemType)
    {
      return new XPListViewItem();
    }

    protected override System.Type CreateCollectionItemType()
    {
      return typeof (XPListViewItem);
    }
  }

  internal class XPListViewItemConverter : ExpandableObjectConverter
  {
    public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
    {
      if (destinationType == typeof (InstanceDescriptor))
      {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context,
                                     System.Globalization.CultureInfo culture, object value, System.Type destinationType)
    {
      if (destinationType == typeof (InstanceDescriptor))
      {
        Type[] signature = {typeof (XPListViewItem.ListViewSubItem[]), typeof (int), typeof (int)};
        XPListViewItem itm = ((XPListViewItem)value);
        object[] args = {itm.SubItemsArray, itm.ImageIndex, itm.GroupIndex};
        return new InstanceDescriptor(typeof (XPListViewItem).GetConstructor(signature), args, false);
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }

  internal class XPListViewGroupConverter : ExpandableObjectConverter
  {
    public override bool CanConvertTo(System.ComponentModel.ITypeDescriptorContext context, System.Type destinationType)
    {
      if (destinationType == typeof (InstanceDescriptor))
      {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(System.ComponentModel.ITypeDescriptorContext context,
                                     System.Globalization.CultureInfo culture, object value, System.Type destinationType)
    {
      if (destinationType == typeof (InstanceDescriptor))
      {
        Type[] signature = {typeof (string), typeof (int)};
        XPListViewGroup itm = ((XPListViewGroup)value);
        object[] args = {itm.GroupText, itm.GroupIndex};
        return new InstanceDescriptor(typeof (XPListViewGroup).GetConstructor(signature), args, false);
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }
  }
}