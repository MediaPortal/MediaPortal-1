using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace MediaPortal.ControlDevices
{
  public abstract class LearnCode : ListViewItem
  {
      string id;
      string displayName;
      bool isEnabled;
      bool isLearned;

    public LearnCode(string id) : base (id)
      {
        this.id = id;
        this.displayName = id;
        this.isEnabled = false;
        this.isLearned = false;
      }

  }
}
