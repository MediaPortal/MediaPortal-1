using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using MediaPortal.GUI.Library;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.Setting
{
  [Serializable]
  public class Image
  {
    private Bitmap bitmap;
    private string fileName;
    private Value value;
    [XmlAttribute] public int X;
    [XmlAttribute] public int Y;

    public Image()
    {
      Log.Info("MiniDisplayPlugin.Setting.Image(): Initializing Image control", new object[0]);
    }

    public Image(int x, int y, string file)
    {
      Log.Info("MiniDisplayPlugin.Setting.Image(): Initializing Image control X = \"{0}\", Y =\"{1}\", File = \"{2}\"",
               new object[] {x, y, file});
      this.X = x;
      this.Y = y;
      this.File = file;
    }

    [XmlIgnore]
    public Bitmap Bitmap
    {
      get
      {
        try
        {
          if (this.value != null)
          {
            string path = this.value.Evaluate();
            if (path != this.fileName)
            {
              this.fileName = path;
              if (System.IO.File.Exists(path))
              {
                Log.Info("MiniDisplayPlugin.Setting.Image(): loading image file \"{0}\".", new object[] {this.File});
                this.bitmap = (Bitmap) System.Drawing.Image.FromFile(path);
              }
            }
            return this.bitmap;
          }
          if (this.bitmap == null)
          {
            if (System.IO.File.Exists(this.fileName))
            {
              Log.Info("MiniDisplayPlugin.Setting.Image(): loading image filename \"{0}\".", new object[] {this.File});
              this.bitmap = (Bitmap) System.Drawing.Image.FromFile(this.fileName);
            }
            else
            {
              Log.Error("Error while loading image file {0}.  File not found!  Defaulting to a single pixel.",
                        new object[] {this.File});
              this.bitmap = new Bitmap(1, 1);
            }
          }
        }
        catch (OutOfMemoryException)
        {
          Log.Error(
            "Out of memory while loading image file {0}!  Probably bad image format.  Defaulting to a single pixel.",
            new object[] {this.File});
          this.bitmap = new Bitmap(1, 1);
        }
        return this.bitmap;
      }
    }

    [DefaultValue((string) null), XmlAttribute]
    public string File
    {
      get
      {
        Log.Info("MiniDisplayPlugin.Setting.Image.File(): Getting image filename = \"{0}\".",
                 new object[] {this.fileName});
        return this.fileName;
      }
      set
      {
        Log.Info("MiniDisplayPlugin.Setting.Image.File(): Setting image filename to \"{0}\".", new object[] {value});
        this.fileName = value;
      }
    }

    [XmlElement("Property", typeof (Property)), XmlElement("Parse", typeof (Parse)), DefaultValue((string) null),
     XmlElement("Text", typeof (Text))]
    public Value Value
    {
      get { return this.value; }
      set { this.value = value; }
    }
  }
}