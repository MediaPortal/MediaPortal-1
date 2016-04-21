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

using System.Collections.Generic;

namespace MediaPortal.Player
{
  public enum SubtitleCodec
  {
    S_UNDEFINED,
    S_ASS,
    S_IMAGE_BMP,
    S_SSA,
    S_TEXT_ASS,
    S_TEXT_SSA,
    S_TEXT_USF,
    S_TEXT_UTF8,
    S_USF,
    S_UTF8,
    S_VOBSUB,
    S_HDMV_PGS,
    S_HDMV_TEXTST
  }

  public class SubtitleStream : LanguageMediaStream
  {
    #region match dictionary

    private static readonly Dictionary<string, SubtitleCodec> SubtitleCodecs = new Dictionary<string, SubtitleCodec>
    {
        { "S_ASS", SubtitleCodec.S_ASS },
        { "S_IMAGE/BMP", SubtitleCodec.S_IMAGE_BMP },
        { "S_SSA", SubtitleCodec.S_SSA },
        { "S_TEXT/ASS", SubtitleCodec.S_TEXT_ASS },
        { "S_TEXT/SSA", SubtitleCodec.S_TEXT_SSA },
        { "S_TEXT/USF", SubtitleCodec.S_TEXT_USF },
        { "S_TEXT/UTF8", SubtitleCodec.S_TEXT_UTF8 },
        { "S_USF", SubtitleCodec.S_USF },
        { "S_UTF8", SubtitleCodec.S_UTF8 },
        { "S_VOBSUB", SubtitleCodec.S_VOBSUB },
        { "S_HDMV/PGS", SubtitleCodec.S_HDMV_PGS },
        { "S_HDMV/TEXTST", SubtitleCodec.S_HDMV_TEXTST }
    };

    #endregion

    public SubtitleStream(MediaInfo info, int number, int position)
        : base(info, number, position)
    {
    }

    public string Format { get; private set; }

    public SubtitleCodec Codec { get; private set; }

    public override MediaStreamKind Kind
    {
      get { return MediaStreamKind.Text; }
    }

    protected override StreamKind StreamKind
    {
      get { return StreamKind.Text; }
    }

    protected override void AnalyzeStreamInternal(MediaInfo info)
    {
      base.AnalyzeStreamInternal(info);
      Format = GetString(info, "Format");
      Codec = GetCodec(GetString(info, "CodecID").ToUpper());
    }

    private static SubtitleCodec GetCodec(string source)
    {
      SubtitleCodec result;
      return SubtitleCodecs.TryGetValue(source, out result) ? result : SubtitleCodec.S_UNDEFINED;
    }
  }
}