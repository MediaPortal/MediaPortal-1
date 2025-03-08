using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaPortal.Services
{
  [Flags]
  public enum NotifyMessageClassEnum : long
  {
    General = 0,
    Video = 1,
    Music = 2,
    Picture = 4,
    TV = 8,
    Radio = 16,
    Recording = 32,
    Channel = 64,
    News = 128,
    Extension = 256,
    Movie = 512,
    Show = 1024,
    Episode = 2048,
    Weather = 4096,
    Game = 8192,
    Sport = 16384,
    Settings = 32768,
    Online = 65536,
    Social = 131072,
    Community = 262144,
    Mail = 524288,
    Feed = 1048576,
    All = long.MaxValue
  }
}
