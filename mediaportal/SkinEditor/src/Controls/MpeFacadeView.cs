using System.Xml.XPath;
using Mpe.Controls.Properties;

namespace Mpe.Controls
{
  /// <summary>
  /// Summary description for MpeFacadeView.
  /// </summary>
  public class MpeFacadeView : MpeGroup
  {
    #region Constructors

    public MpeFacadeView() : base()
    {
      MpeLog.Debug("MpeFacadeView()");
      Type = MpeControlType.FacadeView;
      AllowDrop = false;
    }

    public MpeFacadeView(MpeFacadeView facade) : base(facade)
    {
      MpeLog.Debug("MpeFacadeView(facade)");
      Type = MpeControlType.FacadeView;
      AllowDrop = false;
    }

    public override void Load(XPathNodeIterator iterator, MpeParser parser)
    {
      MpeLog.Debug("FacadeView.Load()");
      base.Load(iterator, parser);
    }

    #endregion
  }
}