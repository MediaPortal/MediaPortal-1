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

using MediaPortal.Localisation;

namespace MediaPortal.Player
{
  public abstract class LanguageMediaStream : MediaStream
  {
    protected LanguageMediaStream(MediaInfo info, int number, int position)
        : base(info, number, position)
    {
    }

    public string Language { get; set; }

    public int Lcid { get; set; }

    public bool Default { get; set; }

    public bool Forced { get; set; }

    protected override void AnalyzeStreamInternal(MediaInfo info)
    {
      base.AnalyzeStreamInternal(info);
      var language = GetString(info, "Language").ToLower();
      Default = GetBool(info, "Default");
      Forced = GetBool(info, "Forced");
      Language = LanguageHelper.GetLanguageByShortName(language);
      Lcid = LanguageHelper.GetLcidByShortName(language);
    }
  }
}