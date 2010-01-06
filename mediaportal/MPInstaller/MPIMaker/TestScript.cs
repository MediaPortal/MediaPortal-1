//css_reference "core.dll";
//css_reference "Databases.dll";
//css_reference "utils.dll";
//css_reference "MPInstaller.Library.dll";
//css_reference "plugins\Windows\Dialogs.dll";

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Text;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using MediaPortal.MPInstaller;


public class InstallScript : MPInstallerScript, IMPInstallerScript
{
  public InstallScript() {}

  /// <summary>
  /// Execute when the package is downloaded via GUI 
  /// </summary>
  public override void GUI_GetOptions()
  {
    base.GUI_GetOptions();
  }

  /// <summary>
  /// Test if version is compatible and show warning
  /// This use when installing via GUI
  /// </summary>
  /// <returns></returns>
  public override bool GUI_Warning()
  {
    return base.GUI_Warning();
  }

  /// <summary>
  /// Test if version is compatible and show warning
  /// </summary>
  /// <returns></returns>
  public override bool Warning()
  {
    return base.Warning();
  }

  /// <summary>
  /// Inits this instance.
  /// executed when the package it is loaded
  /// </summary>
  public override void Init()
  {
    base.Init();
  }

  /// <summary>
  /// Installs the current package.
  /// </summary>
  /// <param name="pb">ProgressBar for overall progress (can bee null) </param>
  /// <param name="pb1">ProgressBar for current copied file (can bee null)</param>
  /// <param name="listbox">Listbox for file listing(can bee null) </param>
  public override void Install(ProgressBar pb, ProgressBar pb1, ListBox listbox)
  {
    base.Install(pb, pb1, listbox);
  }

  /// <summary>
  /// Called when [install file procesed].
  /// </summary>
  /// <param name="mpiFileInfo">The mpi file info.</param>
  public override void OnInstallFileProcesed(MPIFileList mpiFileInfo) {}

  /// <summary>
  /// Called when [install done].
  /// </summary>
  public override void OnInstallDone() {}
}