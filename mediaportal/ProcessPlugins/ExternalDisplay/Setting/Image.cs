using System;
using System.Drawing;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
    /// <author>JoeDalton</author>
    [Serializable]
    public class Image
    {
        private Bitmap bitmap;

        [XmlAttribute]
        public int X;

        [XmlAttribute]
        public int Y;

        [XmlAttribute]
        public string File;

        public Image()
        {
        }

        public Image(int x, int y, string file)
        {
            X = x;
            Y = y;
            File = file;
        }

        [XmlIgnore]
        public Bitmap Bitmap
        {
            get
            {
                if (bitmap == null)
                {
                    bitmap = (Bitmap) Bitmap.FromFile(File);
                }
                return bitmap;
            }
        }
    }
}