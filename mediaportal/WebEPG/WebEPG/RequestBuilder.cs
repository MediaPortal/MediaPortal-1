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
using System.Globalization;
using MediaPortal.Utils.Time;
using MediaPortal.Utils.Web;
using MediaPortal.WebEPG.Config.Grabber;

namespace MediaPortal.WebEPG
{
  /// <summary>
  /// Builds HTTP requests
  /// </summary>
  public class RequestBuilder
  {
    #region Variables

    private HTTPRequest _baseRequest;
    private WorldDateTime _requestTime;
    private RequestData _data;
    private int _dayOffset;
    private int _offset;

    #endregion

    #region Constructors/Destructors

    public RequestBuilder(HTTPRequest baseRequest, DateTime startTime, RequestData data)
    {
      _baseRequest = baseRequest;
      _requestTime = new WorldDateTime(startTime);
      _data = data;
      _dayOffset = 0;
      _offset = 0;
    }

    #endregion

    #region Properties

    public int DayOffset
    {
      get { return _dayOffset; }
      set { _dayOffset = value; }
    }

    public int Offset
    {
      get { return _offset; }
      set { _offset = value; }
    }

    #endregion

    #region Public Methods

    public void AddDays(int days)
    {
      _dayOffset += days;
      _requestTime = _requestTime.AddDays(days);
    }

    public HTTPRequest GetRequest()
    {
      HTTPRequest request = new HTTPRequest(_baseRequest);
      CultureInfo culture = new CultureInfo(_data.SearchLang);

      if (_data.DayNames != null)
      {
        request.ReplaceTag("[DAY_NAME]", _data.DayNames[_dayOffset]);
      }

      if (_data.BaseDate != null)
      {
        DateTime basedt = DateTime.Parse(_data.BaseDate);
        request.ReplaceTag("[DAYS_SINCE]", _requestTime.DaysSince(basedt).ToString());
      }

      request.ReplaceTag("[ID]", _data.ChannelId);

      request.ReplaceTag("[DAY_OFFSET]", (_dayOffset + _data.OffsetStart).ToString());
      request.ReplaceTag("[EPOCH_TIME]", _requestTime.ToEpochTime().ToString());
      request.ReplaceTag("[EPOCH_DATE]", _requestTime.ToEpochDate().ToString());
      request.ReplaceTag("[DAYOFYEAR]", _requestTime.DateTime.DayOfYear.ToString());
      request.ReplaceTag("[YYYY]", _requestTime.Year.ToString());
      request.ReplaceTag("[MM]", String.Format("{0:00}", _requestTime.Month));
      request.ReplaceTag("[_M]", _requestTime.Month.ToString());
      request.ReplaceTag("[MONTH]", _requestTime.DateTime.ToString("MMMM", culture));
      request.ReplaceTag("[DD]", String.Format("{0:00}", _requestTime.Day));
      request.ReplaceTag("[_D]", _requestTime.Day.ToString());
      request.ReplaceTag("[WEEKDAY]", _requestTime.DateTime.ToString(_data.WeekDay, culture));

      // this fix is needed for countries where the first day (0) is Sunday (not Monday)
      // thoose grabbers should include OffsetStart="1" in the Search tag.
      int dayNum = (int) _requestTime.DateTime.DayOfWeek + _data.OffsetStart;
      if (dayNum < 0)
      {
        dayNum += 7;
      }
      if (dayNum > 6)
      {
        dayNum = dayNum%7;
      }
      request.ReplaceTag("[DAY_OF_WEEK]", dayNum.ToString());

      request.ReplaceTag("[LIST_OFFSET]", ((_offset*_data.MaxListingCount) + _data.ListStart).ToString());
      request.ReplaceTag("[PAGE_OFFSET]", (_offset + _data.PageStart).ToString());

      return request;
    }

    public bool HasDate()
    {
      if (
        _baseRequest.HasTag("[DD]") ||
        _baseRequest.HasTag("[_D]") ||
        _baseRequest.HasTag("[MM]") ||
        _baseRequest.HasTag("[_M]") ||
        _baseRequest.HasTag("[YYYY]") ||
        _baseRequest.HasTag("[MONTH]") ||
        _baseRequest.HasTag("[DAY_OF_WEEK]") ||
        _baseRequest.HasTag("[WEEKDAY]") ||
        _baseRequest.HasTag("[DAY_NAME]") ||
        _baseRequest.HasTag("[DAY_OFFSET]") ||
        _baseRequest.HasTag("[EPOCH_TIME]") ||
        _baseRequest.HasTag("[EPOCH_DATE]") ||
        _baseRequest.HasTag("[DAYS_SINCE]") ||
        _baseRequest.HasTag("[DAYOFYEAR]"))
      {
        return true;
      }

      return false;
    }

    public bool HasList()
    {
      if (_baseRequest.HasTag("[LIST_OFFSET]"))
      {
        return true;
      }

      return false;
    }

    public bool IsLastPage()
    {
      if (_offset + _data.PageStart == _data.PageEnd)
      {
        return true;
      }

      return false;
    }

    public bool IsMaxListing(int count)
    {
      if (_data.MaxListingCount != 0 && _data.MaxListingCount == count)
      {
        return true;
      }

      return false;
    }

    #endregion
  }
}