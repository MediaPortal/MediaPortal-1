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

using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml.Serialization;

//JH 1.1: Version 1.1 changes labelled thus.
//JH 1.2: Version 1.2 changes labelled thus.
//JH 1.3: Version 1.3 changes labelled thus.

namespace JH.CommBase
{
  /// <summary>
  /// Lowest level Com driver handling all Win32 API calls and processing send and receive in terms of
  /// individual bytes. Used as a base class for higher level drivers.
  /// </summary>
  public abstract class CommBase : IDisposable
  {
    private IntPtr hPort;
    private IntPtr ptrUWO = IntPtr.Zero;
    private Thread rxThread = null;
    private bool online = false;
    private bool auto = false;
    private bool checkSends = true;
    private Exception rxException = null;
    private bool rxExceptionReported = false;
    private int writeCount = 0;
    private ManualResetEvent writeEvent = new ManualResetEvent(false);
    //JH 1.2: Added below to improve robustness of thread start-up.
    private ManualResetEvent startEvent = new ManualResetEvent(false);
    private int stateRTS = 2;
    private int stateDTR = 2;
    private int stateBRK = 2;
    //JH 1.3: Added to support the new congestion detection scheme (following two lines):
    private bool[] empty = new bool[1];
    private bool dataQueued = false;


    /// <summary>
    /// Parity settings
    /// </summary>
    public enum Parity
    {
      /// <summary>
      /// Characters do not have a parity bit.
      /// </summary>
      none = 0,
      /// <summary>
      /// If there are an odd number of 1s in the data bits, the parity bit is 1.
      /// </summary>
      odd = 1,
      /// <summary>
      /// If there are an even number of 1s in the data bits, the parity bit is 1.
      /// </summary>
      even = 2,
      /// <summary>
      /// The parity bit is always 1.
      /// </summary>
      mark = 3,
      /// <summary>
      /// The parity bit is always 0.
      /// </summary>
      space = 4
    } ;

    /// <summary>
    /// Stop bit settings
    /// </summary>
    public enum StopBits
    {
      /// <summary>
      /// Line is asserted for 1 bit duration at end of each character
      /// </summary>
      one = 0,
      /// <summary>
      /// Line is asserted for 1.5 bit duration at end of each character
      /// </summary>
      onePointFive = 1,
      /// <summary>
      /// Line is asserted for 2 bit duration at end of each character
      /// </summary>
      two = 2
    } ;

    /// <summary>
    /// Uses for RTS or DTR pins
    /// </summary>
    public enum HSOutput
    {
      /// <summary>
      /// Pin is asserted when this station is able to receive data.
      /// </summary>
      handshake = 2,
      /// <summary>
      /// Pin is asserted when this station is transmitting data (RTS on NT, 2000 or XP only).
      /// </summary>
      gate = 3,
      /// <summary>
      /// Pin is asserted when this station is online (port is open).
      /// </summary>
      online = 1,
      /// <summary>
      /// Pin is never asserted.
      /// </summary>
      none = 0
    } ;

    /// <summary>
    /// Standard handshake methods
    /// </summary>
    public enum Handshake
    {
      /// <summary>
      /// No handshaking
      /// </summary>
      none,
      /// <summary>
      /// Software handshaking using Xon / Xoff
      /// </summary>
      XonXoff,
      /// <summary>
      /// Hardware handshaking using CTS / RTS
      /// </summary>
      CtsRts,
      /// <summary>
      /// Hardware handshaking using DSR / DTR
      /// </summary>
      DsrDtr
    }

    /// <summary>
    /// Set the public fields to supply settings to CommBase.
    /// </summary>
    public class CommBaseSettings
    {
      /// <summary>
      /// Port Name (default: "COM1:")
      /// </summary>
      public string port = "COM1:";

      /// <summary>
      /// Baud Rate (default: 2400) unsupported rates will throw "Bad settings"
      /// </summary>
      public int baudRate = 9600;

      /// <summary>
      /// The parity checking scheme (default: none)
      /// </summary>
      public Parity parity = Parity.none;

      /// <summary>
      /// Number of databits 1..8 (default: 8) unsupported values will throw "Bad settings"
      /// </summary>
      public int dataBits = 8;

      /// <summary>
      /// Number of stop bits (default: one)
      /// </summary>
      public StopBits stopBits = StopBits.one;

      /// <summary>
      /// If true, transmission is halted unless CTS is asserted by the remote station (default: false)
      /// </summary>
      public bool txFlowCTS = false;

      /// <summary>
      /// If true, transmission is halted unless DSR is asserted by the remote station (default: false)
      /// </summary>
      public bool txFlowDSR = false;

      /// <summary>
      /// If true, transmission is halted when Xoff is received and restarted when Xon is received (default: false)
      /// </summary>
      public bool txFlowX = false;

      /// <summary>
      /// If false, transmission is suspended when this station has sent Xoff to the remote station (default: true)
      /// Set false if the remote station treats any character as an Xon.
      /// </summary>
      public bool txWhenRxXoff = true;

      /// <summary>
      /// If true, received characters are ignored unless DSR is asserted by the remote station (default: false)
      /// </summary>
      public bool rxGateDSR = false;

      /// <summary>
      /// If true, Xon and Xoff characters are sent to control the data flow from the remote station (default: false)
      /// </summary>
      public bool rxFlowX = false;

      /// <summary>
      /// Specifies the use to which the RTS output is put (default: none)
      /// </summary>
      public HSOutput useRTS = HSOutput.none;

      /// <summary>
      /// Specidies the use to which the DTR output is put (default: none)
      /// </summary>
      public HSOutput useDTR = HSOutput.none;

      /// <summary>
      /// The character used to signal Xon for X flow control (default: DC1)
      /// </summary>
      public ASCII XonChar = ASCII.DC1;

      /// <summary>
      /// The character used to signal Xoff for X flow control (default: DC3)
      /// </summary>
      public ASCII XoffChar = ASCII.DC3;

      //JH 1.2: Next two defaults changed to 0 to use new defaulting mechanism dependant on queue size.
      /// <summary>
      /// The number of free bytes in the reception queue at which flow is disabled
      /// (Default: 0 = Set to 1/10th of actual rxQueue size)
      /// </summary>
      public int rxHighWater = 0;

      /// <summary>
      /// The number of bytes in the reception queue at which flow is re-enabled
      /// (Default: 0 = Set to 1/10th of actual rxQueue size)
      /// </summary>
      public int rxLowWater = 0;

      /// <summary>
      /// Multiplier. Max time for Send in ms = (Multiplier * Characters) + Constant
      /// (default: 0 = No timeout)
      /// </summary>
      public uint sendTimeoutMultiplier = 0;

      /// <summary>
      /// Constant.  Max time for Send in ms = (Multiplier * Characters) + Constant (default: 0)
      /// </summary>
      public uint sendTimeoutConstant = 0;

      /// <summary>
      /// Requested size for receive queue (default: 0 = use operating system default)
      /// </summary>
      public int rxQueue = 0;

      /// <summary>
      /// Requested size for transmit queue (default: 0 = use operating system default)
      /// </summary>
      public int txQueue = 0;

      /// <summary>
      /// If true, the port will automatically re-open on next send if it was previously closed due
      /// to an error (default: false)
      /// </summary>
      public bool autoReopen = false;

      /// <summary>
      /// If true, subsequent Send commands wait for completion of earlier ones enabling the results
      /// to be checked. If false, errors, including timeouts, may not be detected, but performance
      /// may be better.
      /// </summary>
      public bool checkAllSends = true;

      /// <summary>
      /// Pre-configures settings for most modern devices: 8 databits, 1 stop bit, no parity and
      /// one of the common handshake protocols. Change individual settings later if necessary.
      /// </summary>
      /// <param name="Port">The port to use (i.e. "COM1:")</param>
      /// <param name="Baud">The baud rate</param>
      /// <param name="Hs">The handshake protocol</param>
      public void SetStandard(string Port, int Baud, Handshake Hs)
      {
        dataBits = 8;
        stopBits = StopBits.one;
        parity = Parity.none;
        port = Port;
        baudRate = Baud;
        switch (Hs)
        {
          case Handshake.none:
            txFlowCTS = false;
            txFlowDSR = false;
            txFlowX = false;
            rxFlowX = false;
            useRTS = HSOutput.online;
            useDTR = HSOutput.online;
            txWhenRxXoff = true;
            rxGateDSR = false;
            break;
          case Handshake.XonXoff:
            txFlowCTS = false;
            txFlowDSR = false;
            txFlowX = true;
            rxFlowX = true;
            useRTS = HSOutput.online;
            useDTR = HSOutput.online;
            txWhenRxXoff = true;
            rxGateDSR = false;
            XonChar = ASCII.DC1;
            XoffChar = ASCII.DC3;
            break;
          case Handshake.CtsRts:
            txFlowCTS = true;
            txFlowDSR = false;
            txFlowX = false;
            rxFlowX = false;
            useRTS = HSOutput.handshake;
            useDTR = HSOutput.online;
            txWhenRxXoff = true;
            rxGateDSR = false;
            break;
          case Handshake.DsrDtr:
            txFlowCTS = false;
            txFlowDSR = true;
            txFlowX = false;
            rxFlowX = false;
            useRTS = HSOutput.online;
            useDTR = HSOutput.handshake;
            txWhenRxXoff = true;
            rxGateDSR = false;
            break;
        }
      }

      /// <summary>
      /// Save the object in XML format to a stream
      /// </summary>
      /// <param name="s">Stream to save the object to</param>
      public void SaveAsXML(Stream s)
      {
        XmlSerializer sr = new XmlSerializer(this.GetType());
        sr.Serialize(s, this);
      }

      /// <summary>
      /// Create a new CommBaseSettings object initialised from XML data
      /// </summary>
      /// <param name="s">Stream to load the XML from</param>
      /// <returns>CommBaseSettings object</returns>
      public static CommBaseSettings LoadFromXML(Stream s)
      {
        return LoadFromXML(s, typeof (CommBaseSettings));
      }

      /// <summary>
      /// Create a new object loading members from the stream in XML format.
      /// Derived class should call this from a static method i.e.:
      /// return (ComDerivedSettings)LoadFromXML(s, typeof(ComDerivedSettings));
      /// </summary>
      /// <param name="s">Stream to load the object from</param>
      /// <param name="t">Type of the derived object</param>
      /// <returns></returns>
      protected static CommBaseSettings LoadFromXML(Stream s, Type t)
      {
        XmlSerializer sr = new XmlSerializer(t);
        try
        {
          return (CommBaseSettings)sr.Deserialize(s);
        }
        catch
        {
          return null;
        }
      }
    }

    //JH 1.3: Corrected STH -> STX (Thanks - Johan Thelin!)
    /// <summary>
    /// Byte type with enumeration constants for ASCII control codes.
    /// </summary>
    public enum ASCII : byte
    {
      NULL = 0x00,
      SOH = 0x01,
      STX = 0x02,
      ETX = 0x03,
      EOT = 0x04,
      ENQ = 0x05,
      ACK = 0x06,
      BELL = 0x07,
      BS = 0x08,
      HT = 0x09,
      LF = 0x0A,
      VT = 0x0B,
      FF = 0x0C,
      CR = 0x0D,
      SO = 0x0E,
      SI = 0x0F,
      DC1 = 0x11,
      DC2 = 0x12,
      DC3 = 0x13,
      DC4 = 0x14,
      NAK = 0x15,
      SYN = 0x16,
      ETB = 0x17,
      CAN = 0x18,
      EM = 0x19,
      SUB = 0x1A,
      ESC = 0x1B,
      FS = 0x1C,
      GS = 0x1D,
      RS = 0x1E,
      US = 0x1F,
      SP = 0x20,
      DEL = 0x7F
    }


    //JH 1.3: Added AltName function, PortStatus enum and IsPortAvailable function.

    /// <summary>
    /// Returns the alternative name of a com port i.e. \\.\COM1 for COM1:
    /// Some systems require this form for double or more digit com port numbers.
    /// </summary>
    /// <param name="s">Name in form COM1 or COM1:</param>
    /// <returns>Name in form \\.\COM1</returns>
    private string AltName(string s)
    {
      string r = s.Trim();
      if (s.EndsWith(":"))
      {
        s = s.Substring(0, s.Length - 1);
      }
      if (s.StartsWith(@"\"))
      {
        return s;
      }
      return @"\\.\" + s;
    }

    /// <summary>
    /// Availability status of a port
    /// </summary>
    public enum PortStatus
    {
      /// <summary>
      /// Port exists but is unavailable (may be open to another program)
      /// </summary>
      unavailable = 0,
      /// <summary>
      /// Available for use
      /// </summary>
      available = 1,
      /// <summary>
      /// Port does not exist
      /// </summary>
      absent = -1
    }

    /// <summary>
    /// Tests the availability of a named comm port.
    /// </summary>
    /// <param name="s">Name of port</param>
    /// <returns>Availability of port</returns>
    public PortStatus IsPortAvailable(string s)
    {
      IntPtr h;

      h = Win32Com.CreateFile(s, Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
                              Win32Com.OPEN_EXISTING, Win32Com.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
      if (h == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
      {
        if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
        {
          return PortStatus.unavailable;
        }
        else
        {
          //JH 1.3: Automatically try AltName if supplied name fails:
          h = Win32Com.CreateFile(AltName(s), Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
                                  Win32Com.OPEN_EXISTING, Win32Com.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
          if (h == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
          {
            if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
            {
              return PortStatus.unavailable;
            }
            else
            {
              return PortStatus.absent;
            }
          }
        }
      }
      Win32Com.CloseHandle(h);
      return PortStatus.available;
    }

    /// <summary>
    /// Opens the com port and configures it with the required settings
    /// </summary>
    /// <returns>false if the port could not be opened</returns>
    public bool Open()
    {
      Win32Com.DCB PortDCB = new Win32Com.DCB();
      Win32Com.COMMTIMEOUTS CommTimeouts = new Win32Com.COMMTIMEOUTS();
      CommBaseSettings cs;
      Win32Com.OVERLAPPED wo = new Win32Com.OVERLAPPED();
      Win32Com.COMMPROP cp;

      if (online)
      {
        return false;
      }
      cs = CommSettings();

      hPort = Win32Com.CreateFile(cs.port, Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
                                  Win32Com.OPEN_EXISTING, Win32Com.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
      if (hPort == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
      {
        if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
        {
          return false;
        }
        else
        {
          //JH 1.3: Try alternative name form if main one fails:
          hPort = Win32Com.CreateFile(AltName(cs.port), Win32Com.GENERIC_READ | Win32Com.GENERIC_WRITE, 0, IntPtr.Zero,
                                      Win32Com.OPEN_EXISTING, Win32Com.FILE_FLAG_OVERLAPPED, IntPtr.Zero);
          if (hPort == (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
          {
            if (Marshal.GetLastWin32Error() == Win32Com.ERROR_ACCESS_DENIED)
            {
              return false;
            }
            else
            {
              throw new CommPortException("Port Open Failure");
            }
          }
        }
      }

      online = true;

      //JH1.1: Changed from 0 to "magic number" to give instant return on ReadFile:
      CommTimeouts.ReadIntervalTimeout = Win32Com.MAXDWORD;
      CommTimeouts.ReadTotalTimeoutConstant = 0;
      CommTimeouts.ReadTotalTimeoutMultiplier = 0;

      //JH1.2: 0 does not seem to mean infinite on non-NT platforms, so default it to 10
      //seconds per byte which should be enough for anyone.
      if (cs.sendTimeoutMultiplier == 0)
      {
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
          CommTimeouts.WriteTotalTimeoutMultiplier = 0;
        }
        else
        {
          CommTimeouts.WriteTotalTimeoutMultiplier = 10000;
        }
      }
      else
      {
        CommTimeouts.WriteTotalTimeoutMultiplier = cs.sendTimeoutMultiplier;
      }
      CommTimeouts.WriteTotalTimeoutConstant = cs.sendTimeoutConstant;

      PortDCB.init(((cs.parity == Parity.odd) || (cs.parity == Parity.even)), cs.txFlowCTS, cs.txFlowDSR,
                   (int)cs.useDTR, cs.rxGateDSR, !cs.txWhenRxXoff, cs.txFlowX, cs.rxFlowX, (int)cs.useRTS);
      PortDCB.BaudRate = cs.baudRate;
      PortDCB.ByteSize = (byte)cs.dataBits;
      PortDCB.Parity = (byte)cs.parity;
      PortDCB.StopBits = (byte)cs.stopBits;
      PortDCB.XoffChar = (byte)cs.XoffChar;
      PortDCB.XonChar = (byte)cs.XonChar;
      if ((cs.rxQueue != 0) || (cs.txQueue != 0))
      {
        if (!Win32Com.SetupComm(hPort, (uint)cs.rxQueue, (uint)cs.txQueue))
        {
          ThrowException("Bad queue settings");
        }
      }

      //JH 1.2: Defaulting mechanism for handshake thresholds - prevents problems of setting specific
      //defaults which may violate the size of the actually granted queue. If the user specifically sets
      //these values, it's their problem!
      if ((cs.rxLowWater == 0) || (cs.rxHighWater == 0))
      {
        if (!Win32Com.GetCommProperties(hPort, out cp))
        {
          cp.dwCurrentRxQueue = 0;
        }
        if (cp.dwCurrentRxQueue > 0)
        {
          //If we can determine the queue size, default to 1/10th, 8/10ths, 1/10th.
          //Note that HighWater is measured from top of queue.
          PortDCB.XoffLim = PortDCB.XonLim = (short)((int)cp.dwCurrentRxQueue / 10);
        }
        else
        {
          //If we do not know the queue size, set very low defaults for safety.
          PortDCB.XoffLim = PortDCB.XonLim = 8;
        }
      }
      else
      {
        PortDCB.XoffLim = (short)cs.rxHighWater;
        PortDCB.XonLim = (short)cs.rxLowWater;
      }

      if (!Win32Com.SetCommState(hPort, ref PortDCB))
      {
        MediaPortal.GUI.Library.Log.Error("CommBase: Bad com settings");
        return false;
      }
      if (!Win32Com.SetCommTimeouts(hPort, ref CommTimeouts))
      {
        MediaPortal.GUI.Library.Log.Error("CommBase: Bad timeout settings");
        return false;
      }

      stateBRK = 0;
      if (cs.useDTR == HSOutput.none)
      {
        stateDTR = 0;
      }
      if (cs.useDTR == HSOutput.online)
      {
        stateDTR = 1;
      }
      if (cs.useRTS == HSOutput.none)
      {
        stateRTS = 0;
      }
      if (cs.useRTS == HSOutput.online)
      {
        stateRTS = 1;
      }

      checkSends = cs.checkAllSends;
      wo.Offset = 0;
      wo.OffsetHigh = 0;
      if (checkSends)
      {
        wo.hEvent = writeEvent.SafeWaitHandle.DangerousGetHandle();
      }
      else
      {
        wo.hEvent = IntPtr.Zero;
      }

      ptrUWO = Marshal.AllocHGlobal(Marshal.SizeOf(wo));

      Marshal.StructureToPtr(wo, ptrUWO, true);
      writeCount = 0;
      //JH1.3:
      empty[0] = true;
      dataQueued = false;

      rxException = null;
      rxExceptionReported = false;
      rxThread = new Thread(new ThreadStart(this.ReceiveThread));
      rxThread.IsBackground = true;
      rxThread.Name = "CommBaseRx";
      rxThread.Priority = ThreadPriority.AboveNormal;
      rxThread.Start();

      //JH1.2: More robust thread start-up wait.
      startEvent.WaitOne(500, false);

      auto = false;
      if (AfterOpen())
      {
        auto = cs.autoReopen;
        return true;
      }
      else
      {
        Close();
        return false;
      }
    }

    /// <summary>
    /// Closes the com port.
    /// </summary>
    public virtual void Close()
    {
      if (online)
      {
        auto = false;
        BeforeClose(false);
        InternalClose();
        rxException = null;
      }
    }

    private void InternalClose()
    {
      Win32Com.CancelIo(hPort);
      if (rxThread != null)
      {
        rxThread.Abort();
        //JH 1.3: Improve robustness of Close in case were followed by Open:
        rxThread.Join(100);
        rxThread = null;
      }
      Win32Com.CloseHandle(hPort);
      if (ptrUWO != IntPtr.Zero)
      {
        Marshal.FreeHGlobal(ptrUWO);
      }
      stateRTS = 2;
      stateDTR = 2;
      stateBRK = 2;
      online = false;
    }

    /// <summary>
    /// For IDisposable
    /// </summary>
    public void Dispose()
    {
      Close();
    }

    /// <summary>
    /// Destructor (just in case)
    /// </summary>
    ~CommBase()
    {
      Close();
    }

    /// <summary>
    /// True if online.
    /// </summary>
    public bool Online
    {
      get
      {
        if (!online)
        {
          return false;
        }
        else
        {
          return CheckOnline();
        }
      }
    }

    /// <summary>
    /// Block until all bytes in the queue have been transmitted.
    /// </summary>
    public void Flush()
    {
      CheckOnline();
      CheckResult();
    }

    /// <summary>
    /// Use this to throw exceptions in derived classes. Correctly handles threading issues
    /// and closes the port if necessary.
    /// </summary>
    /// <param name="reason">Description of fault</param>
    protected void ThrowException(string reason)
    {
      if (Thread.CurrentThread == rxThread)
      {
        throw new CommPortException(reason);
      }
      else
      {
        if (online)
        {
          BeforeClose(true);
          InternalClose();
        }
        if (rxException == null)
        {
          throw new CommPortException(reason);
        }
        else
        {
          //				throw new CommPortException(rxException);
        }
      }
    }

    /// <summary>
    /// Queues bytes for transmission. 
    /// </summary>
    /// <param name="tosend">Array of bytes to be sent</param>
    protected void Send(byte[] tosend)
    {
      uint sent = 0;
      CheckOnline();
      CheckResult();
      writeCount = tosend.GetLength(0);
      if (Win32Com.WriteFile(hPort, tosend, (uint)writeCount, out sent, ptrUWO))
      {
        writeCount -= (int)sent;
      }
      else
      {
        if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_PENDING)
        {
          ThrowException("Send failed");
        }
        //JH1.3:
        dataQueued = true;
      }
    }

    /// <summary>
    /// Queues a single byte for transmission.
    /// </summary>
    /// <param name="tosend">Byte to be sent</param>
    protected void Send(byte tosend)
    {
      byte[] b = new byte[1];
      b[0] = tosend;
      Send(b);
    }

    private void CheckResult()
    {
      uint sent = 0;

      //JH 1.3: Fixed a number of problems working with checkSends == false. Byte counting was unreliable because
      //occasionally GetOverlappedResult would return true with a completion having missed one or more previous
      //completions. The test for ERROR_IO_INCOMPLETE was incorrectly for ERROR_IO_PENDING instead.

      if (writeCount > 0)
      {
        if (Win32Com.GetOverlappedResult(hPort, ptrUWO, out sent, checkSends))
        {
          if (checkSends)
          {
            writeCount -= (int)sent;
            if (writeCount != 0)
            {
              ThrowException("Send Timeout");
            }
            writeCount = 0;
          }
        }
        else
        {
          if (Marshal.GetLastWin32Error() != Win32Com.ERROR_IO_INCOMPLETE)
          {
            ThrowException("Write Error");
          }
        }
      }
    }

    /// <summary>
    /// Sends a protocol byte immediately ahead of any queued bytes.
    /// </summary>
    /// <param name="tosend">Byte to send</param>
    protected void SendImmediate(byte tosend)
    {
      CheckOnline();
      if (!Win32Com.TransmitCommChar(hPort, tosend))
      {
        ThrowException("Transmission failure");
      }
    }

    /// <summary>
    /// Delay processing.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to delay by</param>
    protected void Sleep(int milliseconds)
    {
      Thread.Sleep(milliseconds);
    }

    /// <summary>
    /// Represents the status of the modem control input signals.
    /// </summary>
    public struct ModemStatus
    {
      private uint status;

      internal ModemStatus(uint val)
      {
        status = val;
      }

      /// <summary>
      /// Condition of the Clear To Send signal.
      /// </summary>
      public bool cts
      {
        get { return ((status & Win32Com.MS_CTS_ON) != 0); }
      }

      /// <summary>
      /// Condition of the Data Set Ready signal.
      /// </summary>
      public bool dsr
      {
        get { return ((status & Win32Com.MS_DSR_ON) != 0); }
      }

      /// <summary>
      /// Condition of the Receive Line Status Detection signal.
      /// </summary>
      public bool rlsd
      {
        get { return ((status & Win32Com.MS_RLSD_ON) != 0); }
      }

      /// <summary>
      /// Condition of the Ring Detection signal.
      /// </summary>
      public bool ring
      {
        get { return ((status & Win32Com.MS_RING_ON) != 0); }
      }
    }

    /// <summary>
    /// Gets the status of the modem control input signals.
    /// </summary>
    /// <returns>Modem status object</returns>
    protected ModemStatus GetModemStatus()
    {
      uint f;

      CheckOnline();
      if (!Win32Com.GetCommModemStatus(hPort, out f))
      {
        ThrowException("Unexpected failure");
      }
      return new ModemStatus(f);
    }

    /// <summary>
    /// Represents the current condition of the port queues.
    /// </summary>
    public struct QueueStatus
    {
      private uint status;
      private uint inQueue;
      private uint outQueue;
      private uint inQueueSize;
      private uint outQueueSize;

      internal QueueStatus(uint stat, uint inQ, uint outQ, uint inQs, uint outQs)
      {
        status = stat;
        inQueue = inQ;
        outQueue = outQ;
        inQueueSize = inQs;
        outQueueSize = outQs;
      }

      /// <summary>
      /// Output is blocked by CTS handshaking.
      /// </summary>
      public bool ctsHold
      {
        get { return ((status & Win32Com.COMSTAT.fCtsHold) != 0); }
      }

      /// <summary>
      /// Output is blocked by DRS handshaking.
      /// </summary>
      public bool dsrHold
      {
        get { return ((status & Win32Com.COMSTAT.fDsrHold) != 0); }
      }

      /// <summary>
      /// Output is blocked by RLSD handshaking.
      /// </summary>
      public bool rlsdHold
      {
        get { return ((status & Win32Com.COMSTAT.fRlsdHold) != 0); }
      }

      /// <summary>
      /// Output is blocked because software handshaking is enabled and XOFF was received.
      /// </summary>
      public bool xoffHold
      {
        get { return ((status & Win32Com.COMSTAT.fXoffHold) != 0); }
      }

      /// <summary>
      /// Output was blocked because XOFF was sent and this station is not yet ready to receive.
      /// </summary>
      public bool xoffSent
      {
        get { return ((status & Win32Com.COMSTAT.fXoffSent) != 0); }
      }

      /// <summary>
      /// There is a character waiting for transmission in the immediate buffer.
      /// </summary>
      public bool immediateWaiting
      {
        get { return ((status & Win32Com.COMSTAT.fTxim) != 0); }
      }

      /// <summary>
      /// Number of bytes waiting in the input queue.
      /// </summary>
      public long InQueue
      {
        get { return (long)inQueue; }
      }

      /// <summary>
      /// Number of bytes waiting for transmission.
      /// </summary>
      public long OutQueue
      {
        get { return (long)outQueue; }
      }

      /// <summary>
      /// Total size of input queue (0 means information unavailable)
      /// </summary>
      public long InQueueSize
      {
        get { return (long)inQueueSize; }
      }

      /// <summary>
      /// Total size of output queue (0 means information unavailable)
      /// </summary>
      public long OutQueueSize
      {
        get { return (long)outQueueSize; }
      }

      public override string ToString()
      {
        StringBuilder m = new StringBuilder("The reception queue is ", 60);
        if (inQueueSize == 0)
        {
          m.Append("of unknown size and ");
        }
        else
        {
          m.Append(inQueueSize.ToString() + " bytes long and ");
        }
        if (inQueue == 0)
        {
          m.Append("is empty.");
        }
        else if (inQueue == 1)
        {
          m.Append("contains 1 byte.");
        }
        else
        {
          m.Append("contains ");
          m.Append(inQueue.ToString());
          m.Append(" bytes.");
        }
        m.Append(" The transmission queue is ");
        if (outQueueSize == 0)
        {
          m.Append("of unknown size and ");
        }
        else
        {
          m.Append(outQueueSize.ToString() + " bytes long and ");
        }
        if (outQueue == 0)
        {
          m.Append("is empty");
        }
        else if (outQueue == 1)
        {
          m.Append("contains 1 byte. It is ");
        }
        else
        {
          m.Append("contains ");
          m.Append(outQueue.ToString());
          m.Append(" bytes. It is ");
        }
        if (outQueue > 0)
        {
          if (ctsHold || dsrHold || rlsdHold || xoffHold || xoffSent)
          {
            m.Append("holding on");
            if (ctsHold)
            {
              m.Append(" CTS");
            }
            if (dsrHold)
            {
              m.Append(" DSR");
            }
            if (rlsdHold)
            {
              m.Append(" RLSD");
            }
            if (xoffHold)
            {
              m.Append(" Rx XOff");
            }
            if (xoffSent)
            {
              m.Append(" Tx XOff");
            }
          }
          else
          {
            m.Append("pumping data");
          }
        }
        m.Append(". The immediate buffer is ");
        if (immediateWaiting)
        {
          m.Append("full.");
        }
        else
        {
          m.Append("empty.");
        }
        return m.ToString();
      }
    }

    /// <summary>
    /// Get the status of the queues
    /// </summary>
    /// <returns>Queue status object</returns>
    protected QueueStatus GetQueueStatus()
    {
      Win32Com.COMSTAT cs;
      Win32Com.COMMPROP cp;
      uint er;

      CheckOnline();
      if (!Win32Com.ClearCommError(hPort, out er, out cs))
      {
        ThrowException("Unexpected failure");
      }
      if (!Win32Com.GetCommProperties(hPort, out cp))
      {
        ThrowException("Unexpected failure");
      }
      return new QueueStatus(cs.Flags, cs.cbInQue, cs.cbOutQue, cp.dwCurrentRxQueue, cp.dwCurrentTxQueue);
    }

    // JH 1.3. Added for this version.
    /// <summary>
    /// Test if the line is congested (data being queued for send faster than it is being dequeued)
    /// This detects if baud rate is too slow or if handshaking is not allowing enough transmission
    /// time. It should be called at reasonably long fixed intervals. If data has been sent during
    /// the interval, congestion is reported if the queue was never empty during the interval.
    /// </summary>
    /// <returns>True if congested</returns>
    protected bool IsCongested()
    {
      bool e;
      if (!dataQueued)
      {
        return false;
      }
      lock (empty)
      {
        e = empty[0];
        empty[0] = false;
      }
      dataQueued = false;
      return !e;
    }

    /// <summary>
    /// True if the RTS pin is controllable via the RTS property
    /// </summary>
    protected bool RTSavailable
    {
      get { return (stateRTS < 2); }
    }

    /// <summary>
    /// Set the state of the RTS modem control output
    /// </summary>
    protected bool RTS
    {
      set
      {
        if (stateRTS > 1)
        {
          return;
        }
        CheckOnline();
        if (value)
        {
          if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETRTS))
          {
            stateRTS = 1;
          }
          else
          {
            ThrowException("Unexpected Failure");
          }
        }
        else
        {
          if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRRTS))
          {
            //JH 1.3: Was 1, should be 0:
            stateRTS = 0;
          }
          else
          {
            ThrowException("Unexpected Failure");
          }
        }
      }
      get { return (stateRTS == 1); }
    }

    /// <summary>
    /// True if the DTR pin is controllable via the DTR property
    /// </summary>
    protected bool DTRavailable
    {
      get { return (stateDTR < 2); }
    }

    /// <summary>
    /// The state of the DTR modem control output
    /// </summary>
    protected bool DTR
    {
      set
      {
        if (stateDTR > 1)
        {
          return;
        }
        CheckOnline();
        if (value)
        {
          if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETDTR))
          {
            stateDTR = 1;
          }
          else
          {
            ThrowException("Unexpected Failure");
          }
        }
        else
        {
          if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRDTR))
          {
            stateDTR = 0;
          }
          else
          {
            ThrowException("Unexpected Failure");
          }
        }
      }
      get { return (stateDTR == 1); }
    }

    /// <summary>
    /// Assert or remove a break condition from the transmission line
    /// </summary>
    protected bool Break
    {
      set
      {
        if (stateBRK > 1)
        {
          return;
        }
        CheckOnline();
        if (value)
        {
          if (Win32Com.EscapeCommFunction(hPort, Win32Com.SETBREAK))
          {
            stateBRK = 0;
          }
          else
          {
            ThrowException("Unexpected Failure");
          }
        }
        else
        {
          if (Win32Com.EscapeCommFunction(hPort, Win32Com.CLRBREAK))
          {
            stateBRK = 0;
          }
          else
          {
            ThrowException("Unexpected Failure");
          }
        }
      }
      get { return (stateBRK == 1); }
    }

    /// <summary>
    /// Override this to provide settings. (NB this is called during Open method)
    /// </summary>
    /// <returns>CommBaseSettings, or derived object with required settings initialised</returns>
    protected virtual CommBaseSettings CommSettings()
    {
      return new CommBaseSettings();
    }

    /// <summary>
    /// Override this to provide processing after the port is openned (i.e. to configure remote
    /// device or just check presence).
    /// </summary>
    /// <returns>false to close the port again</returns>
    protected virtual bool AfterOpen()
    {
      return true;
    }

    /// <summary>
    /// Override this to provide processing prior to port closure.
    /// </summary>
    /// <param name="error">True if closing due to an error</param>
    protected virtual void BeforeClose(bool error) {}

    /// <summary>
    /// Override this to process received bytes.
    /// </summary>
    /// <param name="ch">The byte that was received</param>
    protected virtual void OnRxChar(byte ch) {}

    /// <summary>
    /// Override this to take action when transmission is complete (i.e. all bytes have actually
    /// been sent, not just queued).
    /// </summary>
    protected virtual void OnTxDone() {}

    /// <summary>
    /// Override this to take action when a break condition is detected on the input line.
    /// </summary>
    protected virtual void OnBreak() {}

    //JH 1.3: Deleted OnRing() which was never called: use OnStatusChange instead (Thanks Jim Foster)

    /// <summary>
    /// Override this to take action when one or more modem status inputs change state
    /// </summary>
    /// <param name="mask">The status inputs that have changed state</param>
    /// <param name="state">The state of the status inputs</param>
    protected virtual void OnStatusChange(ModemStatus mask, ModemStatus state) {}

    /// <summary>
    /// Override this to take action when the reception thread closes due to an exception being thrown.
    /// </summary>
    /// <param name="e">The exception which was thrown</param>
    protected virtual void OnRxException(Exception e) {}

    private void ReceiveThread()
    {
      byte[] buf = new Byte[1];
      uint gotbytes;
      bool starting;

      starting = true;
      AutoResetEvent sg = new AutoResetEvent(false);
      Win32Com.OVERLAPPED ov = new Win32Com.OVERLAPPED();

      IntPtr unmanagedOv;
      IntPtr uMask;
      uint eventMask = 0;
      unmanagedOv = Marshal.AllocHGlobal(Marshal.SizeOf(ov));
      uMask = Marshal.AllocHGlobal(Marshal.SizeOf(eventMask));

      ov.Offset = 0;
      ov.OffsetHigh = 0;
      ov.hEvent = sg.SafeWaitHandle.DangerousGetHandle();
      Marshal.StructureToPtr(ov, unmanagedOv, true);

      try
      {
        while (true)
        {
          if (!Win32Com.SetCommMask(hPort, Win32Com.EV_RXCHAR | Win32Com.EV_TXEMPTY | Win32Com.EV_CTS | Win32Com.EV_DSR
                                           | Win32Com.EV_BREAK | Win32Com.EV_RLSD | Win32Com.EV_RING | Win32Com.EV_ERR))
          {
            throw new CommPortException("IO Error [001]");
          }
          Marshal.WriteInt32(uMask, 0);
          //JH 1.2: Tells the main thread that this thread is ready for action.
          if (starting)
          {
            startEvent.Set();
            starting = false;
          }
          if (!Win32Com.WaitCommEvent(hPort, uMask, unmanagedOv))
          {
            if (Marshal.GetLastWin32Error() == Win32Com.ERROR_IO_PENDING)
            {
              sg.WaitOne();
            }
            else
            {
              throw new CommPortException("IO Error [002]");
            }
          }
          eventMask = (uint)Marshal.ReadInt32(uMask);
          if ((eventMask & Win32Com.EV_ERR) != 0)
          {
            UInt32 errs;
            if (Win32Com.ClearCommError(hPort, out errs, IntPtr.Zero))
            {
              //JH 1.2: BREAK condition has an error flag and and an event flag. Not sure if both
              //are always raised, so if CE_BREAK is only error flag ignore it and set the EV_BREAK
              //flag for normal handling. Also made more robust by handling case were no recognised
              //error was present in the flags. (Thanks to Fred Pittroff for finding this problem!)
              int ec = 0;
              StringBuilder s = new StringBuilder("UART Error: ", 40);
              if ((errs & Win32Com.CE_FRAME) != 0)
              {
                s = s.Append("Framing,");
                ec++;
              }
              if ((errs & Win32Com.CE_IOE) != 0)
              {
                s = s.Append("IO,");
                ec++;
              }
              if ((errs & Win32Com.CE_OVERRUN) != 0)
              {
                s = s.Append("Overrun,");
                ec++;
              }
              if ((errs & Win32Com.CE_RXOVER) != 0)
              {
                s = s.Append("Receive Cverflow,");
                ec++;
              }
              if ((errs & Win32Com.CE_RXPARITY) != 0)
              {
                s = s.Append("Parity,");
                ec++;
              }
              if ((errs & Win32Com.CE_TXFULL) != 0)
              {
                s = s.Append("Transmit Overflow,");
                ec++;
              }
              if (ec > 0)
              {
                s.Length = s.Length - 1;
                throw new CommPortException(s.ToString());
              }
              else
              {
                if (errs == Win32Com.CE_BREAK)
                {
                  eventMask |= Win32Com.EV_BREAK;
                }
                else
                {
                  throw new CommPortException("IO Error [003]");
                }
              }
            }
            else
            {
              throw new CommPortException("IO Error [003]");
            }
          }
          if ((eventMask & Win32Com.EV_RXCHAR) != 0)
          {
            do
            {
              gotbytes = 0;
              if (!Win32Com.ReadFile(hPort, buf, 1, out gotbytes, unmanagedOv))
              {
                //JH 1.1: Removed ERROR_IO_PENDING handling as comm timeouts have now
                //been set so ReadFile returns immediately. This avoids use of CancelIo
                //which was causing loss of data. Thanks to Daniel Moth for suggesting this
                //might be a problem, and to many others for reporting that it was!

                int x = Marshal.GetLastWin32Error();

                throw new CommPortException("IO Error [004]");
              }
              if (gotbytes == 1)
              {
                OnRxChar(buf[0]);
              }
            } while (gotbytes > 0);
          }
          if ((eventMask & Win32Com.EV_TXEMPTY) != 0)
          {
            //JH1.3:
            lock (empty) empty[0] = true;
            OnTxDone();
          }
          if ((eventMask & Win32Com.EV_BREAK) != 0)
          {
            OnBreak();
          }

          uint i = 0;
          if ((eventMask & Win32Com.EV_CTS) != 0)
          {
            i |= Win32Com.MS_CTS_ON;
          }
          if ((eventMask & Win32Com.EV_DSR) != 0)
          {
            i |= Win32Com.MS_DSR_ON;
          }
          if ((eventMask & Win32Com.EV_RLSD) != 0)
          {
            i |= Win32Com.MS_RLSD_ON;
          }
          if ((eventMask & Win32Com.EV_RING) != 0)
          {
            i |= Win32Com.MS_RING_ON;
          }
          if (i != 0)
          {
            uint f;
            if (!Win32Com.GetCommModemStatus(hPort, out f))
            {
              throw new CommPortException("IO Error [005]");
            }
            OnStatusChange(new ModemStatus(i), new ModemStatus(f));
          }
        }
      }
      catch (Exception e)
      {
        //JH 1.3: Added for shutdown robustness (Thanks to Fred Pittroff, Mark Behner and Kevin Williamson!), .
        Win32Com.CancelIo(hPort);
        if (uMask != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(uMask);
        }
        if (unmanagedOv != IntPtr.Zero)
        {
          Marshal.FreeHGlobal(unmanagedOv);
        }

        if (!(e is ThreadAbortException))
        {
          rxException = e;
          OnRxException(e);
        }
      }
    }

    private bool CheckOnline()
    {
      if ((rxException != null) && (!rxExceptionReported))
      {
        rxExceptionReported = true;
        ThrowException("rx");
      }
      if (online)
      {
        //JH 1.1: Avoid use of GetHandleInformation for W98 compatability.
        if (hPort != (IntPtr)Win32Com.INVALID_HANDLE_VALUE)
        {
          return true;
        }
        ThrowException("Offline");
        return false;
      }
      else
      {
        if (auto)
        {
          if (Open())
          {
            return true;
          }
        }
        ThrowException("Offline");
        return false;
      }
    }
  }

  /// <summary>
  /// Overlays CommBase to provide line or packet oriented communications to derived classes. Strings
  /// are sent and received and the Transact method is added which transmits a string and then blocks until
  /// a reply string has been received (subject to a timeout).
  /// </summary>
  public abstract class CommLine : CommBase
  {
    private byte[] RxBuffer;
    private uint RxBufferP = 0;
    private ASCII RxTerm;
    private ASCII[] TxTerm;
    private ASCII[] RxFilter;
    private string RxString = "";
    private ManualResetEvent TransFlag = new ManualResetEvent(true);
    private uint TransTimeout;

    /// <summary>
    /// Extends CommBaseSettings to add the settings used by CommLine.
    /// </summary>
    public class CommLineSettings : CommBaseSettings
    {
      /// <summary>
      /// Maximum size of received string (default: 256)
      /// </summary>
      public int rxStringBufferSize = 256;

      /// <summary>
      /// ASCII code that terminates a received string (default: CR)
      /// </summary>
      public ASCII rxTerminator = ASCII.CR;

      /// <summary>
      /// ASCII codes that will be ignored in received string (default: null)
      /// </summary>
      public ASCII[] rxFilter;

      /// <summary>
      /// Maximum time (ms) for the Transact method to complete (default: 500)
      /// </summary>
      public int transactTimeout = 500;

      /// <summary>
      /// ASCII codes transmitted after each Send string (default: null)
      /// </summary>
      public ASCII[] txTerminator;

      public new static CommLineSettings LoadFromXML(Stream s)
      {
        return (CommLineSettings)LoadFromXML(s, typeof (CommLineSettings));
      }
    }

    /// <summary>
    /// Queue the ASCII representation of a string and then the set terminator bytes for sending.
    /// </summary>
    /// <param name="toSend">String to be sent.</param>
    protected void Send(string toSend)
    {
      //JH 1.1: Use static encoder for efficiency. Thanks to Prof. Dr. Peter Jesorsky!
      uint l = (uint)Encoding.ASCII.GetByteCount(toSend);
      if (TxTerm != null)
      {
        l += (uint)TxTerm.GetLength(0);
      }
      byte[] b = new byte[l];
      byte[] s = Encoding.ASCII.GetBytes(toSend);
      int i;
      for (i = 0; (i <= s.GetUpperBound(0)); i++)
      {
        b[i] = s[i];
      }
      if (TxTerm != null)
      {
        for (int j = 0; (j <= TxTerm.GetUpperBound(0)); j++, i++)
        {
          b[i] = (byte)TxTerm[j];
        }
      }
      Send(b);
    }

    /// <summary>
    /// Transmits the ASCII representation of a string followed by the set terminator bytes and then
    /// awaits a response string.
    /// </summary>
    /// <param name="toSend">The string to be sent.</param>
    /// <returns>The response string.</returns>
    protected string Transact(string toSend)
    {
      Send(toSend);
      TransFlag.Reset();
      if (!TransFlag.WaitOne((int)TransTimeout, false))
      {
        ThrowException("Timeout");
      }
      string s;
      lock (RxString)
      {
        s = RxString;
      }
      return s;
    }

    /// <summary>
    /// If a derived class overrides ComSettings(), it must call this prior to returning the settings to
    /// the base class.
    /// </summary>
    /// <param name="s">Class containing the appropriate settings.</param>
    protected void Setup(CommLineSettings s)
    {
      RxBuffer = new byte[s.rxStringBufferSize];
      RxTerm = s.rxTerminator;
      RxFilter = s.rxFilter;
      TransTimeout = (uint)s.transactTimeout;
      TxTerm = s.txTerminator;
    }

    /// <summary>
    /// Override this to process unsolicited input lines (not a result of Transact).
    /// </summary>
    /// <param name="s">String containing the received ASCII text.</param>
    protected virtual void OnRxLine(string s) {}

    protected override void OnRxChar(byte ch)
    {
      ASCII ca = (ASCII)ch;
      if ((ca == RxTerm) || (RxBufferP > RxBuffer.GetUpperBound(0)))
      {
        //JH 1.1: Use static encoder for efficiency. Thanks to Prof. Dr. Peter Jesorsky!
        lock (RxString)
        {
          RxString = Encoding.ASCII.GetString(RxBuffer, 0, (int)RxBufferP);
        }
        RxBufferP = 0;
        if (TransFlag.WaitOne(0, false))
        {
          OnRxLine(RxString);
        }
        else
        {
          TransFlag.Set();
        }
      }
      else
      {
        bool wr = true;
        if (RxFilter != null)
        {
          for (int i = 0; i <= RxFilter.GetUpperBound(0); i++)
          {
            if (RxFilter[i] == ca)
            {
              wr = false;
            }
          }
        }
        if (wr)
        {
          RxBuffer[RxBufferP] = ch;
          RxBufferP++;
        }
      }
    }
  }

  /// <summary>
  /// Exception used for all errors.
  /// </summary>
  public class CommPortException : ApplicationException
  {
    /// <summary>
    /// Constructor for raising direct exceptions
    /// </summary>
    /// <param name="desc">Description of error</param>
    public CommPortException(string desc) : base(desc) {}

    /// <summary>
    /// Constructor for re-raising exceptions from receive thread
    /// </summary>
    /// <param name="e">Inner exception raised on receive thread</param>
    public CommPortException(Exception e) : base("Receive Thread Exception", e) {}
  }

  internal class Win32Com
  {
    /// <summary>
    /// Opening Testing and Closing the Port Handle.
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern IntPtr CreateFile(String lpFileName, UInt32 dwDesiredAccess, UInt32 dwShareMode,
                                             IntPtr lpSecurityAttributes, UInt32 dwCreationDisposition,
                                             UInt32 dwFlagsAndAttributes,
                                             IntPtr hTemplateFile);

    //Constants for errors:
    internal const UInt32 ERROR_FILE_NOT_FOUND = 2;
    internal const UInt32 ERROR_INVALID_NAME = 123;
    internal const UInt32 ERROR_ACCESS_DENIED = 5;
    internal const UInt32 ERROR_IO_PENDING = 997;
    internal const UInt32 ERROR_IO_INCOMPLETE = 996;

    //Constants for return value:
    internal const Int32 INVALID_HANDLE_VALUE = -1;

    //Constants for dwFlagsAndAttributes:
    internal const UInt32 FILE_FLAG_OVERLAPPED = 0x40000000;

    //Constants for dwCreationDisposition:
    internal const UInt32 OPEN_EXISTING = 3;

    //Constants for dwDesiredAccess:
    internal const UInt32 GENERIC_READ = 0x80000000;
    internal const UInt32 GENERIC_WRITE = 0x40000000;

    [DllImport("kernel32.dll")]
    internal static extern Boolean CloseHandle(IntPtr hObject);

    /// <summary>
    /// Manipulating the communications settings.
    /// </summary>
    [DllImport("kernel32.dll")]
    internal static extern Boolean GetCommState(IntPtr hFile, ref DCB lpDCB);

    [DllImport("kernel32.dll")]
    internal static extern Boolean GetCommTimeouts(IntPtr hFile, out COMMTIMEOUTS lpCommTimeouts);

    [DllImport("kernel32.dll")]
    internal static extern Boolean BuildCommDCBAndTimeouts(String lpDef, ref DCB lpDCB, ref COMMTIMEOUTS lpCommTimeouts);

    [DllImport("kernel32.dll")]
    internal static extern Boolean SetCommState(IntPtr hFile, [In] ref DCB lpDCB);

    [DllImport("kernel32.dll")]
    internal static extern Boolean SetCommTimeouts(IntPtr hFile, [In] ref COMMTIMEOUTS lpCommTimeouts);

    [DllImport("kernel32.dll")]
    internal static extern Boolean SetupComm(IntPtr hFile, UInt32 dwInQueue, UInt32 dwOutQueue);

    [StructLayout(LayoutKind.Sequential)]
    internal struct COMMTIMEOUTS
    {
      //JH 1.1: Changed Int32 to UInt32 to allow setting to MAXDWORD
      internal UInt32 ReadIntervalTimeout;
      internal UInt32 ReadTotalTimeoutMultiplier;
      internal UInt32 ReadTotalTimeoutConstant;
      internal UInt32 WriteTotalTimeoutMultiplier;
      internal UInt32 WriteTotalTimeoutConstant;
    }

    //JH 1.1: Added to enable use of "return immediately" timeout.
    internal const UInt32 MAXDWORD = 0xffffffff;

    [StructLayout(LayoutKind.Sequential)]
    internal struct DCB
    {
      internal Int32 DCBlength;
      internal Int32 BaudRate;
      internal Int32 PackedValues;
      internal Int16 wReserved;
      internal Int16 XonLim;
      internal Int16 XoffLim;
      internal Byte ByteSize;
      internal Byte Parity;
      internal Byte StopBits;
      internal Byte XonChar;
      internal Byte XoffChar;
      internal Byte ErrorChar;
      internal Byte EofChar;
      internal Byte EvtChar;
      internal Int16 wReserved1;

      internal void init(bool parity, bool outCTS, bool outDSR, int dtr, bool inDSR, bool txc, bool xOut,
                         bool xIn, int rts)
      {
        //JH 1.3: Was 0x8001 ans so not setting fAbortOnError - Thanks Larry Delby!
        DCBlength = 28;
        PackedValues = 0x4001;
        if (parity)
        {
          PackedValues |= 0x0002;
        }
        if (outCTS)
        {
          PackedValues |= 0x0004;
        }
        if (outDSR)
        {
          PackedValues |= 0x0008;
        }
        PackedValues |= ((dtr & 0x0003) << 4);
        if (inDSR)
        {
          PackedValues |= 0x0040;
        }
        if (txc)
        {
          PackedValues |= 0x0080;
        }
        if (xOut)
        {
          PackedValues |= 0x0100;
        }
        if (xIn)
        {
          PackedValues |= 0x0200;
        }
        PackedValues |= ((rts & 0x0003) << 12);
      }
    }

    /// <summary>
    /// Reading and writing.
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern Boolean WriteFile(IntPtr fFile, Byte[] lpBuffer, UInt32 nNumberOfBytesToWrite,
                                             out UInt32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

    [StructLayout(LayoutKind.Sequential)]
    internal struct OVERLAPPED
    {
      internal UIntPtr Internal;
      internal UIntPtr InternalHigh;
      internal UInt32 Offset;
      internal UInt32 OffsetHigh;
      internal IntPtr hEvent;
    }

    [DllImport("kernel32.dll")]
    internal static extern Boolean SetCommMask(IntPtr hFile, UInt32 dwEvtMask);

    // Constants for dwEvtMask:
    internal const UInt32 EV_RXCHAR = 0x0001;
    internal const UInt32 EV_RXFLAG = 0x0002;
    internal const UInt32 EV_TXEMPTY = 0x0004;
    internal const UInt32 EV_CTS = 0x0008;
    internal const UInt32 EV_DSR = 0x0010;
    internal const UInt32 EV_RLSD = 0x0020;
    internal const UInt32 EV_BREAK = 0x0040;
    internal const UInt32 EV_ERR = 0x0080;
    internal const UInt32 EV_RING = 0x0100;
    internal const UInt32 EV_PERR = 0x0200;
    internal const UInt32 EV_RX80FULL = 0x0400;
    internal const UInt32 EV_EVENT1 = 0x0800;
    internal const UInt32 EV_EVENT2 = 0x1000;

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern Boolean WaitCommEvent(IntPtr hFile, IntPtr lpEvtMask, IntPtr lpOverlapped);

    [DllImport("kernel32.dll")]
    internal static extern Boolean CancelIo(IntPtr hFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern Boolean ReadFile(IntPtr hFile, [Out] Byte[] lpBuffer, UInt32 nNumberOfBytesToRead,
                                            out UInt32 nNumberOfBytesRead, IntPtr lpOverlapped);

    [DllImport("kernel32.dll")]
    internal static extern Boolean TransmitCommChar(IntPtr hFile, Byte cChar);

    /// <summary>
    /// Control port functions.
    /// </summary>
    [DllImport("kernel32.dll")]
    internal static extern Boolean EscapeCommFunction(IntPtr hFile, UInt32 dwFunc);

    // Constants for dwFunc:
    internal const UInt32 SETXOFF = 1;
    internal const UInt32 SETXON = 2;
    internal const UInt32 SETRTS = 3;
    internal const UInt32 CLRRTS = 4;
    internal const UInt32 SETDTR = 5;
    internal const UInt32 CLRDTR = 6;
    internal const UInt32 RESETDEV = 7;
    internal const UInt32 SETBREAK = 8;
    internal const UInt32 CLRBREAK = 9;

    [DllImport("kernel32.dll")]
    internal static extern Boolean GetCommModemStatus(IntPtr hFile, out UInt32 lpModemStat);

    // Constants for lpModemStat:
    internal const UInt32 MS_CTS_ON = 0x0010;
    internal const UInt32 MS_DSR_ON = 0x0020;
    internal const UInt32 MS_RING_ON = 0x0040;
    internal const UInt32 MS_RLSD_ON = 0x0080;

    /// <summary>
    /// Status Functions.
    /// </summary>
    [DllImport("kernel32.dll", SetLastError = true)]
    internal static extern Boolean GetOverlappedResult(IntPtr hFile, IntPtr lpOverlapped,
                                                       out UInt32 nNumberOfBytesTransferred, Boolean bWait);

    [DllImport("kernel32.dll")]
    internal static extern Boolean ClearCommError(IntPtr hFile, out UInt32 lpErrors, IntPtr lpStat);

    [DllImport("kernel32.dll")]
    internal static extern Boolean ClearCommError(IntPtr hFile, out UInt32 lpErrors, out COMSTAT cs);

    //Constants for lpErrors:
    internal const UInt32 CE_RXOVER = 0x0001;
    internal const UInt32 CE_OVERRUN = 0x0002;
    internal const UInt32 CE_RXPARITY = 0x0004;
    internal const UInt32 CE_FRAME = 0x0008;
    internal const UInt32 CE_BREAK = 0x0010;
    internal const UInt32 CE_TXFULL = 0x0100;
    internal const UInt32 CE_PTO = 0x0200;
    internal const UInt32 CE_IOE = 0x0400;
    internal const UInt32 CE_DNS = 0x0800;
    internal const UInt32 CE_OOP = 0x1000;
    internal const UInt32 CE_MODE = 0x8000;

    [StructLayout(LayoutKind.Sequential)]
    internal struct COMSTAT
    {
      internal const uint fCtsHold = 0x1;
      internal const uint fDsrHold = 0x2;
      internal const uint fRlsdHold = 0x4;
      internal const uint fXoffHold = 0x8;
      internal const uint fXoffSent = 0x10;
      internal const uint fEof = 0x20;
      internal const uint fTxim = 0x40;
      internal UInt32 Flags;
      internal UInt32 cbInQue;
      internal UInt32 cbOutQue;
    }

    [DllImport("kernel32.dll")]
    internal static extern Boolean GetCommProperties(IntPtr hFile, out COMMPROP cp);

    [StructLayout(LayoutKind.Sequential)]
    internal struct COMMPROP
    {
      internal UInt16 wPacketLength;
      internal UInt16 wPacketVersion;
      internal UInt32 dwServiceMask;
      internal UInt32 dwReserved1;
      internal UInt32 dwMaxTxQueue;
      internal UInt32 dwMaxRxQueue;
      internal UInt32 dwMaxBaud;
      internal UInt32 dwProvSubType;
      internal UInt32 dwProvCapabilities;
      internal UInt32 dwSettableParams;
      internal UInt32 dwSettableBaud;
      internal UInt16 wSettableData;
      internal UInt16 wSettableStopParity;
      internal UInt32 dwCurrentTxQueue;
      internal UInt32 dwCurrentRxQueue;
      internal UInt32 dwProvSpec1;
      internal UInt32 dwProvSpec2;
      internal Byte wcProvChar;
    }
  }
}