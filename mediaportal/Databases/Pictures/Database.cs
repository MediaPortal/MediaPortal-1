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

using System.Collections.Generic;
using MediaPortal.Database;

namespace MediaPortal.Picture.Database
{
  public class PictureDatabase
  {
    public static IPictureDatabase _database = DatabaseFactory.GetPictureDatabase();

    private PictureDatabase()
    {
    }

    static PictureDatabase()
    {
    }

    public static void ReOpen()
    {
      Dispose();
      _database = DatabaseFactory.GetPictureDatabase();
    }

    public static void Dispose()
    {
      if (_database != null)
      {
        _database.Dispose();
      }      
      //_database = null;
    }

    public static int AddPicture(string strPicture, int iRotation)
    {
      return _database.AddPicture(strPicture, iRotation);
    }

    public static void DeletePicture(string strPicture)
    {
      _database.DeletePicture(strPicture);
    }

    public static int GetRotation(string strPicture)
    {
      return _database.GetRotation(strPicture);
    }

    public static void SetRotation(string strPicture, int iRotation)
    {
      _database.SetRotation(strPicture, iRotation);
    }

    //public static DateTime GetDateTaken(string strPicture)
    //{
    //  return _database.GetDateTaken(strPicture);
    //}

    public static int EXIFOrientationToRotation(int orientation)
    {
      return _database.EXIFOrientationToRotation(orientation);
    }

    public static int ListYears(ref List<string> Years)
    {
      return _database.ListYears(ref Years);
    }

    public static int ListMonths(string Year, ref List<string> Months)
    {
      return _database.ListMonths(Year, ref Months);
    }

    public static int ListDays(string Month, string Year, ref List<string> Days)
    {
      return _database.ListDays(Month, Year, ref Days);
    }

    public static int ListPicsByDate(string Date, ref List<string> Pics)
    {
      return _database.ListPicsByDate(Date, ref Pics);
    }

    public static int CountPicsByDate(string Date)
    {
      return _database.CountPicsByDate(Date);
    }

    public static string DatabaseName
    {
      get
      {
        if (_database != null)
        {
          return _database.DatabaseName;
        }
        return "";
      }
    }
  }
}