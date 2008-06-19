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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;

namespace MPTvClient
{
  public partial class frmConnectionTest : Form
  {
    private string hostname;
    private int countFailed;

    private bool CheckTcpPort(int port)
    {
      TcpClient client = new TcpClient();
      try
      {
        client.Connect(hostname, port);
      }
      catch (Exception)
      {
        return false;
      }
      client.Close();
      return true;
    }
    private bool CheckUdpPort(int port)
    {
      UdpClient client = new UdpClient();
      try
      {
        client.Connect(hostname, port);
      }
      catch (Exception)
      {
        return false;
      }
      client.Close();
      return true;
    }

    public frmConnectionTest()
    {
      InitializeComponent();
      ListViewItem row=listView1.Items.Add("31456");
      row.UseItemStyleForSubItems = false;
      row.SubItems.Add("TCP");
      row.SubItems.Add("TvService RemoteControl");
      row.SubItems.Add("unchecked");

      row = listView1.Items.Add("554");
      row.UseItemStyleForSubItems = false;
      row.SubItems.Add("TCP");
      row.SubItems.Add("RTSP streaming");
      row.SubItems.Add("unchecked");

      listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
    }
    public int GetFailedCount()
    {
      return countFailed;
    }
    public void RunChecks(string hostname,string provider)
    {
      this.hostname = hostname;
      if (provider.ToLower() == "sqlserver")
      {
        ListViewItem row = listView1.Items.Add("1433");
        row.UseItemStyleForSubItems = false;
        row.SubItems.Add("TCP");
        row.SubItems.Add("MS SQL (TCP)");
        row.SubItems.Add("unchecked");
        row = listView1.Items.Add("1434");
        row.UseItemStyleForSubItems = false;
        row.SubItems.Add("UDP");
        row.SubItems.Add("MS SQL (UDP)");
        row.SubItems.Add("unchecked");
      }
      else
      {
        ListViewItem row = listView1.Items.Add("3306");
        row.UseItemStyleForSubItems = false;
        row.SubItems.Add("TCP");
        row.SubItems.Add("MySQL");
        row.SubItems.Add("unchecked");
      }
      timer1.Enabled = true;
      countFailed = 0;
      this.ShowDialog();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
      timer1.Enabled = false;
      foreach (ListViewItem row in listView1.Items)
      {
        row.SubItems[3].ForeColor = System.Drawing.Color.Blue;
        row.SubItems[3].Text = "running...";
        listView1.Update();
        int port = int.Parse(row.Text);
        bool passed;
        if (row.SubItems[1].Text == "TCP")
          passed=CheckTcpPort(port);
        else
          passed=CheckUdpPort(port);
        if (passed)
        {
          row.SubItems[3].ForeColor = System.Drawing.Color.Green;
          row.SubItems[3].Text = "OK";
        }
        else
        {
          row.SubItems[3].ForeColor = System.Drawing.Color.Red;
          row.SubItems[3].Text = "FAILED";
          countFailed++;
        }
        listView1.Update();
      }
    }
  }

  public enum DBMSType
  {
    MSSQL,
    MySQL
  }
}