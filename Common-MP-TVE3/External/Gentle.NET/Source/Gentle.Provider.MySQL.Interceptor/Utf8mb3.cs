#region Copyright (C) 2024 Team MediaPortal

// Copyright (C) 2024 Team MediaPortal
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
using System.Collections;
using MySql.Data.MySqlClient;

namespace Gentle.Provider.MySQL.Interceptor
{
  // ReSharper disable once InconsistentNaming
  public static class Utf8mb3
  {
    private static readonly Version NewFieldNamingVersion = new Version(6, 10, 0);

    public static void Enable()
    {
      // Add internal mapping of database utf8mb3 charset to .NET framework's UTF-8 encoding
      var assembly = System.Reflection.Assembly.GetAssembly(typeof(MySqlConnection));
      var connectorVersion = assembly.GetName().Version;

      var mappingFieldName = connectorVersion >= NewFieldNamingVersion ? "_mapping" : "mapping";

      var mappingField = assembly
        .GetType("MySql.Data.MySqlClient.CharSetMap").GetField(mappingFieldName,
          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.GetField |
          System.Reflection.BindingFlags.Static);

      if (mappingField != null)
      {
        var mappingDictionary = (IDictionary)mappingField.GetValue(null);
        var utf8Mapping = mappingDictionary["utf8"];

        if (utf8Mapping != null)
        {
          try
          {
            mappingDictionary.Add("utf8mb3", utf8Mapping);
          }
          catch (ArgumentException)
          {
            // Item already exist
          }
        }
      }
    }
  }
}