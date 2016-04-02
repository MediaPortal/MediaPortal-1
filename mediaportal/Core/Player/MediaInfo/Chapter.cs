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

namespace MediaPortal.Player
{
  public class Chapter : MediaStream
  {
    public Chapter(MediaInfo info, int number, int position)
      : base(info, number, position)
    {
    }

    public override MediaStreamKind Kind
    {
      get { return MediaStreamKind.Menu; }
    }

    protected override StreamKind StreamKind
    {
      get { return StreamKind.Other; }
    }

    public double Offset { get; set; }

    public string Description { get; set; }

    protected override void AnalyzeStreamInternal(MediaInfo info)
    {
      base.AnalyzeStreamInternal(info);
    }
  }
}