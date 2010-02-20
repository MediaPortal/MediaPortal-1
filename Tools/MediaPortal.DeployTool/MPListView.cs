using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.DeployTool
{
  public class MPListView : ListView
  {

    const int WM_KILLFOCUS = 8;
    protected override void WndProc(ref Message m)
    {
      if (m.Msg != WM_KILLFOCUS)
      {
        base.WndProc(ref m);
      }
    }
  }
}
