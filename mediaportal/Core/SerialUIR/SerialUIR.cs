#region Copyright (C) 2005-2009 Team MediaPortal

/* 
 *	Copyright (C) 2005-2009 Team MediaPortal
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
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using JH.CommBase;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.SerialIR
{
  /// <summary>
  /// This class will handle all communication with an external SerialUIR device
  /// The Serial UIR, allows your PC to Receive infrared signals -- 
  /// exactly like those used by the collection of remote controls you've acquired for your TV, 
  /// VCR, Audio System, etc. 
  /// </summary>
  public class LearningEventArgs : EventArgs
  {
    public string Button;

    public LearningEventArgs(string button)
    {
      this.Button = button;
    }
  }

  public class ListeningEventArgs : EventArgs
  {
    public string Code;

    public ListeningEventArgs(string Code)
    {
      this.Code = Code;
    }
  }

  public delegate void StartLearningEventHandler(object sender, LearningEventArgs e);

  public delegate void StartListeningEventHandler(object sender, ListeningEventArgs e);

  public class SerialUIR : CommBase
  {
    /// <summary>
    /// Private constructor disables the user from creating an instance of the class. All access
    /// to methods should be done through the singleton property "Instance".
    /// </summary>
    private SerialUIR()
    {
      remotefile = Config.GetFile(Config.Dir.Config, "remotevalues.xml");
    }

    public delegate void OnRemoteCommand(object command);

    public int LearningTimeOut = 4000; // time out in milliseconds
    public int CommandDelay = 300; // time between 2 commands

    private bool loaded = false;
    private OnRemoteCommand remoteCommandCallback = null;

    private bool recInternalCommands = false;
    private string remotefile;

    private static SerialUIR instance = null;

    /// <summary>
    /// Event fired when we start learning a remote command
    /// </summary>
    public event StartLearningEventHandler StartLearning;

    /// <summary>
    /// Event fired when we receive a code in config mode
    /// </summary>
    public event StartListeningEventHandler StartListening;

    public OnRemoteCommand RemoteCommandCallback
    {
      set { remoteCommandCallback = value; }
    }

    public new bool DTR
    {
      set { base.DTR = value; }
      get { return base.DTR; }
    }

    public new bool RTS
    {
      set { base.RTS = value; }
      get { return base.RTS; }
    }

    private string commport = "COM1:";
    private int baudrate = 9600;
    private string handshake = "None";
    private string parity = "None";
    private int irbytes = 6;
    private bool uirirmaninit = true;

    protected override CommBaseSettings CommSettings()
    {
      CommBaseSettings cs = new CommBaseSettings();
      Handshake hs;
      switch (handshake)
      {
        case "CtsRts":
          hs = Handshake.CtsRts;
          break;
        case "DsrDtr":
          hs = Handshake.DsrDtr;
          break;
        case "XonXoff":
          hs = Handshake.XonXoff;
          break;
        default:
          hs = Handshake.none;
          break;
      }
      cs.SetStandard(commport, baudrate, hs);
      switch (parity)
      {
        case "Odd":
          cs.parity = Parity.odd;
          break;
        case "Even":
          cs.parity = Parity.even;
          break;
        case "Mark":
          cs.parity = Parity.mark;
          break;
        case "Space":
          cs.parity = Parity.space;
          break;
        default:
          cs.parity = Parity.none;
          break;
      }

      return cs;
    }

    private Hashtable commandsLearned = new Hashtable();

    public static SerialUIR Create(OnRemoteCommand remoteCommandCallback)
    {
      try
      {
        if (instance == null)
        {
          instance = new SerialUIR(remoteCommandCallback);
        }
      }
      catch (Exception) {}
      return instance;
    }

    public static SerialUIR Instance
    {
      get { return instance; }
    }

    public bool InternalCommandsActive
    {
      get { return this.recInternalCommands; }

      set { this.recInternalCommands = value; }
    }

    public bool ReOpen()
    {
      base.Close();
      if (base.IsPortAvailable(commport) == PortStatus.available)
      {
        return base.Open();
      }
      else
      {
        return false;
      }
    }

    public bool SetPort(string CommPort)
    {
      commport = CommPort;
      return ReOpen();
    }

    public bool SetBaudRate(int BaudRate)
    {
      baudrate = BaudRate;
      return ReOpen();
    }

    public bool SetHandShake(string Handshake)
    {
      handshake = Handshake;
      return ReOpen();
    }

    public bool SetParity(string pr)
    {
      parity = pr;
      return ReOpen();
    }

    public bool SetIRBytes(int IRBytes)
    {
      irbytes = IRBytes;
      return ReOpen();
    }

    public bool SetUIRIRmanInit(bool UIRIRManInit)
    {
      uirirmaninit = UIRIRManInit;
      return ReOpen();
    }

    private SerialUIR(OnRemoteCommand remoteCommandCallback) : this()
    {
      try
      {
        using (Settings xmlreader = new MPSettings())
        {
          recInternalCommands = xmlreader.GetValueAsString("SerialUIR", "internal", "false") == "true";
          commport = xmlreader.GetValueAsString("SerialUIR", "commport", "COM1:");
          baudrate = xmlreader.GetValueAsInt("SerialUIR", "baudrate", 9600);
          handshake = xmlreader.GetValueAsString("SerialUIR", "handshake", "None");
          irbytes = xmlreader.GetValueAsInt("SerialUIR", "irbytes", 6);
          uirirmaninit = xmlreader.GetValueAsString("SerialUIR", "uirirmaninit", "true") == "true";
          LearningTimeOut = 1000 * xmlreader.GetValueAsInt("SerialUIR", "timeout", 4);
          CommandDelay = xmlreader.GetValueAsInt("SerialUIR", "delay", 300);
        }
        this.remoteCommandCallback = remoteCommandCallback;
        commandsLearned = new Hashtable();
        if (File.Exists(remotefile))
        {
          LoadValues();
        }
        base.Open();
      }
      catch (Exception)
      {
        //most users dont have serial device on their system so will get a exception here
      }
    }

    public string GetName()
    {
      return "SerialUIR";
    }

    private void LoadValues()
    {
      XmlTextReader reader = new XmlTextReader(remotefile);
      reader.MoveToContent();
      // skip the document element
      reader.Read();

      while (!reader.EOF)
      {
        Debug.Assert(reader.LocalName == "entry");
        try
        {
          int actionInt = Int32.Parse(reader.GetAttribute("actionInt"));
          reader.Read();
          string remoteCode = reader.ReadString();
          reader.ReadEndElement();
          commandsLearned[remoteCode] = actionInt;
        }
        catch (Exception)
        {
          break;
        }
        if (reader.LocalName != "entry")
        {
          break;
        }
      }
      reader.Close();
    }

    public void SaveInternalValues()
    {
      try
      {
        XmlTextWriter writer = new XmlTextWriter(remotefile, Encoding.Unicode);

        writer.WriteStartElement("docElement");
        foreach (string key in commandsLearned.Keys)
        {
          writer.WriteStartElement("entry");
          writer.WriteAttributeString("actionInt", ((int)commandsLearned[key]).ToString());
          writer.WriteAttributeString("actionDescription", (commandsLearned[key]).ToString());
          writer.WriteString(key);
          writer.WriteEndElement();
        }
        writer.WriteEndElement();
        writer.Close();
      }
      catch (Exception) {}
    }

    public string GetVersions()
    {
      if (loaded)
      {
        DateTime plugdate = new DateTime(2004, 12, 10);
        string plug = "Plugin Version: 1.1 (" + plugdate.ToString("MMMM, dd, yyyy") + ")";
        return plug;
      }
      else
      {
        return "plugin is offline";
      }
    }

    /// <summary>
    /// Method used to fire the "StartLearning" event. Any subscribers will be notified with the name of
    /// the button that is to be learned.
    /// </summary>
    /// <param name="button"></param>
    protected void OnStartLearning(string button)
    {
      if (StartLearning != null)
      {
        StartLearning(this, new LearningEventArgs(button));
      }
    }

    /// <summary>
    /// Method used to fire the "StartListening" event.
    /// Any subscribers will be notified with the Code received
    /// </summary>
    /// <param name="button"></param>
    protected void OnStartListening(string Code)
    {
      if (StartListening != null)
      {
        StartListening(this, new ListeningEventArgs(Code));
      }
    }

    private object commandToLearn = null;
    private bool internalLearnDone = false;

    private bool InternalLearn(object command, string buttonName)
    {
      try
      {
        this.commandToLearn = command;
        internalLearnDone = false;

        OnStartLearning(buttonName);

        timestamp = DateTime.Now;

        while (! internalLearnDone)
        {
          base.Sleep(10);
          if ((DateTime.Now - timestamp) > new TimeSpan(0, 0, 0, 0, LearningTimeOut))
          {
            return false;
          }
        }
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    private bool learning = false;

    public void BulkLearn(object[] commands, string[] buttonNames)
    {
      //erase old learn
      commandsLearned = new Hashtable();
      if (commands.Length != buttonNames.Length)
      {
        throw new Exception("invalid call to BulkLearn");
      }
      for (int i = 0; i < commands.Length; i++)
      {
        learning = true;
        if (! InternalLearn(commands[i], buttonNames[i]))
        {
          switch (
            MessageBox.Show("Learn failed (timeout). Try again ?", "IR Learn failed", MessageBoxButtons.AbortRetryIgnore,
                            MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1))
          {
            case DialogResult.Abort:
              i = commands.Length;
              break;
            case DialogResult.Retry:
              i--;
              continue;
          }
        }
      }
      learning = false;
    }

    private bool ignore = true;
    private int charcount = 0;
    private string lastcommand;
    private string lastcommand2;
    private string lastbuff;

    //string lastCommand = null;
    private DateTime timestamp = DateTime.Now;

    private DateTime bytetimestamp = DateTime.Now;

    protected override void OnRxChar(byte ch)
    {
      if (ignore)
      {
        return;
      }

      if ((DateTime.Now - bytetimestamp) > new TimeSpan(0, 0, 0, 0, 5 * CommandDelay))
      {
        Trace.WriteLine("IR byte train timeout");
        charcount = 0;
        lastbuff = "";
      }
      bytetimestamp = DateTime.Now;

      charcount = (charcount + 1) % irbytes;

      if (charcount == 0)
      {
        lastcommand2 = lastcommand;
        lastcommand = lastbuff + (ch < 16 ? "0" : "") + ch.ToString("X");
        lastbuff = "";

        Trace.WriteLine("received IR command " + lastcommand + " " + DateTime.Now.ToString());
        if (learning)
        {
          if (lastcommand.Equals(lastcommand2))
          {
            if (commandsLearned[lastcommand] == null)
            {
              learning = false;
              internalLearnDone = true;
              commandsLearned[lastcommand] = commandToLearn;
            }
          }
        }
        else
        {
          internalLearnDone = true;
          if (recInternalCommands)
          {
            object command = commandsLearned[lastcommand];
            if (((DateTime.Now - timestamp) > new TimeSpan(0, 0, 0, 0, CommandDelay)))
            {
              if (command != null)
              {
                OnStartListening(lastcommand + " => " + ((Action.ActionType)command).ToString());
                this.remoteCommandCallback(command);
                timestamp = DateTime.Now;
              }
              else
              {
                OnStartListening(lastcommand + " => No Action");
              }
            }
          }
        }
      }
      else
      {
        lastbuff += (ch < 16 ? "0" : "") + ch.ToString("X");
      }
    }

    protected override bool AfterOpen()
    {
      Sleep(50);
      ignore = false;
      if (uirirmaninit)
      {
        base.SendImmediate((byte)'I');
        Sleep(10);
        base.SendImmediate((byte)'R');
      }
      return true;
    }
  }
}