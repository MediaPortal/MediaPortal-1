using System;

namespace WindowsApplication13
{
  internal class NetworkNameDescriptor : Descriptor
  {
    #region Private Fields
    private string networkName;
    private int _lastIndex = -1;
    #endregion
    #region Constructor
    /// <summary>
    /// Initialize a new instance of the NetworkNameDescriptor class.
    /// </summary>
    internal NetworkNameDescriptor() { }
    #endregion
    #region Properties
    /// <summary>
    /// Get the network name.
    /// </summary>
    public string NetworkName { get { return (networkName); } }
    #endregion
    #region Overrides
    /// <summary>
    /// Get the index of the next byte in the section following this descriptor.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// The descriptor has not been processed.
    /// </exception> 
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("NetworkName Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }
    /// <summary>
    /// Parse the descriptor.
    /// </summary>
    /// <param name="buffer">The MPEG2 section containing the descriptor.</param>
    /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;

      try
      {
        if (DescriptorLength != 0)
        {
          networkName = Utils.GetString(buffer, _lastIndex, DescriptorLength);
          _lastIndex += DescriptorLength;
        }        
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Network Name Descriptor message is short"));
      }
    }
  #endregion
  }
}