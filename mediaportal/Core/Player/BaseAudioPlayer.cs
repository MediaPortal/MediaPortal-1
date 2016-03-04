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

using MediaPortal.Player.MediaInfo;

namespace MediaPortal.Player
{
  public abstract class BaseAudioPlayer : IPlayer
  {
    public override int EditionStreams
    {
      get { return 0; }
    }

    public override int CurrentEditionStream
    {
      get { return 0; }
      set { }
    }

    public override int VideoStreams
    {
      get { return 0; }
    }

    public override int CurrentVideoStream
    {
      get { return 0; }
      set { }
    }

    public override VideoStream CurrentVideo
    {
      get { return null; }
    }

    public override VideoStream BestVideo
    {
      get { return null; }
    }

    public override int SubtitleStreams
    {
      get { return 0; }
    }

    public override int CurrentSubtitleStream
    {
      get { return 0; }
      set { }
    }
  }
}