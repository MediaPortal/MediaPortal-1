/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Xml;
using System.Net;

namespace SetupTv
{
  public partial class SetupDatabaseForm : Form
  {
    public SetupDatabaseForm()
    {
      InitializeComponent();
    }

    void LoadConnectionString(bool lookupMachineName)
    {
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load("IdeaBlade.ibconfig");
        XmlNode node = doc.SelectSingleNode("/ideaBlade/rdbKey/connection");
        string text = node.InnerText;
        string[] parts = text.Split(';');
        for (int i = 0; i < parts.Length; ++i)
        {
          string part = parts[i];
          string[] keyValue = part.Split('=');
          if (keyValue[0].ToLower() == "password")
          {
            mpTextBoxPassword.Text = keyValue[1];
          }
          if (keyValue[0].ToLower() == "user id")
          {
            mpTextBoxUserId.Text = keyValue[1];
          }
          if (keyValue[0].ToLower() == "data source")
          {
            if (keyValue[1] == "-")
            {
              if (lookupMachineName)
              {
                keyValue[1] = Dns.GetHostName() + @"\SQLEXPRESS";
              }
            }
            mpTextBoxServer.Text = keyValue[1];
          }
        }
      }
      catch (Exception)
      {
        MessageBox.Show("IdeaBlade.ibconfig file not found!");
      }
    }
    private void SetupDatabaseForm_Load(object sender, EventArgs e)
    {
      LoadConnectionString(true);
    }

    public bool TestConnection()
    {
      LoadConnectionString(true);
      try
      {
        string connectionString = String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog=master;Data Source={2};Pooling=false;",
              mpTextBoxPassword.Text, mpTextBoxUserId.Text, mpTextBoxServer.Text);
        using (SqlConnection connect = new SqlConnection(connectionString))
        {
          connect.Open();
          connect.Close();
        }
      }
      catch (Exception)
      {
        GC.Collect();
        GC.Collect();
        GC.Collect();
        GC.Collect();
        return false;
      }
      GC.Collect();
      GC.Collect();
      GC.Collect();
      GC.Collect();
      SqlConnection.ClearAllPools();

      //database server is found
      return true;
    }

    public void CreateDatabase()
    {
      try
      {
        Assembly assm = Assembly.GetExecutingAssembly();
        string[] names = assm.GetManifestResourceNames();
        Stream stream = assm.GetManifestResourceStream("SetupTv.database.sql");
        StreamReader reader = new StreamReader(stream);
        string sql = reader.ReadToEnd();
        string connectionString = String.Format("Provider=SQLOLEDB.1;Password={0};Persist Security Info=True;User ID={1};Initial Catalog=master;Data Source={2}",
                mpTextBoxPassword.Text, mpTextBoxUserId.Text, mpTextBoxServer.Text);
        string currentDir = System.IO.Directory.GetCurrentDirectory();
        currentDir += @"\";
        sql = sql.Replace(@"C:\Program Files\Microsoft SQL Server\MSSQL\data\", currentDir);
        sql = sql.Replace("GO\r\n", "!");
        sql = sql.Replace("\r\n", " ");
        sql = sql.Replace("\t", " ");
        string[] cmds = sql.Split('!');

        connectionString = String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog=master;Data Source={2}",
                mpTextBoxPassword.Text, mpTextBoxUserId.Text, mpTextBoxServer.Text);
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
              TvLibrary.Log.Log.Write("sql:{0}", cmds[i]);
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
    private void mpButtonTest_Click(object sender, EventArgs e)
    {
      try
      {
        string connectionString = String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog=master;Data Source={2}",
              mpTextBoxPassword.Text, mpTextBoxUserId.Text, mpTextBoxServer.Text);
        using (SqlConnection connect = new SqlConnection(connectionString))
        {
          connect.Open();
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Connection failed!" + ex.Message);
        return;
      }
      SqlConnection.ClearAllPools();
      MessageBox.Show("Connection succeeded!");
    }
    void Save()
    {
      string connectionString = String.Format("Provider=SQLOLEDB.1;Password={0};Persist Security Info=True;User ID={1};Initial Catalog=TvLibrary;Data Source={2};",
            mpTextBoxPassword.Text, mpTextBoxUserId.Text, mpTextBoxServer.Text);
      XmlDocument doc = new XmlDocument();
      doc.Load("IdeaBlade.ibconfig");
      XmlNode node = doc.SelectSingleNode("/ideaBlade/rdbKey/connection");
      node.InnerText = connectionString;
      doc.Save("IdeaBlade.ibconfig");
    }

    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      Save();
      Close();
    }

    public bool ShouldDoUpgrade()
    {
      LoadConnectionString(false);
      try
      {
        string connectionString = String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog=TvLibrary;Data Source={2};Pooling=false;",
                mpTextBoxPassword.Text, mpTextBoxUserId.Text, mpTextBoxServer.Text);

        using (SqlConnection connect = new SqlConnection(connectionString))
        {
          connect.Open();
          using (SqlCommand cmd = connect.CreateCommand())
          {
            cmd.CommandType = CommandType.Text;
            cmd.CommandText = "select * from version";
            using (IDataReader reader = cmd.ExecuteReader())
            {
              if (reader.Read())
              {
                int version = (int)reader["versionNumber"];
                reader.Close();
                connect.Close();
                if (version != 7)
                {
                  return true;
                }
                return false;
              }
              connect.Close();
              reader.Close();
            }
          }
        }
        return false;
      }
      catch (Exception)
      {
        return true;
      }
      finally
      {
        SqlConnection.ClearAllPools();
        GC.Collect();
        GC.Collect();
        GC.Collect();
        GC.Collect();
      }
    }
  }
}