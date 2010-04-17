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
        XmlSerializer serializer = new XmlSerializer(typeof (QueueEnumerator));
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
      try
      {
        if (File.Exists(filename))
        {
          XmlSerializer serializer = new XmlSerializer(typeof (QueueEnumerator));
          FileStream fs = new FileStream(filename, FileMode.Open);
          en = (QueueEnumerator)serializer.Deserialize(fs);
          fs.Close();
        }
      }
      catch {}
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