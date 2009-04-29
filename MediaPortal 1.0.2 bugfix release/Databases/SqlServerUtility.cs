#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Data;
using System.Data.SqlClient;

namespace MediaPortal.Database
{
  public class SqlServerUtility
  {
    public const string DefaultConnectionString =
      "server=127.0.0.1;Integrated Security=false;database=mediaportal;User=sa;Password=sa;";

    public static int GetRowCount(SqlConnection connection, string sql)
    {
      using (SqlCommand cmd = connection.CreateCommand())
      {
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          return reader.RecordsAffected;
        }
      }
    }

    public static void ExecuteNonQuery(SqlConnection connection, string sql)
    {
      using (SqlCommand cmd = connection.CreateCommand())
      {
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.ExecuteNonQuery();
      }
    }

    public static int InsertRecord(SqlConnection connection, string sql)
    {
      using (SqlCommand cmd = connection.CreateCommand())
      {
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;
        cmd.ExecuteNonQuery();
        cmd.CommandText = "select @@identity";
        using (SqlDataReader reader = cmd.ExecuteReader())
        {
          if (reader.Read())
          {
            if (reader[0].GetType() != typeof (DBNull))
            {
              int id = Int32.Parse(reader[0].ToString());
              reader.Close();
              return id;
            }
          }
          reader.Close();
        }
        return -1;
      }
    }

    public static void AddTable(SqlConnection connection, string tableName, string sqlCreateStatement)
    {
      string sql =
        String.Format(
          "if not exists (select * from dbo.sysobjects where id = object_id(N'{0}') and OBJECTPROPERTY(id, N'IsUserTable') = 1)\n",
          tableName);
      sql += "begin\n";
      sql += sqlCreateStatement;
      sql += "\n";
      sql += "end\n";

      ExecuteNonQuery(connection, sql);
    }

    public static void AddConstraint(SqlConnection connection, string constraintName, string sqlCreateStatement)
    {
      string sql = String.Format("if not exists (select * from dbo.sysobjects where id = object_id(N'{0}'))\n",
                                 constraintName);
      sql += "begin\n";
      sql += sqlCreateStatement;
      sql += "\n";
      sql += "end\n";

      ExecuteNonQuery(connection, sql);
    }

    public static void AddIndex(SqlConnection connection, string constraintName, string sqlCreateStatement)
    {
      string sql = String.Format("if not exists (select * from sysindexes where name like '{0}')\n", constraintName);
      sql += "begin\n";
      sql += sqlCreateStatement;
      sql += "\n";
      sql += "end\n";

      ExecuteNonQuery(connection, sql);
    }

    public static void AddPrimaryKey(SqlConnection connection, string table, string field)
    {
      string sql = String.Format("ALTER TABLE {0} WITH NOCHECK ADD CONSTRAINT PK_{0} PRIMARY KEY ({1})", table, field);
      AddConstraint(connection, String.Format("PK_{0}", table), sql);
    }
  }
}