#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
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

using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using MediaPortal.GUI.Library;
using Microsoft.Win32;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  public class XplMsg
  {
    private bool bValid;
    private bool DoDebug = false;
    private IPAddress pBroadcastAddress;
    private const int XPL_BASE_PORT = 0xf19;
    public List<structXplSection> XPL_Msg;
    private string XPL_Raw;

    public XplMsg()
    {
      this.bValid = false;
      try
      {
        this.XPL_Raw = string.Empty;
        this.XPL_Msg = new List<structXplSection>();
        this.XPL_Msg.Clear();
      }
      catch
      {
        Log.Info("xPL.XplMsg()[constructor]: caught EXCEPTION");
      }
    }

    public XplMsg(string XPLMsg)
    {
      if (this.DoDebug)
      {
        Log.Info("xPL.XplMsg([constructor]): called");
      }
      try
      {
        this.bValid = false;
        this.XPL_Raw = XPLMsg;
        this.XPL_Msg = new List<structXplSection>();
        this.XPL_Msg.Clear();
        if (XPLMsg.Length > 0)
        {
          this.ExtractMsg();
        }
      }
      catch
      {
        Log.Info("xPL.XplMsg([constructor]): caught EXCEPTION");
      }
      if (this.DoDebug)
      {
        Log.Info("xPL.XplMsg([constructor]): completed");
      }
    }

    private void ExtractMsg()
    {
      string newKeyName = string.Empty;
      string newKeyValue = string.Empty;
      if (this.DoDebug)
      {
        Log.Info("xPL.XplMsg.ExtractMsg(): parsing received data");
      }
      try
      {
        int num2;
        string str3 = this.XPL_Raw;
        this.bValid = true;
        Label_0032:
        num2 = str3.IndexOf('\n' + "{");
        if (this.DoDebug)
        {
          Log.Info("xPL.XplMsg.ExtractMsg(): next part - index = {0}", new object[] {this.XPL_Msg.Count});
        }
        if (num2 == -1)
        {
          if (this.XPL_Msg.Count == 0)
          {
            if (this.DoDebug)
            {
              Log.Info("xPL.XplMsg.ExtractMsg(): received data is not valid");
            }
            this.bValid = false;
          }
          else if (this.DoDebug)
          {
            Log.Info("xPL.XplMsg.ExtractMsg(): received message data is valid");
          }
        }
        else
        {
          this.XPL_Msg.Add(new structXplSection(str3.Substring(0, num2).ToUpper().Trim()));
          if (this.XPL_Msg.Count == 1)
          {
            string str4;
            if (((str4 = this.XPL_Msg[this.XPL_Msg.Count - 1].Section) == null) ||
                (((str4 != "XPL-CMND") && (str4 != "XPL-STAT")) && (str4 != "XPL-TRIG")))
            {
              if (this.DoDebug)
              {
                Log.Info("xPL.XplMsg.ExtractMsg(): message header is not valid");
              }
              this.bValid = false;
              return;
            }
            if (this.DoDebug)
            {
              Log.Info("xPL.XplMsg.ExtractMsg(): message header is valid");
            }
          }
          str3 = str3.Substring(num2 + 3);
          while (true)
          {
            int index = str3.IndexOf("=");
            int num3 = str3.IndexOf("!");
            if ((num3 != -1) & (num3 < index))
            {
              index = num3;
            }
            newKeyName = str3.Substring(0, index).Trim().ToUpper();
            str3 = str3.Substring(index + 1);
            index = str3.IndexOf('\n'.ToString());
            newKeyValue = str3.Substring(0, index);
            this.XPL_Msg[this.XPL_Msg.Count - 1].Details.Add(new structXPLMsg(newKeyName, newKeyValue));
            str3 = str3.Substring(index - 1);
            if (str3.IndexOf('\n' + "}") == 1)
            {
              str3 = str3.Substring(4);
              goto Label_0032;
            }
            str3 = str3.Substring(2);
          }
        }
      }
      catch
      {
        if (this.DoDebug)
        {
          Log.Info("xPL.XplMsg.ExtractMsg(): caught EXCEPTION while parsing received data");
        }
        this.bValid = false;
      }
    }

    public string GetParam(int BodyPart, string strName)
    {
      int num = 0;
      if (!((BodyPart < 0) | (BodyPart > (this.XPL_Msg.Count - 1))))
      {
        while (num < this.XPL_Msg[BodyPart].Details.Count)
        {
          structXPLMsg msg = this.XPL_Msg[BodyPart].Details[num];
          if (msg.keyName.ToLower() == strName.ToLower())
          {
            return msg.Value;
          }
          num++;
        }
        return string.Empty;
      }
      return "!InvalidBodyPart";
    }

    public void Send()
    {
      IPEndPoint ep = new IPEndPoint(this.BroadcastAddress, 0xf19);
      this.Send(ep);
    }

    public void Send(IPEndPoint ep)
    {
      Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
      string ipString = XplListener.sListenOnIP();
      if (!ipString.Equals("ANY_LOCAL"))
      {
        IPEndPoint localEP = new IPEndPoint(IPAddress.Parse(ipString), 0);
        socket.Bind(localEP);
      }
      socket.SendTo(Encoding.ASCII.GetBytes(this.XPL_Raw), ep);
      socket.Close();
    }

    public void Send(string s)
    {
      this.XPL_Raw = s;
      IPEndPoint ep = new IPEndPoint(this.BroadcastAddress, 0xf19);
      this.Send(ep);
    }

    public int Bodies
    {
      get { return (this.XPL_Msg.Count - 1); }
    }

    private IPAddress BroadcastAddress
    {
      get
      {
        if (this.pBroadcastAddress == null)
        {
          RegistryKey key = null;
          try
          {
            key = Registry.LocalMachine.OpenSubKey(@"Software\xPL");
            this.pBroadcastAddress = IPAddress.Parse((string)key.GetValue("BroadcastAddress", "255.255.255.255"));
          }
          catch
          {
            this.pBroadcastAddress = IPAddress.Broadcast;
          }
          if (key != null)
          {
            key.Close();
          }
        }
        return this.pBroadcastAddress;
      }
    }

    public string Content
    {
      get { return this.XPL_Raw; }
    }

    public bool IsMessageValid
    {
      get { return this.bValid; }
    }

    public XplSchema Schema
    {
      get
      {
        XplSchema schema;
        schema.msgClass = this.XPL_Msg[1].Section.ToLower();
        schema.msgType = schema.msgClass.Substring(schema.msgClass.IndexOf(".") + 1,
                                                   (schema.msgClass.Length - schema.msgClass.IndexOf(".")) - 1);
        schema.msgClass = schema.msgClass.Substring(0, schema.msgClass.IndexOf("."));
        return schema;
      }
    }

    public XplSource Source
    {
      get
      {
        XplSource source;
        string str = this.GetParam(0, "source").ToLower();
        source.Vendor = str.Substring(0, str.IndexOf("-"));
        source.Device = str.Substring(str.IndexOf("-") + 1, (str.IndexOf(".") - str.IndexOf("-")) - 1);
        source.Instance = str.Substring(str.IndexOf(".") + 1, (str.Length - str.IndexOf(".")) - 1);
        return source;
      }
    }

    public XplSource Target
    {
      get
      {
        XplSource source;
        string str = this.GetParam(0, "target").ToLower();
        if (str == "*")
        {
          source.Vendor = "*";
          source.Device = "*";
          source.Instance = "*";
          return source;
        }
        source.Vendor = str.Substring(0, str.IndexOf("-"));
        source.Device = str.Substring(str.IndexOf("-") + 1, (str.IndexOf(".") - str.IndexOf("-")) - 1);
        source.Instance = str.Substring(str.IndexOf(".") + 1, (str.Length - str.IndexOf(".")) - 1);
        return source;
      }
    }
  }
}