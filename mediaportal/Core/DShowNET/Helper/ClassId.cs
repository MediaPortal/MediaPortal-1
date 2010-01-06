#region Copyright (C) 2005-2009 Team MediaPortal

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

namespace DShowNET.Helper
{
  public class ClassId
  {
    /// <summary>Prevent instantiation.</summary>
    private ClassId() {}

    public static readonly Guid SystemDeviceEnum = new Guid(0x62BE5D10, 0x60EB, 0x11d0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9,
                                                            0x11, 0xCE, 0x86);

    /// <summary>The File Writer filter can be used to write files to disc regardless of format. </summary>
    public static readonly Guid FileWriter = new Guid("8596E5F0-0DA5-11D0-BD21-00A0C911CE86");

    /// <summary>The Filter Graph Manager builds and controls filter graphs.</summary>
    public static readonly Guid FilterGraph = new Guid("E436EBB3-524F-11CE-9F53-0020AF0BA770");

    /// <summary>The WM ASF Writer filter accepts a variable number of input streams and creates an ASF file.</summary>
    public static readonly Guid WMAsfWriter = new Guid("7C23220E-55BB-11D3-8B16-00C04FB6BD3D");

    /// <summary>The RecComp object creates new content recordings by concatenating existing recordings.</summary>
    public static readonly Guid RecComp = new Guid("D682C4BA-A90A-42FE-B9E1-03109849C423");

    /// <summary>The Recording object creates permanent recordings from streams that the Stream Buffer Sink filter captures.</summary>
    public static readonly Guid RecordingAttributes = new Guid("CCAA63AC-1057-4778-AE92-1206AB9ACEE6");

    /// <summary>The WavDes filter writes an audio stream to a WAV file.</summary>
    public static readonly Guid WavDest = new Guid("3C78B8E2-6C4D-11d1-ADE2-0000F8754B99");

    /// <summary>The Decrypter/Detagger filter conditionally decrypts samples that are encrypted by the Encrypter/Tagger filter.</summary>
    public static readonly Guid DecryptTag = new Guid("C4C4C4F2-0049-4E2B-98FB-9537F6CE516D");

    public static readonly Guid MPTSWriter = new Guid("8943BEB7-E0BC-453b-9EA5-EB93899FA51C");
    public static readonly Guid MPStreamAnalyzer = new Guid("BAAC8911-1BA2-4ec2-96BA-6FFE42B62F72");

    public static readonly Guid PinCategoryVBI = new Guid(0xfb6c4284, 0x0353, 0x11d1, 0x90, 0x5f, 0x00, 0x00, 0xc0, 0xcc,
                                                          0x16, 0xba);

    public static readonly Guid DirectVobSubAutoload = new Guid("9852A670-F845-491b-9BE6-EBD841B8A613");
    public static readonly Guid DirectVobSubNormal = new Guid("93A22E7A-5091-45ef-BA61-6DA26156A5D0");

    public static readonly Guid InternalScriptRenderer = new Guid("48025243-2D39-11CE-875D-00608CB78066");

    public static readonly Guid HaaliGuid = new Guid("55DA30FC-F16B-49FC-BAA5-AE59FC65F82D");

    /// <summary>Creates an instance of a COM object by class ID.</summary>
    /// <param name="id">The class ID of the component to instantiate.</param>
    /// <returns>A new instance of the class.</returns>
    public static object CoCreateInstance(Guid id)
    {
      return Activator.CreateInstance(Type.GetTypeFromCLSID(id));
    }
  }
}