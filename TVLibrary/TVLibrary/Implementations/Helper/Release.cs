using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace TvLibrary
{
  public class Release
  {
    static public void ComObject(string line,object o)
    {
      if (o != null)
      {
        int hr = Marshal.ReleaseComObject(o);
        // Log.Log.WriteFile("  Release {0} returns {1}", line, hr);
      }
    }
  }
}
