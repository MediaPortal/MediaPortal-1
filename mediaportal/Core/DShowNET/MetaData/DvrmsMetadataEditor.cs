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
using MediaPortal.GUI.Library;
using MediaPortal.Services;

namespace Toub.MediaCenter.Dvrms.Metadata
{
  /// <summary>
  /// The AmMediaType structure is the primary structure used to describe media formats 
  /// for the objects of the Windows Media Format SDK.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  public class AmMediaType
  {
    /// <summary>Major type of the media sample. For example, WMMEDIATYPE_Video specifies a video stream.</summary>
    public Guid majortype;

    /// <summary>Subtype of the media sample. The subtype defines a specific format (usually an encoding scheme) within a major media type.</summary>
    public Guid subtype;

    /// <summary>
    /// Set to true if the samples are of a fixed size. Compressed audio samples are of a fixed size while video samples are not.
    /// </summary>
    [MarshalAs(UnmanagedType.Bool)] public bool bFixedSizeSamples;

    /// <summary>
    /// Set to true if the samples are temporally compressed. Temporal compression is compression where some 
    /// samples describe the changes in content from the previous sample, instead of describing the sample in its entirety.
    /// </summary>
    [MarshalAs(UnmanagedType.Bool)] public bool bTemporalCompression;

    /// <summary>Long integer containing the size of the sample, in bytes.</summary>
    public uint lSampleSize;

    /// <summary>GUID of the format type.</summary>
    public Guid formattype;

    /// <summary>Not used. Should be NULL.</summary>
    public IntPtr pUnk;

    /// <summary>Size, in bytes, of the structure pointed to by pbFormat.</summary>
    public uint cbFormat;

    /// <summary>Pointer to the format structure of the media type.</summary>
    public IntPtr pbFormat;
  } ;

  /// <summary>The IFileSourceFilter interface is implemented on filters that read media streams from a file.</summary>
  [ComImport]
  [Guid("56a868a6-0ad4-11ce-b03a-0020af0ba770")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IFileSourceFilter
  {
    /// <summary>Load a file and assign it the given media type.</summary>
    /// <param name="pszFileName">Pointer to absolute path of file to open</param>
    /// <param name="pmt">Media type of file - can be NULL</param>
    [PreserveSig]
    int Load(
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
      [In, MarshalAs(UnmanagedType.LPStruct)] AmMediaType pmt);

    /// <summary>The GetCurFile method retrieves the name and media type of the current file.</summary>
    /// <param name="ppszFileName">Address of a pointer that receives the name of the file, as an OLESTR type.</param>
    /// <param name="pmt">Pointer to an AM_MEDIA_TYPE structure that receives the media type.</param>
    [PreserveSig]
    int GetCurFile(
      [Out, MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName,
      [Out, MarshalAs(UnmanagedType.LPStruct)] AmMediaType pmt);
  }

  [ComImport]
  [Guid("16CA4E03-FE69-4705-BD41-5B7DFC0C95F3")]
  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  public interface IStreamBufferRecordingAttribute
  {
    /// <summary>Sets an attribute on a recording object. If an attribute of the same name already exists, overwrites the old.</summary>
    /// <param name="ulReserved">Reserved. Set this parameter to zero.</param>
    /// <param name="pszAttributeName">Wide-character string that contains the name of the attribute.</param>
    /// <param name="StreamBufferAttributeType">Defines the data type of the attribute data.</param>
    /// <param name="pbAttribute">Pointer to a buffer that contains the attribute data.</param>
    /// <param name="cbAttributeLength">The size of the buffer specified in pbAttribute.</param>
    void SetAttribute(
      [In] uint ulReserved,
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszAttributeName,
      [In] MetadataItemType StreamBufferAttributeType,
      [In, MarshalAs(UnmanagedType.LPArray)] byte[] pbAttribute,
      [In] ushort cbAttributeLength);

    /// <summary>Returns the number of attributes that are currently defined for this stream buffer file.</summary>
    /// <param name="ulReserved">Reserved. Set this parameter to zero.</param>
    /// <returns>Number of attributes that are currently defined for this stream buffer file.</returns>
    ushort GetAttributeCount([In] uint ulReserved);

    /// <summary>Given a name, returns the attribute data.</summary>
    /// <param name="pszAttributeName">Wide-character string that contains the name of the attribute.</param>
    /// <param name="pulReserved">Reserved. Set this parameter to zero.</param>
    /// <param name="pStreamBufferAttributeType">
    /// Pointer to a variable that receives a member of the STREAMBUFFER_ATTR_DATATYPE enumeration. 
    /// This value indicates the data type that you should use to interpret the attribute, which is 
    /// returned in the pbAttribute parameter.
    /// </param>
    /// <param name="pbAttribute">
    /// Pointer to a buffer that receives the attribute, as an array of bytes. Specify the size of the buffer in the 
    /// pcbLength parameter. To find out the required size for the array, set pbAttribute to NULL and check the 
    /// value that is returned in pcbLength.
    /// </param>
    /// <param name="pcbLength">
    /// On input, specifies the size of the buffer given in pbAttribute, in bytes. On output, 
    /// contains the number of bytes that were copied to the buffer.
    /// </param>
    void GetAttributeByName(
      [In, MarshalAs(UnmanagedType.LPWStr)] string pszAttributeName,
      [In] ref uint pulReserved,
      [Out] out MetadataItemType pStreamBufferAttributeType,
      [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbAttribute,
      [In, Out] ref ushort pcbLength);

    /// <summary>The GetAttributeByIndex method retrieves an attribute, specified by index number.</summary>
    /// <param name="wIndex">Zero-based index of the attribute to retrieve.</param>
    /// <param name="pulReserved">Reserved. Set this parameter to zero.</param>
    /// <param name="pszAttributeName">Pointer to a buffer that receives the name of the attribute, as a null-terminated wide-character string.</param>
    /// <param name="pcchNameLength">On input, specifies the size of the buffer given in pszAttributeName, in wide characters.</param>
    /// <param name="pStreamBufferAttributeType">Pointer to a variable that receives a member of the STREAMBUFFER_ATTR_DATATYPE enumeration.</param>
    /// <param name="pbAttribute">Pointer to a buffer that receives the attribute, as an array of bytes.</param>
    /// <param name="pcbLength">On input, specifies the size of the buffer given in pbAttribute, in bytes.</param>
    void GetAttributeByIndex(
      [In] ushort wIndex,
      [In, Out] ref uint pulReserved,
      [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszAttributeName,
      [In, Out] ref ushort pcchNameLength,
      [Out] out MetadataItemType pStreamBufferAttributeType,
      [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbAttribute,
      [In, Out] ref ushort pcbLength);

    /// <summary>The EnumAttributes method enumerates the existing attributes of the stream buffer file.</summary>
    /// <returns>Address of a variable that receives an IEnumStreamBufferRecordingAttrib interface pointer.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    object EnumAttributes();
  }

  /// <summary>Metadata editor for DVR-MS files.</summary>
  public class DvrmsMetadataEditor : MetadataEditor
  {
    private IFileSourceFilter sourceFilter = null;
    private IStreamBufferRecordingAttribute _editor = null;

    /// <summary>Initializes the editor.</summary>
    /// <param name="filepath">The path to the file.</param>
    public DvrmsMetadataEditor(string filepath) : base()
    {
      sourceFilter = ClassId.CoCreateInstance(ClassId.RecordingAttributes) as IFileSourceFilter;
      if (sourceFilter == null)
      {
        Log.WriteFile(LogType.Recorder, true, "Unable to create IFileSourceFilter");
        return;
      }
      int hr = sourceFilter.Load(filepath, null);
      if (hr != 0)
      {
        //Log.WriteFile(LogType.Recorder,true,"Unable to open:{0} hr:0x{1:X}",filepath,hr);
        return;
      }
      _editor = sourceFilter as IStreamBufferRecordingAttribute;
    }

    /// <summary>Gets all of the attributes on a file.</summary>
    /// <returns>A collection of the attributes from the file.</returns>
    public override IDictionary GetAttributes()
    {
      if (_editor == null)
      {
        return null;
      }

      Hashtable propsRetrieved = new Hashtable();
      object obj = _editor.EnumAttributes();

      // Get the number of attributes
      ushort attributeCount = _editor.GetAttributeCount(0);

      // Get each attribute by index
      for (ushort i = 0; i < attributeCount; i++)
      {
        MetadataItemType attributeType;
        StringBuilder attributeName = null;
        byte[] attributeValue = null;
        ushort attributeNameLength = 0;
        ushort attributeValueLength = 0;

        // Get the lengths of the name and the value, then use them to create buffers to receive them
        uint reserved = 0;
        _editor.GetAttributeByIndex(i, ref reserved, attributeName, ref attributeNameLength,
                                    out attributeType, attributeValue, ref attributeValueLength);
        attributeName = new StringBuilder(attributeNameLength);
        attributeValue = new byte[attributeValueLength];

        // Get the name and value
        _editor.GetAttributeByIndex(i, ref reserved, attributeName, ref attributeNameLength,
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

    /// <summary>Sets the collection of string attributes onto the specified file and stream.</summary>
    /// <param name="propsToSet">The properties to set on the file.</param>
    public override void SetAttributes(IDictionary propsToSet)
    {
      if (_editor == null)
      {
        return;
      }
      if (propsToSet == null)
      {
        return;
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
            _editor.SetAttribute(0, item.Name, item.Type,
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

    /// <summary>Release all resources.</summary>
    /// <param name="disposing">Whether this is being called from IDisposable.Dispose.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing)
      {
        if (sourceFilter != null)
        {
          DirectShowUtil.ReleaseComObject(sourceFilter);
        }
        _editor = null;
        sourceFilter = null;
      }
    }
  }
}