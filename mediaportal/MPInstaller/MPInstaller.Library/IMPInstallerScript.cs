using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.MPInstaller
{
  public interface IMPInstallerScript
  {
    void GUI_GetOptions();
    bool GUI_Warning();
    bool Warning();
    void Init();
    void OnInstallStart();
    void Install(ProgressBar pb, ProgressBar pb1, ListBox listbox);
    void OnInstallFileProcesed(MPIFileList mpiFileInfo);
    void OnInstallDone();
    MPpackageStruct CurrentPackage {  get ;  set  ;}
    bool EnableWizard { get;  set;}
    bool UnInstall();
  }
}
