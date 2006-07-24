using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using Mpe.Controls.Design;

namespace Mpe.Controls
{

  #region MpeTag

  [TypeConverter(typeof(MpeTagConverter))]
  [Editor(typeof(MpeTagEditor), typeof(UITypeEditor))]
  public class MpeTag
  {
    private bool readOnly;
    private string tagName;
    private string tagValue;

    public MpeTag()
    {
      readOnly = false;
      tagName = "";
      tagValue = "";
    }


    public delegate void NameChangedHandler(MpeTag tag);


    public event NameChangedHandler NameChanged;


    public delegate void ValueChangedHandler(MpeTag tag);


    public event ValueChangedHandler ValueChanged;

    public MpeTag(string tagName, string tagValue, bool readOnly)
    {
      this.readOnly = readOnly;
      this.tagName = tagName;
      this.tagValue = tagValue;
    }

    public MpeTag(MpeTag tag)
    {
      readOnly = tag.readOnly;
      tagName = tag.tagName;
      tagValue = tag.tagValue;
    }

    public bool ReadOnly
    {
      get { return readOnly; }
      set { readOnly = value; }
    }

    public string Name
    {
      get { return tagName; }
      set
      {
        tagName = value;
        if (NameChanged != null)
        {
          NameChanged(this);
        }
      }
    }

    public string Value
    {
      get { return tagValue; }
      set
      {
        tagValue = value;
        if (ValueChanged != null)
        {
          ValueChanged(this);
        }
      }
    }
  }

  #endregion

  #region MpeTagDescriptor

  internal class MpeTagDescriptor : PropertyDescriptor
  {
    private MpeTag tag;

    public MpeTagDescriptor(MpeTag tag) : base("MpeTagDescriptor", null)
    {
      this.tag = tag;
    }

    public override AttributeCollection Attributes
    {
      get
      {
        ArrayList l = new ArrayList();
        for (int i = 0; i < base.Attributes.Count; i++)
        {
          l.Add(base.Attributes[i]);
        }
        l.Add(new RefreshPropertiesAttribute(RefreshProperties.All));
        return new AttributeCollection((Attribute[]) l.ToArray(typeof(Attribute)));
      }
    }

    public override bool CanResetValue(object component)
    {
      return false;
    }

    public override Type ComponentType
    {
      get { return typeof(MpeTag); }
    }

    public override object GetValue(object component)
    {
      return tag.Value;
    }

    public override bool IsReadOnly
    {
      get { return tag.ReadOnly; }
    }

    public override string Name
    {
      get { return tag.Name; }
    }

    public override string DisplayName
    {
      get { return tag.Name; }
    }

    public override Type PropertyType
    {
      get { return typeof(MpeTag); }
    }

    public override void ResetValue(object component)
    {
      //
    }

    public override void SetValue(object component, object value)
    {
      //
    }

    public override bool ShouldSerializeValue(object component)
    {
      return false;
    }
  }

  #endregion

  #region MpeTagConverter

  internal class MpeTagConverter : StringConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == typeof(string))
      {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                     Type destinationType)
    {
      if (destinationType == typeof(String) && value is MpeTag)
      {
        MpeTag tag = (MpeTag) value;
        return tag.Value;
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
      {
        return false;
      }
      return base.CanConvertFrom(context, sourceType);
    }
  }

  #endregion
}