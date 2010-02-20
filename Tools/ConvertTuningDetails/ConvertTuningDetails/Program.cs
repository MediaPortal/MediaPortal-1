using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using TvLibrary.Log;

namespace ConvertTuningDetails
{
  class Program
  {
    static void Main(string[] args)
    {
      DirectoryInfo Dir;
      FileInfo[] files;

      Dir = new DirectoryInfo(String.Format(@"{0}\TuningParameters\", Log.GetPathName()));
      files = Dir.GetFiles("*.dvbc");
      foreach (FileInfo file in files)
      {
        DVBC.ConvertList(file.FullName);
      }

      Dir = new DirectoryInfo(String.Format(@"{0}\TuningParameters\", Log.GetPathName()));
      files = Dir.GetFiles("*.ini");
      foreach (FileInfo file in files)
      {
        // no s2, get combined
        if (!file.FullName.EndsWith("-S2.ini"))
          DVBS.ConvertList(file.FullName, false);
      }

      Dir = new DirectoryInfo(String.Format(@"{0}\TuningParameters\", Log.GetPathName()));
      files = Dir.GetFiles("*.qam");
      foreach (FileInfo file in files)
      {
        ATSC.ConvertList(file.FullName);
      }
      DVBT.ConvertList(String.Format(@"{0}\TuningParameters\dvbt.xml", Log.GetPathName()));
    }
  }
}
