using System;
using System.ComponentModel;
using System.Globalization;

namespace Mpe.Controls.Properties
{
  [TypeConverter(typeof(MpeControlLockConverter))]
  [DescriptionAttribute("Defines whether or not a control can be moved or resized using the mouse.")]
  [Category("Designer")]
  public class MpeControlLock
  {
    #region Variables

    private bool sizeLocked;
    private bool locationLocked;
    //private bool locationReadOnly;
    //private bool sizeReadOnly;

    #endregion

    #region Events and Delegates

    public delegate void LockChangedHandler(MpeControlLockType type, bool value);


    public event LockChangedHandler LockChanged;

    #endregion

    #region Constructors

    public MpeControlLock()
    {
      sizeLocked = false;
      locationLocked = false;
    }

    public MpeControlLock(bool location, bool size)
    {
      sizeLocked = size;
      locationLocked = location;
    }

    public MpeControlLock(MpeControlLock controlLock)
    {
      sizeLocked = controlLock.sizeLocked;
      locationLocked = controlLock.locationLocked;
    }

    #endregion

    #region Properties

    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("If true, the control cannot be moved with the mouse or keyboard.")]
    public bool Location
    {
      get { return locationLocked; }
      set
      {
        //if (locationReadOnly == false && locationLocked != value) {
        if (locationLocked != value)
        {
          locationLocked = value;
          FireLockChanged(MpeControlLockType.Location, value);
        }
      }
    }

    [RefreshPropertiesAttribute(RefreshProperties.Repaint)]
    [Description("If true, the control cannot be resized using the mouse.")]
    public bool Size
    {
      get { return sizeLocked; }
      set
      {
        //if (sizeReadOnly == false && sizeLocked != value) {
        if (sizeLocked != value)
        {
          sizeLocked = value;
          FireLockChanged(MpeControlLockType.Size, value);
        }
      }
    }

    #endregion

    #region Methods

    private void FireLockChanged(MpeControlLockType type, bool value)
    {
      if (LockChanged != null)
      {
        LockChanged(type, value);
      }
    }

    /*
		public void SetReadOnly(bool location, bool size) {
			locationReadOnly = location;
			sizeReadOnly = size;
		}
		*/

    public override bool Equals(object obj)
    {
      if (obj != null && obj is MpeControlLock)
      {
        MpeControlLock clock = (MpeControlLock) obj;
        if (clock.Location == Location && clock.Size == Size)
        {
          return true;
        }
      }
      return false;
    }

    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    #endregion
  }


  public enum MpeControlLockType
  {
    Location = 1001,
    Size = 1002
  }

  #region MpeControlLockConverter

  internal class MpeControlLockConverter : ExpandableObjectConverter
  {
    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    {
      if (destinationType == typeof(MpeControlLock))
      {
        return true;
      }
      return base.CanConvertTo(context, destinationType);
    }

    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
                                     Type destinationType)
    {
      if (destinationType == typeof(String) && value is MpeControlLock)
      {
        MpeControlLock clp = (MpeControlLock) value;
        return clp.Location + ", " + clp.Size;
      }
      return base.ConvertTo(context, culture, value, destinationType);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    {
      if (sourceType == typeof(string))
      {
        return true;
      }
      return base.CanConvertFrom(context, sourceType);
    }

    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    {
      if (value is string)
      {
        try
        {
          string s = (string) value;
          int i = s.IndexOf(',');
          if (i > 0)
          {
            string szLocation = s.Substring(0, i).Trim();
            string szSize = s.Substring(i + 1).Trim();
            MpeControlLock clp = new MpeControlLock();
            clp.Location = bool.Parse(szLocation);
            clp.Size = bool.Parse(szSize);
            return clp;
          }
        }
        catch
        {
          throw new ArgumentException("Can not convert '" + (string) value + "' to type ControlLock");
        }
      }
      return base.ConvertFrom(context, culture, value);
    }
  }

  #endregion
}