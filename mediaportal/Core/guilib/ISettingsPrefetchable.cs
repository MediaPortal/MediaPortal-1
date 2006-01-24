using System;
using System.Collections.Generic;
using System.Text;

namespace MediaPortal.Profile
{
  public delegate void RememberDelegate(string section, string entry, object value);
  public interface ISettingsPrefetchable
  {
    void Prefetch(RememberDelegate function);
  }
}
