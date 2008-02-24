/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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
using TvLibrary.Log;
using System.Diagnostics;

namespace SetupTv
{
  public partial class SetupDatabaseForm : Form
  {
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

    private void LoadConnectionDetailsFromConfig(bool lookupMachineName)
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
          rbMySQL.Checked = true;
        }
        else
        {
          _provider = ProviderType.SqlServer;
          rbSQLServer.Checked = true;
        }

        string[] parts = connectionString.Split(';');
        for (int i = 0 ; i < parts.Length ; ++i)
        {
          string part = parts[i];
          string[] keyValue = part.Split('=');
          if (keyValue[0].ToLower() == "password")
          {
            tbPassword.Text = keyValue[1];
          }
          if (keyValue[0].ToLower() == "user id")
          {
            tbUserID.Text = keyValue[1];
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
            tbServerHostName.Text = keyValue[1];
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, string.Format("gentle.config file not found! ({0})", ex.Message));
      }
    }

    private string ComposeConnectionString(string server, string userid, string password, string database, bool pooling)
    {
      switch (_provider)
      {
        case ProviderType.SqlServer:
          if (database == "") database = "master";
          if (pooling == false)
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
        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "", false);

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
        return false;
      }     

      SqlConnection.ClearAllPools();

      GC.Collect();
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
        Stream stream = null;
        switch (_provider)
        {
          case ProviderType.SqlServer:
            stream = assm.GetManifestResourceStream("SetupTv." + prefix + "_sqlserver_database.sql");
            break;
          case ProviderType.MySql:
            stream = assm.GetManifestResourceStream("SetupTv." + prefix + "_mysql_database.sql");
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
            for (int i = 0 ; i < lines.Length ; ++i)
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

        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "", true);
        switch (_provider)
        {
          case ProviderType.SqlServer:
            using (SqlConnection connect = new SqlConnection(connectionString))
            {
              connect.Open();
              for (int i = 0 ; i < cmds.Length ; ++i)
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
                    TvLibrary.Log.Log.Error("reason:{0}", ex.ToString());
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
              for (int i = 0 ; i < cmds.Length ; ++i)
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
        MessageBox.Show(this, "Unable to " + prefix + " database:" + ex.Message);
        succeeded = false;
      }
      SqlConnection.ClearAllPools();
      return succeeded;
    }

    private void mpButtonTest_Click(object sender, EventArgs e)
    {
      CheckServiceName();

      if (rbSQLServer.Checked)
      {
        _provider = ProviderType.SqlServer;
        try
        {
          string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "", false);
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
          string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "", false);
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
    private string ParseServerHostName(string ServerConfigText)
    {
      string ServerName = string.Empty;

      int delimiterPos = ServerConfigText.IndexOf(@"\");
      if (delimiterPos > 0)
        ServerName = ServerConfigText.Remove(delimiterPos);
      else
        ServerName = ServerConfigText;

      return ServerName;
    }

    private void Save()
    {
      string fname = String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));

      string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "TvLibrary", true);
      XmlDocument doc = new XmlDocument();
      doc.Load(fname);
      XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
      XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString"); ;
      XmlNode nodeName = nodeKey.Attributes.GetNamedItem("name"); ;
      if (rbSQLServer.Checked)
        nodeName.InnerText = "SQLServer";
      else
        nodeName.InnerText = "MySQL";
      node.InnerText = connectionString;

      string ServerName = ParseServerHostName(tbServerHostName.Text);
      bool LocalServer = IsDatabaseOnLocalMachine(ServerName);
      TvLibrary.Log.Log.Info("---- SetupDatabaseForm: server = {0}, local = {1}", ServerName, Convert.ToString(LocalServer));
      CheckServiceName();

      doc.Save(fname);
    }

    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      if (tbServerHostName.Text.ToLower().IndexOf("localhost") >= 0)
      {
        MessageBox.Show(this, "Please specify the hostname or ip-address for the server. Not Localhost!");
        return;
      }
      if (tbServerHostName.Text.ToLower().IndexOf("127.0.0.1") >= 0)
      {
        MessageBox.Show(this, "Please specify the hostname or ip-address for the server. Not 127.0.0.1!");
        return;
      }
      Save();
      Close();
    }

    /// <summary>
    /// Gets the current schema version (-1= No database installed)
    /// </summary>
    /// <returns>the current schema version</returns>
    public int GetCurrentShemaVersion()
    {
      int currentSchemaVersion = -1;
      LoadConnectionDetailsFromConfig(false);
      try
      {
        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "TvLibrary", false);
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
                  cmd.CommandText = "select * from Version";
                  using (IDataReader reader = cmd.ExecuteReader())
                  {
                    if (reader.Read())
                    {
                      currentSchemaVersion = (int)reader["versionNumber"];
                      reader.Close();
                      connect.Close();
                    }
                  }
                }
              }
            }
            break;

          case ProviderType.MySql:
            {
              using (MySqlConnection connect = new MySqlConnection(connectionString))
              {
                connect.Open();
                using (MySqlCommand cmd = connect.CreateCommand())
                {
                  cmd.CommandType = CommandType.Text;
                  cmd.CommandText = "select * from Version";
                  using (IDataReader reader = cmd.ExecuteReader())
                  {
                    if (reader.Read())
                    {
                      currentSchemaVersion = (int)reader["versionNumber"];
                      reader.Close();
                      connect.Close();
                    }
                  }
                }
              }
            }
            break;
        }
        return currentSchemaVersion;
      }
      catch (Exception)
      {
        return -1;
      }
      finally
      {
        SqlConnection.ClearAllPools();
        GC.Collect();
      }
    }

    private bool ResourceExists(string[] names, string resource)
    {
      foreach (string name in names)
      {
        if (name == resource)
          return true;
      }
      return false;
    }
    /// <summary>
    /// Upgrades the db schema 
    /// </summary>
    /// <param name="currentSchemaVersion">the current schema version, the db has</param>
    /// <returns></returns>
    public bool UpgradeDBSchema(int currentSchemaVersion)
    {
      Assembly assm = Assembly.GetExecutingAssembly();
      string[] names = assm.GetManifestResourceNames();
      Stream stream = null;
      for (int version = currentSchemaVersion + 1 ; version < 100 ; version++)
      {
        if (ResourceExists(names, "SetupTv." + version.ToString() + "_upgrade_sqlserver_database.sql"))
        {
          if (ExecuteSQLScript(version.ToString() + "_upgrade"))
            Log.Info("- database upgraded to schema version " + version.ToString());
          else
            return false;
        }
        else
          break;
      }
      return true;
    }

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
      if (!IsDatabaseOnLocalMachine(ParseServerHostName(tbServerHostName.Text)))
      {
        tbServiceDependency.Enabled = false;
        return;
      }
      else
      {
        tbServiceDependency.Enabled = true;

        // first try the quick method and assume the user is right or using defaults
        string ConfiguredServiceName = tbServiceDependency.Text;
        string DBSearchPattern = @"MySQL";
        Color clAllOkay = Color.GreenYellow;

        if (ServiceHelper.IsInstalled(ConfiguredServiceName))
        {
          tbServiceDependency.BackColor = clAllOkay;
          DBSearchPattern = ConfiguredServiceName;
        }
        else
        {
          // MSSQL
          if (rbSQLServer.Checked)
            DBSearchPattern = @"SQLBrowser";

          if (ServiceHelper.GetDBServiceName(ref DBSearchPattern))
          {
            tbServiceDependency.Text = DBSearchPattern;
            tbServiceDependency.BackColor = clAllOkay;
          }
          else
          {
            TvLibrary.Log.Log.Info("SetupDatabaseForm: DB service name not recognized - using defaults");
            tbServiceDependency.BackColor = Color.Red;
          }
        }

        // if a matching service name is available - add it now
        if (tbServiceDependency.BackColor == clAllOkay && tbServiceDependency.Enabled)
        {
          if (ServiceHelper.AddDependencyByName(DBSearchPattern))
          {
            TvLibrary.Log.Log.Info("SetupDatabaseForm: Added dependency for TvService - {0}", DBSearchPattern);
            if (!ServiceHelper.IsServiceEnabled(DBSearchPattern, false))
            {
              if (MessageBox.Show(this,
                                  string.Format("The tv service depends on {0} but this service does not autostart - enable now?", DBSearchPattern),
                                  "Dependency avoids autostart",
                                  MessageBoxButtons.YesNo,
                                  MessageBoxIcon.Warning,
                                  MessageBoxDefaultButton.Button1) == DialogResult.Yes)
              {
                if (!ServiceHelper.IsServiceEnabled(DBSearchPattern, true))
                  MessageBox.Show("Failed to change the startup behaviour", "Dependency error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
            }
          }
          else
            TvLibrary.Log.Log.Info("SetupDatabaseForm: Could not add dependency for TvService - {0}", DBSearchPattern);
        }
      }
    }

    private void OnDBTypeSelected()
    {
      gbServerLocation.Enabled = true;
      gbDbLogon.Enabled = true;
      tbPassword.Focus();
    }

    private void radioButton2_CheckedChanged(object sender, EventArgs e)
    {
      if (rbMySQL.Checked)
      {        
        if (tbUserID.Text == "sa" || string.IsNullOrEmpty(tbUserID.Text))
        {
          OnDBTypeSelected();
          tbUserID.Text = "root";
          tbServerHostName.Text = Dns.GetHostName();
          tbServiceDependency.Enabled = true;
          tbServiceDependency.BackColor = tbServerHostName.BackColor;
          tbServiceDependency.Text = @"MySQL5";
        }
      }
    }

    private void radioButton1_CheckedChanged(object sender, EventArgs e)
    {
      if (rbSQLServer.Checked)
      {
        if (tbUserID.Text == "root" || string.IsNullOrEmpty(tbUserID.Text))
        {
          OnDBTypeSelected();
          tbUserID.Text = "sa";
          tbServerHostName.Text = Dns.GetHostName() + @"\SQLEXPRESS";
          tbServiceDependency.Enabled = true;
          tbServiceDependency.BackColor = tbServerHostName.BackColor;
          tbServiceDependency.Text = @"SQLBrowser";
        }
      }
    }

    private void lblDBChoice_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
      try
      {
        Process.Start("http://wiki.team-mediaportal.com/TV-Engine_0.3");
      }
      catch (Exception) {}
    }
  }
}
