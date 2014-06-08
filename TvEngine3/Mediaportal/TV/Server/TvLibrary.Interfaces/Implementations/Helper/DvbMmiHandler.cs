#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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

using System;
using System.Collections.Generic;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using Mediaportal.TV.Server.TVLibrary.Interfaces.TunerExtension;

namespace Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Helper
{
  /// <summary>
  /// This class parses DVB EN 50221 compliant MMI APDU objects, performing CA menu call backs appropriately.
  /// It is compatible with any conditional access interface that provides access to "raw" DVB compliant APDU
  /// objects.
  /// </summary>
  public class DvbMmiHandler
  {
    #region MMI/APDU interpretation

    /// <summary>
    /// Handle raw MMI data which contains one or more APDU objects, performing CA menu call backs appropriately.
    /// </summary>
    /// <param name="mmi">The MMI data.</param>
    /// <param name="callBack">The call back delegate.</param>
    public static void HandleMmiData(byte[] mmi, IConditionalAccessMenuCallBack callBack)
    {
      Log.Debug("DVB MMI: handle MMI data");
      if (mmi == null || mmi.Length < 4)
      {
        Log.Error("DVB MMI: data not supplied or too short");
      }

      //Dump.DumpBinary(mmi, mmi.Length);

      // The first 3 bytes contains an MMI tag to tell us which APDUs we should expect to encounter.
      MmiTag tag = DvbMmiHandler.ReadMmiTag(mmi, 0);
      int countLengthBytes;
      int apduLength = ReadLength(mmi, 3, out countLengthBytes);
      Log.Debug("DVB MMI: data length = {0}, first APDU tag = {1}, length = {2}", mmi.Length, tag, apduLength);
      int offset = 3 + countLengthBytes;
      if (apduLength < 0 || offset + apduLength != mmi.Length)
      {
        Log.Error("DVB MMI: APDU length is invalid, APDU length = {0}, offset = {1}, MMI data length = {2}", apduLength, offset, mmi.Length);
        Dump.DumpBinary(mmi);
        return;
      }

      if (tag == MmiTag.CloseMmi)
      {
        // The CAM is requesting that we close the menu.
        HandleClose(mmi, offset, apduLength, callBack);
      }
      else if (tag == MmiTag.Enquiry)
      {
        // The CAM wants input from the user.
        HandleEnquiry(mmi, offset, apduLength, callBack);
      }
      else if (tag == MmiTag.ListLast || tag == MmiTag.MenuLast ||
          tag == MmiTag.MenuMore || tag == MmiTag.ListMore)
      {
        // The CAM is providing a menu or list to present to the user.
        HandleMenu(mmi, offset, apduLength, callBack);
      }
      else
      {
        Log.Warn("DVB MMI: unexpected MMI tag {0}", tag);
        Dump.DumpBinary(mmi);
      }
    }

    private static void HandleClose(byte[] apdu, int offset, int apduLength, IConditionalAccessMenuCallBack callBack)
    {
      Log.Debug("DVB MMI: handle close");

      if (offset >= apdu.Length)
      {
        Log.Error("DVB MMI: invalid close APDU, offset = {0}, APDU length = {1}", offset, apdu.Length);
        return;
      }

      MmiCloseType command = (MmiCloseType)apdu[offset++];
      int delay = 0;
      if (command == MmiCloseType.Delayed)
      {
        if (offset >= apdu.Length)
        {
          Log.Error("DVB MMI: invalid delayed close APDU, offset = {0}, APDU length = {1}", offset, apdu.Length);
          return;
        }
        delay = apdu[offset];
      }

      Log.Debug("  command = {0}", command);
      Log.Debug("  delay   = {0} s", delay);

      if (callBack != null)
      {
        callBack.OnCiCloseDisplay(delay);
      }
      else
      {
        Log.Debug("DVB MMI: menu call back not set");
      }
    }

    private static void HandleEnquiry(byte[] apdu, int offset, int apduLength, IConditionalAccessMenuCallBack callBack)
    {
      Log.Debug("DVB MMI: handle enquiry");

      if (offset + 4 >= apdu.Length)
      {
        Log.Error("DVB MMI: invalid enquiry APDU, offset = {0}, APDU length = {1}", offset, apdu.Length);
        return;
      }

      bool passwordMode = (apdu[offset++] & 0x01) != 0;
      byte expectedAnswerLength = apdu[offset++];
      // Note: there are 2 other bytes before text starts.
      string prompt = DvbTextConverter.Convert(apdu, apdu.Length - offset - 2, offset + 2);
      Log.Debug("  prompt = {0}", prompt);
      Log.Debug("  length = {0}", expectedAnswerLength);
      Log.Debug("  blind  = {0}", passwordMode);

      if (callBack != null)
      {
        callBack.OnCiRequest(passwordMode, expectedAnswerLength, prompt);
      }
      else
      {
        Log.Debug("DVB MMI: menu call back not set");
      }
    }

    private static void HandleMenu(byte[] apdu, int offset, int apduLength, IConditionalAccessMenuCallBack callBack)
    {
      Log.Debug("DVB MMI: handle menu");

      int stringCount = apdu[offset++];
      List<string> strings = new List<string>();

      // Read the menu entries into the entries list.
      while (stringCount > 0 && offset < apdu.Length)
      {
        if (apdu[offset] != 0x9f)
        {
          Log.Error("DVB MMI: unexpected APDU format, expected MMI tag at offset {0}", offset);
          Dump.DumpBinary(apdu);
          return;
        }
        int bytesRead;
        string s = ReadText(apdu, offset, out bytesRead);
        if (s == null)
        {
          Log.Error("DVB MMI: unexpected APDU format, null entry at offset {0}", offset);
          Dump.DumpBinary(apdu);
          return;
        }
        strings.Add(s);
        offset += bytesRead;
      }
      if (strings.Count < 3)
      {
        Log.Error("DVB MMI: unexpected MMI format, {0} string(s)", strings.Count);
        Dump.DumpBinary(apdu);
        return;
      }

      if (callBack == null)
      {
        Log.Debug("DVB MMI: menu call back not set");
      }

      Log.Debug("  title     = {0}", strings[0]);
      Log.Debug("  sub-title = {0}", strings[1]);
      Log.Debug("  footer    = {0}", strings[2]);
      Log.Debug("  # entries = {0}", strings.Count - 3);
      if (callBack != null)
      {
        callBack.OnCiMenu(strings[0], strings[1], strings[2], strings.Count - 3);
      }

      for (byte i = 3; i < strings.Count; i++)
      {
        Log.Debug("    {0, -7} = {1}", i - 2, strings[i]);
        if (callBack != null)
        {
          callBack.OnCiMenuChoice(i - 3, strings[i]);
        }
      }
    }

    #endregion

    #region helpers

    /// <summary>
    /// Interpret an MMI tag.
    /// </summary>
    /// <param name="sourceData">An MMI data array containing the tag.</param>
    /// <param name="offset">The offset of the tag in sourceData.</param>
    /// <returns>the tag</returns>
    public static MmiTag ReadMmiTag(byte[] sourceData, int offset)
    {
      if (offset + 2 >= sourceData.Length)
      {
        return MmiTag.Unknown;
      }
      return (MmiTag)((sourceData[offset] << 16) | (sourceData[offset + 1] << 8) | (sourceData[offset + 2]));
    }

    /// <summary>
    /// Write an MMI tag.
    /// </summary>
    /// <param name="tag">The tag to write.</param>
    /// <param name="outputData">The MMI data array to write the tag into.</param>
    /// <param name="offset">The offset of the tag in outputData.</param>
    public static void WriteMmiTag(MmiTag tag, ref byte[] outputData, int offset)
    {
      if (outputData == null || offset + 2 >= outputData.Length)
      {
        Log.Error("DVB MMI: failed to write tag");
        return;
      }
      outputData[offset++] = (byte)(((int)tag >> 16) & 0xff);
      outputData[offset++] = (byte)(((int)tag >> 8) & 0xff);
      outputData[offset++] = (byte)((int)tag & 0xff);
    }

    /// <summary>
    /// Interpret a DVB MMI APDU length field, which is encoded using ASN.1 length encoding rules.
    /// </summary>
    /// <remarks>
    /// The value is encoded in a variable number of bytes as follows:
    /// - if the most significant bit in the first byte is set, it means that the first byte contains the number
    /// of byte(s) in which the length is encoded - the following byte(s) contain the length value
    /// - if the most significant bit in the first byte is *not* set, the first byte is the length value
    /// </remarks>
    /// <param name="sourceData">An MMI data array containing the length field.</param>
    /// <param name="offset">The offset of the length field in sourceData.</param>
    /// <param name="bytesRead">The number of bytes in the length field.</param>
    /// <returns>the value encoded in the length field, otherwise <c>-1</c> if the length is not valid</returns>
    public static int ReadLength(byte[] sourceData, int offset, out int bytesRead)
    {
      bytesRead = -1;

      if (sourceData == null || offset >= sourceData.Length)
      {
        Log.Error("DVB MMI: failed to read length, offset {0} is out of range {1}", offset, sourceData.Length);
        return -1;
      }

      byte byte1 = sourceData[offset++];

      // When the MSB of the first byte is not set, the first byte contains the length value.
      if ((byte1 & 0x80) == 0)
      {
        bytesRead = 1;
        return byte1;
      }

      // Multi-byte length field.
      bytesRead = byte1 & 0x7f;
      if (bytesRead > 4)
      {
        Log.Error("DVB MMI: length encoded in {0} bytes, can't be interpretted", bytesRead);
        return -1;
      }
      if (offset + bytesRead >= sourceData.Length)
      {
        Log.Error("DVB MMI: failed to read length, length byte count {0} is invalid", bytesRead);
        return -1;
      }

      int value = sourceData[offset++];
      for (byte i = 1; i < bytesRead; i++)
      {
        value = (value << 8) + sourceData[offset++];
      }
      bytesRead++;    // (for the first byte read into byte1)
      return value;
    }

    /// <summary>
    /// Write a DVB MMI APDU length field using ASN.1 length encoding rules.
    /// </summary>
    /// <remarks>
    /// The value is encoded in a variable number of bytes as follows:
    /// - if the most significant bit in the first byte is set, it means that the first byte contains the number
    /// of byte(s) in which the length is encoded - the following byte(s) contain the length value
    /// - if the most significant bit in the first byte is *not* set, the first byte is the length value
    /// </remarks>
    /// <param name="length">The length to write.</param>
    /// <param name="outputData">The MMI data array to write the length field into.</param>
    /// <param name="offset">The offset of the field in outputData.</param>
    /// <param name="bytesWritten">The number of bytes used to encode the length field.</param>
    public static void WriteLength(int length, ref byte[] outputData, int offset, out int bytesWritten)
    {
      bytesWritten = 1;

      // Figure out how many bytes we're going to need to encode the length.
      if (length > 127)
      {
        int tempLength = length;
        while (tempLength > 255)
        {
          tempLength = tempLength >> 8;
          bytesWritten++;
        }
      }

      if (outputData == null || offset + bytesWritten >= outputData.Length)
      {
        Log.Error("DVB MMI: failed to write length");
        return;
      }

      // One byte length.
      if (length < 128)
      {
        outputData[offset] = (byte)length;
        return;
      }

      // Multi-byte length.
      outputData[offset] = (byte)(bytesWritten-- | 0x80);
      while (bytesWritten > 0)
      {
        outputData[offset + bytesWritten--] = (byte)(length & 0xff);
        length = length >> 8;
      }
    }

    /// <summary>
    /// Intepret an MMI text APDU.
    /// </summary>
    /// <param name="sourceData">An MMI data array containing the text APDU.</param>
    /// <param name="offset">The offset of the APDU in sourceData.</param>
    /// <param name="bytesRead">The number of bytes in the APDU field.</param>
    /// <returns>the string encoded in the text APDU, otherwise <c>null</c></returns>
    public static string ReadText(byte[] sourceData, int offset, out int bytesRead)
    {
      bytesRead = -1;

      MmiTag tag = ReadMmiTag(sourceData, offset);
      if (tag != MmiTag.TextMore && tag != MmiTag.TextLast)
      {
        Log.Error("DVB MMI: invalid text tag {0}", tag);
        return null;
      }

      int lengthByteCount;
      int length = ReadLength(sourceData, offset + 3, out lengthByteCount);
      if (length == -1)
      {
        return null;
      }

      bytesRead = 3 + lengthByteCount;
      if (length > 0)
      {
        bytesRead += length;
        return DvbTextConverter.Convert(sourceData, length, offset + 3 + lengthByteCount);
      }
      else
      {
        return string.Empty;
      }
    }

    #endregion

    #region MMI/APDU encoding

    /// <summary>
    /// Create a "close_mmi" APDU, used to close an MMI dialog.
    /// </summary>
    /// <param name="delay">The delay before the host should close the MMI dialog.</param>
    /// <returns>the APDU</returns>
    public static byte[] CreateMmiClose(byte delay)
    {
      byte[] apdu;
      if (delay > 0)
      {
        apdu = new byte[6];
        apdu[3] = 2;    // length
        apdu[4] = (byte)MmiCloseType.Delayed;
        apdu[5] = delay;
      }
      else
      {
        apdu = new byte[5];
        apdu[3] = 1;    // length
        apdu[4] = (byte)MmiCloseType.Immediate;
      }
      WriteMmiTag(MmiTag.CloseMmi, ref apdu, 0);
      return apdu;
    }

    /// <summary>
    /// Create a "menu_answ" APDU, used to select an entry in an MMI menu.
    /// </summary>
    /// <param name="choice">The elected index (0 means back)</param>
    /// <returns>the APDU</returns>
    public static byte[] CreateMmiMenuAnswer(byte choice)
    {
      byte[] apdu = new byte[5];
      WriteMmiTag(MmiTag.MenuAnswer, ref apdu, 0);
      apdu[3] = 1;    // length
      apdu[4] = choice;
      return apdu;
    }

    /// <summary>
    /// Create an enquiry "answ" APDU, used to respond to an enquiry from the host.
    /// </summary>
    /// <param name="responseType">The response type.</param>
    /// <param name="answer">The answer.</param>
    /// <returns>the APDU</returns>
    public static byte[] CreateMmiEnquiryAnswer(MmiResponseType responseType, string answer)
    {
      if (answer == null)
      {
        answer = string.Empty;
      }
      char[] answerChars = answer.ToCharArray();

      // Encode the length into a temporary array so we know how many bytes are required for the APDU.
      byte[] length = new byte[5];  // max possible bytes for length field
      int lengthByteCount;
      WriteLength(answerChars.Length + 1, ref length, 0, out lengthByteCount);  // + 1 for response type

      // Encode the APDU.
      byte[] apdu = new byte[answerChars.Length + lengthByteCount + 4]; // + 4 = 3 for MMI tag, 1 for response type
      WriteMmiTag(MmiTag.Answer, ref apdu, 0);
      Buffer.BlockCopy(length, 0, apdu, 3, lengthByteCount);
      apdu[3 + lengthByteCount] = (byte)responseType;
      Buffer.BlockCopy(answerChars, 0, apdu, 4 + lengthByteCount, answerChars.Length);
      return apdu;
    }

    /// <summary>
    /// Create a "ca_pmt" APDU, used to query or manage a host's capabilities and actions in relation to a
    /// particular service.
    /// </summary>
    /// <param name="caPmt">A CA PMT structure encoded according to EN 50221, describing the service and
    ///   associated elementary streams.</param>
    /// <returns>the APDU</returns>
    public static byte[] CreateCaPmtRequest(byte[] caPmt)
    {
      if (caPmt == null || caPmt.Length == 0)
      {
        // This is bad!
        throw new ArgumentException("DVB MMI: CA PMT passed to CreateCaPmtRequest() is not set!");
      }

      // Encode the length into a temporary array so we know how many bytes are required for the APDU.
      byte[] length = new byte[5];  // max possible bytes for length field
      int lengthByteCount;
      WriteLength(caPmt.Length, ref length, 0, out lengthByteCount);

      // Encode the APDU.
      byte[] apdu = new byte[caPmt.Length + lengthByteCount + 3]; // + 3 for MMI tag
      WriteMmiTag(MmiTag.ConditionalAccessPmt, ref apdu, 0);
      Buffer.BlockCopy(length, 0, apdu, 3, lengthByteCount);
      Buffer.BlockCopy(caPmt, 0, apdu, 3 + lengthByteCount, caPmt.Length);
      return apdu;
    }

    #endregion
  }
}