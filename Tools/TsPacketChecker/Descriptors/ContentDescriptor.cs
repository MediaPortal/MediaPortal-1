using System;
using System.Collections.ObjectModel;

namespace TsPacketChecker
{
  internal class ContentDescriptor : Descriptor
  {
    private Collection<ContentType> _contentTypes;
    private int _lastIndex = -1;

    /// <summary>
    /// Initialize a new instance of the DVBContentDescriptor class.
    /// </summary>
    internal ContentDescriptor() { }
    /// <summary>
    /// Get the list of content types.
    /// </summary>
    public Collection<ContentType> ContentTypes { get { return (_contentTypes); } }
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("ContentDescriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }
    
    internal override void Process(byte[] byteData, int index)
    {
      _lastIndex = index;

      _contentTypes = new Collection<ContentType>();
      int dataLength = DescriptorLength;

      while (dataLength > 0)
      {

        try
        {
          int contentType = (int)(byteData[_lastIndex] >> 4);
          int contentSubType = (int)(byteData[_lastIndex] & 0x0f);
          _lastIndex++;

          int userType = (int)byteData[_lastIndex];
          _lastIndex++;

          _contentTypes.Add(new ContentType(contentType, contentSubType, userType));
          dataLength -= 2;
        }
        catch (IndexOutOfRangeException)
        {
          throw (new ArgumentOutOfRangeException("The DVB Content Descriptor message is short"));
        }
      }      
    }

    public class ContentType
    {
      private int contentType;
      private int contentSubType;
      private int userType;

      public ContentType(int contentType, int contentSubType, int userType)
      {
        this.contentType = contentType;
        this.contentSubType = contentSubType;
        this.userType = userType;
      }
    }
  }
}