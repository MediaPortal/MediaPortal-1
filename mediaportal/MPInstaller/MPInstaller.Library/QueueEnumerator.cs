using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

namespace MediaPortal.MPInstaller
{
  public class QueueEnumerator
  {
    private List<QueueItem> items;

    public List<QueueItem> Items
    {
      get { return items; }
      set { items = value; }
    }

    public QueueEnumerator()
    {
      Items = new List<QueueItem>();
    }

    public void Save(string filename)
    {
      if (Items.Count > 0)
      {
        XmlSerializer serializer = new XmlSerializer(typeof(QueueEnumerator));
        TextWriter writer = new StreamWriter(filename);
        serializer.Serialize(writer, this);
        writer.Close();
      }
      else if (File.Exists(filename))
      {
        File.Delete(filename);
      }
    }

    public QueueEnumerator Load(string filename)
    {
      QueueEnumerator en = new QueueEnumerator();
      if (File.Exists(filename))
      {
        XmlSerializer serializer = new XmlSerializer(typeof(QueueEnumerator));
        FileStream fs = new FileStream(filename, FileMode.Open);
        en = (QueueEnumerator)serializer.Deserialize(fs);
        fs.Close();
      }
      return en;
    }

    public bool ContainName(string name)
    {
      foreach (QueueItem item in Items)
      {
        if (name.ToUpper() == item.Name.ToUpper())
          return true;
      }
      return false;
    }

    /// <summary>
    /// Removes the specified name entry.
    /// </summary>
    /// <param name="name">The name of package.</param>
    public void Remove(string name)
    {
      List<QueueItem> ls = new List<QueueItem>();
      foreach (QueueItem item in Items)
      {
        if (name.ToUpper() == item.Name.ToUpper())
          ls.Add(item);
      }
      foreach (QueueItem item in ls)
      {
        Items.Remove(item);
      }
    }
  }
}
