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
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Xml;
using System.Net;

namespace SetupTv
{
  public partial class SetupDatabaseForm : Form
  {
    int _currentSchemaVersion = 28;

    enum ProviderType
    {
      SqlServer,
      MySql
    }
    ProviderType _provider = ProviderType.SqlServer;
    public SetupDatabaseForm()
    {
      InitializeComponent();
    }

    void LoadConnectionDetailsFromConfig(bool lookupMachineName)
    {
      //<DefaultProvider name="SQLServer" connectionString="Password=sa;Persist Security Info=True;User ID=sa;Initial Catalog=TvLibrary;Data Source=pcebeckers;" />
      //<DefaultProvider name="MySQL" connectionString="Server=10.0.0.2;Database=test;User ID=xxx;Password=xxx" />
      try
      {
        XmlDocument doc = new XmlDocument();
        string fname = String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        doc.Load(fname);
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode serverName = nodeKey.Attributes.GetNamedItem("name");
        XmlNode attributeConnectionString = nodeKey.Attributes.GetNamedItem("connectionString");
        string connectionString = attributeConnectionString.InnerText;
        string serverType = serverName.InnerText.ToLower();
        if (serverType == "mysql")
        {
          _provider = ProviderType.MySql;
          radioButton2.Checked = true;
        }
        else
        {
          _provider = ProviderType.SqlServer;
          radioButton1.Checked = true;
        }

        string[] parts = connectionString.Split(';');
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
          if (keyValue[0].ToLower() == "data source" || keyValue[0].ToLower() == "server")
          {
            if (keyValue[1].Length == 0 || keyValue[1] == "-")
            {
              if (lookupMachineName && _provider == ProviderType.SqlServer)
              {
                keyValue[1] = Dns.GetHostName() + @"\SQLEXPRESS";
              }
            }
            mpTextBoxServer.Text = keyValue[1];
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, "gentle.config file not found!");
      }
    }

    string ComposeConnectionString(string server, string userid, string password, string database, bool pooling)
    {
      switch (_provider)
      {
        case ProviderType.SqlServer:
          if (database == "") database = "master";
          if (pooling==false)
            return String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog={3};Data Source={2};Pooling=false;", password, userid, server, database);
          return String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog={3};Data Source={2};", password, userid, server, database);

        case ProviderType.MySql:
          if (database == "") database = "mysql";
          return String.Format("Server={0};Database={3};User ID={1};Password={2};", server, userid, password, database);
      }
      return "";
    }

    private void SetupDatabaseForm_Load(object sender, EventArgs e)
    {
      LoadConnectionDetailsFromConfig(true);
    }

    public bool TestConnection()
    {

      LoadConnectionDetailsFromConfig(true);
      try
      {
        string connectionString = ComposeConnectionString(mpTextBoxServer.Text, mpTextBoxUserId.Text, mpTextBoxPassword.Text, "",false);

        switch (_provider)
        {
          case ProviderType.SqlServer:
            using (SqlConnection connect = new SqlConnection(connectionString))
            {
              connect.Open();
              connect.Close();
            }
            break;
          case ProviderType.MySql:
            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
              connect.Open();
              connect.Close();
            }
            break;
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

    public bool ExecuteSQLScript(string prefix)
    {
      bool succeeded = true;
      try
      {
        Assembly assm = Assembly.GetExecutingAssembly();
        string[] names = assm.GetManifestResourceNames();
        Stream stream=null;
        switch (_provider)
        {
          case ProviderType.SqlServer:
            stream = assm.GetManifestResourceStream("SetupTv."+prefix+"database.sql");
            break;
          case ProviderType.MySql:
            stream = assm.GetManifestResourceStream("SetupTv."+prefix+"mysqldatabase.sql");
            break;
        }
        StreamReader reader = new StreamReader(stream);
        string sql = reader.ReadToEnd();
        string[] cmds = null;
        switch (_provider)
        {
          case ProviderType.SqlServer:
            string currentDir = System.IO.Directory.GetCurrentDirectory();
            currentDir += @"\";
            sql = sql.Replace(@"C:\Program Files\Microsoft SQL Server\MSSQL\data\", currentDir);
            sql = sql.Replace("GO\r\n", "!");
            sql = sql.Replace("\r\n", " ");
            sql = sql.Replace("\t", " ");
            cmds = sql.Split('!');
            break;

          case ProviderType.MySql:
            sql = sql.Replace("\r\n", "\r");
            sql = sql.Replace("\t", " ");
            string[] lines = sql.Split('\r');
            sql = "";
            for (int i = 0; i < lines.Length; ++i)
            {
              string line = lines[i].Trim();
              if (line.StartsWith("/*")) continue;
              if (line.StartsWith("--")) continue;
              if (line.Length == 0) continue;
              sql += line;
            }

            cmds = sql.Split('#');
            break;
        }

        string connectionString = ComposeConnectionString(mpTextBoxServer.Text, mpTextBoxUserId.Text, mpTextBoxPassword.Text, "",true);
        switch (_provider)
        {
          case ProviderType.SqlServer:
            using (SqlConnection connect = new SqlConnection(connectionString))
            {
              connect.Open();
              for (int i = 0; i < cmds.Length; ++i)
              {
                cmds[i] = cmds[i].Trim();
                if (cmds[i].Length > 0)
                {
                  try
                  {
                    SqlCommand cmd = connect.CreateCommand();
                    cmd.CommandText = cmds[i];
                    cmd.CommandType = CommandType.Text;
                    TvLibrary.Log.Log.Write("sql:{0}", cmds[i]);
                    cmd.ExecuteNonQuery();
                  }
                  catch (Exception ex)
                  {
                    TvLibrary.Log.Log.Error("failed:sql:{0}", cmds[i]);
                    TvLibrary.Log.Log.Error("reason:{0}",ex.ToString());
                    succeeded = false;
                  }
                }
              }
            }
            break;
          case ProviderType.MySql:
            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
              connect.Open();
              for (int i = 0; i < cmds.Length; ++i)
              {
                cmds[i] = cmds[i].Trim();
                if (cmds[i].Length > 0)
                {
                  if (!cmds[i].StartsWith("--") && !cmds[i].StartsWith("/*"))
                  {
                    try
                    {
                      MySqlCommand cmd = connect.CreateCommand();
                      cmd.CommandText = cmds[i];
                      cmd.CommandType = CommandType.Text;
                      TvLibrary.Log.Log.Write("sql:{0}", cmds[i]);
                      cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                      TvLibrary.Log.Log.Error("failed:sql:{0}", cmds[i]);
                      TvLibrary.Log.Log.Error("reason:{0}", ex.ToString());
                      succeeded = false;
                    }
                  }
                }
              }
            }
            break;
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, "Unable to "+prefix+" database:" + ex.Message);
        succeeded = false;
      }
      SqlConnection.ClearAllPools();
      return succeeded;
    }

    private void mpButtonTest_Click(object sender, EventArgs e)
    {
      CheckServiceName();

      if (radioButton1.Checked)
      {
        _provider = ProviderType.SqlServer;
        try
        {
          string connectionString = ComposeConnectionString(mpTextBoxServer.Text, mpTextBoxUserId.Text, mpTextBoxPassword.Text, "",false);
          using (SqlConnection connect = new SqlConnection(connectionString))
          {
            connect.Open();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(this, "Connection failed!" + ex.Message);
          return;
        }
        SqlConnection.ClearAllPools();
        MessageBox.Show(this, "Connection succeeded!");
      }
      else
      {
        _provider = ProviderType.MySql;
        try
        {
          string connectionString = ComposeConnectionString(mpTextBoxServer.Text, mpTextBoxUserId.Text, mpTextBoxPassword.Text,"",false);
          using (MySqlConnection connect = new MySqlConnection(connectionString))
          {
            connect.Open();
            connect.Close();
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(this, "Connection failed!" + ex.Message);
          return;
        }

        MessageBox.Show(this, "Connection succeeded!");
      }
    }

    /// <summary>
    /// Gets the server name from the config field (strips MSSQL instance name)
    /// </summary>
    /// <param name="ServerConfigText">The server config value from the connection string</param>
    /// <returns>Hostname of Server</returns>
    public string ParseServerHostName(string ServerConfigText)
    {
      string ServerName = string.Empty;

      int delimiterPos = ServerConfigText.IndexOf(@"\");
      if (delimiterPos > 0)
        ServerName = ServerConfigText.Remove(delimiterPos);
      else
        ServerName = ServerConfigText;

      return ServerName;
    }

    void Save()
    {
      string fname = String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

      string connectionString = ComposeConnectionString(mpTextBoxServer.Text, mpTextBoxUserId.Text, mpTextBoxPassword.Text, "TvLibrary", true);
      XmlDocument doc = new XmlDocument();
      doc.Load(fname);
      XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
      XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString"); ;
      XmlNode nodeName = nodeKey.Attributes.GetNamedItem("name"); ;
      if (radioButton1.Checked)
        nodeName.InnerText = "SQLServer";
      else
        nodeName.InnerText = "MySQL";
      node.InnerText = connectionString;

      string ServerName = ParseServerHostName(mpTextBoxServer.Text);
      bool LocalServer = IsDatabaseOnLocalMachine(ServerName);
      TvLibrary.Log.Log.Info("SetupDatabaseForm: Is server {0} local = {1}", ServerName, Convert.ToString(LocalServer));
      CheckServiceName();

      doc.Save(fname);
    }

    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      if (mpTextBoxServer.Text.ToLower().IndexOf("localhost") >= 0)
      {
        MessageBox.Show(this, "Please specify the hostname or ip-address for the server. Not Localhost!");
        return;
      }
      if (mpTextBoxServer.Text.ToLower().IndexOf("127.0.0.1") >= 0)
      {
        MessageBox.Show(this, "Please specify the hostname or ip-address for the server. Not 127.0.0.1!");
        return;
      }
      Save();
      Close();
    }

    public bool ShouldDoUpgrade(out bool isPreviousVersion)
    {
      isPreviousVersion = false;
      LoadConnectionDetailsFromConfig(false);
      try
      {
        string connectionString=ComposeConnectionString(mpTextBoxServer.Text, mpTextBoxUserId.Text, mpTextBoxPassword.Text, "TvLibrary",false);
        switch (_provider)
        {
          case ProviderType.SqlServer:
            {
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
                      if (version != _currentSchemaVersion)
                      {
                        isPreviousVersion = (_currentSchemaVersion -1 == version);
                        return true;
                      }
                      return false;
                    }
                    else return true;
                  }
                }
              }
            }
          
          case ProviderType.MySql:
            {
              using (MySqlConnection connect = new MySqlConnection(connectionString))
              {
                connect.Open();
                using (MySqlCommand cmd = connect.CreateCommand())
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
                      if (version != _currentSchemaVersion)
                      {
                        isPreviousVersion = (_currentSchemaVersion == version + 1);
                        return true;
                      }
                      return false;
                    }
                    else return true;
                  }
                }
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

    /// <summary>
    /// Checks whether the supplied Database server is installed locally.
    /// </summary>
    /// <param name="DBServerName"></param>
    /// <returns></returns>
    public bool IsDatabaseOnLocalMachine(string DBServerName)
    {
      // please add better check if needed
      if (DBServerName.ToLowerInvariant() == Environment.MachineName.ToLowerInvariant())
        return true;
      else
        return false;
    }

    private void CheckServiceName()
    {
      // only query service names of local machine
      if (!IsDatabaseOnLocalMachine(ParseServerHostName(mpTextBoxServer.Text)))
      {
        textBoxServiceName.Enabled = false;
        return;
      }
      else
      {
        textBoxServiceName.Enabled = true;

        // first try the quick method and assume the user is right or using defaults
        string ConfiguredServiceName = textBoxServiceName.Text;
        if (ServiceHelper.IsInstalled(ConfiguredServiceName))
        {
          textBoxServiceName.BackColor = Color.DimGray;
        }
        else
        {
          string DBSearchPattern = @"MySQL";
          // MSSQL
          if (radioButton1.Checked)
            DBSearchPattern = @"MSSQL$";

          if (ServiceHelper.GetDBServiceName(ref DBSearchPattern))
          {
            textBoxServiceName.Text = DBSearchPattern;
            textBoxServiceName.BackColor = Color.DimGray;

            if (ServiceHelper.AddDependencyByName(DBSearchPattern))
              TvLibrary.Log.Log.Info("SetupDatabaseForm: Added dependency for TvService - {0}", DBSearchPattern);
          }
          else
          {
            TvLibrary.Log.Log.Info("SetupDatabaseForm: DB service name not recognized - using defaults");
            textBoxServiceName.BackColor = Color.Red;
          }
        }
      }
    }  

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton2.Checked)
      {
        if (mpTextBoxUserId.Text == "sa")
        {
          mpTextBoxUserId.Text = "root";
          mpTextBoxServer.Text = Dns.GetHostName();
          textBoxServiceName.Enabled = true;
          textBoxServiceName.Text = @"MySQL5";          
        }
      }
    }

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      if (radioButton1.Checked)
      {
        if (mpTextBoxUserId.Text == "root")
        {
          mpTextBoxUserId.Text = "sa";
          mpTextBoxServer.Text = Dns.GetHostName() + @"\SQLEXPRESS";
          textBoxServiceName.Enabled = true;
          textBoxServiceName.Text = @"MSSQL$SQLEXPRESS";
        }
      }
    }
  }
}