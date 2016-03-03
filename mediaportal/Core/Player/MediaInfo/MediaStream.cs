#region Copyright (C) 2005-2016 Team MediaPortal

// Copyright (C) 2005-2016 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Globalization;

namespace MediaPortal.Player.MediaInfo
{
  public enum MediaStreamKind
  {
    Video,
    Audio,
    Text,
    Image,
    Menu
  }

  public abstract class MediaStream : MarshalByRefObject
  {
    protected MediaStream(MediaInfo info, int number)
    {
      StreamNumber = number;
      if (info != null)
      {
        AnalyzeStream(info);
      }
    }

    public int Id { get; set; }

    public string Name { get; set; }

    public abstract MediaStreamKind Kind { get; }

    protected abstract StreamKind StreamKind { get; }

    public int StreamNumber { get; private set; }

    protected virtual void AnalyzeStreamInternal(MediaInfo info)
    {
      Id = GetInt(info, "ID");
      Name = GetString(info, "Title");
    }

    private void AnalyzeStream(MediaInfo info)
    {
      AnalyzeStreamInternal(info);
    }

    protected long GetLong(MediaInfo info, string parameter)
    {
      long parsedValue;
      var result = info.Get(StreamKind, StreamNumber, parameter);
      return long.TryParse(result, out parsedValue) ? parsedValue : 0;
    }

    protected double GetDouble(MediaInfo info, string parameter)
    {
      NumberFormatInfo providerNumber = new NumberFormatInfo();
      providerNumber.NumberDecimalSeparator = ".";
      double parsedValue;
      var result = info.Get(StreamKind, StreamNumber, parameter);
      return double.TryParse(result, NumberStyles.AllowDecimalPoint, providerNumber, out parsedValue) ? parsedValue : 0;
    }

    protected int GetInt(MediaInfo info, string parameter)
    {
      int parsedValue;
      var result = info.Get(StreamKind, StreamNumber, parameter);
      return int.TryParse(result, out parsedValue) ? parsedValue : 0;
    }

    protected bool GetBool(MediaInfo info, string parameter)
    {
      bool parsedValue;
      var result = info.Get(StreamKind, StreamNumber, parameter);
      return bool.TryParse(result, out parsedValue) && parsedValue;
    }

    protected DateTime GetDateTime(MediaInfo info, string parameter)
    {
      DateTime parsedValue;
      var result = info.Get(StreamKind, StreamNumber, parameter);
      return DateTime.TryParse(result, out parsedValue) ? parsedValue : DateTime.MinValue;
    }

    protected string GetString(MediaInfo info, string parameter)
    {
      var result = info.Get(StreamKind, StreamNumber, parameter);
      return result ?? string.Empty;
    }
  }
}