#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

// Stephen Toub
// stoub@microsoft.com

using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using DShowNET.Helper;

namespace Toub.MediaCenter.Dvrms.Metadata
{
  /// <summary>Metadata editor for ASF files, including WMA and WMV files.</summary>
  public sealed class AsfMetadataEditor : MetadataEditor
  {
    /// <summary>The number of the stream from which to get metadata.</summary>
    private const ushort DEFAULT_STREAM_NUMBER = 0;

    /// <summary>IWMHeaderInfo3 interface for the editor.</summary>
    private IWMHeaderInfo3 _headerInfo;

    /// <summary>Underlying metadata editor used to edit the metadata for the file.</summary>
    private IWMMetadataEditor _editor;

    /// <summary>Path to the file whose metadata is being edited.</summary>
    private string _path;

    /// <summary>Initialize the editor.</summary>
    /// <param name="filepath">The path to the file whose metadata needs to be edited.</param>
    public AsfMetadataEditor(string filepath) : base()
    {
      if (filepath == null)
      {
        throw new ArgumentNullException("filepath");
      }
      _path = filepath;

      try
      {
        _editor = WMCreateEditor();
        _editor.Open(filepath);
        _headerInfo = (IWMHeaderInfo3) _editor;
      }
      catch
      {
        Dispose(true);
        throw;
      }
    }

    /// <summary>Releases all of the resources for the editor.</summary>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (_editor != null)
        {
          try
          {
            _editor.Flush();
          }
          catch (COMException)
          {
          }
          while (DirectShowUtil.ReleaseComObject(_editor) > 0)
          {
            ;
          }
          _editor = null;
        }
        if (_headerInfo != null)
        {
          while (DirectShowUtil.ReleaseComObject(_headerInfo) > 0)
          {
            ;
          }
          _headerInfo = null;
        }
      }
    }

    /// <summary>Creates a metadata editor.</summary>
    [DllImport("WMVCore.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, PreserveSig = false)
    ]
    private static extern IWMMetadataEditor WMCreateEditor();

    /// <summary>Sets the collection of string attributes onto the specified file and stream.</summary>
    /// <param name="propsToSet">The properties to set on the file.</param>
    public override void SetAttributes(IDictionary propsToSet)
    {
      if (_editor == null)
      {
        throw new ObjectDisposedException(GetType().Name);
      }
      if (propsToSet == null)
      {
        throw new ArgumentNullException("propsToSet");
      }

      byte[] attributeValueBytes;

      // Add each metadata item
      foreach (DictionaryEntry entry in propsToSet)
      {
        // Get the current item and convert it as appropriate to a byte array
        MetadataItem item = (MetadataItem) entry.Value;
        if (TranslateAttributeToByteArray(item, out attributeValueBytes))
        {
          try
          {
            // Set the attribute onto the file
            _headerInfo.SetAttribute(DEFAULT_STREAM_NUMBER, item.Name, item.Type,
                                     attributeValueBytes, (ushort) attributeValueBytes.Length);
          }
          catch (ArgumentException)
          {
          }
          catch (COMException)
          {
          }
        }
      }
    }

    /// <summary>Gets all of the attributes on a file.</summary>
    /// <returns>A collection of the attributes from the file.</returns>
    public override IDictionary GetAttributes()
    {
      if (_editor == null)
      {
        throw new ObjectDisposedException(GetType().Name);
      }

      ushort streamNum = DEFAULT_STREAM_NUMBER;
      Hashtable propsRetrieved = new Hashtable();

      // Get the number of attributes
      ushort attributeCount = _headerInfo.GetAttributeCount(streamNum);

      // Get each attribute by index
      for (ushort i = 0; i < attributeCount; i++)
      {
        MetadataItemType attributeType;
        StringBuilder attributeName = null;
        byte[] attributeValue = null;
        ushort attributeNameLength = 0;
        ushort attributeValueLength = 0;

        // Get the lengths of the name and the value, then use them to create buffers to receive them
        _headerInfo.GetAttributeByIndex(i, ref streamNum, attributeName, ref attributeNameLength,
                                        out attributeType, attributeValue, ref attributeValueLength);
        attributeName = new StringBuilder(attributeNameLength);
        attributeValue = new byte[attributeValueLength];

        // Get the name and value
        _headerInfo.GetAttributeByIndex(i, ref streamNum, attributeName, ref attributeNameLength,
                                        out attributeType, attributeValue, ref attributeValueLength);

        // If we got a name, parse the value and add the metadata item
        if (attributeName != null && attributeName.Length > 0)
        {
          object val = ParseAttributeValue(attributeType, attributeValue);
          string key = attributeName.ToString().TrimEnd('\0');
          propsRetrieved[key] = new MetadataItem(key, val, attributeType);
        }
      }

      // Return the parsed items
      return propsRetrieved;
    }

    /// <summary>
    /// The IWMMetadataEditor interface is used to edit metadata information in ASF file headers. 
    /// It is obtained by calling the WMCreateEditor function.
    /// </summary>
    [ComImport]
    [Guid("96406BD9-2B2B-11d3-B36B-00C04F6108FF")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IWMMetadataEditor
    {
      /// <summary>The Open method opens an ASF file.</summary>
      /// <param name="pwszFilename">Pointer to a wide-character null-terminated string containing the file name.</param>
      void Open([In, MarshalAs(UnmanagedType.LPWStr)] string pwszFilename);

      /// <summary>The Close method closes the open file without saving any changes.</summary>
      void Close();

      /// <summary>The Flush method closes the open file, saving any changes.</summary>
      void Flush();
    }

    /// <summary>Sets and retrieves information in the header section of an ASF file.</summary>
    [ComImport]
    [Guid("15CC68E3-27CC-4ecd-B222-3F5D02D80BD5")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IWMHeaderInfo3
    {
      /// <summary>The GetAttributeCount method returns the number of attributes defined in the header section of the ASF file.</summary>
      /// <param name="wStreamNum">WORD containing the stream number. Pass zero for file-level attributes.</param>
      /// <returns>The number of attributes.</returns>
      ushort GetAttributeCount([In] ushort wStreamNum);

      /// <summary>
      /// The GetAttributeByIndex method returns a descriptive attribute that is stored in the header section of the ASF file.
      /// </summary>
      /// <param name="wIndex">WORD containing the index.</param>
      /// <param name="pwStreamNum">
      /// Pointer to a WORD containing the stream number. Although this parameter is a pointer, the method will not change 
      /// the value. For file-level attributes, use zero for the stream number.
      /// </param>
      /// <param name="pwszName">
      /// Pointer to a wide-character null-terminated string containing the name. Pass NULL to this parameter 
      /// to retrieve the required length for the name. Attribute names are limited to 1024 wide characters.
      /// </param>
      /// <param name="pcchNameLen">
      /// On input, a pointer to a variable containing the length of the pwszName array in wide characters (2 bytes). 
      /// On output, if the method succeeds, the variable contains the actual length of the name, including the 
      /// terminating null character.
      /// </param>
      /// <param name="pType">
      /// Pointer to a variable containing one value from the WMT_ATTR_DATATYPE enumeration type.
      /// </param>
      /// <param name="pValue">
      /// Pointer to a byte array containing the value. Pass NULL to this parameter to retrieve the required length for the value.
      /// </param>
      /// <param name="pcbLength">
      /// On input, a pointer to a variable containing the length of the pValue array, in bytes. On output, 
      /// if the method succeeds, the variable contains the actual number of bytes written to pValue by the method.
      /// </param>
      void GetAttributeByIndex(
        [In] ushort wIndex, [Out, In] ref ushort pwStreamNum,
        [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszName,
        [Out, In] ref ushort pcchNameLen, [Out] out MetadataItemType pType,
        [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
        [Out, In] ref ushort pcbLength);

      /// <summary>
      /// The GetAttributeByName method returns a descriptive attribute that is stored in the header section of the ASF file.
      /// </summary>
      /// <param name="pwStreamNum">Pointer to a WORD containing the stream number, or zero to indicate any stream.</param>
      /// <param name="pszName">Pointer to a null-terminated string containing the name of the attribute. Attribute names are limited to 1024 wide characters.</param>
      /// <param name="pType">
      /// Pointer to a variable containing one value from the WMT_ATTR_DATATYPE enumeration type.
      /// </param>
      /// <param name="pValue">
      /// Pointer to a byte array containing the value. Pass NULL to this parameter to retrieve the required length for the value.
      /// </param>
      /// <param name="pcbLength">
      /// On input, a pointer to a variable containing the length of the pValue array, in bytes. On output, 
      /// if the method succeeds, the variable contains the actual number of bytes written to pValue by the method.
      /// </param>
      void GetAttributeByName(
        [Out, In] ref ushort pwStreamNum,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
        [Out] out MetadataItemType pType,
        [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
        [Out, In] ref ushort pcbLength);

      /// <summary>The SetAttribute method sets a descriptive attribute that is stored in the header section of the ASF file.</summary>
      /// <param name="wStreamNum">WORD containing the stream number. To set a file-level attribute, pass zero.</param>
      /// <param name="pszName">
      /// Pointer to a wide-character null-terminated string containing the name of the attribute. 
      /// Attribute names are limited to 1024 wide characters.
      /// </param>
      /// <param name="Type">A value from the WMT_ATTR_DATATYPE enumeration type.</param>
      /// <param name="pValue">Pointer to a byte array containing the value of the attribute.</param>
      /// <param name="cbLength">The size of pValue, in bytes.</param>
      void SetAttribute(
        [In] ushort wStreamNum,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
        [In] MetadataItemType Type,
        [In, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
        [In] ushort cbLength);

      /// <summary>The GetMarkerCount method returns the number of markers currently in the header section of the ASF file.</summary>
      /// <returns>The number of markers currently in the header section of the ASF file.</returns>
      short GetMarkerCount();

      /// <summary>The GetMarker method returns the name and time of a marker.</summary>
      /// <param name="wIndex">WORD containing the index.</param>
      /// <param name="pwszMarkerName">Pointer to a wide-character null-terminated string containing the marker name.</param>
      /// <param name="pcchMarkerNameLen">
      /// On input, a pointer to a variable containing the length of the pwszMarkerName array in wide characters (2 bytes). 
      /// On output, if the method succeeds, the variable contains the actual length of the name, including the terminating 
      /// null character. To retrieve the length of the name, you must set this to zero and set pwszMarkerName and 
      /// pcnsMarkerTime to NULL.
      /// </param>
      /// <param name="pcnsMarkerTime">Pointer to a variable specifying the marker time in 100-nanosecond increments.</param>
      void GetMarker([In] ushort wIndex,
                     [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszMarkerName,
                     [Out, In] ref ushort pcchMarkerNameLen,
                     [Out] out ulong pcnsMarkerTime);

      /// <summary>
      /// The AddMarker method adds a marker, consisting of a name and a specific time, to the header section of the ASF file.
      /// </summary>
      /// <param name="pwszMarkerName">
      /// Pointer to a wide-character null-terminated string containing the marker name. Marker names are limited to 5120 wide characters.
      /// </param>
      /// <param name="cnsMarkerTime">
      /// The marker time in 100-nanosecond increments.
      /// </param>
      void AddMarker([In, MarshalAs(UnmanagedType.LPWStr)] string pwszMarkerName, [In] ulong cnsMarkerTime);

      /// <summary>The RemoveMarker method removes a marker from the header section of the ASF file.</summary>
      /// <param name="wIndex">WORD containing the index of the marker.</param>
      void RemoveMarker([In] ushort wIndex);

      /// <summary>The GetScriptCount method returns the number of scripts currently in the header section of the ASF file.</summary>
      /// <returns>The number of scripts currently in the header section of the ASF file.</returns>
      short GetScriptCount();

      /// <summary>The GetScript method returns the type and command strings, and presentation time of a script.</summary>
      /// <param name="wIndex">WORD containing the index.</param>
      /// <param name="pwszType">Pointer to a wide-character null-terminated string containing the type.</param>
      /// <param name="pcchTypeLen">
      /// On input, a pointer to a variable containing the length of the pwszType array in wide characters (2 bytes). 
      /// On output, if the method succeeds, the variable contains the actual length of the string loaded into 
      /// pwszType, including the terminating null character. To retrieve the length of the type, you must set 
      /// this to zero and set pwszType to NULL.
      /// </param>
      /// <param name="pwszCommand">Pointer to a wide-character null-terminated string containing the command.</param>
      /// <param name="pcchCommandLen">
      /// On input, a pointer to a variable containing the length of the pwszCommand array in wide characters (2 bytes). 
      /// On output, if the method succeeds, the variable contains the actual length of the command string, including 
      /// the terminating null character. To retrieve the length of the command, you must set this to zero and set 
      /// pwszCommand to NULL.
      /// </param>
      /// <param name="pcnsScriptTime">
      /// Pointer to a QWORD specifying the presentation time of this script command in 100-nanosecond increments.
      /// </param>
      void GetScript(
        [In] ushort wIndex,
        [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszType,
        [Out, In] ref ushort pcchTypeLen,
        [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand,
        [Out, In] ref ushort pcchCommandLen,
        [Out] out ulong pcnsScriptTime);

      /// <summary>
      /// The AddScript method adds a script, consisting of type and command strings, and a specific time, 
      /// to the header section of the ASF file.
      /// </summary>
      /// <param name="pwszType">
      /// Pointer to a wide-character null-terminated string containing the type. Script types are limited to 
      /// 1024 wide characters.
      /// </param>
      /// <param name="pwszCommand">
      /// Pointer to a wide-character null-terminated string containing the command. Script commands 
      /// are limited to 10240 wide characters.
      /// </param>
      /// <param name="cnsScriptTime">The script time in 100-nanosecond increments.</param>
      void AddScript(
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszType,
        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszCommand,
        [In] ulong cnsScriptTime);

      /// <summary>The RemoveScript method enables the object to remove a script from the header section of the ASF file.</summary>
      /// <param name="wIndex">WORD containing the index of the script.</param>
      void RemoveScript([In] ushort wIndex);

      /// <summary>
      /// The GetCodecInfoCount method retrieves the number of codecs for which information is available. The 
      /// codecs counted are those that were used to encode the streams of the file loaded in the metadata editor, 
      /// reader, or synchronous reader object to which the IWMHeaderInfo2 interface belongs.
      /// </summary>
      /// <returns>The number of codecs for which information is available.</returns>
      int GetCodecInfoCount();

      /// <summary>
      /// The GetCodecInfo method retrieves information about a codec used to create the content of a file.
      /// </summary>
      /// <param name="wIndex">WORD containing the codec index.</param>
      /// <param name="pcchName">On input, pointer to the length of pwszName in wide characters. On output, pointer to a count of the characters used in pwszName, including the terminating null character.</param>
      /// <param name="pwszName">Pointer to a wide-character null-terminated string containing the name of the codec.</param>
      /// <param name="pcchDescription">
      /// On input, pointer to the length of pwszDescription in wide characters. On output, pointer to a count of the 
      /// characters used in pwszDescription, including the terminating null character.
      /// </param>
      /// <param name="pwszDescription">
      /// Pointer to a wide-character null-terminated string containing the description of the codec.
      /// </param>
      /// <param name="pCodecType">Pointer to a wide-character null-terminated string containing the description of the codec.</param>
      /// <param name="pcbCodecInfo">On input, pointer to the length of pbCodecInfo, in bytes.</param>
      /// <param name="pbCodecInfo">Pointer to a byte array.</param>
      void GetCodecInfo(
        [In] uint wIndex,
        [Out, In] ref ushort pcchName,
        [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszName,
        [Out, In] ref ushort pcchDescription,
        [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszDescription,
        [Out] out uint pCodecType,
        [Out, In] ref ushort pcbCodecInfo,
        [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbCodecInfo);

      /// <summary>
      /// The GetAttributeCountEx method retrieves the total number of attributes associated with a specified stream number. 
      /// You can also use this method to get the number of attributes not associated with a specific stream (file-level 
      /// attributes), or to get the total number of attributes in the file, regardless of stream number.
      /// </summary>
      /// <param name="wStreamNum">
      /// WORD containing the stream number for which to retrieve the attribute count. Pass zero to retrieve the 
      /// count of attributes that apply to the file rather than a specific stream. Pass 0xFFFF to retrieve the total 
      /// count of all attributes in the file, both stream-specific and file-level.
      /// </param>
      /// <returns>The number of attributes that exist for the specified stream</returns>
      short GetAttributeCountEx([In] ushort wStreamNum);

      /// <summary>
      /// The GetAttributeIndices method retrieves a list of valid attribute indexes within specified parameters. 
      /// You can retrieve indexes for all attributes with the same name or for all attributes in a specified language. 
      /// The indexes found are for a single specific stream. Alternatively, you can retrieve the specified indexes for 
      /// the entire file.
      /// </summary>
      /// <param name="wStreamNum">
      /// WORD containing the stream number for which to retrieve attribute indexes. Passing zero retrieves indexes 
      /// for file-level attributes. Passing 0xFFFF retrieves indexes for all appropriate attributes, regardless of 
      /// their association to streams.
      /// </param>
      /// <param name="pwszName">
      /// Pointer to a wide-character null-terminated string containing the attribute name for which you want to 
      /// retrieve indexes. Pass NULL to retrieve indexes for attributes based on language. Attribute names are limited 
      /// to 1024 wide characters.
      /// </param>
      /// <param name="pwLangIndex">
      /// Pointer to a WORD containing the language index of the language for which to retrieve attribute indexes. 
      /// Pass NULL to retrieve indexes for attributes by name.
      /// </param>
      /// <param name="pwIndices">
      /// Pointer to a WORD array containing the indexes that meet the criteria described by the input parameters. 
      /// Pass NULL to retrieve the size of the array, which will be returned in pwCount.
      /// </param>
      /// <param name="pwCount">
      /// On output, pointer to a WORD containing the number of elements in the pwIndices array.
      /// </param>
      void GetAttributeIndices(
        [In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.LPWStr)] string pwszName,
        [In] ref ushort pwLangIndex, [Out, MarshalAs(UnmanagedType.LPArray)] ushort[] pwIndices,
        [Out, In] ref ushort pwCount);

      /// <summary>
      /// The GetAttributeByIndexEx method retrieves the value of an attribute specified by the attribute index. 
      /// You can use this method in conjunction with the GetAttributeCountEx method to retrieve all of the attributes 
      /// associated with a particular stream number.
      /// </summary>
      /// <param name="wStreamNum">
      /// WORD containing the stream number to which the attribute applies. Set to zero to retrieve a file-level attribute.
      /// </param>
      /// <param name="wIndex">WORD containing the index of the attribute to be retrieved.</param>
      /// <param name="pwszName">
      /// Pointer to a wide-character null-terminated string containing the attribute name. Pass NULL to retrieve the size of the string, which will be returned in pwNameLen.
      /// </param>
      /// <param name="pwNameLen">
      /// Pointer to a WORD containing the size of pwszName, in wide characters. This size includes the terminating null character. Attribute names are limited to 1024 wide characters.
      /// </param>
      /// <param name="pType">
      /// Type of data used for the attribute. For more information about the types of data supported, see WMT_ATTR_DATATYPE.
      /// </param>
      /// <param name="pwLangIndex">
      /// Pointer to a WORD containing the language index of the language associated with the attribute. 
      /// This is the index of the language in the language list for the file.
      /// </param>
      /// <param name="pValue">
      /// Pointer to an array of bytes containing the attribute value. Pass NULL to 
      /// retrieve the size of the attribute value, which will be returned in pdwDataLength.
      /// </param>
      /// <param name="pdwDataLength">Pointer to a DWORD containing the length, in bytes, of the attribute value pointed to by pValue.</param>
      void GetAttributeByIndexEx(
        [In] ushort wStreamNum, [In] ushort wIndex,
        [Out, MarshalAs(UnmanagedType.LPWStr)] string pwszName,
        [Out, In] ref ushort pwNameLen, [Out] out MetadataItemType pType,
        [Out] out ushort pwLangIndex, [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
        [Out, In] ref uint pdwDataLength);

      /// <summary>The ModifyAttribute method modifies the settings of an existing attribute.</summary>
      /// <param name="wStreamNum">WORD containing the stream number to which the attribute applies. Pass zero for file-level attributes.</param>
      /// <param name="wIndex">WORD containing the index of the attribute to change.</param>
      /// <param name="Type">Type of data used for the new attribute value. For more information about the types of data supported, see WMT_ATTR_DATATYPE.</param>
      /// <param name="wLangIndex">WORD containing the language index of the language to be associated with the new attribute. This is the index of the language in the language list for the file.</param>
      /// <param name="pValue">Pointer to an array of bytes containing the attribute value.</param>
      /// <param name="dwLength">DWORD containing the length of the attribute value, in bytes.</param>
      void ModifyAttribute([In] ushort wStreamNum, [In] ushort wIndex, [In] MetadataItemType Type,
                           [In] ushort wLangIndex, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pValue,
                           [In] uint dwLength);

      /// <summary>
      /// The AddAttribute method adds a metadata attribute. To change the value of an existing attribute, 
      /// use the IWMHeaderInfo3::ModifyAttribute method.
      /// </summary>
      /// <param name="wStreamNum">
      /// WORD containing the stream number of the stream to which the attribute applies. Setting this 
      /// value to zero indicates an attribute that applies to the entire file.
      /// </param>
      /// <param name="pszName">
      /// Pointer to a wide-character null-terminated string containing the name of the attribute. 
      /// Attribute names are limited to 1024 wide characters.
      /// </param>
      /// <param name="pwIndex">
      /// Pointer to a WORD. On successful completion of the method, this value is set to the index assigned 
      /// to the new attribute.
      /// </param>
      /// <param name="Type">
      /// Type of data used for the new attribute. For more information about the types of data supported, see WMT_ATTR_DATATYPE.
      /// </param>
      /// <param name="wLangIndex">
      /// WORD containing the language index of the language to be associated with the new attribute. This is the index 
      /// of the language in the language list for the file. Setting this value to zero indicates that the default 
      /// language will be used. A default language is created and set according to the regional settings on the 
      /// computer running your application.
      /// </param>
      /// <param name="pValue">
      /// Pointer to an array of bytes containing the attribute value.
      /// </param>
      /// <param name="dwLength">DWORD containing the length of the attribute value, in bytes.</param>
      void AddAttribute([In] ushort wStreamNum, [In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                        [Out] out ushort pwIndex, [In] MetadataItemType Type, [In] ushort wLangIndex,
                        [In, MarshalAs(UnmanagedType.LPArray)] byte[] pValue, [In] uint dwLength);

      /// <summary>The DeleteAttribute method removes an attribute from the file header.</summary>
      /// <param name="wStreamNum">
      /// WORD containing the stream number for which the attribute applies. Setting this value to 
      /// zero indicates a file-level attribute.
      /// </param>
      /// <param name="wIndex">WORD containing the index of the attribute to be deleted.</param>
      void DeleteAttribute([In] ushort wStreamNum, [In] ushort wIndex);

      /// <summary>
      /// The AddCodecInfo method adds codec information to a file. When you copy a compressed stream from 
      /// one file to another, use this method to include the information about the encoding codec in the file header.
      /// </summary>
      /// <param name="pszName">
      /// Pointer to a wide-character null-terminated string containing the name.
      /// </param>
      /// <param name="pwszDescription">
      /// Pointer to a wide-character null-terminated string containing the description.
      /// </param>
      /// <param name="codecType">
      /// A value from the WmtCodecInfoType enumeration specifying the codec type.
      /// </param>
      /// <param name="cbCodecInfo">The size of the codec information, in bytes.</param>
      /// <param name="pbCodecInfo">Pointer to a byte containing the codec information.</param>
      void AddCodecInfo([In, MarshalAs(UnmanagedType.LPWStr)] string pszName,
                        [In, MarshalAs(UnmanagedType.LPWStr)] string pwszDescription,
                        [In] uint codecType,
                        [In] ushort cbCodecInfo, [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbCodecInfo);
    }
  }
}