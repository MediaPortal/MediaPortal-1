#region Copyright (C) 2005-2011 Team MediaPortal

// Copyright (C) 2005-2011 Team MediaPortal
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
using Microsoft.Win32;

namespace SetMerit
{
  internal static class Program
  {
    private const string FiltersKey = @"CLSID\{083863F1-70DE-11d0-BD40-00A0C911CE86}\Instance\";
    private const string MeritKey = "FilterData";


    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main(string[] args)
    {
      try
      {
        if (args.Length == 2)
        {
          if (args[1].Length != 8)
            throw new InvalidOperationException(String.Format("Not a valid merit value \"{0}\"", args[1]));

          // Reverse the merit bytes ...
          byte[] meritData = new byte[4];
          int meritByte = 3;
          for (int index = 0; index < 8; index += 2)
          {
            string byteStr = args[1].Substring(index, 2);

            meritData[meritByte--] = byte.Parse(byteStr, System.Globalization.NumberStyles.HexNumber);
          }

          string filter = FiltersKey + args[0];
          Console.WriteLine("Modifying HKCR\\{0} ...", filter);

          using (RegistryKey regKey = Registry.ClassesRoot.OpenSubKey(filter, true))
          {
            byte[] data = (byte[])regKey.GetValue(MeritKey, null);

            Array.Copy(meritData, 0, data, 4, 4);

            regKey.SetValue(MeritKey, (object)data);
          }

          Console.WriteLine("Success");
        }
        else
        {
          Console.WriteLine("Usage: SetMerit [Filter GUID] [New Merit]");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
      }
    }
  }
}