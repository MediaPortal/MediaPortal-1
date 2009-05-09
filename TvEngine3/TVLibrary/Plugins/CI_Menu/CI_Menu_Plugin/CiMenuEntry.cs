using System;
using System.Collections.Generic;
using System.Text;

namespace TvEngine
{
  public class CiMenuEntry
  {
    Int32   m_Index;
    String  m_Message;

    public CiMenuEntry(Int32 Index, String Message)
    {
      m_Index   = Index;
      m_Message = Message;
    }
    public override string ToString()
    {
      return String.Format("{0}) {1}", m_Index, m_Message);
    }

  }
}
