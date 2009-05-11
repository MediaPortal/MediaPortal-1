using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;

using TvLibrary.Log;
using TvControl;
using SetupTv;
using TvEngine.Events;
using TvLibrary.Interfaces;
using TvLibrary.Implementations;
using TVEngine.Devices;
using TvDatabase;

namespace TvEngine
{

  public class ServerBlaster : ITvServerPlugin
  {

    #region properties

    /// <summary>
    /// returns the name of the plugin
    /// </summary>
    public string Name
    {
      get
      {
        return "ServerBlaster";
      }
    }
    /// <summary>
    /// returns the version of the plugin
    /// </summary>
    public string Version
    {
      get
      {
        return "1.0.0.0";
      }
    }
    /// <summary>
    /// returns the author of the plugin
    /// </summary>
    public string Author
    {
      get
      {
        return "joboehl";
      }
    }
    /// <summary>
    /// returns if the plugin should only run on the master server
    /// or also on slave servers
    /// </summary>
    public bool MasterOnly
    {
      get
      {
        return false;
      }
    }
    #endregion

    #region public methods

    public void Start(IController controller)
    {
      Log.WriteFile("ServerBlaster.Start: Starting");
      ITvServerEvent events = GlobalServiceProvider.Instance.Get<ITvServerEvent>();
      events.OnTvServerEvent += new TvServerEventHandler(events_OnTvServerEvent);

      Blaster.DeviceArrival += new DeviceEventHandler(OnDeviceArrival);
      Blaster.DeviceRemoval += new DeviceEventHandler(OnDeviceRemoval);
      LoadRemoteCodes();

      Log.WriteFile("plugin: ServerBlaster start sender");
      Thread thread = new Thread(new ThreadStart(Sender));
      thread.SetApartmentState(ApartmentState.STA);
      thread.IsBackground = true;
      thread.Name = "Remote blaster";
      _running = true;
      thread.Start();


      Log.WriteFile("plugin: ServerBlaster.Start started");
    }

    public void Stop()
    {
      Log.WriteFile("plugin: ServerBlaster.Stop stopping");
      _running = false;

      if (GlobalServiceProvider.Instance.IsRegistered<ITvServerEvent>())
      {
        GlobalServiceProvider.Instance.Get<ITvServerEvent>().OnTvServerEvent -= events_OnTvServerEvent;
      }
      Log.WriteFile("plugin: ServerBlaster.Stop stopped");
    }

    public SetupTv.SectionSettings Setup
    {
      get
      {
        return new SetupTv.Sections.BlasterSetup();
      }
    }
    #endregion public implementation

    #region Implementation

    void events_OnTvServerEvent(object sender, EventArgs eventArgs)
    {

      TvServerEventArgs tvEvent = (TvServerEventArgs)eventArgs;
      AnalogChannel analogChannel = tvEvent.channel as AnalogChannel;
      if (analogChannel == null) return;
      if (tvEvent.EventType == TvServerEventType.StartZapChannel)
      {
        Log.WriteFile("ServerBlaster - CardId: {0}, Channel: {1} - Channel:{2}", tvEvent.Card.Id, analogChannel.ChannelNumber, analogChannel.Name);
        _send = true;
        _channel = analogChannel.ChannelNumber;
        _card = tvEvent.Card.Id;
        Log.WriteFile("ServerBlaster - Done");

      }
    }

    void OnDeviceArrival()
    {
      Log.WriteFile("ServerBlaster.OnDeviceArrival: Device installed");
    }

    void OnDeviceRemoval()
    {
      Log.WriteFile("ServerBlaster.OnDeviceRemoval: Device removed");
    }

    bool LoadRemoteCodes()
    {
      try
      {
        using (FileStream fs = new FileStream(String.Format(@"{0}\ServerBlaster.dat", Log.GetPathName()), FileMode.Open, FileAccess.Read))
        {
          BinaryFormatter bf = new BinaryFormatter();
          _packetCollection = bf.Deserialize(fs) as Hashtable;

          foreach (string buttonName in _packetCollection.Keys)
          {
            Log.WriteFile("ServerBlaster.LoadRemoteCodes: Packet '{0}' ({1} bytes)", buttonName, ((byte[])_packetCollection[buttonName]).Length);
          }
        }

        TvBusinessLayer layer = new TvBusinessLayer();
        _sendSelect = (layer.GetSetting("SrvBlasterSendSelect", "False").Value == "True");
        _sleepTime = 100; //xmlreader.GetValueAsInt("ServerBlaster", "delay", 100);
        _sendPort = 1; //xmlreader.GetValueAsInt("ServerBlaster", "forceport", 1);
        _blaster1Card = Convert.ToInt16(layer.GetSetting("SrvBlaster1Card", "0").Value);
        _blaster2Card = Convert.ToInt16(layer.GetSetting("SrvBlaster2Card", "0").Value);
        _deviceType = Convert.ToInt16(layer.GetSetting("SrvBlasterType", "0").Value);
        _deviceSpeed = Convert.ToInt16(layer.GetSetting("SrvBlasterSpeed", "0").Value);
        _advandeLogging = (layer.GetSetting("SrvBlasterLog", "False").Value == "True");
        _sendPort = Math.Max(1, Math.Min(2, _sendPort));

        Log.WriteFile("ServerBlaster.LoadRemoteCodes: Default port {0}", _sendPort);
        Log.WriteFile("ServerBlaster.RemoteType {0}", _deviceType);
        Log.WriteFile("ServerBlaster.DeviceSpeed {0}", _deviceSpeed);
        Log.WriteFile("ServerBlaster.Blaster1Card {0}", _blaster1Card);
        Log.WriteFile("ServerBlaster.Blaster2Card {0}", _blaster2Card);
        Log.WriteFile("ServerBlaster.Type {0}", _deviceType);
        Log.WriteFile("ServerBlaster.AdvancedLogging {0}", _advandeLogging);
        Log.WriteFile("ServerBlaster.SendSelect {0}", _sendSelect);
      }
      catch (Exception e)
      {
        Log.WriteFile("ServerBlaster.LoadRemoteCodes: {0}", e.Message);
      }

      return false;
    }

    void Sender()
    {
      while (_running)
      {
        if (_sending || !_send)
        {
          Thread.Sleep(50);
          continue;
        }
        _sending = true;
        Log.WriteFile("Blaster Sending: {0}, {1}", _channel, _card);
        Send(_channel, _card);
        _sending = false;
        _send = false;
      }

    }



    void Send(int externalChannel, int card)
    {
      if (_blaster1Card == card) _sendPort = 1;
      else if (_blaster2Card == card) _sendPort = 2;
      else if (_blaster1Card == _blaster2Card) _sendPort = 0;

      if (_advandeLogging) Log.WriteFile("ServerBlaster.Send: C {0} - B1{1} - B2{2}. Channel is {3}", card, _blaster1Card, _blaster2Card, externalChannel);

      try
      {
        foreach (char ch in externalChannel.ToString())
        {
          if (char.IsDigit(ch) == false) continue;
          if (_advandeLogging) Log.WriteFile("ServerBlaster.Sending {0} on blaster {1}", ch, _sendPort);

          byte[] packet = _packetCollection[ch.ToString()] as byte[];

          if (packet == null) Log.WriteFile("ServerBlaster.Send: Missing packet for '{0}'", ch);
          if (packet != null) Blaster.Send(_sendPort, packet, _deviceType, _deviceSpeed, _advandeLogging);
          if (packet != null && _sleepTime != 0) Thread.Sleep(_sleepTime);
          if (_advandeLogging) Log.WriteFile("ServerBlaster.Send logic is done");
        }

        if (_sendSelect)
        {
          //byte[] packet = _packetCollection["Select"] as byte[];
          if (_advandeLogging) Log.Write("ServerBlaster.Send: Sending Select");
          byte[] packet = _packetCollection["Select"] as byte[];
          if (packet != null) Blaster.Send(_sendPort, packet, _deviceType, _deviceSpeed, _advandeLogging);

        }
      }
      catch (Exception e)
      {
        Log.WriteFile("ServerBlaster.Send: {0}", e.Message);
      }
    }

    #endregion Implementation

    #region Members

    Hashtable _packetCollection;
    int _sendPort = 1;
    int _sleepTime = 100;
    bool _sendSelect;
    int _blaster1Card = -1;
    int _blaster2Card = 1;
    bool _advandeLogging;
    int _deviceType = 1;
    int _deviceSpeed = 0;
    int _channel;
    int _card;
    bool _send = false;
    bool _sending = false;
    bool _running = false;

    #endregion Members

  }
}