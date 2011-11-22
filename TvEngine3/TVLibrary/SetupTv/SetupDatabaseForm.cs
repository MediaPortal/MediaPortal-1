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

#region Usings

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using MySql.Data.MySqlClient;
using TvLibrary.Interfaces;
using TvLibrary.Log;

#endregion

namespace SetupTv
{
  public partial class SetupDatabaseForm : SetupControls.MPForm
  {
    public enum ProviderType
    {
      SqlServer,
      MySql,
    }

    private readonly StartupMode _dialogMode = StartupMode.Normal;
    private ProviderType _provider = ProviderType.MySql;
    private static string _schemaNameDefault = "MpTvDb";
    private static string _schemaName = _schemaNameDefault;

    public SetupDatabaseForm(StartupMode aStartMode)
    {
      InitializeComponent();
      _dialogMode = aStartMode;

      if (aStartMode == StartupMode.DbCleanup)
      {
        btnSave.Visible = false;
        btnDrop.Visible = true;
      }
      // A user might want to save a "wrong" database name so TV-Server creates a new DB.
      if (aStartMode == StartupMode.DbConfig)
        btnSave.Enabled = true;
    }

    public string schemaName
    {
      get { return _schemaName; }
      set { _schemaName = value; }
    }

    public string schemaNameDefault
    {
      get { return _schemaNameDefault; }
      set { _schemaNameDefault = value; }
    }

    public ProviderType provider
    {
      get { return _provider; }
      set { _provider = value; }
    }

    private void SetupDatabaseForm_Load(object sender, EventArgs e)
    {
      LoadConnectionDetailsFromConfig(true);
      SetInitialFocus();
    }

    private void SetInitialFocus()
    {
      switch (_dialogMode)
      {
        case StartupMode.Normal:
          if (gbDbLogon.Enabled)
            ActiveControl = tbPassword;
          break;
        case StartupMode.DbCleanup:
          ActiveControl = btnTest;
          break;
        case StartupMode.DbConfig:
          if (gbServerLocation.Enabled)
            ActiveControl = tbDatabaseName;
          break;
      }
    }

    #region Settings

    public void LoadConnectionDetailsFromConfig(bool lookupMachineName)
    {
      //<DefaultProvider name="Firebird" connectionString="User=SYSDBA;Password=masterkey;Data Source=TvLibrary.fdb;ServerType=1;Dialect=3;Charset=UNICODE_FSS;Role=;Pooling=true;" />
      //<DefaultProvider name="SQLServer" connectionString="Password=sa;Persist Security Info=True;User ID=sa;Initial Catalog=TvLibrary;Data Source=pcebeckers;" />
      //<DefaultProvider name="MySQL" connectionString="Server=10.0.0.2;Database=TvLibrary;User ID=xxx;Password=xxx" />
      try
      {
        XmlDocument doc = new XmlDocument();
        doc.Load(String.Format(@"{0}\gentle.config", PathManager.GetDataPath));
        XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
        XmlNode serverName = nodeKey.Attributes.GetNamedItem("name");
        XmlNode attributeConnectionString = nodeKey.Attributes.GetNamedItem("connectionString");
        string connectionString = attributeConnectionString.InnerText;
        string serverType = serverName.InnerText.ToLowerInvariant();
        switch (serverType)
        {
          case "mysql":
            provider = ProviderType.MySql;
            rbMySQL.Checked = true;
            break;
          case "sqlserver":
            provider = ProviderType.SqlServer;
            rbSQLServer.Checked = true;
            break;
          default:
            return;
        }

        string[] parts = connectionString.Split(';');
        for (int i = 0; i < parts.Length; ++i)
        {
          string part = parts[i];
          string[] keyValue = part.Split('=');
          if (keyValue[0].ToLowerInvariant() == "password")
            tbPassword.Text = keyValue[1];

          if (keyValue[0].ToLowerInvariant() == "user id" || keyValue[0].ToLowerInvariant() == "user")
            tbUserID.Text = keyValue[1];

          if (keyValue[0].ToLowerInvariant() == "initial catalog" || keyValue[0].ToLowerInvariant() == "database")
          {
            tbDatabaseName.Text = keyValue[1];
            schemaName = tbDatabaseName.Text;
          }

          if (keyValue[0].ToLowerInvariant() == "data source" || keyValue[0].ToLowerInvariant() == "server")
          {
            if (keyValue[1].Length == 0 || keyValue[1] == "-")
            {
              if (lookupMachineName)
              {
                switch (provider)
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

    private string ComposeConnectionString(string server, string userid, string password, string database, bool pooling,
                                           int timeout)
    {
      schemaName = database;
      switch (provider)
      {
        case ProviderType.SqlServer:
          if (database == "") database = "master";
          if (pooling)
            return
              String.Format(
                "Password={0};Persist Security Info=True;User ID={1};Initial Catalog={3};Data Source={2};Connection Timeout={4};",
                password, userid, server, database, timeout);
          return
            String.Format(
              "Password={0};Persist Security Info=True;User ID={1};Initial Catalog={3};Data Source={2};Pooling=false;Connection Timeout={4};",
              password, userid, server, database, timeout);

        case ProviderType.MySql:
          if (database == "") database = "mysql";
          return String.Format("Server={0};Database={3};User ID={1};Password={2};charset=utf8;Connection Timeout={4};",
                               server, userid, password, database, timeout);
      }
      return "";
    }

    public bool TestConnection(StartupMode ModeType)
    {
      try
      {
        if (ModeType != StartupMode.DeployMode)
        {
          LoadConnectionDetailsFromConfig(true);
        }

        if (string.IsNullOrEmpty(tbServerHostName.Text) || string.IsNullOrEmpty(tbPassword.Text))
          return false;

        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "",
                                                          false, 15);

        switch (provider)
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
        return false;
      }

      SqlConnection.ClearAllPools();

      //database server is found
      return true;
    }

    #endregion

    #region SQL methods

    public bool ExecuteSQLScript(string prefix)
    {
      bool succeeded = true;
      try
      {
        Assembly assm = Assembly.GetExecutingAssembly();
        Stream stream = null;
        switch (provider)
        {
          case ProviderType.SqlServer:
            stream = assm.GetManifestResourceStream("SetupTv." + prefix + "_sqlserver_database.sql");
            break;
          case ProviderType.MySql:
            stream = assm.GetManifestResourceStream("SetupTv." + prefix + "_mysql_database.sql");
            break;
        }

        schemaName = tbDatabaseName.Text;
        string[] CommandScript = null;
        string sql = string.Empty;
        if (stream != null)
          using (StreamReader reader = new StreamReader(stream))
            sql = reader.ReadToEnd();

        switch (provider)
        {
          case ProviderType.SqlServer:
            CommandScript = CleanMsSqlStatement(sql);
            break;

          case ProviderType.MySql:
            CommandScript = CleanMySqlStatement(sql);
            break;
        }

        // As the connection string sets the DB schema name we need to compose it after cleaning the statement.
        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, "",
                                                          true, 30);
        switch (provider)
        {
          case ProviderType.SqlServer:
            using (SqlConnection connect = new SqlConnection(connectionString))
            {
              connect.Open();
              if (CommandScript != null)
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
                      Log.Write("  ********* Error code: {0}, Line: {1} *********", ex.Number.ToString(),
                                ex.LineNumber.ToString());
                      succeeded = false;
                      if (connect.State != ConnectionState.Open)
                      {
                        Log.Write("  ********* Connection status = {0} - aborting further command execution..",
                                  connect.State.ToString());
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
              if (CommandScript != null)
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
                        Log.Write("  ********* Connection status = {0} - aborting further command execution..",
                                  connect.State.ToString());
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
      string currentDir = Directory.GetCurrentDirectory();
      currentDir += @"\";
      sql = sql.Replace(@"%TvLibrary%", schemaName);
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
      sql = sql.Replace('"', '`'); // allow usage of ANSI quoted identifiers
      sql = sql.Replace(@"%TvLibrary%", schemaName);
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
      return sql.Split('#');
    }

    #endregion

    #region Connection test

    private void mpButtonTest_Click(object sender, EventArgs e)
    {
      btnTest.Enabled = false;
      try
      {
        if (string.IsNullOrEmpty(tbUserID.Text))
        {
          tbUserID.BackColor = Color.Red;
          MessageBox.Show("Please specify a valid database user!", "Specify user", MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
          return;
        }
        if (string.IsNullOrEmpty(tbPassword.Text))
        {
          tbPassword.BackColor = Color.Red;
          MessageBox.Show("Please specify a valid password for the database user!", "Specify password",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        if (string.IsNullOrEmpty(tbDatabaseName.Text) || tbDatabaseName.Text.ToLowerInvariant() == "mysql" ||
            tbDatabaseName.Text.ToLowerInvariant() == "master")
        {
          tbDatabaseName.BackColor = Color.Red;
          MessageBox.Show("Please specify a valid schema name!", "Specify schema name", MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
          return;
        }

        if (tbServerHostName.Text.ToLowerInvariant().IndexOf("localhost") >= 0 ||
            tbServerHostName.Text.ToLowerInvariant().IndexOf("127.0.0.1") >= 0)
        {
          tbServerHostName.BackColor = Color.Red;
          MessageBox.Show("Please specify a valid hostname or IP address for the server!", "Specify server name",
                          MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        CheckServiceName();

        //string TestDb = _dialogMode == StartupMode.Normal ? string.Empty : tbDatabaseName.Text;
        string TestDb = string.Empty; // don't check if DB exists, it will be created if needed

        bool TestSuccess = rbSQLServer.Checked ? AttemptMsSqlTestConnect(TestDb) : AttemptMySqlTestConnect(TestDb);

        // Do not allow to "use" incorrect data
        if (_dialogMode != StartupMode.DbConfig)
          btnSave.Enabled = btnDrop.Enabled = TestSuccess;
      }
      finally
      {
        // Now the user can click again
        btnTest.Enabled = true;
      }
    }

    private bool AttemptMySqlTestConnect(string aTestDb)
    {
      provider = ProviderType.MySql;
      string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, aTestDb,
                                                        false, 5);

      try
      {
        using (MySqlConnection connect = new MySqlConnection(connectionString))
        {
          connect.Open();
          connect.Close();
        }
      }
      catch (MySqlException myex)
      {
        if (myex.Number == 1049) //unknown database
          tbDatabaseName.BackColor = Color.Red;
        else
          tbServerHostName.BackColor = Color.Red;
        MessageBox.Show(this, "Connection failed!\n" + myex.Message);
        return false;
      }
      catch (Exception ex)
      {
        tbServerHostName.BackColor = Color.Red;
        MessageBox.Show(this, "Connection failed!\n" + ex.Message);
        return false;
      }
      tbServerHostName.BackColor = Color.GreenYellow;
      tbUserID.BackColor = Color.GreenYellow;
      tbPassword.BackColor = Color.GreenYellow;
      tbDatabaseName.BackColor = Color.GreenYellow;
      MessageBox.Show(this, "Connection succeeded!");
      return true;
    }

    private bool AttemptMsSqlTestConnect(string aTestDb)
    {
      provider = ProviderType.SqlServer;
      string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, aTestDb,
                                                        false, 5);

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
            if (sqlex.Number == 18456 || sqlex.Number == 233) // Wrong login
            {
              tbServerHostName.BackColor = Color.GreenYellow;
              tbDatabaseName.BackColor = Color.GreenYellow;
              tbUserID.BackColor = Color.Red;
              tbPassword.BackColor = Color.Red;
            }
            else if (sqlex.Number == 4060) // Cannot open database "TvLibrary" requested by the login
            {
              tbDatabaseName.BackColor = Color.Red;
            }
            else
            {
              tbServerHostName.BackColor = Color.Yellow;
              MessageBox.Show(string.Format("Test failed: {0}", sqlex.Message), "Warning", MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
            }
          }
          else
          {
            tbServerHostName.BackColor = Color.Red;
            MessageBox.Show(string.Format("Connection error: {0}", sqlex.Message), "Error", MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
          }
        }
        MessageBox.Show(this, "Connection failed!\n" + sqlex.Message);
        return false;
      }
      catch (Exception ex)
      {
        tbServerHostName.BackColor = Color.Red;
        MessageBox.Show(this, "Connection failed!\n" + ex.Message);
        return false;
      }
      SqlConnection.ClearAllPools();
      tbServerHostName.BackColor = Color.GreenYellow;
      tbUserID.BackColor = Color.GreenYellow;
      tbPassword.BackColor = Color.GreenYellow;
      tbDatabaseName.BackColor = Color.GreenYellow;
      MessageBox.Show(this, "Connection succeeded!");
      return true;
    }

    #endregion

    /// <summary>
    /// Gets the server name from the config field (strips MSSQL instance name)
    /// </summary>
    /// <param name="ServerConfigText">The server config value from the connection string</param>
    /// <returns>Hostname of Server</returns>
    private static string ParseServerHostName(string ServerConfigText)
    {
      int delimiterPos = ServerConfigText.IndexOf(@"\");
      string ServerName = delimiterPos > 0 ? ServerConfigText.Remove(delimiterPos) : ServerConfigText;

      return ServerName;
    }

    public void SaveGentleConfig()
    {
      string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text,
                                                        tbDatabaseName.Text, true, 30);
      XmlDocument doc = new XmlDocument();
      try
      {
        doc.Load(String.Format(@"{0}\gentle.config", PathManager.GetDataPath));
      }
      catch (Exception ex)
      {
        MessageBox.Show(string.Format("Could not load generic gentle config to insert matching connection string: {0}",
                                      ex.Message));
        return;
      }

      XmlNode nodeKey = doc.SelectSingleNode("/Gentle.Framework/DefaultProvider");
      XmlNode node = nodeKey.Attributes.GetNamedItem("connectionString");
      XmlNode nodeName = nodeKey.Attributes.GetNamedItem("name");
      nodeName.InnerText = rbSQLServer.Checked ? "SQLServer" : "MySQL";
      node.InnerText = connectionString;

      string ServerName = ParseServerHostName(tbServerHostName.Text);
      bool LocalServer = IsDatabaseOnLocalMachine(ServerName);
      Log.Info("---- SetupDatabaseForm: server = {0}, local = {1}", ServerName, Convert.ToString(LocalServer));

      doc.Save(String.Format(@"{0}\gentle.config", PathManager.GetDataPath));
    }

    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      SaveGentleConfig();

      DialogResult = DialogResult.OK;

      if (_dialogMode == StartupMode.Normal)
        Application.Restart();
      else
        Close();
    }

    private void btnDrop_Click(object sender, EventArgs e)
    {
      if (ExecuteSQLScript("delete"))
        MessageBox.Show("Your old database has been dropped.", "Success", MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
      else
        MessageBox.Show("Failed to drop the database.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

      Close();
    }

    #region Schema update methods

    /// <summary>
    /// Gets the current schema version (-1= No database installed)
    /// </summary>
    /// <returns>the current schema version</returns>
    public int GetCurrentShemaVersion(StartupMode ModeType)
    {
      int currentSchemaVersion = -1;
      if (ModeType != StartupMode.DeployMode)
      {
        LoadConnectionDetailsFromConfig(false);
      }
      try
      {
        string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text,
                                                          tbDatabaseName.Text, false, 15);
        switch (provider)
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
                    if (reader != null)
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
      }
    }

    private static bool ResourceExists(IEnumerable<string> names, string resource)
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
      for (int version = currentSchemaVersion + 1; version < 200; version++)
      {
        if (ResourceExists(names, "SetupTv." + version + "_upgrade_sqlserver_database.sql") && ResourceExists(names, "SetupTv." + version + "_upgrade_mysql_database.sql"))
        {
          if (ExecuteSQLScript(version + "_upgrade"))
            Log.Info("- database upgraded to schema version " + version);
          else
            return false;
        }
        else
          break;
      }
      return true;
    }

    #endregion

    #region Service check methods

    public bool IsDatabaseOnLocalMachine(string DBServerName)
    {
      // please add better check if needed
      if (DBServerName.ToLowerInvariant() == Environment.MachineName.ToLowerInvariant())
        return true;
      // Check for ip addresses
      IPHostEntry ipEntry = Dns.GetHostEntry(Environment.MachineName);
      IPAddress[] addr = ipEntry.AddressList;

      for (int i = 0; i < addr.Length; i++)
      {
        if (addr[i].ToString().Equals(DBServerName))
        {
          return true;
        }
      }

      return false;
    }

    public void CheckServiceName()
    {
      // only query service names of local machine
      if (!IsDatabaseOnLocalMachine(ParseServerHostName(tbServerHostName.Text)))
      {
        tbServiceDependency.Enabled = false;
        return;
      }
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
          Log.Info("SetupDatabaseForm: DB service name not recognized - using defaults");
          tbServiceDependency.BackColor = Color.Red;
        }
      }

      // if a matching service name is available - add it now
      if (tbServiceDependency.BackColor == clAllOkay && tbServiceDependency.Enabled)
      {
        if (ServiceHelper.AddDependencyByName(DBSearchPattern))
        {
          Log.Info("SetupDatabaseForm: Added dependency for TvService - {0}", DBSearchPattern);
          if (!ServiceHelper.IsServiceEnabled(DBSearchPattern, false))
          {
            if (MessageBox.Show(this,
                                string.Format(
                                  "The tv service depends on {0} but this service does not autostart - enable now?",
                                  DBSearchPattern),
                                "Dependency avoids autostart",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning,
                                MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
              // enable the dependency now
              if (!ServiceHelper.IsServiceEnabled(DBSearchPattern, true))
                MessageBox.Show("Failed to change the startup behaviour", "Dependency error", MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            // start the service right now
            if (!ServiceHelper.Start(DBSearchPattern))
              MessageBox.Show(string.Format("Failed to start the dependency service: {0}", DBSearchPattern),
                              "Dependency start error", MessageBoxButtons.OK, MessageBoxIcon.Error);
          }
        }
        else
          Log.Info("SetupDatabaseForm: Could not add dependency for TvService - {0}", DBSearchPattern);
      }
    }

    #endregion

    #region Control events

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

    private void tbServerHostName_TextChanged(object sender, EventArgs e)
    {
      if (tbServerHostName.BackColor == Color.Red)
        tbServerHostName.BackColor = SystemColors.Window;
    }

    private void tbServiceDependency_TextChanged(object sender, EventArgs e)
    {
      if (tbServiceDependency.BackColor == Color.Red)
        tbServiceDependency.BackColor = SystemColors.Window;
    }

    private void tbUserID_TextChanged(object sender, EventArgs e)
    {
      if (tbUserID.BackColor == Color.Red)
        tbUserID.BackColor = SystemColors.Window;
    }

    private void tbPassword_TextChanged(object sender, EventArgs e)
    {
      if (tbPassword.BackColor == Color.Red)
        tbPassword.BackColor = SystemColors.Window;
    }

    private void tbDatabaseName_TextChanged(object sender, EventArgs e)
    {
      if (tbDatabaseName.BackColor == Color.Red)
        tbDatabaseName.BackColor = SystemColors.Window;
    }

    #endregion
  }
}