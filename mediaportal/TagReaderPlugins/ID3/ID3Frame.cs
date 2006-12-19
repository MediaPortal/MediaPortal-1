#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.IO;
using MediaPortal.TagReader;

using MediaPortal.GUI.Library;

namespace ID3
{
  public class ID3FramePaddingException : Exception
  {
    public ID3FramePaddingException(string message)
      : base(message)
    {
    }
  }

  public class ID3Frame
  {
    #region Variables
    // Status Flag Values
    //                              0abc 0000 
    //                              ========
    // a: Tag alter preservation    0100 0000   (0x40)       
    // b: File alter preservation   0010 0000   (0x20)
    // c: Read only                 0001 0000   (0x10)

    public const byte TagAlterPreservationFlag = 0x40;
    public const byte FileAlterPreservationFlag = 0x20;
    public const byte ReadOnlyFlag = 0x10;

    // Encoding Flag Values
    //                              0h00 kmnp
    //                              =========
    // h: Grouping identity         0100 0000   (0x40)
    // k: Compression               0000 1000   (0x08)
    // m: Encryption                0000 0100   (0x04)
    // n: Unsynchronisation         0000 0010   (0x02)
    // p: Data length indicator     0000 0001   (0x01)

    public const byte GroupingIdentityFlag = 0x40;
    public const byte CompressionFlag = 0x08;
    public const byte EncryptionFlag = 0x04;
    public const byte UnsynchronisationFlag = 0x02;

    private byte[] _ID = new byte[4];
    private byte[] _Flags = new byte[2];    // byte[0] bits: abc00000 are status bits / byte[1] bits: ijk00000 are encoding bits
    private byte[] _Data;
    private string _FrameName = string.Empty;

    private bool _TagAlterPreservation = false;
    private bool _FileAlterPreservation = false;
    private bool _ReadOnly = false;

    private bool _HasCompression = false;
    private bool _HasEncryption = false;
    private bool _IsGroupingIdentity = false;
    private bool _UsesUnsynchronization = false;

    private int _FrameSize = -1;
    private int _Encoding = 0;
    private int id3HeaderLength = 10;
    private int id3IdLength = 4;
    private int id3FrameSizeLength = 4;
    #endregion

    #region Properties

    public int Size
    {
      get { return _Data.Length + Utils.ID3_HEADERSIZE; }
    }

    public string ID
    {
      get { return Encoding.UTF8.GetString(_ID); }
    }

    public string Data
    {
      //get { return Encoding.UTF8.GetString(_Data).Trim(); }
      get { return Encoding.Unicode.GetString(_Data).Trim(); }
    }

    public byte[] BinaryData
    {
      get { return this._Data; }
    }

    //public string FrameName
    //{
    //    get{return _FrameName;}
    //}

    #endregion

    #region Constructors/Destructors
    public ID3Frame(FileStream s, int Version)
    {
      // Check for the id3 Version to adjust the Header Length
      if (Version == 2)
      {
        id3HeaderLength = 6;
        id3IdLength = 3;
        id3FrameSizeLength = 3;
      }
      else
      {
        id3HeaderLength = 10;
        id3IdLength = 4;
        id3FrameSizeLength = 4;
      }

      byte[] header = new byte[id3HeaderLength];
      s.Read(header, 0, id3HeaderLength);

      if (header[0] == 0)
      {
        s.Seek(-id3HeaderLength, SeekOrigin.Current);
        throw new ID3FramePaddingException("ID3v2 Frame Padding Exception");
      }

      Array.Copy(header, 0, _ID, 0, id3IdLength);

      // Flags are only there for Version 3 and above
      if (Version > 2)
      {
        Array.Copy(header, 8, _Flags, 0, 2);

        _TagAlterPreservation = Utils.IsFlagSet(_Flags[0], TagAlterPreservationFlag);
        _FileAlterPreservation = Utils.IsFlagSet(_Flags[0], FileAlterPreservationFlag);
        _ReadOnly = Utils.IsFlagSet(_Flags[0], ReadOnlyFlag);

        _HasCompression = Utils.IsFlagSet(_Flags[1], CompressionFlag);
        _HasEncryption = Utils.IsFlagSet(_Flags[1], EncryptionFlag);
        _IsGroupingIdentity = Utils.IsFlagSet(_Flags[1], GroupingIdentityFlag);

        _UsesUnsynchronization = Utils.IsFlagSet(_Flags[1], UnsynchronisationFlag);
      }

      // ID3v2.4 uses synchsafe inegers for frame size
      if (Version >= 4)
        _FrameSize = Utils.GetSynchSafeInt(header, id3IdLength, id3FrameSizeLength);

      else
        _FrameSize = Utils.ReadSynchronizedData(header, id3IdLength, id3FrameSizeLength);

      byte[] dataBytes = new byte[_FrameSize];

      // Check if we're going to read past the end of the stream
      if (s.Position + _FrameSize > s.Length)
      {
        s.Seek(-id3HeaderLength, SeekOrigin.Current);
        throw new ID3FramePaddingException("ID3v2 Frame Padding Exception");
      }

      s.Read(dataBytes, 0, _FrameSize);

      if (IsBinaryFrame(this.ID))
        _Data = dataBytes;

    // Did we get a Text Information Frame
      else if (IsTextFrame(this.ID))
      {
        // Text Information frames start with a "T" and have the Text encoding before the actual text
        //
        // Meaning of the encoding byte:
        // $00   ISO-8859-1 [ISO-8859-1]. Terminated with $00.
        // $01   UTF-16 [UTF-16] encoded Unicode [UNICODE] with BOM. 
        //       All strings in the same frame SHALL have the same byteorder. Terminated with $00 00.
        // $02   UTF-16BE [UTF-16] encoded Unicode [UNICODE] without BOM. Terminated with $00 00.
        // $03   UTF-8 [UTF-8] encoded Unicode [UNICODE]. Terminated with $00.

        _Encoding = (int)dataBytes[0];
        byte[] tempBytes = new byte[_FrameSize - 1];
        Array.Copy(dataBytes, 1, tempBytes, 0, _FrameSize - 1);
        _Data = tempBytes;

        switch (_Encoding)
        {
          case 0: // ISO-8859-1. Use Default Encoding
            _Data = Utils.CleanData(dataBytes);
            _Data = Encoding.Convert(Encoding.Default, Encoding.Unicode, _Data);
            break;

          case 1: // Data is already in  Unicodeformat. Don't do anything
          case 2:
            break;

          case 3:
            _Data = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, _Data);

            break;
        }
      }

      // If it's not a binary data frame or a text frame what is it?
      // It could be a lyrics frame (USLT)
      else
        _Data = dataBytes;
    }
    #endregion

    #region Private Methods
    private bool IsBinaryFrame(string frameID)
    {
      switch (frameID)
      {
        case "APIC":
          return true;

        default:
          return false;
      }
    }

    private bool IsTextFrame(string frameID)
    {
      if (frameID.StartsWith("T"))
        return true;

      else
        return false;
    }
    #endregion
  }
}
