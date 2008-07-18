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

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;

namespace Mpe.Controls
{

  #region MpeTagCollection

  [TypeConverter(typeof(MpeTagCollectionConverter))]
  [Editor(typeof(MpeTagCollectionEditor), typeof(UITypeEditor))]
  public class MpeTagCollection : ICustomTypeDescriptor
  {
    #region Variables

    private Hashtable hashtable;

    #endregion

    #region Constructors

    public MpeTagCollection()
    {
      hashtable = new Hashtable();
      Add("-", "-", false);
    }

    public MpeTagCollection(MpeTagCollection tags)
    {
      hashtable = new Hashtable();
      string[] keys = tags.Keys;
      if (keys.Length > 0)
      {
        for (int i = 0; i < keys.Length; i++)
        {
          Add(tags[keys[i]]);
        }
      }
      else
      {
        Add("-", "-", false);
      }
    }

    #endregion

    #region Events and Delegates

    public delegate void TagChangedHandler(MpeTag tag);


    public event TagChangedHandler TagChanged;


    public delegate void TagAddedHandler(MpeTag tag);


    public event TagAddedHandler TagAdded;


    public delegate void TagRemovedHandler(MpeTag tag);


    public event TagRemovedHandler TagRemoved;

    #endregion

    #region Properties

    public int Count
    {
      get { return hashtable.Count; }
    }

    public string[] Keys
    {
      get
      {
        IEnumerator e = hashtable.Keys.GetEnumerator();
        ArrayList l = new ArrayList();
        while (e.MoveNext())
        {
          string s = (string) e.Current;
          if (s.Equals("-") == false)
          {
            l.Add((string) e.Current);
          }
        }
        return (string[]) l.ToArray(typeof(string));
      }
    }

    #endregion

    #region Methods

    public void Add(MpeTag tag)
    {
      hashtable.Remove(tag.Name);
      hashtable.Add(tag.Name, tag);
      if (this["-"] == null)
      {
        Add("-", "-", false);
      }
      tag.NameChanged += new MpeTag.NameChangedHandler(OnTagNameChanged);
      tag.ValueChanged += new MpeTag.ValueChangedHandler(OnTagValueChanged);
      if (TagAdded != null)
      {
        TagAdded(tag);
      }
    }

    public void Add(string tagName, string tagValue, bool readOnly)
    {
      Add(new MpeTag(tagName, tagValue, readOnly));
    }

    public void Remove(string tagName)
    {
      MpeTag tag = this[tagName];
      if (tag != null)
      {
        hashtable.Remove(tagName);
        tag.NameChanged -= new MpeTag.NameChangedHandler(OnTagNameChanged);
        tag.ValueChanged -= new MpeTag.ValueChangedHandler(OnTagValueChanged);
        if (TagRemoved != null)
        {
          TagRemoved(tag);
        }
      }
    }

    public MpeTag this[string tagName]
    {
      get { return (MpeTag) hashtable[tagName]; }
    }

    public void Disable(string tagName)
    {
      MpeTag t = this[tagName];
      if (t != null)
      {
        t.ReadOnly = true;
      }
    }

    public void Enable(string tagName)
    {
      MpeTag t = this[tagName];
      if (t != null)
      {
        t.ReadOnly = false;
      }
    }

    #endregion

    #region Methods - TypeDescriptor

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
      string[] keys = Keys;
      for (int i = 0; i < keys.Length; i++)
      {
        pds.Add(new MpeTagDescriptor(this[keys[i]]));
      }
      MpeTag tag = this["-"];
      if (tag != null)
      {
        pds.Add(new MpeTagDescriptor(tag));
      }
      return pds;
    }

    #endregion

    private void OnTagNameChanged(MpeTag tag)
    {
      if (TagChanged != null)
      {
        TagChanged(tag);
      }
    }

    private void OnTagValueChanged(MpeTag tag)
    {
      if (TagChanged != null)
      {
        TagChanged(tag);
      }
    }
  }

  #endregion

  #region MpeTagCollectionConverter

  internal class MpeTagCollectionConverter : ExpandableObjectConverter
  {
    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destType)
    {
      if (destType == typeof(string) && value is MpeTagCollection)
      {
        MpeTagCollection c = (MpeTagCollection) value;
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

  #region MpeTagCollectionEditor

  public class MpeTagCollectionEditor : UITypeEditor
  {
    public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
    {
      return UITypeEditorEditStyle.None;
    }
  }

  #endregion
}