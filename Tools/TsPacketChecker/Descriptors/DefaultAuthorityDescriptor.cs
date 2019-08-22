using System;

namespace TsPacketChecker
{
  internal class DefaultAuthorityDescriptor : Descriptor
  {
    private string _defaultAuthority;
    private int _lastIndex = -1;

    /// <summary>
    /// Initialize a new instance of the DVBDefaultAuthorityDescriptor class.
    /// </summary>
    internal DefaultAuthorityDescriptor() { }

    public string DefaultAuthority { get { return (_defaultAuthority); } }

    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Default Authority Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }
    internal override void Process(byte[] byteData, int index)
    {
      _lastIndex = index;

      try
      {
        if (DescriptorLength != 0)
        {
          _defaultAuthority = Utils.GetString(byteData, _lastIndex, DescriptorLength);
          _lastIndex += DescriptorLength;
        }
        
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Default Authority Descriptor message is short"));
      }
    }
  }
}