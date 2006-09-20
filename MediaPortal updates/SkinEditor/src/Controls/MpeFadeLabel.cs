using System.ComponentModel;
using Mpe.Controls.Properties;

namespace Mpe.Controls
{
  /// <summary>
  ///
  /// </summary>
  public class MpeFadeLabel : MpeLabel
  {
    #region Variables

    private MpeItemManager items;

    #endregion

    #region Constructors

    public MpeFadeLabel() : base()
    {
      MpeLog.Debug("MpeFadeLabel()");
      Init();
    }

    public MpeFadeLabel(MpeFadeLabel label) : base(label)
    {
      MpeLog.Debug("MpeFadeLabel(label)");
      Init();
    }

    private void Init()
    {
      MpeLog.Debug("MpeFadeLabel.Init()");
      Type = MpeControlType.FadeLabel;
      items = new MpeItemManager();
    }

    #endregion

    #region Properties

    [Category("Items")]
    public MpeItemManager Items
    {
      get { return items; }
      set
      {
        items = value;
        Invalidate(false);
      }
    }

    #endregion
  }
}