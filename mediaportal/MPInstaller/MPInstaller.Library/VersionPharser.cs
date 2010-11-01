#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

namespace MediaPortal.MPInstaller
{
  public class VersionPharser
  {
    public Int32 intMajor;
    public Int32 intMinor;
    public Int32 intBuild;
    public Int32 intRevision;
    public String strMajor;
    public String strMinor;
    public String strBuild;
    public String strRevision;

    public VersionPharser(string strVer)
    {
      PharseVersion(strVer);
    }

    public override string ToString()
    {
      return intMajor + strMajor + "." + intMinor + strMinor + "." + intBuild + strBuild + "." + intRevision +
             strRevision;
    }

    public string PharseVersion(String version)
    {
      try
      {
        if (version.StartsWith("."))
        {
          version = "0" + version;
        }

        if (version.EndsWith("."))
        {
          version = version + "0";
        }

        while (version.Split('.').Length < 4)
        {
          version += ".0";
        }

        String[] splitVersion = version.Split('.');

        strMajor = splitVersion[0];
        strMinor = splitVersion[1];
        strBuild = splitVersion[2];
        strRevision = splitVersion[3];

        Regex numerals = new Regex("^[0-9]*",
                                   RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        if (numerals.IsMatch(strMajor))
        {
          intMajor = Int32.Parse(numerals.Match(strMajor).Value);
          strMajor = strMajor.Substring(intMajor.ToString().Length);
        }
        if (numerals.IsMatch(strMinor))
        {
          intMinor = Int32.Parse(numerals.Match(strMinor).Value);
          strMinor = strMinor.Substring(intMinor.ToString().Length);
        }
        if (numerals.IsMatch(strBuild))
        {
          intBuild = Int32.Parse(numerals.Match(strBuild).Value);
          strBuild = strBuild.Substring(intBuild.ToString().Length);
        }
        if (numerals.IsMatch(strMajor))
        {
          intRevision = Int32.Parse(numerals.Match(strRevision).Value);
          strRevision = strRevision.Substring(intRevision.ToString().Length);
        }
      }
      catch {}
      return ToString();
    }

    /// <summary>
    /// Compares the versions.
    /// </summary>
    /// <param name="v1">The v1.</param>
    /// <param name="v2">The v2.</param>
    /// <returns></returns>
    public static int CompareVersions(String v1, String v2)
    {
      VersionPharser version1 = new VersionPharser(v1);
      VersionPharser version2 = new VersionPharser(v2);

      Version numV1 = new Version(version1.intMajor, version1.intMinor, version1.intBuild, version1.intRevision);
      Version numV2 = new Version(version2.intMajor, version2.intMinor, version2.intBuild, version2.intRevision);
      Version alphaV1 = new Version(StringToInt32(version1.strMajor), StringToInt32(version1.strMinor),
                                    StringToInt32(version1.strBuild), StringToInt32(version1.strRevision));
      Version alphaV2 = new Version(StringToInt32(version2.strMajor), StringToInt32(version2.strMinor),
                                    StringToInt32(version2.strBuild), StringToInt32(version2.strRevision));

      if (numV1.CompareTo(numV2) == 0)
      {
        return alphaV1.CompareTo(alphaV2);
      }
      else
      {
        return numV1.CompareTo(numV2);
      }
    }

    public static Int32 StringToInt32(String text)
    {
      Int32 returnValue = 0;
      foreach (Char character in text)
      {
        returnValue += (int)character;
      }
      return returnValue;
    }
  }
}