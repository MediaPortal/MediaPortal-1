#region Copyright (C) 2005-2008 Team MediaPortal

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

#endregion

#region Usings

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
using MySql.Data.MySqlClient;

#endregion

namespace MPTvClient
{
  public partial class SetupDatabaseForm : Form
  {
    enum ProviderType
    {
      SqlServer,
      MySql,
    }

    ProviderType _provider = ProviderType.MySql;
    string _schemaName = "MpTvDbRC1";

    public SetupDatabaseForm()
    {
      InitializeComponent();
    }

    private void SetupDatabaseForm_Load(object sender, EventArgs e)
    {
      LoadConnectionDetailsFromConfig();
      SetInitialFocus();
    }

    private void SetInitialFocus()
    {
      if (gbDbLogon.Enabled)
        this.ActiveControl = tbPassword;
    }

    #region Settings

    private void LoadConnectionDetailsFromConfig()
    {
      //<DefaultProvider name="Firebird" connectionString="User=SYSDBA;Password=masterkey;Data Source=TvLibrary.fdb;ServerType=1;Dialect=3;Charset=UNICODE_FSS;Role=;Pooling=true;" />
      //<DefaultProvider name="SQLServer" connectionString="Password=sa;Persist Security Info=True;User ID=sa;Initial Catalog=TvLibrary;Data Source=pcebeckers;" />
      //<DefaultProvider name="MySQL" connectionString="Server=10.0.0.2;Database=TvLibrary;User ID=xxx;Password=xxx" />
      try
      {
        XmlDocument doc = new XmlDocument();
        string fname = String.Format(@"{0}\gentle.config", Application.StartupPath);
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

          if (keyValue[0].ToLower() == "initial catalog" || keyValue[0].ToLower() == "database")
          {
            tbDatabaseName.Text = keyValue[1];
            _schemaName = tbDatabaseName.Text;
          }

          if (keyValue[0].ToLower() == "data source" || keyValue[0].ToLower() == "server")
          {
            tbServerHostName.Text = keyValue[1];
          }
        }
        if (tbServerHostName.Text == "")
        {
          tbServerHostName.Text = ClientSettings.serverHostname;
          if (rbSQLServer.Checked)
            tbServerHostName.Text += "\\SQLEXPRESS";
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show(this, string.Format("gentle.config file not found! ({0})", ex.Message));
      }
    }

    private string ComposeConnectionString(string server, string userid, string password, string database, bool pooling, int timeout)
    {
      _schemaName = database;
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
      }
      return "";
    }

    public bool TestConnection()
    {
      try
      {
        LoadConnectionDetailsFromConfig();
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
          MessageBox.Show("Please specify a valid database user!", "Specify user", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        if (string.IsNullOrEmpty(tbPassword.Text))
        {
          tbPassword.BackColor = Color.Red;
          MessageBox.Show("Please specify a valid password for the database user!", "Specify password", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }
        if (string.IsNullOrEmpty(tbDatabaseName.Text) || tbDatabaseName.Text.ToLower() == "mysql" || tbDatabaseName.Text.ToLower() == "master")
        {
          tbDatabaseName.BackColor = Color.Red;
          MessageBox.Show("Please specify a valid schema name!", "Specify schema name", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        if (tbServerHostName.Text.ToLower().IndexOf("localhost") >= 0 || tbServerHostName.Text.ToLower().IndexOf("127.0.0.1") >= 0)
        {
          tbServerHostName.BackColor = Color.Red;
          MessageBox.Show("Please specify a valid hostname or IP address for the server!", "Specify server name", MessageBoxButtons.OK, MessageBoxIcon.Error);
          return;
        }

        string TestDb = tbDatabaseName.Text;
        bool TestSuccess = false;

        if (rbSQLServer.Checked)
          TestSuccess = AttemptMsSqlTestConnect(TestDb);
        else
          TestSuccess = AttemptMySqlTestConnect(TestDb);

        // Do not allow to "use" incorrect data
          btnSave.Enabled = TestSuccess;
      }
      finally
      {
        // Now the user can click again
        btnTest.Enabled = true;
      }
    }

    private bool AttemptMySqlTestConnect(string aTestDb)
    {
      _provider = ProviderType.MySql;
      string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, aTestDb, false, 5);

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
      _provider = ProviderType.SqlServer;
      string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, aTestDb, false, 5);

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
              MessageBox.Show(string.Format("Test failed: {0}", sqlex.Message), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
          }
          else
          {
            tbServerHostName.BackColor = Color.Red;
            MessageBox.Show(string.Format("Connection error: {0}", sqlex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

    private void SaveGentleConfig()
    {
      string connectionString = ComposeConnectionString(tbServerHostName.Text, tbUserID.Text, tbPassword.Text, tbDatabaseName.Text, true, 300);
      string fname = String.Format(@"{0}\gentle.config", Application.StartupPath);
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

      doc.Save(fname);
    }

    private void mpButtonSave_Click(object sender, EventArgs e)
    {
      SaveGentleConfig();

      this.Close();
    }


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
        }
        if (tbServerHostName.Text.EndsWith("\\SQLEXPRESS"))
        {
          OnDBTypeSelected();
          tbServerHostName.Text = tbServerHostName.Text.Remove(tbServerHostName.Text.IndexOf("\\SQLEXPRESS"), 11);
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
        }
        if (!tbServerHostName.Text.EndsWith("\\SQLEXPRESS"))
        {
          OnDBTypeSelected();
          tbServerHostName.Text += "\\SQLEXPRESS";
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

    private void tbServerHostName_TextChanged(object sender, EventArgs e)
    {
      if (tbServerHostName.BackColor == Color.Red)
        tbServerHostName.BackColor = SystemColors.Window;
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
