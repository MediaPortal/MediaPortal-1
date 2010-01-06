using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;
using System.Xml;
using MediaPortal.GUI.Library;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

namespace MediaPortal.ProcessPlugins.MiniDisplayPlugin.xPL
{
  public class XplListener : IDisposable
  {
    private bool _suppressDuplicateMessages;
    private bool bConfigOnly;
    private bool bListening;
    public XplConfigItems ConfigItems;
    private const int DEFAULT_HBEAT = 5;
    private const string DEFAULT_LISTENTOIP = "ANY";
    private bool disposing;
    private bool DoDebug = false;
    private IPEndPoint epIncoming;
    private int HBeat_Count;
    public int InstanceNumber;
    private byte[] Last_XPL_Buff;
    private ArrayList LocalIP;
    private const int MAX_FILTERS = 0x10;
    private const int MAX_GROUPS = 0x10;
    private const int MAX_HBEAT = 9;
    private const int MAX_XPL_MSG_SIZE = 0x5dc;
    private EventLog mEventLog;
    private XplFilters mFilters;
    private bool mHubFound;
    private const int MIN_HBEAT = 5;
    private string[] mListenTo;
    private bool mNoHubPastInitialProbe;
    private int mNoHubTimerCount;
    private const int NOHUB_HBEAT = 3;
    private const int NOHUB_LOWERFREQ = 30;
    private const int NOHUB_TIMEOUT = 120;
    private string privSource;
    public static string sListenOn = string.Empty;
    private Socket sockIncoming;
    private const int TIMER_FREQ = 0xea60;
    private string VersionNumber;
    private bool WriteDebugInfo;
    private const int XPL_BASE_DYNAMIC_PORT = 0xc350;
    private const int XPL_BASE_PORT = 0xf19;
    private byte[] XPL_Buff;
    private const string XPL_LIB_VERSION = "3.0";
    private int XPL_Portnum;
    public HBeatItemsCallback XplHBeatItems;
    public OnTimerCallback XplOnTimer;
    private Timer XPLTimer;

    public event XplConfigDoneEventHandler XplConfigDone;

    public event XplConfigItemDoneEventHandler XplConfigItemDone;

    public event XplJoinedxPLNetworkEventHandler XplJoinedxPLNetwork;

    public event XplMessageReceivedEventHandler XplMessageReceived;

    public event XplReConfigDoneEventHandler XplReConfigDone;

    public XplListener(string strSource, int InstanceNo)
    {
      this._suppressDuplicateMessages = true;
      this.XPL_Buff = new byte[0x5dc];
      this.Last_XPL_Buff = new byte[0x5dc];
      this.BasicInit(strSource, InstanceNo, null, false);
    }

    public XplListener(string strSource, int InstanceNo, bool debug)
    {
      this._suppressDuplicateMessages = true;
      this.XPL_Buff = new byte[0x5dc];
      this.Last_XPL_Buff = new byte[0x5dc];
      this.BasicInit(strSource, InstanceNo, null, debug);
    }

    public XplListener(string strSource, int InstanceNo, EventLog ThisEventLog)
    {
      this._suppressDuplicateMessages = true;
      this.XPL_Buff = new byte[0x5dc];
      this.Last_XPL_Buff = new byte[0x5dc];
      this.BasicInit(strSource, InstanceNo, ThisEventLog, false);
    }

    private void BasicInit(string strSource, int InstanceNo, EventLog ThisEventLog, bool debug)
    {
      this.VersionNumber = this.getVersionNumber();
      if (strSource.Length == 0)
      {
        throw new Exception(
          "You must pass the XPL Vendor-Device identifier for this instance to the constructor when creating a new instance of the xplListener object");
      }
      this.privSource = strSource;
      this.InstanceNumber = InstanceNo;
      this.mEventLog = ThisEventLog;
      this.WriteDebugInfo = debug;
      this.LogInfo("A new instance of the listener is initialising.");
      this.bListening = false;
      this.bConfigOnly = false;
      this.mHubFound = false;
      this.mNoHubTimerCount = 0;
      this.mNoHubPastInitialProbe = false;
      this.LoadState();
      this.LocalIP = IPAddresses.LocalIPAddresses(this.mEventLog);
    }

    private bool CheckGroups(string t)
    {
      try
      {
        t = t.ToLower();
        if (!t.StartsWith("xpl-group"))
        {
          return false;
        }
        string str = t.Substring(9, t.Length - 9);
        for (int i = 0; i < this.Groups.Length; i++)
        {
          if (this.Groups[i] == str)
          {
            return true;
          }
        }
      }
      catch {}
      return false;
    }

    public void Dispose()
    {
      this.disposing = true;
      try
      {
        if (this.bListening)
        {
          if (this.mHubFound)
          {
            this.SendHeartbeatMessage(true);
          }
          this.sockIncoming.Shutdown(SocketShutdown.Both);
          this.sockIncoming.Close();
        }
      }
      catch {}
    }

    ~XplListener()
    {
      try
      {
        if (!this.disposing)
        {
          this.Dispose();
        }
      }
      catch {}
    }

    private string getVersionNumber()
    {
      try
      {
        Version version = Assembly.GetEntryAssembly().GetName().Version;
        return string.Concat(new object[] {version.Major, ".", version.Minor, ".", version.Build, ".", version.Revision});
      }
      catch
      {
        return "0.0.0.0";
      }
    }

    private void HandleConfigMessage(XplMsg x)
    {
      string strMessage = "";
      if (x.XPL_Msg[0].Section.ToLower() == "xpl-cmnd")
      {
        this.LogInfo("Processing config message: " + x.XPL_Msg[1].Section.ToLower());
        if (this.DoDebug)
        {
          Log.Info("xPL.XplListener.HandleConfigMessage(): received config message: " + x.XPL_Msg[1].Section.ToLower(),
                   new object[0]);
        }
        string str4 = x.XPL_Msg[1].Section.ToLower();
        if (str4 != null)
        {
          XplConfigItem item;
          if (!(str4 == "config.current"))
          {
            if (!(str4 == "config.list"))
            {
              if ((str4 == "config.response") &&
                  (x.GetParam(0, "target").ToLower() == (this.Source.ToLower() + "." + this.InstanceName.ToLower())))
              {
                if (this.DoDebug)
                {
                  Log.Info("xPL.XplListener.HandleConfigMessage(): parsing config.response message");
                }
                ArrayList list = new ArrayList();
                for (int i = 0; i < x.XPL_Msg[1].Details.Count; i++)
                {
                  try
                  {
                    string key = x.XPL_Msg[1].Details[i].keyName.ToLower();
                    string s = x.XPL_Msg[1].Details[i].Value;
                    this.LogInfo("Processing configuration item: " + key);
                    item = this.ConfigItems.ConfigItem(key);
                    if (!list.Contains(key))
                    {
                      item.ResetValues();
                      list.Add(key);
                    }
                    if (key == "interval")
                    {
                      int num5;
                      try
                      {
                        num5 = int.Parse(s);
                      }
                      catch
                      {
                        num5 = 5;
                      }
                      if (num5 < 5)
                      {
                        num5 = 5;
                      }
                      else if (num5 > 9)
                      {
                        num5 = 9;
                      }
                      item.AddValue(num5.ToString());
                      if (this.XplConfigItemDone != null)
                      {
                        this.XplConfigItemDone(key, num5.ToString());
                      }
                    }
                    else
                    {
                      item.AddValue(s);
                      if (this.XplConfigItemDone != null)
                      {
                        this.XplConfigItemDone(key, s);
                      }
                    }
                  }
                  catch (Exception exception)
                  {
                    this.LogError(exception.Message);
                  }
                }
                try
                {
                  if (this.bConfigOnly)
                  {
                    this.bConfigOnly = false;
                    if (this.XplConfigDone != null)
                    {
                      this.XplConfigDone();
                    }
                  }
                  else if (this.XplReConfigDone != null)
                  {
                    this.XplReConfigDone();
                  }
                }
                catch {}
                this.SaveState();
                this.SendHeartbeatMessage(false);
              }
            }
            else
            {
              for (int j = 0; j < this.ConfigItems.Count; j++)
              {
                item = this.ConfigItems.ConfigItem(j);
                switch (item.ConfigType)
                {
                  case xplConfigTypes.xConfig:
                    strMessage = strMessage + "config=";
                    break;

                  case xplConfigTypes.xReconf:
                    strMessage = strMessage + "reconf=";
                    break;

                  case xplConfigTypes.xOption:
                    strMessage = strMessage + "option=";
                    break;
                }
                strMessage = strMessage + item.Name;
                if (item.MaxValues > 1)
                {
                  strMessage = strMessage + "[" + item.MaxValues.ToString() + "]";
                }
                strMessage = strMessage + '\n';
              }
              if (this.DoDebug)
              {
                Log.Info("xPL.XplListener.HandleConfigMessage(): responding to config.list request");
              }
              this.SendMessage("xpl-stat", "*", "config.list", strMessage);
            }
          }
          else
          {
            string str5;
            if (((str5 = x.GetParam(1, "command").ToLower()) != null) && (str5 == "request"))
            {
              if (this.DoDebug)
              {
                Log.Info(
                  "xPL.XplListener.HandleConfigMessage(): parsing config for config.current command=request ({0} config items)",
                  new object[] {this.ConfigItems.Count});
              }
              for (int k = 0; k < this.ConfigItems.Count; k++)
              {
                if (this.DoDebug)
                {
                  Log.Info("xPL.XplListener.HandleConfigMessage(): parsing config items {0} ({1})",
                           new object[] {k, this.ConfigItems.ConfigItem(k).Name});
                }
                item = this.ConfigItems.ConfigItem(k);
                for (int m = 0; m < item.ValueCount; m++)
                {
                  object obj2 = strMessage;
                  strMessage = string.Concat(new object[] {obj2, item.Name, "=", item.Values[m], '\n'});
                }
              }
              if (this.DoDebug)
              {
                Log.Info("xPL.XplListener.HandleConfigMessage(): responding to config.current command=request",
                         new object[0]);
              }
              this.SendMessage("xpl-stat", "*", "config.current", strMessage);
            }
          }
        }
      }
    }

    private void InitSocket()
    {
      if (this.DoDebug)
      {
        Log.Info("xPL.XplListener.InitSocket(): Called");
      }
      this.XPL_Portnum = 0xc350;
      this.sockIncoming = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
      int port = this.XPL_Portnum;
      while ((port < 0xc550) & (port != 0))
      {
        if (this.DoDebug)
        {
          Log.Info("xPL.XplListener.InitSocket(): trying IP = {0}:{1}",
                   new object[] {this.ListenOnIP_IP.ToString(), port});
        }
        try
        {
          if (this.DoDebug)
          {
            Log.Info("xPL.XplListener.InitSocket(): Binding to IP = {0}:{1}",
                     new object[] {this.ListenOnIP_IP.ToString(), port});
          }
          this.sockIncoming.Bind(new IPEndPoint(this.ListenOnIP_IP, port));
          this.XPL_Portnum = port;
          port = 0;
          continue;
        }
        catch
        {
          if (this.DoDebug)
          {
            Log.Info("xPL.XplListener.InitSocket(): IP = {0}:{1} not available...",
                     new object[] {this.ListenOnIP_IP.ToString(), port});
          }
          port++;
          this.sockIncoming.Close();
          this.sockIncoming = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
          continue;
        }
      }
      if (port == 0xc550)
      {
        throw new Exception("Unable to bind to a free UDP port for listening.");
      }
      this.XPLTimer = new Timer();
      this.XPLTimer.Interval = 3000.0;
      this.XPLTimer.AutoReset = false;
      this.XPLTimer.Elapsed += new ElapsedEventHandler(this.XPLTimerElapsed);
      this.XPLTimer.Enabled = true;
      this.epIncoming = new IPEndPoint(IPAddress.Any, 0);
      EndPoint epIncoming = this.epIncoming;
      this.sockIncoming.BeginReceiveFrom(this.XPL_Buff, 0, 0x5dc, SocketFlags.None, ref epIncoming,
                                         new AsyncCallback(this.ReceiveData), null);
      this.bListening = true;
      this.XPLTimerElapsed(null, null);
    }

    public void Listen()
    {
      if (!this.bListening)
      {
        this.InitSocket();
      }
    }

    private void LoadState()
    {
      string path = "xpl_" + this.Source + ".instance" + this.InstanceNumber.ToString() + ".3.0.xml";
      this.LogInfo("Loading state from " + path + "...");
      this.ConfigItems = new XplConfigItems();
      this.mFilters = new XplFilters(this.ConfigItems);
      bool flag = false;
      try
      {
        if (!File.Exists(path))
        {
          this.LogInfo("The config file " + path + " does not exist. Going into remote config mode.");
        }
        else
        {
          XmlTextReader reader = new XmlTextReader(path);
          while (reader.Read())
          {
            if ((reader.NodeType == XmlNodeType.Element) & (reader.Name == "configItem"))
            {
              this.ConfigItems.Add(reader.GetAttribute("key"), reader.GetAttribute("value"),
                                   (xplConfigTypes)int.Parse(reader.GetAttribute("cfgtype")));
            }
          }
          reader.Close();
          flag = true;
          this.LogInfo("Configuration loaded OK.");
        }
      }
      catch (Exception exception)
      {
        this.LogError("Loading did not succeed: " + exception.Message);
      }
      if (!flag)
      {
        string str2 = Environment.MachineName.ToLower().Replace("-", "").Replace("_", "");
        if (str2.Length > 14)
        {
          str2 = str2.Substring(0, 14);
        }
        this.InstanceName = str2 + this.InstanceNumber.ToString();
        this.HBeat_Interval = 5;
        this.bConfigOnly = true;
        this.LogInfo("Awaiting configuration.");
      }
    }

    private void LogError(string s)
    {
      if (this.mEventLog != null)
      {
        this.mEventLog.WriteEntry(s);
      }
      else if (this.DoDebug)
      {
        Log.Info("xPL.XplListener (LogError) - Source {0}: {1}", new object[] {this.Source, s});
      }
    }

    private void LogInfo(string s)
    {
      if (this.WriteDebugInfo)
      {
        StreamWriter writer = File.AppendText(@"c:\xpllib-debug-log.txt");
        writer.WriteLine(DateTime.Now.ToString("dd-MMM-yy HH:mm:ss") + " " + this.Source + ": " + s);
        writer.Close();
      }
      else if (this.DoDebug)
      {
        Log.Info("xPL.XplListener (LogInfo) - Source {0}: {1}", new object[] {this.Source, s});
      }
    }

    private bool MsgSchemaMatchesFilter(string c, string t, XplSchema fSchema)
    {
      if ((fSchema.msgClass != "*") & (fSchema.msgClass != c))
      {
        return false;
      }
      if ((fSchema.msgType != "*") & (fSchema.msgType != t))
      {
        return false;
      }
      this.LogInfo("SchemaMatchesFilter=true");
      return true;
    }

    private bool MsgSourceMatchesFilter(XplSource mySource, XplSource fSource)
    {
      string vendor = mySource.Vendor;
      string device = mySource.Device;
      string instance = mySource.Instance;
      this.LogInfo("vendor=" + vendor + ",device=" + device + ",instance=" + instance);
      if ((fSource.Vendor != "*") & (fSource.Vendor != vendor))
      {
        return false;
      }
      if ((fSource.Device != "*") & (fSource.Device != device))
      {
        return false;
      }
      if ((fSource.Instance != "*") & (fSource.Instance != instance))
      {
        return false;
      }
      this.LogInfo("SourceMatchesfilter = true");
      return true;
    }

    private bool MsgTypeMatchesFilter(string m, XplMessageTypes f)
    {
      if (((int)f) == 0xff)
      {
        return true;
      }
      bool flag = false;
      string str = m;
      if (str != null)
      {
        if (!(str == "xpl-cmnd"))
        {
          if (str == "xpl-stat")
          {
            if (((byte)(f & XplMessageTypes.Status)) > 0)
            {
              flag = true;
            }
          }
          else if ((str == "xpl-trig") && (((byte)(f & XplMessageTypes.Trigger)) > 0))
          {
            flag = true;
          }
        }
        else if (((byte)(f & XplMessageTypes.Command)) > 0)
        {
          flag = true;
        }
      }
      this.LogInfo("Msgtypematchesfilter=" + flag.ToString());
      return flag;
    }

    public void New(string strSource, int InstanceNo)
    {
      this.BasicInit(strSource, InstanceNo, null, false);
    }

    private void ReceiveData(IAsyncResult ar)
    {
      if (!this.disposing)
      {
        EndPoint epIncoming = this.epIncoming;
        int count = this.sockIncoming.EndReceiveFrom(ar, ref epIncoming);
        this.epIncoming = (IPEndPoint)epIncoming;
        if (this.DoDebug)
        {
          Log.Info("xPL.XplListener.ReceiveData(): received {0} bytes", new object[] {count});
        }
        bool flag = false;
        for (int i = 0; !flag & (i < this.ListenToIPs.Length); i++)
        {
          if (this.ListenToIPs[i].ToUpper() == "ANY")
          {
            flag = true;
          }
          else if (this.ListenToIPs[i].ToUpper() == "ANY_LOCAL")
          {
            flag = this.LocalIP.Contains(this.epIncoming.Address.ToString());
          }
          else if (this.ListenToIPs[i] == this.epIncoming.Address.ToString())
          {
            flag = true;
          }
        }
        if (!flag)
        {
          if (this.DoDebug)
          {
            Log.Info("xPL.XplListener.ReceiveData(): Illegal source - " + this.epIncoming.Address.ToString(),
                     new object[0]);
          }
          this.LogInfo("Illegal source: " + this.epIncoming.Address.ToString());
        }
        else
        {
          if (this.DoDebug)
          {
            Log.Info("xPL.XplListener.ReceiveData(): reading data");
          }
          XplMsg x = new XplMsg(Encoding.ASCII.GetString(this.XPL_Buff, 0, count));
          if (this.DoDebug)
          {
            Log.Info("xPL.XplListener.ReceiveData(): read data\n\n{0}\n",
                     new object[] {Encoding.ASCII.GetString(this.XPL_Buff, 0, count)});
          }
          try
          {
            if (x.IsMessageValid)
            {
              if (this.DoDebug)
              {
                Log.Info("xPL.XplListener.ReceiveData(): received data is a valid xPL message");
              }
              string t = x.GetParam(0, "target").ToLower();
              bool flag2 = x.GetParam(0, "target").ToLower() == (this.Source + "." + this.InstanceName).ToLower();
              bool flag3 = x.GetParam(0, "target") == "*";
              bool flag4 = this.Filters.Count == 0;
              string c = x.Schema.msgClass.ToLower();
              string str3 = x.Schema.msgType.ToLower();
              if (this.DoDebug)
              {
                Log.Info(
                  "xPL.XplListener.ReceiveData(): received Source = {0}, Instance = {1} - Schema - Class = {2}, Type = {3}",
                  new object[] {this.Source, this.InstanceName, c, str3});
              }
              if ((!this.mHubFound && (c.Equals("hbeat") || c.Equals("config"))) &&
                  (str3.Equals("app") & x.GetParam(0, "source").Equals(this.Source + "." + this.InstanceName)))
              {
                if (this.DoDebug)
                {
                  Log.Info("xPL.XplListener.ReceiveData(): received message from Source = {0}",
                           new object[] {x.GetParam(0, "source")});
                }
                if (this.DoDebug)
                {
                  Log.Info("xPL.XplListener.ReceiveData(): Found xPL hub");
                }
                this.mHubFound = true;
                this.XPLTimer.Interval = 60000.0;
                this.XplJoinedxPLNetwork();
              }
              if (flag2 & c.Equals("config"))
              {
                this.HandleConfigMessage(x);
              }
              if ((flag2 | flag3) & (c.Equals("hbeat") & str3.Equals("request")))
              {
                this.WaitForRandomPeriod();
                this.SendHeartbeatMessage(false);
              }
              if (!this.bConfigOnly | this.Filters.AlwaysPassMessages)
              {
                if ((flag3 | flag2) | (!this.Filters.MatchTarget | this.CheckGroups(t)))
                {
                  string m = x.XPL_Msg[0].Section.ToLower();
                  for (int j = 0; j < this.Filters.Count; j++)
                  {
                    XplFilter filter = this.Filters.Item(j);
                    if ((this.MsgTypeMatchesFilter(m, filter.MessageType) &&
                         this.MsgSourceMatchesFilter(x.Source, filter.Source)) &&
                        this.MsgSchemaMatchesFilter(c, str3, filter.Schema))
                    {
                      flag4 = true;
                      break;
                    }
                  }
                }
                if (flag4 && (this.XplMessageReceived != null))
                {
                  this.XplMessageReceived(this, new XplEventArgs(x));
                }
              }
            }
          }
          catch (Exception exception)
          {
            this.LogError(exception.ToString());
          }
        }
        this.sockIncoming.BeginReceiveFrom(this.XPL_Buff, 0, 0x5dc, SocketFlags.None, ref epIncoming,
                                           new AsyncCallback(this.ReceiveData), null);
      }
    }

    public void SaveState()
    {
      if (!this.bConfigOnly)
      {
        this.LogInfo("Saving state...");
        try
        {
          XmlTextWriter writer =
            new XmlTextWriter("xpl_" + this.Source + ".instance" + this.InstanceNumber.ToString() + ".3.0.xml", null);
          writer.Formatting = Formatting.Indented;
          writer.WriteStartDocument(false);
          writer.WriteComment("This file was automatically generated by the xPL Library for c# .NET");
          writer.WriteStartElement("xplConfiguration");
          writer.WriteStartElement("configItems");
          for (int i = 0; i < this.ConfigItems.Count; i++)
          {
            XplConfigItem item = this.ConfigItems.ConfigItem(i);
            for (int j = 0; j < item.ValueCount; j++)
            {
              writer.WriteStartElement("configItem");
              writer.WriteAttributeString("key", item.Name);
              writer.WriteAttributeString("value", item.Values[j]);
              writer.WriteAttributeString("cfgtype", ((int)item.ConfigType).ToString());
              writer.WriteEndElement();
            }
          }
          writer.WriteEndElement();
          writer.WriteEndElement();
          writer.Flush();
          writer.Close();
        }
        catch (Exception exception)
        {
          this.LogError("Error saving state: " + exception.ToString());
        }
      }
    }

    private void SendHeartbeatMessage(bool closingDown)
    {
      try
      {
        object obj2 = string.Concat(new object[] {"xpl-stat", '\n', "{", '\n'}) + "hop=1" + '\n';
        string str = (string.Concat(new object[] {obj2, "source=", this.Source, ".", this.InstanceName, '\n'}) +
                      "target=*" + '\n') + "}" + '\n';
        if (this.bConfigOnly)
        {
          if (closingDown)
          {
            str = str + "config.end";
          }
          else
          {
            str = str + "config.app";
          }
        }
        else if (closingDown)
        {
          str = str + "hbeat.end";
        }
        else
        {
          str = str + "hbeat.app";
        }
        object obj3 = str;
        object obj4 = string.Concat(new object[] {obj3, '\n', "{", '\n'});
        object obj5 = string.Concat(new object[] {obj4, "interval=", this.HBeat_Interval.ToString(), '\n'});
        str = string.Concat(new object[] {obj5, "port=", this.XPL_Portnum.ToString(), '\n'});
        string listenOnIP = this.ListenOnIP;
        if (!this.LocalIP.Contains(listenOnIP))
        {
          listenOnIP = this.LocalIP[0].ToString();
        }
        object obj6 = str;
        object obj7 = string.Concat(new object[] {obj6, "remote-ip=", listenOnIP, '\n'});
        str = string.Concat(new object[] {obj7, "version=", this.VersionNumber, '\n'});
        if ((this.XplHBeatItems != null) & !this.bConfigOnly)
        {
          str = str + this.XplHBeatItems();
        }
        new XplMsg(str + "}" + '\n').Send();
      }
      catch (Exception exception)
      {
        this.LogError("Error sending heartbeat: " + exception.ToString());
      }
    }

    public void SendMessage(string MsgType, string strTarget, string strSchema, string strMessage)
    {
      string str = string.Concat(new object[] {MsgType, '\n', "{", '\n'}) + "hop=1" + '\n';
      str = string.Concat(new object[] {str, "source=", this.Source, ".", this.InstanceName, '\n'});
      str = string.Concat(new object[] {str, "target=", strTarget, '\n'});
      str = string.Concat(new object[] {str, "}", '\n', strSchema, '\n', "{", '\n'});
      new XplMsg(string.Concat(new object[] {str, strMessage, "}", '\n'})).Send();
    }

    public static string sListenOnIP()
    {
      bool extensiveLogging = Settings.Instance.ExtensiveLogging;
      RegistryKey key = null;
      if (sListenOn.Equals(string.Empty))
      {
        string str;
        try
        {
          if (extensiveLogging)
          {
            Log.Info("xPL.XplListener.sListenOnIP: checking registry");
          }
          key = Registry.LocalMachine.OpenSubKey(@"Software\xPL");
          if (key != null)
          {
            str = (string)key.GetValue("ListenOnAddress", "ANY_LOCAL");
            if (extensiveLogging)
            {
              Log.Info("xPL.XplListener.sListenOnIP: retreived \"{0}\" from registry", new object[] {str});
            }
          }
          else
          {
            str = "ANY_LOCAL";
            if (extensiveLogging)
            {
              Log.Info("xPL.XplListener.sListenOnIP: using \"{0}\" as default value", new object[] {str});
            }
          }
        }
        catch
        {
          if (extensiveLogging)
          {
            Log.Info("xPL.XplListener.sListenOnIP: registry read threw exception");
          }
          str = "ANY_LOCAL";
        }
        finally
        {
          if (key != null)
          {
            key.Close();
          }
        }
        sListenOn = str;
      }
      return sListenOn;
    }

    private static XplFilter Str2Filter(string StrFilter)
    {
      string[] strArray = StrFilter.Split(new char[] {'.'});
      if (strArray.Length != 6)
      {
        throw new Exception("Malformed filter: " + StrFilter);
      }
      XplMessageTypes t = StrCmnd2MsgType(strArray[0]);
      if (t == XplMessageTypes.None)
      {
        throw new Exception("Malformed message type in filter: " + StrFilter);
      }
      return new XplFilter(t, strArray[1], strArray[2], strArray[3], strArray[4], strArray[5]);
    }

    private static XplMessageTypes StrCmnd2MsgType(string StrCmnd)
    {
      switch (StrCmnd.ToLower())
      {
        case "xpl-cmnd":
          return XplMessageTypes.Command;

        case "xpl-stat":
          return XplMessageTypes.Status;

        case "xpl-trig":
          return XplMessageTypes.Trigger;

        case "*":
          return (XplMessageTypes)0xff;
      }
      return XplMessageTypes.None;
    }

    private string StrMsgType2Cmnd(XplMessageTypes Messagetype)
    {
      switch (Messagetype)
      {
        case XplMessageTypes.Command:
          return "xpl-cmnd";

        case XplMessageTypes.Status:
          return "xpl-stat";

        case XplMessageTypes.Trigger:
          return "xpl-trig";

        case (XplMessageTypes)0xff:
          return "*";
      }
      return "";
    }

    private void WaitForRandomPeriod()
    {
      Random random = new Random();
      Thread.Sleep(random.Next(0x3e8, 0xbb8));
    }

    public void XPLTimerElapsed(object sender, ElapsedEventArgs e)
    {
      if (!this.disposing)
      {
        if (this.WriteDebugInfo)
        {
          this.LogInfo("The timer has elapsed.");
        }
        if (!this.mHubFound)
        {
          if (!this.mNoHubPastInitialProbe)
          {
            this.mNoHubTimerCount += 3;
            if (this.mNoHubTimerCount >= 120)
            {
              this.XPLTimer.Interval = 30000.0;
              this.mNoHubPastInitialProbe = true;
            }
          }
          this.SendHeartbeatMessage(false);
        }
        else
        {
          if ((this.XplOnTimer != null) & (sender != null))
          {
            try
            {
              this.XplOnTimer();
            }
            catch {}
          }
          this.HBeat_Count++;
          if ((this.HBeat_Count >= this.HBeat_Interval) | this.bConfigOnly)
          {
            this.SendHeartbeatMessage(false);
            this.HBeat_Count = 0;
          }
          this.XPLTimer.Interval = ((60 - DateTime.Now.Second) + 1) * 0x3e8;
        }
        if (this.WriteDebugInfo)
        {
          this.LogInfo("Timer interval is " + this.XPLTimer.Interval);
        }
        this.XPLTimer.Start();
      }
    }

    public bool AwaitingConfiguration
    {
      get { return this.bConfigOnly; }
    }

    public EventLog ErrorEventLog
    {
      set { this.mEventLog = value; }
    }

    public XplFilters Filters
    {
      get { return this.mFilters; }
    }

    public string[] Groups
    {
      get
      {
        if (this.ConfigItems.ConfigItem("group").Value == "")
        {
          return new string[0];
        }
        return this.ConfigItems.ConfigItem("group").Values;
      }
    }

    private int HBeat_Interval
    {
      get
      {
        try
        {
          return int.Parse(this.ConfigItems.ConfigItem("interval").Value);
        }
        catch
        {
          this.HBeat_Interval = 5;
          return 5;
        }
      }
      set { this.ConfigItems.ConfigItem("interval").Value = value.ToString(); }
    }

    public int HBeatInterval
    {
      get { return this.HBeat_Interval; }
      set
      {
        if (value < 5)
        {
          this.HBeat_Interval = 5;
        }
        else if (value > 30)
        {
          this.HBeat_Interval = 30;
        }
        else
        {
          this.HBeat_Interval = value;
        }
      }
    }

    public string InstanceName
    {
      get { return this.ConfigItems.ConfigItem("newconf").Value; }
      set { this.ConfigItems.ConfigItem("newconf").Value = value; }
    }

    public bool JoinedxPLNetwork
    {
      get { return this.mHubFound; }
    }

    private string ListenOnIP
    {
      get { return sListenOnIP(); }
    }

    private IPAddress ListenOnIP_IP
    {
      get
      {
        IPAddress any;
        if (this.DoDebug)
        {
          Log.Info("xPL.XplListener.ListenOnIP_IP: called");
        }
        string listenOnIP = this.ListenOnIP;
        if (listenOnIP == "ANY_LOCAL")
        {
          any = IPAddress.Any;
        }
        else
        {
          try
          {
            any = IPAddress.Parse(listenOnIP);
          }
          catch (Exception exception)
          {
            throw new Exception("Could not decode to valid IPAddress: " + listenOnIP, exception);
          }
        }
        if (this.DoDebug)
        {
          Log.Info("xPL.XplListener.ListenOnIP_IP - {0}", new object[] {listenOnIP});
        }
        return any;
      }
    }

    private string[] ListenToIPs
    {
      get
      {
        RegistryKey key = null;
        if (this.mListenTo == null)
        {
          string str;
          try
          {
            if (this.DoDebug)
            {
              Log.Info("xPL.XplListener.ListenOnIPs: checking registry");
            }
            key = Registry.LocalMachine.OpenSubKey(@"Software\xPL");
            if (key != null)
            {
              str = (string)key.GetValue("ListenToAddresses", "ANY");
              if (this.DoDebug)
              {
                Log.Info("xPL.XplListener.ListenOnIPs: retreived \"{0}\" from registry", new object[] {str});
              }
            }
            else
            {
              str = "ANY";
            }
          }
          catch
          {
            if (this.DoDebug)
            {
              Log.Info("xPL.XplListener.ListenToIPs: registry read threw exception");
            }
            str = "ANY";
          }
          finally
          {
            if (key != null)
            {
              key.Close();
            }
          }
          this.mListenTo = str.Split(new char[] {','});
          for (int i = 0; i < this.mListenTo.Length; i++)
          {
            this.mListenTo[i] = this.mListenTo[i].Trim();
          }
          if (this.DoDebug)
          {
            Log.Info("xPL.XplListener.ListenToIPs: found {0}", new object[] {this.mListenTo.Length});
          }
        }
        return this.mListenTo;
      }
    }

    public int Port
    {
      get { return this.XPL_Portnum; }
    }

    public string Source
    {
      get { return this.privSource; }
    }

    public bool suppressDuplicateMessages
    {
      get { return this._suppressDuplicateMessages; }
      set { this._suppressDuplicateMessages = value; }
    }

    public delegate string HBeatItemsCallback();

    public delegate void OnTimerCallback();

    public delegate void XplConfigDoneEventHandler();

    public class XplConfigItem
    {
      private xplConfigTypes mConfigType;
      private int mMaxValues;
      private string mName;
      private List<string> mValue;

      public XplConfigItem(string itemName, string itemValue, xplConfigTypes itemtype)
      {
        this.mName = itemName;
        this.mConfigType = itemtype;
        if (itemName == "filter")
        {
          this.mMaxValues = 0x10;
        }
        else if (itemName == "group")
        {
          this.mMaxValues = 0x10;
        }
        else
        {
          this.mMaxValues = 1;
        }
        this.mValue = new List<string>(this.mMaxValues);
        this.AddValue(itemValue);
      }

      public void AddValue(string itemValue)
      {
        if (this.mValue.Count == 0)
        {
          this.mValue.Add(itemValue);
        }
        else if (this.mMaxValues == 1)
        {
          this.mValue[0] = itemValue;
        }
        else
        {
          int num = -1;
          for (int i = 0; i < this.mValue.Count; i++)
          {
            if (this.mValue[i] == itemValue)
            {
              num = i;
              break;
            }
          }
          if (num < 0)
          {
            this.mValue.Add(itemValue);
          }
        }
      }

      public void ResetValues()
      {
        this.mValue.Clear();
      }

      public xplConfigTypes ConfigType
      {
        get { return this.mConfigType; }
      }

      public int MaxValues
      {
        get { return this.mMaxValues; }
        set { this.mMaxValues = value; }
      }

      public string Name
      {
        get { return this.mName; }
      }

      public string Value
      {
        get { return this.mValue[0]; }
        set { this.mValue[0] = value; }
      }

      public int ValueCount
      {
        get { return this.mValue.Count; }
      }

      public string[] Values
      {
        get { return this.mValue.ToArray(); }
      }
    }

    public delegate void XplConfigItemDoneEventHandler(string itemName, string itemValue);

    public class XplConfigItems
    {
      private List<string> mKeys = new List<string>();
      private List<XplConfigItem> mValues = new List<XplConfigItem>();

      public XplConfigItems()
      {
        this.mKeys.Add("newconf");
        this.mValues.Add(new XplConfigItem("newconf", "", xplConfigTypes.xReconf));
        this.mKeys.Add("interval");
        this.mValues.Add(new XplConfigItem("interval", "", xplConfigTypes.xReconf));
        this.mKeys.Add("filter");
        this.mValues.Add(new XplConfigItem("filter", "", xplConfigTypes.xOption));
        this.mKeys.Add("group");
        this.mValues.Add(new XplConfigItem("group", "", xplConfigTypes.xOption));
      }

      public void Add(string itemName, string itemValue)
      {
        this.Add(itemName, itemValue, xplConfigTypes.xConfig);
      }

      public void Add(string itemName, string itemValue, xplConfigTypes itemtype)
      {
        if (!this.mKeys.Contains(itemName.ToLower()))
        {
          XplConfigItem item = new XplConfigItem(itemName, itemValue, itemtype);
          this.mKeys.Add(itemName.ToLower());
          this.mValues.Add(item);
        }
        else
        {
          this.ConfigItem(itemName).AddValue(itemValue);
        }
      }

      public XplConfigItem ConfigItem(int idx)
      {
        return this.mValues[idx];
      }

      public XplConfigItem ConfigItem(string key)
      {
        return this.mValues[this.mKeys.IndexOf(key.ToLower())];
      }

      public string Item(string key)
      {
        return this.mValues[this.mKeys.IndexOf(key.ToLower())].Value;
      }

      public int Count
      {
        get { return this.mKeys.Count; }
      }
    }

    public class XplEventArgs : EventArgs
    {
      public XplMsg XplMsg;

      public XplEventArgs(XplMsg x)
      {
        this.XplMsg = x;
      }
    }

    public class XplFilter
    {
      public XplMessageTypes MessageType;
      public XplSchema Schema;
      public XplSource Source;

      public XplFilter()
      {
        this.MessageType = XplMessageTypes.None;
        this.Source.Vendor = "";
        this.Source.Device = "";
        this.Source.Instance = "";
        this.Schema.msgClass = "";
        this.Schema.msgType = "";
      }

      public XplFilter(XplMessageTypes t, string Source_Vendor, string Source_Device, string Source_Instance,
                       string Schema_class, string Schema_Type)
      {
        this.MessageType = t;
        this.Source.Vendor = Source_Vendor.ToLower();
        this.Source.Device = Source_Device.ToLower();
        this.Source.Instance = Source_Instance.ToLower();
        this.Schema.msgClass = Schema_class.ToLower();
        this.Schema.msgType = Schema_Type.ToLower();
      }
    }

    public class XplFilters
    {
      private bool mAlwaysPassMessages;
      private XplConfigItems mConfigItems;
      private bool mMatchTarget;

      public XplFilters(XplConfigItems ConfigItems)
      {
        this.mConfigItems = ConfigItems;
        this.mMatchTarget = true;
        this.mAlwaysPassMessages = false;
      }

      public void Add(XplFilter f)
      {
        this.mConfigItems.Add("filter", f.ToString(), xplConfigTypes.xOption);
      }

      public XplFilter Item(int index)
      {
        return Str2Filter(this.mConfigItems.ConfigItem("filter").Values[index]);
      }

      public bool AlwaysPassMessages
      {
        get { return this.mAlwaysPassMessages; }
        set { this.mAlwaysPassMessages = value; }
      }

      public int Count
      {
        get
        {
          if (this.mConfigItems.ConfigItem("filter").Value.Length == 0)
          {
            return 0;
          }
          return this.mConfigItems.ConfigItem("filter").ValueCount;
        }
      }

      public bool MatchTarget
      {
        get { return this.mMatchTarget; }
        set { this.mMatchTarget = value; }
      }
    }

    public delegate void XplJoinedxPLNetworkEventHandler();

    public delegate void XplMessageReceivedEventHandler(object sender, XplEventArgs e);

    public delegate void XplReConfigDoneEventHandler();
  }
}