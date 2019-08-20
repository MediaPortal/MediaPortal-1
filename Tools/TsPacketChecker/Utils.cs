using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindowsApplication13
{
  public sealed class Utils
  {
    public static byte[] GetBytes(byte[] byteData, int offset, int length)
    {
      if (length < 1)
        throw (new ArgumentOutOfRangeException("GetBytes length wrong"));

      try
      {
        byte[] outputBytes = new byte[length];

        for (int index = 0; index < length; index++)
          outputBytes[index] = byteData[offset + index];

        return (outputBytes);
      }
      catch (OutOfMemoryException)
      {
        throw (new ArgumentOutOfRangeException("GetBytes length wrong"));
      }
    }
    public static string GetString(byte[] data, int offset, int length)
    {
      string encoding = "utf-8"; // Standard latin alphabet
      List<byte> bytes = new List<byte>();
      for (int i = 0; i < length; i++)
      {
        byte character = data[offset + i];
        bool notACharacter = false;
        if (i == 0)
        {
          if (character < 0x20)
          {
            switch (character)
            {
              case 0x00:
                break;
              case 0x01:
                encoding = "iso-8859-5";
                break;
              case 0x02:
                encoding = "iso-8859-6";
                break;
              case 0x03:
                encoding = "iso-8859-7";
                break;
              case 0x04:
                encoding = "iso-8859-8";
                break;
              case 0x05:
                encoding = "iso-8859-9";
                break;
              default:
                break;
            }
            notACharacter = true;
          }
        }
        if (character < 0x20 || (character >= 0x80 && character <= 0x9F))
        {
          notACharacter = true;
        }
        if (!notACharacter)
        {
          bytes.Add(character);
        }
      }
      Encoding enc = Encoding.GetEncoding(encoding);
      ASCIIEncoding destEnc = new ASCIIEncoding();
      byte[] destBytes = Encoding.Convert(enc, destEnc, bytes.ToArray());
      return destEnc.GetString(destBytes);
    }
    public static int Convert2BytesToInt(byte[] byteData, int index)
    {
      return (Convert2BytesToInt(byteData, index, 0xff));
    }
    public static int Convert2BytesToInt(byte[] byteData, int index, byte mask)
    {
      return (((byteData[index] & mask) * 256) + (int)byteData[index + 1]);
    }
  }
}
