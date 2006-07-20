using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.DVB.Structures
{
  public struct CaPMT
  {
    public int CAPmt_Listmanagement;
    public int ProgramNumber;
    public int reserved0;
    public int VersionNumber;
    public int CurrentNextIndicator;
    public int reserved1;
    public int ProgramInfoLength;
    public int CAPmt_CommandID_PRG;
    public ArrayList CADescriptors_PRG;
    public int StreamType;
    public int reserved2;
    public int ElementaryStreamPID;
    public int reserved3;
    public int ElementaryStreamInfoLength;
    public int CAPmt_CommandID_ES;
    public ArrayList CADescriptors_ES;


  }

}
