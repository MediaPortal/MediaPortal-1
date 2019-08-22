using System;
using System.Collections.ObjectModel;

namespace TsPacketChecker
{
  internal class LocalTimeOffsetDescriptor : Descriptor
  {
    #region Private Fields
    private int _lastIndex = -1;
    private Collection<LocalTimeOffsetEntry> _timeOffsetEntries;


    #endregion
    #region Constructor
    public LocalTimeOffsetDescriptor()
    {
    }
    #endregion
    #region Properties
    #endregion

    #region Methods
    private DateTime GetChangeTime(byte[] byteData, int index)
    {
      int startDate = Utils.Convert2BytesToInt(byteData, index);

      int year = (int)((startDate - 15078.2) / 365.25);
      int month = (int)(((startDate - 14956.1) - (int)(year * 365.25)) / 30.6001);
      int day = (startDate - 14956) - (int)(year * 365.25) - (int)(month * 30.6001);

      int adjust;

      if (month == 14 || month == 15)
        adjust = 1;
      else
        adjust = 0;

      year = year + 1900 + adjust;
      month = month - 1 - (adjust * 12);

      int hour1 = (int)byteData[index + 2] >> 4;
      int hour2 = (int)byteData[index + 2] & 0x0f;
      int hour = (hour1 * 10) + hour2;

      int minute1 = (int)byteData[index + 3] >> 4;
      int minute2 = (int)byteData[index + 3] & 0x0f;
      int minute = (minute1 * 10) + minute2;

      int second1 = (int)byteData[index + 4] >> 4;
      int second2 = (int)byteData[index + 4] & 0x0f;
      int second = (second1 * 10) + second2;

      try
      {
        DateTime utcStartTime = new DateTime(year, month, day, hour, minute, second);
        return (utcStartTime.ToLocalTime());
      }
      catch (ArgumentOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The start time element(s) are out of range"));
      }
      catch (ArgumentException)
      {
        throw (new ArgumentOutOfRangeException("The start time element(s) result in a start time that is out of range"));
      }
    } 
    #endregion
    #region Overrides
    public override int Index
    {
      get
      {
        if (_lastIndex == -1)
          throw (new InvalidOperationException("Local Time Offset Descriptor: Index requested before block processed"));
        return (_lastIndex);
      }
    }

    internal override void Process(byte[] buffer, int index)
    {
      _lastIndex = index;
      try
      {
        while (_lastIndex < buffer.Length - 4)
        {
          LocalTimeOffsetEntry entry = new LocalTimeOffsetEntry();

          entry.CountryCode = Utils.GetString(buffer, _lastIndex, 3);
          _lastIndex += 3;

          entry.Region = (int)buffer[_lastIndex] >> 2;
          entry.OffsetPositive = (buffer[_lastIndex] & 0x01) == 0;
          _lastIndex++;

          int hoursTens = (int)buffer[_lastIndex] >> 4;
          int hoursUnits = (int)buffer[_lastIndex] & 0x0f;
          int minutesTens = (int)buffer[_lastIndex + 1] >> 4;
          int minutesUnits = (int)buffer[_lastIndex + 1] & 0x0f;
          entry.TimeOffset = new TimeSpan((hoursTens * 10) + hoursUnits, (minutesTens * 10) + minutesUnits, 0);
          _lastIndex += 2;

          entry.ChangeTime = GetChangeTime(buffer, _lastIndex);
          _lastIndex += 5;

          int nextHoursTens = (int)buffer[_lastIndex] >> 4;
          int nextHoursUnits = (int)buffer[_lastIndex] & 0x0f;
          int nextMinutesTens = (int)buffer[_lastIndex + 1] >> 4;
          int nextMinutesUnits = (int)buffer[_lastIndex + 1] & 0x0f;
          entry.NextTimeOffset = new TimeSpan((nextHoursTens * 10) + nextHoursUnits, (nextMinutesTens * 10) + nextMinutesUnits, 0);
          _lastIndex += 2;

          _timeOffsetEntries.Add(entry);
        }
      }
      catch (IndexOutOfRangeException)
      {
        throw (new ArgumentOutOfRangeException("The Local Time Offset Descriptor message is short"));
      }
    }
    #endregion
  }

  internal class LocalTimeOffsetEntry
  {
    public LocalTimeOffsetEntry()
    {
    }

    public string CountryCode { get; internal set; }
    public int Region { get; internal set; }
    public bool OffsetPositive { get; internal set; }
    public TimeSpan TimeOffset { get; internal set; }
    public TimeSpan NextTimeOffset { get; internal set; }
    public DateTime ChangeTime { get; internal set; }
  }
}