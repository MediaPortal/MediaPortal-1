#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

namespace TvLibrary.Teletext
{
  /// <summary>
  /// Odd Parity decoder/encoder
  /// </summary>
  public class OddParity
  {
    private static byte[] m_encode;
    private static byte[] m_decode;

    private static void Initialise()
    {
      // Create array for speed
      m_encode = new byte[128];
      m_decode = new byte[256];
      for (int index = 0; index < 256; index++)
      {
        // Mark as decoding error
        m_decode[index] = 0xff;
      }
      // Fill both arrays
      for (byte data = 0; data < 128; data++)
      {
        // Calculate odd parity
        byte b0 = (byte)(data & 1);
        byte b1 = (byte)((data >> 1) & 1);
        byte b2 = (byte)((data >> 2) & 1);
        byte b3 = (byte)((data >> 3) & 1);
        byte b4 = (byte)((data >> 4) & 1);
        byte b5 = (byte)((data >> 5) & 1);
        byte b6 = (byte)((data >> 6) & 1);
        byte p = (byte)(1 ^ b0 ^ b1 ^ b2 ^ b3 ^ b4 ^ b5 ^ b6);
        byte dataP = (byte)(data | (p << 7));
        m_encode[data] = dataP;
        m_decode[dataP] = data;
      }
    }

    ///<summary>
    /// Encodes the given data
    ///</summary>
    ///<param name="data">Data to encode</param>
    public static void Encode(ref byte data)
    {
      if (null == m_encode)
      {
        Initialise();
      }
      if (data < 0x80)
        if (m_encode != null)
          data = m_encode[data];
    }

    ///<summary>
    /// Checks if the data is correct
    ///</summary>
    ///<param name="data">Data to check</param>
    ///<returns>true, if the data is correct</returns>
    public static bool IsCorrect(byte data)
    {
      if (null == m_encode)
      {
        Initialise();
      }
      if (0xff == m_decode[data])
      {
        return false;
      }
      return true;
    }

    ///<summary>
    /// Decodes the given data
    ///</summary>
    ///<param name="data">Data to decode</param>
    ///<param name="decodingErrors">Number of decoding errors</param>
    public static void Decode(ref byte data, ref int decodingErrors)
    {
      if (null == m_encode)
      {
        Initialise();
      }
      if (0xff == m_decode[data])
      {
        // Decoding error
        decodingErrors++;
        // Replace invalid data by SPACE character, as a hardware decoder would do
        // (this assumes that we're handling display data)
        //data = 0x20;
        data &= 0x7f;
      }
      else
      {
        // Just strip highest bit
        data &= 0x7f;
      }
    }

    /// <summary>
    /// Encodes the given data
    /// </summary>
    /// <param name="data">Data to encode</param>
    public static void Encode(byte[] data)
    {
      for (int index = 0; index < data.Length; index++)
      {
        Encode(ref data[index]);
      }
    }

    ///<summary>
    /// Decodes the given data
    ///</summary>
    ///<param name="data">Data to decode</param>
    ///<param name="decodingErrors">Number of decoding errors</param>
    public static void Decode(byte[] data, ref int decodingErrors)
    {
      for (int index = 0; index < data.Length; index++)
      {
        Decode(ref data[index], ref decodingErrors);
      }
    }
  }
}