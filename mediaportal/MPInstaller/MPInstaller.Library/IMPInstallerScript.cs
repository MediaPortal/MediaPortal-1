#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

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
    MPpackageStruct CurrentPackage { get; set; }
    bool EnableWizard { get; set; }
    bool UnInstall();
  }
}