using System;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin
{
  public static class ByteArray
  {
    public static bool AreEqual(byte[] bytes1, byte[] bytes2)
    {
      if ((bytes1 != null) || (bytes2 != null))
      {
        if ((bytes1 == null) || (bytes2 == null))
        {
          return false;
        }
        if (!bytes1.Equals(bytes2))
        {
          if (bytes1.Length != bytes2.Length)
          {
            return false;
          }
          for (int i = 0; i < bytes1.Length; i++)
          {
            if (bytes1[i] != bytes2[i])
            {
              return false;
            }
          }
        }
      }
      return true;
    }
  }
}

