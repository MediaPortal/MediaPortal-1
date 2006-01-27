using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Globalization;

namespace Mpe.Controls
{

  #region MpeContolItemCollection

  [TypeConverter(typeof(MpeItemCollectionConverter))]
  [Editor(typeof(MpeItemCollectionEditor), typeof(UITypeEditor))]
  public class MpeItemCollection : CollectionBase, ICustomTypeDescriptor
  {
    #region Variables

    private MpeItemType type;

    #endregion

    #region Events and Delegates

    public delegate void ItemInsertedHandler(int index, MpeItem item);


    public event ItemInsertedHandler ItemInserted;


    public delegate void ItemRemovedHandler(int index, MpeItem item);


    public event ItemRemovedHandler ItemRemoved;


    public delegate void ItemSetHandler(int index, MpeItem oldItem, MpeItem newItem);


    public event ItemSetHandler ItemSet;


    public delegate void ItemsClearedHandler();


    public event ItemsClearedHandler ItemsCleared;

    #endregion

    #region Constructors

    public MpeItemCollection()
    {
      type = MpeItemType.Text;
    }

    public MpeItemCollection(MpeItemCollection items)
    {
      type = items.type;
      for (int i = 0; i < items.Count; i++)
      {
        Add(new MpeItem(items[i]));
      }
    }

    #endregion

    #region Methods - Collection Implementation

    public void Add(MpeItem item)
    {
      List.Add(item);
    }

    public void Add(MpeItemType type, string value)
    {
      MpeItem item = new MpeItem();
      item.Type = type;
      item.Value = value;
      Add(item);
    }

    public void Remove(MpeItem item)
    {
      List.Remove(item);
    }

    public MpeItem this[int index]
    {
      get { return (MpeItem) List[index]; }
    }

    public MpeItemType Type
    {
      get { return type; }
      set { type = value; }
    }

    protected override void OnInsertComplete(int index, object value)
    {
      base.OnInsertComplete(index, value);
      if (value is MpeItem)
      {
        if (ItemInserted != null)
        {
          ItemInserted(index, (MpeItem) value);
        }
      }
    }

    protected override void OnRemoveComplete(int index, object value)
    {
      base.OnRemoveComplete(index, value);
      if (value is MpeItem)
      {
        if (ItemRemoved != null)
        {
          ItemRemoved(index, (MpeItem) value);
        }
      }
    }

    protected override void OnSet(int index, object oldValue, object newValue)
    {
      base.OnSet(index, oldValue, newValue);
      if (oldValue is MpeItem && newValue is MpeItem)
      {
        if (ItemSet != null)
        {
          ItemSet(index, (MpeItem) oldValue, (MpeItem) newValue);
        }
      }
    }

    protected override void OnClearComplete()
    {
      base.OnClearComplete();
      if (ItemsCleared != null)
      {
        ItemsCleared();
      }
    }

    #endregion

    #region Methods - Type Descriptor Implementation

    public String GetClassName()
    {
      return TypeDescriptor.GetClassName(this, true);
    }

    public AttributeCollection GetAttributes()
    {
      return TypeDescriptor.GetAttributes(this, true);
    }

    public String GetComponentName()
    {
      return TypeDescriptor.GetComponentName(this, true);
    }

    public TypeConverter GetConverter()
    {
      return TypeDescriptor.GetConverter(this, true);
    }

    public EventDescriptor GetDefaultEvent()
    {
      return TypeDescriptor.GetDefaultEvent(this, true);
    }

    public PropertyDescriptor GetDefaultProperty()
    {
      return TypeDescriptor.GetDefaultProperty(this, true);
    }

    public object GetEditor(Type editorBaseType)
    {
      return TypeDescriptor.GetEditor(this, editorBaseType, true);
    }

    public EventDescriptorCollection GetEvents(Attribute[] attributes)
    {
      return TypeDescriptor.GetEvents(this, attributes, true);
    }

    public EventDescriptorCollection GetEvents()
    {
      return TypeDescriptor.GetEvents(this, true);
    }

    public object GetPropertyOwner(PropertyDescriptor pd)
    {
      return this;
    }

    public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
    {
      return GetProperties();
    }

    public PropertyDescriptorCollection GetProperties()
    {
      // Create a new collection object PropertyDescriptorCollection
      PropertyDescriptorCollection pds = new PropertyDescriptorCollection(null);

      // Iterate the list of items
      for (int i = 0; i < List.Count; i++)
      {
        pds.Add(new MpeItemDescriptor(this[i], i, Count));
      }
      return pds;
    }

    #endregion
  }

  #endregion

  public class MpeItemEventArgs : EventArgs
  {
    private bool cancelTypeChange;
    private MpeItemType existingType;
    private MpeItemType newType;

    public MpeItemEventArgs(MpeItemType existingType, MpeItemType newType)
    {
      this.existingType = existingType;
      this.newType = newType;
      cancelTypeChange = false;
    }

    public bool CancelTypeChange
    {
      get { return cancelTypeChange; }
      set { cancelTypeChange = value; }
    }

    public MpeItemType ExistingType
    {
      get { return existingType; }
    }

    public MpeItemType NewType
    {
      get { return newType; }
    }
  }

  #region Converter

  internal class MpeItemCollectionConverter : ExpandableObjectConverter
  {
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
    {
      if (destType == typeof(string) && value is MpeItemCollection)
      {
        MpeItemCollection c = (MpeItemCollection) value;
        if (c.Count > 0)
        {
          return "(Collection)";
        }
        return "(Empty)";
      }
      return base.ConvertTo(context, culture, value, destType);
    }
  }

  #endregion

  #region Editor

  internal class MpeItemCollectionEditor : CollectionEditor
  {
    public MpeItemCollectionEditor() : base(typeof(MpeItemCollection))
    {
      //
    }

    public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
    {
      if (value is MpeItemCollection)
      {
        MpeItemCollection c = (MpeItemCollection) value;
        if (c.Type == MpeItemType.Text)
        {
          return base.EditValue(context, provider, value);
        }
        else
        {
          MpeLog.Warn("To edit this collection set the First, Last, Interval, and Digits properties.");
        }
      }
      return value;
    }
  }

  #endregion
}