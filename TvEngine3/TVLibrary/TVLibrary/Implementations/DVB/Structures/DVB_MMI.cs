#region Copyright
/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
#endregion
using System;
using System.Collections.Generic;
using System.Text;
using TvLibrary.Interfaces;
using System.Runtime.InteropServices;

namespace TvLibrary.Implementations.DVB
{
  /// <summary>
  /// Common class for handling DVB MMI objects
  /// </summary>
  public static class DVB_MMI
  {
    /// <summary>
    /// MMI Tags inside MMI object
    /// </summary>
    public enum MMI_TAGS
    {
      CLOSE = 0x9F8800,
      ENQUIRY = 0x9F8807,
      MENU_MORE = 0x9F880A,
      MENU_LAST = 0x9F8809,
      LIST_MORE = 0x9F880D,
      LIST_LAST = 0x9F880C,
      TEXT_MORE = 0x9F8804,
      TEXT_LAST = 0x9F8803
    };

    /// <summary>
    /// interpretes parts of an byte[] as status int
    /// </summary>
    /// <param name="sourceData"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    public static MMI_TAGS ToMMITag(byte[] sourceData, int offset)
    {
      return (MMI_TAGS)(((Int32)sourceData[offset] << 16) | ((Int32)sourceData[offset + 1] << 8) | ((Int32)sourceData[offset + 2]));
    }

    /// <summary>
    /// interpretes length() info which can be of different size
    /// </summary>
    /// <param name="sourceData">source byte array</param>
    /// <param name="offset">index to start</param>
    /// <param name="bytesRead">returns the number of bytes interpreted</param>
    /// <returns>length of following object</returns>
    public static int GetLength(byte[] sourceData, int offset, out int bytesRead)
    {
      byte bLen = sourceData[offset];
      // if highest bit set, it means there are > 127 bytes
      if ((bLen & 0x80) == 0)
      {
        bytesRead = 1;
        return bLen;
      }
      else
      {
        bLen &= 0x7f; // clear 8th bit; remaining 7 bit tell the number of following bytes to interpret (most probably 2)
        bytesRead = 1 + bLen;
        Int32 shiftBy;
        Int32 iLen = 0;
        for (Int32 p = 0; p < bLen; p++)
        {
          shiftBy = (Int32)(bLen - p - 1) * 8; // number of bits to shift up, i.e. 2 bytes -> 1st byte <<8, 2nd byte <<0
          iLen = iLen | (sourceData[offset + 1 + p] << shiftBy); // shift byte to right position, concat by "or" operation
        }
        return iLen;
      }
    }

    /// <summary>
    /// Sets length info to byte positions
    /// </summary>
    /// <param name="length">(word) Length</param>
    /// <param name="uLength1">(byte) LowByte of Length</param>
    /// <param name="uLength2">(byte) HighByte of Length</param>
    public static void SetLength(UInt16 length, ref byte uLength1, ref byte uLength2)
    {
      uLength1 = ((byte)(length % 256));
      uLength2 = ((byte)(length / 256));
    }

    /// <summary>
    /// Converts bytes to String
    /// </summary>
    /// <param name="sourceData">source byte[]</param>
    /// <param name="offset">starting offset</param>
    /// <param name="length">length</param>
    /// <returns>String</returns>
    public static String BytesToString(byte[] sourceData, int offset, int length)
    {
      StringBuilder StringEntry = new StringBuilder();
      for (int l = offset; l < offset + length; l++)
      {
        StringEntry.Append((char)sourceData[l]);
      }
      return StringEntry.ToString();
    }

    /// <summary>
    /// intepretes string for ci menu entries
    /// </summary>
    /// <param name="sourceData">source byte array</param>
    /// <param name="offset">index to start</param>
    /// <param name="menuEntries">reference to target string list</param>
    /// <returns>offset for further readings</returns>
    public static int GetCIText(byte[] sourceData, int offset, ref List<String> menuEntries)
    {
      byte Length; // We assume that text Length is smaller 127
      MMI_TAGS Tag;

      Tag = ToMMITag(sourceData, offset);
      if ((Tag != MMI_TAGS.TEXT_MORE) && (Tag != MMI_TAGS.TEXT_LAST))
      {
        return -1;
      }

      Length = sourceData[offset + 3];

      // Check if our assumption is TRUE
      if (Length > 127)
        return -1; // Length is > 127

      if (Length > 0)
      {
        // Create string from byte array 
        String menuEntry = BytesToString(sourceData, offset + 4, Length);
        //Log.Log.Debug("FireDTV: MMI Parse GetCIText: {0}", menuEntry.ToString());
        menuEntries.Add(menuEntry.ToString());
      }
      else
      {
        // empty String ? add to keep correct index positions
        menuEntries.Add("");
      }
      return (Length + 4);
    }

    /// <summary>
    /// Creates a "CloseMMI" data set
    /// </summary>
    /// <param name="uData">reference to byte[]</param>
    public static void CreateMMIClose(ref byte[] uData)
    {
      // MMI Tag
      uData[0] = 0x9F;
      uData[1] = 0x88;
      uData[2] = 0x00;
      uData[3] = 0x01; // length field
      uData[4] = 0x00; // close_mmi_cmd_id (immediately)
    }

    /// <summary>
    /// Creates a "SelectMenuChoice" data set
    /// </summary>
    /// <param name="choice">selected index (0 means back)</param>
    /// <param name="uData">reference to byte[]</param>
    public static void CreateMMISelect(byte choice, ref byte[] uData)
    {
      // MMI Tag
      uData[0] = 0x9F;
      uData[1] = 0x88;
      uData[2] = 0x0B; // send choice
      uData[3] = 0x01; // length field
      uData[4] = choice; // choice
    }

    /// <summary>
    /// Creates an CI Menu Answer package
    /// </summary>
    /// <param name="Cancel">true to cancel</param>
    /// <param name="Answer">answer string</param>
    /// <param name="uData">target buffer</param>
    /// <param name="uLength1">length byte1</param>
    /// <param name="uLength2">length byte2</param>
    /// <returns>byte array</returns>
    public static void CreateMMIAnswer(bool Cancel, String Answer, ref byte[] uData, ref byte uLength1, ref byte uLength2)
    {
      if (Cancel == true)
      {
        uData[3] = 1; // length field
        uData[4] = 0; // answ_id "cancel"
        SetLength(5, ref uLength1, ref uLength2); // set correct length
      }
      else
      {
        uData[3] = (byte)(Answer.Length +1); // length field + 1 byte answ_id
        uData[4] = 1; // answ_id "answer"
        char[] answerChars = Answer.ToCharArray();
        for (int p = 0; p < answerChars.Length; p++)
        {
          uData[5 + p] = (byte)answerChars[p]; // answer string
        }
        SetLength((ushort)(5 + answerChars.Length), ref uLength1, ref uLength2); // set correct length
      }
      // MMI Tag
      uData[0] = 0x9F;
      uData[1] = 0x88;
      uData[2] = 0x08; // send enquiry answer
    }

    /// <summary>
    /// returns a safe "printable" character or _
    /// </summary>
    /// <param name="b">byte code</param>
    /// <returns>char</returns>
    private static char ToSafeAscii(byte b)
    {
      if (b >= 32 && b <= 126)
      {
        return (char)b;
      }
      return '_';
    }
    /// <summary>
    /// Output binary buffer to log for debugging
    /// </summary>
    /// <param name="sourceData">source byte[]</param>
    /// <param name="offset">starting offset</param>
    /// <param name="length">total length</param>
    public static void DumpBinary(byte[] sourceData, int offset, int length)
    {
      StringBuilder row = new StringBuilder();
      StringBuilder rowText = new StringBuilder();

      for (int position = offset; position < offset + length; position++)
      {
        if (position == offset || position % 0x10 == 0)
        {
          if (row.Length > 0)
          {
            Log.Log.WriteFile(String.Format("{0}|{1}", row.ToString().PadRight(55, ' '), rowText.ToString().PadRight(16, ' ')));
          }
          rowText.Length = 0;
          row.Length = 0;
          row.AppendFormat("{0:X4}|", position);
        }
        row.AppendFormat("{0:X2} ", sourceData[position]); // the hex code
        rowText.Append(ToSafeAscii(sourceData[position])); // the ascii char
      }
      if (row.Length > 0)
      {
        Log.Log.WriteFile(String.Format("{0}|{1}", row.ToString().PadRight(55, ' '), rowText.ToString().PadRight(16, ' ')));
      }
    }
    /// <summary>
    /// Output binary buffer to log for debugging (Wrapper for IntPtr)
    /// </summary>
    /// <param name="sourceData">source IntPtr</param>
    /// <param name="offset">starting offset</param>
    /// <param name="length">total length</param>
    public static void DumpBinary(IntPtr sourceData, int offset, int length)
    {
      byte[] tmpBuffer = IntPtrToByteArray(sourceData, offset, length);
      DumpBinary(tmpBuffer, offset, length);
    }

    /// <summary>
    /// Copys data from IntPtr to new byte[]
    /// </summary>
    /// <param name="sourceData">IntPtr to source data</param>
    /// <param name="offset">starting offset</param>
    /// <param name="length">length</param>
    /// <returns>new byte[]</returns>
    public static byte[] IntPtrToByteArray(IntPtr sourceData, int offset, int length)
    {
      byte[] tmpBuffer = new byte[length];
      Marshal.Copy(sourceData, tmpBuffer, offset, length);
      return tmpBuffer;
    }
  }

  /// <summary>
  /// Handles APDU (MMI) objects and perform callbacks
  /// This is a common class that is able to handle MMI objects according to DVB spec
  /// Used for DVB cards that return binary data (DigitalEverywhere FireDTV, Hauppauge WinTvCI, others?)
  /// </summary>
  public class DVB_MMI_Handler
  {
    private ICiMenuCallbacks m_ciMenuCallback;
    private String m_cardName;

    /// <summary>
    /// Sets the callback handler
    /// </summary>
    /// <param name="ciMenuHandler"></param>
    public bool SetCiMenuHandler(ICiMenuCallbacks ciMenuHandler)
    {
      if (ciMenuHandler != null)
      {
        m_ciMenuCallback = ciMenuHandler;
        return true;
      }
      return false;
    }
    /// <summary>
    /// Constructor for an APDU based MMI parser
    /// </summary>
    /// <param name="CardName">Card type for logging</param>
    /// <param name="CiMenuCallbacks">Callbacks</param>
    public DVB_MMI_Handler(String CardName, ICiMenuCallbacks CiMenuCallbacks)
    {
      m_ciMenuCallback = CiMenuCallbacks;
      m_cardName = CardName;
    }

    /// <summary>
    /// Handles APDU (MMI) objects and perform callbacks
    /// </summary>
    /// <param name="MMI">MMI byte[]</param>
    /// <param name="MMI_length">length</param>
    public void HandleMMI(byte[] MMI, int MMI_length)
    {
      // parse starting 3 bytes == tag
      DVB_MMI.MMI_TAGS uMMITag = DVB_MMI.ToMMITag(MMI, 0);

      // dumping binary APDU
#if DEBUG
      DVB_MMI.DumpBinary(MMI, 0, MMI_length);
#endif 

      // calculate length and offset
      int countLengthBytes;
      int mmiLength = DVB_MMI.GetLength(MMI, 3 /* bytes for mmi_tag */, out countLengthBytes);
      int mmiOffset = 3 + countLengthBytes; // 3 bytes mmi tag + 1 byte length field ?

      Log.Log.Debug("{0}: MMITag:{1}, MMIObjectLength: {2} ({2:X2}), mmiOffset: {3}", m_cardName, uMMITag, mmiLength, mmiOffset);

      int offset = 0; // starting with 0; reading whole struct from start
      if (uMMITag == DVB_MMI.MMI_TAGS.CLOSE)
      {
        // Close menu
        byte nDelay = 0;
        byte CloseCmd = MMI[mmiOffset + 0];
        if (CloseCmd != 0)
        {
          nDelay = MMI[mmiOffset + 1];
        }
        if (m_ciMenuCallback != null)
        {
          Log.Log.Debug("{0}: OnCiClose()", m_cardName);
          m_ciMenuCallback.OnCiCloseDisplay(nDelay);
        }
        else
        {
          Log.Log.Debug("{0}: OnCiCloseDisplay: cannot do callback!", m_cardName);
        }
      }
      if (uMMITag == DVB_MMI.MMI_TAGS.ENQUIRY)
      {
        // request input
        bool bPasswordMode = false;
        byte answer_text_length = MMI[mmiOffset + 1];
        string strText = "";

        if ((MMI[mmiOffset + 0] & 0x01) != 0)
        {
          bPasswordMode = true;
        }

        // mmioffset +4 because there a 2 other bytes before text starts
        // length is these 2 bytes shorter 
        strText = DVB_MMI.BytesToString(MMI, mmiOffset + 4, mmiLength - mmiOffset - 2);
        if (m_ciMenuCallback != null)
        {
          Log.Log.Debug("{0}: OnCiRequest: bPasswordMode:{1}, answer_text_length:{2}, strText:{3}", m_cardName, bPasswordMode, answer_text_length, strText);
          m_ciMenuCallback.OnCiRequest(bPasswordMode, answer_text_length, strText);
        }
        else
        {
          Log.Log.Debug("{0}: OnCiRequest: cannot do callback!", m_cardName);
        }
      }
      if (uMMITag == DVB_MMI.MMI_TAGS.LIST_LAST || uMMITag == DVB_MMI.MMI_TAGS.MENU_LAST ||
          uMMITag == DVB_MMI.MMI_TAGS.MENU_MORE || uMMITag == DVB_MMI.MMI_TAGS.LIST_MORE)
      {
        // step forward; begin with offset+1; stop when 0x9F reached
        // should be modified to offset + mmioffset+1 ?
        offset++;
        while (MMI[offset] != (byte)0x9F)
        {
          //Log.Log.Debug("Skip to offset {0} value {1:X2}", offset, MMI[offset]);
          offset++;
        }
        uMMITag = DVB_MMI.ToMMITag(MMI, offset); // get next MMI tag
        Log.Log.Debug("{0}: MMI Parse: Got MENU_LAST, skipped to next block on index: {1}; new Tag {2}", m_cardName, offset, uMMITag);

        int nChoices = 0;
        List<string> Choices = new List<string>();
        // Always three line with menu info (DVB Standard)
        // Title Text
        offset += DVB_MMI.GetCIText(MMI, offset, ref Choices);
        // Subtitle Text
        offset += DVB_MMI.GetCIText(MMI, offset, ref Choices);
        // Bottom Text
        offset += DVB_MMI.GetCIText(MMI, offset, ref Choices);

        // first step through the choices, to get info and count them
        int max = 20;
        while (max-- > 0)
        {
          // if the offset gets to mmi object length then end here
          if (offset >= mmiLength - 1)
            break;

          offset += DVB_MMI.GetCIText(MMI, offset, ref Choices);
          nChoices++;
        }
        // when title and choices are ready now, send to client
        if (m_ciMenuCallback != null)
        {
          m_ciMenuCallback.OnCiMenu(Choices[0], Choices[1], Choices[2], nChoices);
          for (int c = 3; c < Choices.Count; c++)
          {
            m_ciMenuCallback.OnCiMenuChoice(c - 3, Choices[c]);
          }
        }
        else
        {
          Log.Log.Debug("{0}: OnCiMenu: cannot do callback!", m_cardName);
          for (int c = 0; c < Choices.Count; c++)
          {
            Log.Log.Debug("{0}: {1} : {2}", m_cardName, c, Choices[c]);
          }
        }
      }
    }
  }
}
