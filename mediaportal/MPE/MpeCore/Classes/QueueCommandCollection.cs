using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace MpeCore.Classes
{
  public class QueueCommandCollection
  {
    public List<QueueCommand> Items { get; set; }

    public QueueCommandCollection()
    {
      Items = new List<QueueCommand>();
    }

    public void Save()
    {
      Save(MpeInstaller.BaseFolder + "\\queue.xml");
    }

    public void Save(string filename)
    {
      if (Items.Count > 0)
      {
        var serializer = new XmlSerializer(typeof (QueueCommandCollection));
        TextWriter writer = new StreamWriter(filename);
        serializer.Serialize(writer, this);
        writer.Close();
      }
      else
      {
        if (File.Exists(filename))
          File.Delete(filename);
      }
    }

    public static QueueCommandCollection Load()
    {
      return Load(MpeInstaller.BaseFolder + "\\queue.xml");
    }

    public void Add(QueueCommand command)
    {
      Remove(command.TargetId);
      Items.Add(command);
    }

    public void Remove(string id)
    {
      List<QueueCommand> list = new List<QueueCommand>();
      foreach (QueueCommand item in Items)
      {
        if (item.TargetId == id)
          list.Add(item);
      }
      foreach (QueueCommand command in list)
      {
        Items.Remove(command);
      }
    }

    public QueueCommand Get(string id)
    {
      foreach (QueueCommand command in Items)
      {
        if (command.TargetId == id)
          return command;
      }
      return null;
    }

    public static QueueCommandCollection Load(string fileName)
    {
      if (File.Exists(fileName))
      {
        FileStream fs = null;
        try
        {
          XmlSerializer serializer = new XmlSerializer(typeof (QueueCommandCollection));
          fs = new FileStream(fileName, FileMode.Open);
          QueueCommandCollection commandCollection = (QueueCommandCollection)serializer.Deserialize(fs);
          fs.Close();
          return commandCollection;
        }
        catch
        {
          if (fs != null)
            fs.Dispose();
          return new QueueCommandCollection();
        }
      }
      return new QueueCommandCollection();
    }
  }
}