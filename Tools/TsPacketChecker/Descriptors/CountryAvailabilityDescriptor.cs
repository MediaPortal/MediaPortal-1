using System;
using System.Collections.ObjectModel;
using System.Text;

namespace WindowsApplication13
{
  internal class CountryAvailabilityDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private bool _availabilityFlag;
    private Collection<string> _countryCodes;
    #endregion
    #region Constructor
    public CountryAvailabilityDescriptor()
    {
    }
    #endregion
    #region Properties
    public Collection<string> CountryCodes { get { return _countryCodes; } set { _countryCodes = value; } }
    public bool AvailabilityFlag { get { return _availabilityFlag; } set { _availabilityFlag = value; } }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Country Availability Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }   

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        if (DescriptorLength != 0)
        {
          _availabilityFlag = (buffer[_lastIndex] & 0x80) != 0;
          _lastIndex++;

          var countryCount = (DescriptorLength - 1) / 3;

          if (countryCount != 0)
          {
            _countryCodes = new Collection<string>();

            while (_countryCodes.Count != countryCount)
            {
              var countryCode = Encoding.UTF8.GetString(buffer, _lastIndex, 3);
              _countryCodes.Add(countryCode);
              _lastIndex += 3;
            }
          }
        }
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Country Availability Descriptor message is short"));
      }
    }
    #endregion
  }
}