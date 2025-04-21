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
using System.Data;
using MySql.Data.MySqlClient;

namespace Gentle.Provider.MySQL.Interceptor
{
  /// <summary>
  /// Filters out collations with NULL id (e.g. UCA-14.0.0) from SHOW COLLATION command
  /// </summary>
  public sealed class Interceptor : BaseCommandInterceptor
  {
    public override bool ExecuteReader(string sql, CommandBehavior behavior, ref MySqlDataReader returnValue)
    {
      if (!sql.Equals("SHOW COLLATION", StringComparison.OrdinalIgnoreCase))
      {
        return false;
      }

      MySqlCommand command = ActiveConnection.CreateCommand();

      command.CommandText = "SHOW COLLATION WHERE id IS NOT NULL";
      returnValue = command.ExecuteReader(behavior);

      return true;
    }
  }
}