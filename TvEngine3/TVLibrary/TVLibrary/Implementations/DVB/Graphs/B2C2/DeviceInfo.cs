using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TvLibrary.Interfaces;

namespace TvLibrary.Implementations.DVB
{
  public class DeviceInfo
  {
    public String Name { get; set; }
    public String DevicePath { get; set; }
    public CardType CardType { get; set; }
    public uint DeviceId { get; set; }  
  }
}
