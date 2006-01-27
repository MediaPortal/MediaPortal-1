using System.Xml;
using System.Xml.XPath;

namespace Mpe.Controls
{
  /// <summary>
  /// All items that can be editted by the MediaPortal Editor must implement this interface
  /// </summary>
  public interface MpeResource
  {
    #region Properties

    int Id { get; set; }
    string Name { get; }
    string Description { get; }
    bool Modified { get; set; }
    bool Masked { get; set; }

    #endregion

    #region Methods

    void Prepare();
    void Destroy();
    void Load(XPathNodeIterator iterator, MpeParser parser);
    void Save(XmlDocument doc, XmlNode node, MpeParser parser, MpeControl reference);

    #endregion
  }
}