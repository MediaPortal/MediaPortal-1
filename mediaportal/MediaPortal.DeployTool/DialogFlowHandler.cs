using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.DeployTool
{
  public enum DialogType
  {
    BASE_INSTALLATION_TYPE,
    SINGLESEAT
  }
  public sealed class DialogFlowHandler
  {
    #region Singleton implementation
    static readonly DialogFlowHandler _instance = new DialogFlowHandler();
    static DialogFlowHandler()
    {
    }
    DialogFlowHandler()
    {
      _dlgs=new List<DeployDialog>();
    }
    public static DialogFlowHandler Instance
    {
      get
      {
        return _instance;
      }
    }
    #endregion

    #region Variables
    private List<DeployDialog> _dlgs;
    private int _currentDlgIndex=-1;
    #endregion

    #region Private members
    private DeployDialog FindDialog(DialogType dlgType)
    {
      for (int i=0;i<_dlgs.Count;i++)
      {
        if (_dlgs[i].type == dlgType)
        {
          _currentDlgIndex = i;
          return _dlgs[i];
        }
      }
      return null;
    }
    #endregion

    #region Public members
    public DeployDialog GetPreviousDlg()
    {
      if (_currentDlgIndex == 0)
        return null;
      _currentDlgIndex--;
      return _dlgs[_currentDlgIndex];
    }
    public DeployDialog GetDialogInstance(DialogType dlgType)
    {
      DeployDialog dlg = FindDialog(dlgType);
      if (dlg == null)
      {
        switch (dlgType)
        {
          case DialogType.BASE_INSTALLATION_TYPE:
            dlg = (DeployDialog)new BaseInstallationTypeDlg();
            break;
          case DialogType.SINGLESEAT:
            dlg = (DeployDialog)new SingleSeatDlg();
            break;
        }
        if (dlg != null)
        {
          _dlgs.Add(dlg);
          _currentDlgIndex++;
        }
      }
      return dlg;
    }
    #endregion
  }
}
