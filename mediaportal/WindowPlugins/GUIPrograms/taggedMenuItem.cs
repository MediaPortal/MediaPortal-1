using System.Windows.Forms;
using System.Xml;

namespace WindowPlugins.GUIPrograms
{
  /// <summary>
  /// Summary description for taggedMenuItem.
  /// </summary>
  public class taggedMenuItem: MenuItem
  {
    public taggedMenuItem(string text): base(text){}

    int mTag = 0;
    XmlNode mXmlTag = null;

    public int Tag
    {
      get
      {
        return mTag;
      }
      set
      {
        mTag = value;
      }
    }

    public XmlNode XmlTag
    {
      get
      {
        return mXmlTag;
      }
      set
      {
        mXmlTag = value;
      }
    }

  }
}
