using System;
using System.Collections.Generic;
using System.Text;

namespace TsPacketChecker
{
  internal class ParentalRatingDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private string _countryCode;
    private List<int> _parentalRatings;


    #endregion
    #region Constructor
    public ParentalRatingDescriptor()
    {
    }
    #endregion
    #region Properties
    public List<int> ParentalRatings { get => _parentalRatings; set => _parentalRatings = value; }
    public string CountryCode { get => _countryCode; set => _countryCode = value; }
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Parental Rating Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }

    

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {        
        if (DescriptorLength == 0)
          return;
        if (_parentalRatings == null)
          _parentalRatings = new List<int>();
        var length = DescriptorLength;
        while (length != 0)
        {
          _countryCode = Encoding.UTF8.GetString(buffer, _lastIndex, 3);
          _lastIndex += 3;
          var parentalRating = (int)buffer[_lastIndex];
          _lastIndex++;
          _parentalRatings.Add(parentalRating);
          length -= 4;
        }
        _lastIndex = index + DescriptorLength;
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Parental Rating Descriptor message is short"));
      }
    }
    #endregion
  }
}