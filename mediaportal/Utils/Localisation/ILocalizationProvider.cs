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

using System.Globalization;
using MediaPortal.Localisation.LanguageStrings;

namespace MediaPortal.Localisation
{
    public interface ILocalizationProvider
    {
        CultureInfo CurrentLanguage { get; }

        int Characters { get; }

        bool UseRTL { get; }

        CultureInfo[] AvailableLanguages { get; }

        bool IsLocalSupported { get; }

        void AddDirection(string directory);

        void ChangeLanguage(string cultureName);

        StringLocalised Get(string section, int id);

        string GetString(string section, int id);

        string GetString(string section, int id, object[] parameters);

        CultureInfo GetBestLanguage();
    }
}