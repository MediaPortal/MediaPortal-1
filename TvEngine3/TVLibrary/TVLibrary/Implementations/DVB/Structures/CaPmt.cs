using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TvLibrary.Implementations.DVB.Structures
{
  enum ListManagementType : byte
  {
    More=0,
    First=1,
    Last=2,
    Only=3,
    Add=4,
    Update=5
  };
  enum CommandIdType: byte
  {
    Descrambling=1,
    MMI=2,
    Query=3,
    NotSelected=4
  };

  public class CaPMT
  {
    public ListManagementType CAPmt_Listmanagement; //  8 bit
    public int  ProgramNumber;                      // 16 bit
    public int  reserved0;                          //  2 bit
    public int  VersionNumber;                      //  5 bit
    public int  CurrentNextIndicator;               //  1 bit
    public int  reserved1;                          //  4 bit
    public int  ProgramInfoLength;                  // 12 bit
    public CommandIdType CAPmt_CommandID_PRG;       // 8  bit
    public ArrayList CADescriptors_PRG;             // x  bit
    public int StreamType;                          // 8 bit
    public int reserved2;                           // 3 bit
    public int ElementaryStreamPID;                 // 13 bit
    public int reserved3;                           // 4  bit
    public int ElementaryStreamInfoLength;          // 12 bit
    public CommandIdType CAPmt_CommandID_ES;                  // 8 bit
    public ArrayList CADescriptors_ES;


  }

}
