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