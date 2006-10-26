#region Copyright (C) 2006 Team MediaPortal

/* 
 *	Copyright (C) 2006 Team MediaPortal
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
using System.Xml;
using System.Net;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Data;
using System.Data.SqlClient;

namespace TvEngine
{
  static class TvMovieSql
  {
    private static string _userId = "sa";
    private static string _password = "sa";
    private static string _server = @"localhost\SQLEXPRESS";

    private static void LoadConnectionString(bool lookupMachineName)
    {
      try
      {
        XmlDocument doc = new XmlDocument();
        string fname = String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        doc.Load(fname);
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
        string text = node.InnerText;
        string[] parts = text.Split(';');
        for (int i = 0; i < parts.Length; ++i)
        {
          string part = parts[i];
          string[] keyValue = part.Split('=');
          if (keyValue[0].ToLower() == "password")
          {
            _password = keyValue[1];
          }
          if (keyValue[0].ToLower() == "user id")
          {
            _userId = keyValue[1];
          }
          if (keyValue[0].ToLower() == "data source")
          {
            if (keyValue[1].Length == 0 || keyValue[1] == "-")
            {
              if (lookupMachineName)
              {
                keyValue[1] = Dns.GetHostName() + @"\SQLEXPRESS";
              }
            }
            _server = keyValue[1];
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("gentle.config file not found!");
      }
    }

    private static void RunSqlCheckQuery()
    {
      try
      {
        Assembly assm = Assembly.GetExecutingAssembly();
        string[] names = assm.GetManifestResourceNames();
        Stream stream = assm.GetManifestResourceStream("TvMovie.TvMovieSql.sql");
        StreamReader reader = new StreamReader(stream);
        string sql = reader.ReadToEnd();
        string connectionString = String.Format("Provider=SQLOLEDB.1;Password={0};Persist Security Info=True;User ID={1};Initial Catalog=master;Data Source={2}",
                _password, _userId, _server);
        string currentDir = System.IO.Directory.GetCurrentDirectory();
        currentDir += @"\";
        sql = sql.Replace(@"C:\Program Files\Microsoft SQL Server\MSSQL\data\", currentDir);
        sql = sql.Replace("GO\r\n", "!");
        sql = sql.Replace("\r\n", " ");
        sql = sql.Replace("\t", " ");
        string[] cmds = sql.Split('!');

        connectionString = String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog=master;Data Source={2}",
                _password, _userId, _server);
        using (SqlConnection connect = new SqlConnection(connectionString))
        {
          connect.Open();
          for (int i = 0; i < cmds.Length; ++i)
          {
            cmds[i] = cmds[i].Trim();
            if (cmds[i].Length > 0)
            {
              SqlCommand cmd = connect.CreateCommand();
              cmd.CommandText = cmds[i];
              cmd.CommandType = CommandType.Text;
              //TvLibrary.Log.Log.Write("sql:{0}", cmds[i]);
              cmd.ExecuteNonQuery();
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Unable to create database:" + ex.Message);
      }
      SqlConnection.ClearAllPools();
    }

    public static void CheckDatabase()
    {
      LoadConnectionString(true);
      RunSqlCheckQuery();
    }

  }
}
