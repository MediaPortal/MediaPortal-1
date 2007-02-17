#region Copyright (C) 2005-2007 Team MediaPortal

/* 
 *	Copyright (C) 2005-2007 Team MediaPortal
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
using System.Text;
using MediaPortal.Utils.Web;
using MediaPortal.Utils.Time;
using MediaPortal.WebEPG.Parser;

namespace MediaPortal.WebEPG
{
  /// <summary>
  /// Controls the time in EPG listings
  /// </summary>
  public class ListingTimeControl
  {
    #region Enums
    private enum Expect
    {
      Start,
      BeforeMidday,
      AfterMidday
    }
    #endregion

    #region Variables
    private int _lastTime;
    private DateTime _startTime;
    private Expect _expectedTime;
    private int _grabDay;
    private int _addDays;
    private bool _newDay;
    private bool _nextDay;
    private int _programCount;
    #endregion

    #region Constructors/Destructors
    public ListingTimeControl(DateTime startTime)
    {
      _startTime = startTime;
      _nextDay = false;
      _lastTime = 0;
      _grabDay = 0;
      _newDay = true;
    }
    #endregion

    #region Properties
    public int GrabDay
    {
      get { return _grabDay; }
    }
    #endregion

    #region Public Methods
    public bool CheckAdjustTime(ref ProgramData guideData)
    {
      WorldDateTime guideStartTime = guideData.StartTime;
      WorldDateTime guideEndTime = guideData.EndTime;
      _addDays = 1;

      // Day
      if (guideStartTime.Day == 0)
      {
        guideStartTime.Day = _startTime.Day;
      }
      else
      {
        if (guideStartTime.Day != _startTime.Day && _expectedTime != Expect.Start)
        {
          _grabDay++;
          _startTime = _startTime.AddDays(1);
          _nextDay = false;
          _lastTime = 0;
          _expectedTime = Expect.BeforeMidday;
        }
      }

      if (guideStartTime.Year == 0)
        guideStartTime.Year = _startTime.Year;
      if (guideStartTime.Month == 0)
        guideStartTime.Month = _startTime.Month;

      // Start Time
      switch (_expectedTime)
      {
        case Expect.Start:
          if (OnPerviousDay(guideStartTime.Hour))
            return false;				// Guide starts on pervious day ignore these listings.

          if (_newDay)
          {
            _newDay = false;
            //if (guideStartTime.Hour < _startTime.Hour)
            //  return false;

            if (guideStartTime.Hour <= 12)
            {
              _expectedTime = Expect.BeforeMidday;
              goto case Expect.BeforeMidday;
            }

            _expectedTime = Expect.AfterMidday;
            goto case Expect.AfterMidday;
          }

          _expectedTime = Expect.BeforeMidday;
          goto case Expect.BeforeMidday;      // Pass into BeforeMidday Code

        case Expect.BeforeMidday:
          if (_lastTime > guideStartTime.Hour)
          {
            _expectedTime = Expect.AfterMidday;
            //if (_bNextDay)
            //{
            //    _GrabDay++;
            //}
          }
          else
          {
            if (guideStartTime.Hour <= 12)
              break;						// Do nothing
          }

          // Pass into AfterMidday Code
          //_LastStart = 0;
          goto case Expect.AfterMidday;

        case Expect.AfterMidday:
          bool adjusted = false;
          if (guideStartTime.Hour < 12)		// Site doesn't have correct time
          {
            guideStartTime.Hour += 12;    // starts again at 1:00 without "pm"
            adjusted = true;
          }

          if (_lastTime > guideStartTime.Hour)
          {

            if (_nextDay)
            {
              _addDays++;
              _grabDay++;
              _startTime = _startTime.AddDays(1);
              //_bNextDay = false;
            }
            else
            {
              _nextDay = true;
            }
            if (adjusted)
              guideStartTime.Hour -= 12;

            if (guideStartTime.Hour < 12)
              _expectedTime = Expect.BeforeMidday;

            break;
          }

          break;

        default:
          break;
      }

      ////Month
      //int month;
      //if (guideMonth == "")
      //{
      //  month = _startTime.Month;
      //}
      //else
      //{
      //  month = getMonth(guideMonth);
      //}

      _lastTime = guideStartTime.Hour;

      if (guideEndTime != null)
      {
        if (guideEndTime.Year == 0)
          guideEndTime.Year = guideStartTime.Year;
        if (guideEndTime.Month == 0)
          guideEndTime.Month = guideStartTime.Month;
        if (guideEndTime.Day == 0)
          guideEndTime.Day = guideStartTime.Day;

        if (_nextDay)
        {
          if (guideStartTime.Hour > guideEndTime.Hour)
            guideEndTime = guideEndTime.AddDays(_addDays + 1);
          else
            guideEndTime = guideEndTime.AddDays(_addDays);
        }
        else
        {
          if (guideStartTime.Hour > guideEndTime.Hour)
            guideEndTime = guideEndTime.AddDays(_addDays);
        }
      }


      if (_nextDay)
        guideStartTime = guideStartTime.AddDays(_addDays);

      //_log.Debug("WebEPG: Guide, Program Debug: [{0} {1}]", _GrabDay, _bNextDay);

      guideData.StartTime = guideStartTime;
      guideData.EndTime = guideEndTime;

      return true;
    }

    public void NewDay()
    {
      _startTime = _startTime.AddDays(1);
      _newDay = true;
      _expectedTime = Expect.Start;
      _lastTime = 0;
      _nextDay = false;
    }

    public void SetProgramCount(int count)
    {
      _programCount = count;
    }
    #endregion

    #region Private Methods
    private bool OnPerviousDay(int programStartHour)
    {
      // program starts late on the day
      if (programStartHour >= 20)
      {
        // program starts after grab time -> site filters programs based on current time
        // and less then 1 program per hour 
        if (_startTime.Hour >= 20 && _programCount <= 6)
          return false;

        return true;
      }

      return false;
    }
    #endregion
  }
}
