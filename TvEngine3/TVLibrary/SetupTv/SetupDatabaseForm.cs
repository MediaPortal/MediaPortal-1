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
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Reflection;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Xml;
using System.Net;
//using FirebirdSql.Data.Firebird;
using MySql.Data.MySqlClient;
using TvLibrary.Log;

namespace SetupTv
{
  public partial class SetupDatabaseForm : SetupControls.MPForm
  {
    enum ProviderType
    {
      SqlServer,
      MySql,
    }

    ProviderType _provider = ProviderType.MySql;

    public SetupDatabaseForm()
    {
      InitializeComponent();
    }

    private void LoadConnectionDetailsFromConfig(bool lookupMachineName)
    {
      //<DefaultProvider name="Firebird" connectionString="User=SYSDBA;Password=masterkey;Data Source=TvLibrary.fdb;ServerType=1;Dialect=3;Charset=UNICODE_FSS;Role=;Pooling=true;" />
      //<DefaultProvider name="SQLServer" connectionString="Password=sa;Persist Security Info=True;User ID=sa;Initial Catalog=TvLibrary;Data Source=pcebeckers;" />
      //<DefaultProvider name="MySQL" connectionString="Server=10.0.0.2;Database=TvLibrary;User ID=xxx;Password=xxx" />
      try
      {
        XmlDocument doc = new XmlDocument();
        string fname = String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        if (!File.Exists(fname))
        {
          try
          {
            File.Copy(Application.StartupPath + @"\gentle.config", fname, true);
          }
          catch (Exception exc)
          {
            MessageBox.Show(string.Format("Could not copy generic db config to {0} - {1}", fname, exc.Message));
            return;
          }
        }
        doc.Load(fname);
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode serverName = nodeKey.Attributes.GetNamedItem("name");
        XmlNode attributeConnectionString = nodeKey.Attributes.GetNamedItem("connectionString");
        string connectionString = attributeConnectionString.InnerText;
        string serverType = serverName.InnerText.ToLower();
        switch (serverType)
        {
          case "mysql":
            _provider = ProviderType.MySql;
            rbMySQL.Checked = true;
            break;
          case "sqlserver":
            _provider = ProviderType.SqlServer;
            rbSQLServer.Checked = true;
            break;
          default:
            return;
        }

        string[] parts = connectionString.Split(';');
        for (int i = 0 ; i < parts.Length ; ++i)
        {
          string part = parts[i];
          string[] keyValue = part.Split('=');
          if (keyValue[0].ToLower() == "password")
            tbPassword.Text = keyValue[1];

          if (keyValue[0].ToLower() == "user id" || keyValue[0].ToLower() == "user")
            tbUserID.Text = keyValue[1];

          if (keyValue[0].ToLower() == "data source" || keyValue[0].ToLower() == "server")
          {
            if (keyValue[1].Length == 0 || keyValue[1] == "-")
            {
              if (lookupMachineName)
              {
                switch (_provider)
                {
                  case ProviderType.SqlServer:
                    tbServerHostName.Text = keyValue[1] = Dns.GetHostName() + @"\SQLEXPRESS";
                    break;
                  case ProviderType.MySql:
                    tbServerHostName.Text = keyValue[1] = Dns.GetHostName();
                    break;
                }
              }
            }
            else
            {
              tbServerHostName.Text = keyValue[1];
            }
          }
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, string.Format("gentle.config file not found! ({0})", ex.Message));
      }
    }

    private string ComposeConnectionString(string server, string userid, string password, string database, bool pooling, int timeout)
    {
      switch (_provider)
      {
        case ProviderType.SqlServer:
          if (database == "") database = "master";
          if (pooling)
            return String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog={3};Data Source={2};Connection Timeout={4};", password, userid, server, database, timeout);
          return String.Format("Password={0};Persist Security Info=True;User ID={1};Initial Catalog={3};Data Source={2};Pooling=false;Connection Timeout={4};", password, userid, server, database, timeout);

        case ProviderType.MySql:
          if (database == "") database = "mysql";
          return String.Format("Server={0};Database={3};User ID={1};Password={2};charset=utf8;Connection Timeout={4};", server, userid, password, database, timeout);

        //case ProviderType.FbEmbedded:
        //  if (string.IsNullOrEmpty(database)) database = "TvLibrary.fdb";
        //  if (pooling)
        //    return String.Format("Database={0};User={1};Password={2};ServerType=1;Charset=UNICODE_FSS;", database, userid, password);
        //  return String.Format("Database={0};User={1};Password={2};ServerType=1;Charset=UNICODE_FSS;Pooling=false;", database, userid, password);
      }
      return "";
    }

    private void SetupDatabaseForm_Load(object sender, EventArgs e)
    {
      LoadConnectionDetailsFromConfig(true);
    }

    public bool TestConnection()
    {
      try
      {
        LoadConnectionDetailsFromConfig(true);
        if (string.IsNullOrEmpty(tbServerHostName.Text) || string.IsNullOrEmpty(tbPassword.Text))
          return false;

        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "", false, 15);

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
          default:
            throw (new Exception("Unsupported provider!"));
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

        string sql = string.Empty;
        using (StreamReader reader = new StreamReader(stream))
          sql = reader.ReadToEnd();

        string[] CommandScript = null;
        switch (_provider)
        {
          case ProviderType.SqlServer:
            CommandScript = CleanMsSqlStatement(sql);
            break;

          case ProviderType.MySql:
            CommandScript = CleanMySqlStatement(sql);
            break;
        }

        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "", true, 300);
        switch (_provider)
        {
          case ProviderType.SqlServer:
            using (SqlConnection connect = new SqlConnection(connectionString))
            {
              connect.Open();
              foreach (string SingleStmt in CommandScript)
              {
                string SqlStmt = SingleStmt.Trim();
                if (!string.IsNullOrEmpty(SqlStmt) && !SqlStmt.StartsWith("--") && !SqlStmt.StartsWith("/*"))
                {
                  try
                  {
                    using (SqlCommand cmd = new SqlCommand(SqlStmt, connect))
                    {
                      Log.Write("  Exec SQL: {0}", SqlStmt);
                      cmd.ExecuteNonQuery();
                    }
                  }
                  catch (SqlException ex)
                  {
                    Log.Write("  ********* SQL statement failed! *********");
                    Log.Write("  ********* Error reason: {0}", ex.Message);
                    Log.Write("  ********* Error code: {0}, Line: {1} *********", ex.Number.ToString(), ex.LineNumber.ToString());
                    succeeded = false;
                    if (connect.State != ConnectionState.Open)
                    {
                      Log.Write("  ********* Connection status = {0} - aborting further command execution..", connect.State.ToString());
                      break;
                    }
                  }
                }
              }
            }
            break;
          case ProviderType.MySql:
            using (MySqlConnection connect = new MySqlConnection(connectionString))
            {
              connect.Open();
              foreach (string SingleStmt in CommandScript)
              {
                string SqlStmt = SingleStmt.Trim();
                if (!string.IsNullOrEmpty(SqlStmt) && !SqlStmt.StartsWith("--") && !SqlStmt.StartsWith("/*"))
                {
                  try
                  {
                    using (MySqlCommand cmd = new MySqlCommand(SqlStmt, connect))
                    {
                      Log.Write("  Exec SQL: {0}", SqlStmt);
                      cmd.ExecuteNonQuery();
                    }
                  }
                  catch (MySqlException ex)
                  {
                    Log.Write("  ********* SQL statement failed! *********");
                    Log.Write("  ********* Error reason: {0}", ex.Message);
                    Log.Write("  ********* Error code: {0} *********", ex.Number.ToString());
                    succeeded = false;
                    if (connect.State != ConnectionState.Open)
                    {
                      Log.Write("  ********* Connection status = {0} - aborting further command execution..", connect.State.ToString());
                      break;
                    }
                  }
                }
              }
            }
            break;
        }
      }
      catch (Exception gex)
      {
        MessageBox.Show(this, "Unable to " + prefix + " database:" + gex.Message);
        succeeded = false;
      }
      SqlConnection.ClearAllPools();
      return succeeded;
    }

    private string[] CleanMsSqlStatement(string sql)
    {
      string currentDir = System.IO.Directory.GetCurrentDirectory();
      currentDir += @"\";
      sql = sql.Replace(@"C:\Program Files\Microsoft SQL Server\MSSQL\data\", currentDir);
      sql = sql.Replace("GO\r\n", "!");
      sql = sql.Replace("\r\n", " ");
      sql = sql.Replace("\t", " ");
      return sql.Split('!');
    }

    private string[] CleanMySqlStatement(string sql)
    {
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
      return sql.Split('#');
    }

    private void mpButtonTest_Click(object sender, EventArgs e)
    {
      CheckServiceName();
      
      if (rbSQLServer.Checked)
      {
        _provider = ProviderType.SqlServer;
        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "", false, 5);

        try
        {          
          using (SqlConnection connect = new SqlConnection(connectionString))
          {
            connect.Open();
          }
        }
        catch (SqlException sqlex)
        {
          if (sqlex.Class > 10)
          {
            if (sqlex.Class < 20 || sqlex.Number == 233)
            {
              if (sqlex.Number == 18456 || sqlex.Number == 233)
              {
                tbServerHostName.BackColor = Color.GreenYellow;
                tbUserID.BackColor = Color.Red;
                tbPassword.BackColor = Color.Red;
              }
              else
              {
                tbServerHostName.BackColor = Color.Yellow;
                MessageBox.Show(string.Format("Test failed: {0}", sqlex.Message), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
              }
            }
            else
            {
              tbServerHostName.BackColor = Color.Red;
              MessageBox.Show(string.Format("Connection error: {0}", sqlex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
          }
          return;
        }
        catch (Exception ex)
        {
          tbServerHostName.BackColor = Color.Red;
          MessageBox.Show(this, "Connection failed!" + ex.Message);
          return;
        }
        SqlConnection.ClearAllPools();
        tbServerHostName.BackColor = Color.GreenYellow;
        tbUserID.BackColor = Color.GreenYellow;
        tbPassword.BackColor = Color.GreenYellow;
        MessageBox.Show(this, "Connection succeeded!");
      }
      else
      {
        _provider = ProviderType.MySql;
        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "", false, 5);

        try
        {
          using (MySqlConnection connect = new MySqlConnection(connectionString))
          {
            connect.Open();
            connect.Close();
          }
        }
        catch (Exception ex)
        {
          tbServerHostName.BackColor = Color.Red;
          MessageBox.Show(this, "Connection failed!" + ex.Message);
          return;
        }
        tbServerHostName.BackColor = Color.GreenYellow;
        tbUserID.BackColor = Color.GreenYellow;
        tbPassword.BackColor = Color.GreenYellow;
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
      string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "TvLibrary", true, 300);
      string fname = String.Format(@"{0}\MediaPortal TV Server\gentle.config", Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
      if (!File.Exists(fname))
      {
        try
        {
          File.Copy(Application.StartupPath + @"\gentle.config", fname, true);
        }
        catch (Exception exc)
        {
          MessageBox.Show(string.Format("Could not copy generic db config to {0} - {1}", fname, exc.Message));
          return;
        }
      }
      XmlDocument doc = new XmlDocument();
      try
      {
        doc.Load(fname);
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not load generic gentle config to insert matching connection string: {0}", ex.Message));
        return;
      }

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
      Application.Restart();
      //Close();
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
        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "TvLibrary", false, 15);
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
      //Stream stream = null;
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
                // enable the dependency now
                if (!ServiceHelper.IsServiceEnabled(DBSearchPattern, true))
                  MessageBox.Show("Failed to change the startup behaviour", "Dependency error", MessageBoxButtons.OK, MessageBoxIcon.Error);
              }
              // start the service right now
              if (!ServiceHelper.Start(DBSearchPattern))
                MessageBox.Show(string.Format("Failed to start the dependency service: {0}", DBSearchPattern), "Dependency start error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
      catch (Exception) { }
    }

    private void tbPassword_KeyUp(object sender, KeyEventArgs e)
    {
      if (e.KeyCode == Keys.Enter)
        mpButtonTest_Click(sender, null);
    }

    private void pbSQLServer_Click(object sender, EventArgs e)
    {
      rbSQLServer.Checked = true;
    }

    private void pbMySQL_Click(object sender, EventArgs e)
    {
      rbMySQL.Checked = true;
    }
  }
}
