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

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster.Enum;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Implementations.Exception;
using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;
using MediaPortal.Common.Utils;
using Microsoft.Win32.SafeHandles;

namespace Mediaportal.TV.Server.Plugins.TunerExtension.MicrosoftBlaster
{
  /// <summary>
  /// Base class for the MCE transceiver driver implementations.
  /// </summary>
  internal abstract class Driver
  {
    #region enums

    [Flags]
    protected enum ReceivePort
    {
      None = 0,
      Receive = 1,
      Learn = 2
    }

    [Flags]
    protected enum WakeProtocol : byte
    {
      None = 0,
      Rc6 = 1,          // V2_WAKE_PROTOCOL_RC6
      QuatroPulse = 2,  // V2_WAKE_PROTOCOL_QP
      Samsung = 4,      // V2_WAKE_PROTOCOL_SAMSUNG
      DontCare = 8      // V2_WAKE_PROTOCOL_DONTCARE
    }

    protected enum WakeKey : byte
    {
      PowerToggle = 0x0c,
      DiscreteOn = 0x29
    }

    private enum ReceiveThreadState
    {
      Receiving,
      Learning,
      LearningComplete,
      Stop
    }

    #endregion

    #region constants

    protected const int SAMPLE_PERIOD = 50;           // unit = micro-seconds; value as per specifications
    protected static readonly TimeSpan TIMEOUT_WRITE = new TimeSpan(0, 0, 10);

    // At minimum, should exceed the longest space (including space between
    // command and repeat sequences) in all supported protocols. If not long
    // enough, commands may not be decoded/interpretted correctly and/or
    // repetition may not be handled correctly. However the longer the
    // time-out, the greater the receive latency.
    //
    // Longest spaces in commands (unit = ms):
    // Aiwa           4.4
    // Daewoo         4
    // JVC            4.2
    // Kaseikyo       1.7
    // NEC            4.5
    // Nokia          3.5
    // Panasonic old  3.5
    // RC-5/5x        5.6
    // RC-6           1.3
    // RCA            4
    // RECS80         7.6
    // Samsung        4.5
    // Sony           0.6
    //
    // Space between command and repeat (unit = ms):
    // Aiwa           90.75                                                                                                               (repeats with dittos so no need to handle this)
    // Daewoo         60    [command + space]; shortest command = 8 + 4 + (1 * 16) + .55 + 4 + .55 = 33.1         longest space = 26.9
    // JVC            46.42 [command + space]; shortest command = 8.44 + 4.22 + (1.05 * 16) + .527 = 29.987       longest space = 16.433
    // Kaseikyo       48???
    // NEC            108   [command + space]; shortest command = 9 + 4.5 + (32 * 1.124) + 0.562 = 50.03          longest space = 57.97
    // Nokia          100   [command + space]; shortest command = 0.5 + 2.5 + (1 * 17) = 20                       longest space = 80      (repetition is delimited with start/stop commands so no need to handle this)
    // Panasonic old  40???
    // RC-5/5x        113.8 [command + space]; shortest command = (2 * 0.889) * 14 = 24.892                       longest space = 88.886  (toggle bit available so no need to handle this)
    // RC-6           107   [command + space]; shortest command = 2.666 + 0.889 + (20 * 0.889) + 1.777 = 23.112   longest space = 83.888  (toggle bit available so no need to handle this)
    // RCA            64    [command + space]; shortest command = 4 + 4 + (24 * 1.5) + 0.5 = 44.5                 longest space = 19.5
    // RECS80         121.5 [command + space]; shortest command = 0.158 + 7.432 + (10 * 5.06) + 0.158 = 58.348    longest space = 63.152  (toggle bit available so no need to handle this)
    // Samsung        118   [command + space];
    //                                20 bit:  shortest command = 4.5 + 4.5 + (20 * 1.124) + 0.562 = 30.686       longest space = 29.314
    //                                36 bit:  shortest command = 4.5 + 4.5 + (16 * 1.124) + 0.562 + 4.5 + (12 * 1.124) + (8 * 2.25) + 0.562
    //                                                          = 4.5 + 4.5 + 17.984 + 0.562 + 4.5 + 13.488 + 18 + 0.562
    //                                                          = 64.096                                          longest space = 53.904
    // Sony          45     [command + space]; shortest command = 2.4 + 0.6 + (12 * 1.2) = 17.4                   longest space = 27.6
    private static readonly TimeSpan TIMEOUT_RECEIVE = new TimeSpan(0, 0, 0, 0, 70);

    // At minimum, should exceed the longest space in *any* [supported *or* unsupported] protocol.
    // At maximum, can be arbitrarily long because the user is only expected to push the button once during learning.
    private static readonly TimeSpan TIMEOUT_LEARN = new TimeSpan(0, 0, 0, 0, 250);

    protected const int RECEIVE_BUFFER_SIZE = 100;    // unit = bytes; must be a multiple of 4

    #endregion

    #region variables

    protected readonly string _devicePath = null;
    private bool _isOpen = false;
    protected ReceivePort _availablePortsReceive = ReceivePort.None;
    protected TransmitPort _availablePortsTransmit = TransmitPort.None;
    protected TransmitPort _connectedTransmitPorts = TransmitPort.None;
    private DateTime _connectedTransmitPortsUpdateTimeStamp = DateTime.MinValue;
    protected bool _useStandardTransmitPortHandling = false;

    private List<int> _learnedTimingData = new List<int>(500);
    private int _learnedCarrierFrequency = IrCommand.CARRIER_FREQUENCY_UNKNOWN;

    private Thread _receiveThread = null;
    private ReceiveThreadState _receiveThreadState = ReceiveThreadState.Stop;
    private ManualResetEvent _abortEventReceive = null;

    protected ManualResetEvent _abortEventWrite = null;

    #endregion

    protected Driver(string devicePath)
    {
      _devicePath = devicePath;
    }

    public string DevicePath
    {
      get
      {
        return _devicePath;
      }
    }

    /// <summary>
    /// Open the device.
    /// </summary>
    /// <returns><c>true</c> if the device is opened successfully, otherwise <c>false</c></returns>
    public bool Open()
    {
      this.LogDebug("Microsoft blaster driver: open, device path = {0}", _devicePath);
      if (_isOpen)
      {
        this.LogWarn("Microsoft blaster driver: already open, device path = {0}", _devicePath);
        return true;
      }

      if (!OpenHandles())
      {
        return false;
      }
      _abortEventWrite = new ManualResetEvent(false);
      _abortEventWrite.Reset();
      _isOpen = true;

      _availablePortsTransmit = TransmitPort.None;
      _availablePortsReceive = ReceivePort.None;
      _connectedTransmitPorts = TransmitPort.None;
      StartReceiveThread();

      this.LogDebug("Microsoft blaster driver: get capabilities");
      GetCapabilities();

      // Determine which transmit ports have an emitter connected to them.
      if (_availablePortsTransmit != TransmitPort.None)
      {
        this.LogDebug("Microsoft blaster driver: initialise transmit port status");
        UpdateTransmitPortStatus();
        _connectedTransmitPortsUpdateTimeStamp = DateTime.Now;
      }
      return true;
    }

    /// <summary>
    /// Close the device.
    /// </summary>
    public void Close()
    {
      if (!_isOpen)
      {
        return;
      }
      this.LogDebug("Microsoft blaster driver: close, device path = {0}", _devicePath);

      StopReceiveThread();

      // Abort any outstanding write operations.
      if (_abortEventWrite != null)
      {
        _abortEventWrite.Set();
        _abortEventWrite.Reset();
      }

      // TODO: is it bad to attempt to send a stop packet/IOCTL (within CloseHandles()) if we're closing on device removal?
      CloseHandles();

      if (_abortEventWrite != null)
      {
        _abortEventWrite.Close();
        _abortEventWrite.Dispose();
        _abortEventWrite = null;
      }

      _isOpen = false;
    }

    /// <summary>
    /// Transmit an IR command.
    /// </summary>
    /// <param name="commandName">A human-readable name for the command.</param>
    /// <param name="commandString">The command to transmit as a Pronto-formatted string.</param>
    /// <param name="ports">The port(s) to use to transmit the command.</param>
    /// <returns>the result of the transmit process</returns>
    public TransmitResult Transmit(string commandName, string commandString, TransmitPort ports)
    {
      this.LogDebug("Microsoft blaster driver: transmit, command = {0}, port(s) = {1}, device path = {2}", commandName, ports, _devicePath);

      if (!_isOpen)
      {
        this.LogError("Microsoft blaster driver: failed to transmit, the device is not open, device path = {0}", _devicePath);
        return TransmitResult.Fail;
      }
      if (_availablePortsTransmit == 0)
      {
        this.LogError("Microsoft blaster driver: failed to transmit, the device does not have any transmit ports, device path = {0}", _devicePath);
        return TransmitResult.Unsupported;
      }
      IrCommand command = IrCommand.FromProntoString(commandString);
      if (command == null)
      {
        this.LogError("Microsoft blaster driver: failed to transmit, the command is not valid, command = {0}", commandName);
        return TransmitResult.InvalidCommand;
      }

      // Truncate inherent inter-command delay. The caller controls the delay
      // based on configuration.
      command.TimingData[command.TimingData.Length - 1] = 1;

      if ((DateTime.Now - _connectedTransmitPortsUpdateTimeStamp).TotalSeconds > 30)
      {
        this.LogDebug("Microsoft blaster driver: update transmit port status");
        UpdateTransmitPortStatus();
        _connectedTransmitPortsUpdateTimeStamp = DateTime.Now;
      }
      if (!_connectedTransmitPorts.HasFlag(ports))
      {
        this.LogError("Microsoft blaster driver: failed to transmit, emitter(s) are not connected to the specified port(s), port(s) = {0}, device path = {1}", ports, _devicePath);
        return TransmitResult.EmitterNotConnected;
      }

      uint carrierFrequency = (uint)command.CarrierFrequency;
      if (command.CarrierFrequency == IrCommand.CARRIER_FREQUENCY_UNKNOWN)
      {
        carrierFrequency = IrCommand.CARRIER_FREQUENCY_DEFAULT;
        this.LogWarn("Microsoft blaster driver: carrier frequency not specified, using default {0} Hz", carrierFrequency);
      }
      uint portMask = GetTransmitPortMask(ports);

      this.LogDebug("Microsoft blaster driver: port mask = 0x{0:x8}, carrier frequency = {1} Hz, pulse/space count = {2}", portMask, carrierFrequency, command.TimingData.Length);
      //DebugTimingData(command.TimingData);
      try
      {
        Transmit(portMask, carrierFrequency, command.TimingData);
        return TransmitResult.Success;
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Microsoft blaster driver: failed to transmit, command = {0}, device path = {1}", commandName, _devicePath);
        return TransmitResult.Fail;
      }
    }

    /// <summary>
    /// Learn an IR command.
    /// </summary>
    /// <param name="timeLimit">The time limit for the learning process.</param>
    /// <param name="command">The learnt command as a Pronto-formatted string.</param>
    /// <returns>the result of the learning process</returns>
    public LearnResult Learn(TimeSpan timeLimit, out string command)
    {
      this.LogDebug("Microsoft blaster driver: learn, time limit = {0} ms, device path = {1}", timeLimit.TotalMilliseconds, _devicePath);
      command = null;

      if (!_isOpen)
      {
        this.LogError("Microsoft blaster driver: failed to learn, the device is not open, device path = {0}", _devicePath);
        return LearnResult.Fail;
      }
      if (!_availablePortsReceive.HasFlag(ReceivePort.Learn))
      {
        this.LogError("Microsoft blaster driver: failed to learn, the device does not have a wide band learning receiver, device path = {0}", _devicePath);
        return LearnResult.Unsupported;
      }

      _learnedTimingData.Clear();
      _learnedCarrierFrequency = IrCommand.CARRIER_FREQUENCY_UNKNOWN;

      // Start the learning process, then wait while the command is received.
      _receiveThreadState = ReceiveThreadState.Learning;
      _abortEventReceive.Set();

      DateTime startTime = DateTime.Now;
      while (_receiveThreadState == ReceiveThreadState.Learning && DateTime.Now - startTime < timeLimit)
      {
        Thread.Sleep(TIMEOUT_LEARN);
      }

      // Stop the learning process, then determine what the result was.
      ReceiveThreadState endState = _receiveThreadState;
      _receiveThreadState = ReceiveThreadState.Receiving;
      _abortEventReceive.Set();

      try
      {
        int pulseSpaceCount = _learnedTimingData.Count;
        if (endState == ReceiveThreadState.LearningComplete)
        {
          this.LogDebug("Microsoft blaster driver: learning succeeded, pulse/space count = {0} (original = {1}), carrier frequency = {2} Hz", pulseSpaceCount, _learnedCarrierFrequency);
          command = new IrCommand(_learnedCarrierFrequency, _learnedTimingData.ToArray()).ToProntoString();
          return LearnResult.Success;
        }

        if (endState == ReceiveThreadState.Learning && pulseSpaceCount > 0)
        {
          this.LogError("Microsoft blaster driver: learn time limit reached, pulse/space count = {0}, device path = {1}", pulseSpaceCount, _devicePath);
          return LearnResult.TimeOut;
        }

        this.LogError("Microsoft blaster driver: generic learn failure, pulse/space count = {0}, device path = {1}", pulseSpaceCount, _devicePath);
        return LearnResult.Fail;
      }
      finally
      {
        DebugTimingData(_learnedTimingData.ToArray());
      }
    }

    public TransceiverDetail GetDetail()
    {
      TransceiverDetail detail = new TransceiverDetail();
      detail.DevicePath = _devicePath;
      detail.IsReceiveSupported = _availablePortsReceive.HasFlag(ReceivePort.Receive);
      detail.IsLearningSupported = _availablePortsReceive.HasFlag(ReceivePort.Learn);
      detail.AllTransmitPorts = _availablePortsTransmit;
      detail.ConnectedTransmitPorts = _connectedTransmitPorts;
      return detail;
    }

    #region abstract members

    /// <summary>
    /// Open access handle(s).
    /// </summary>
    /// <returns><c>true</c> if the handle(s) are opened successfully, otherwise <c>false</c></returns>
    protected abstract bool OpenHandles();

    /// <summary>
    /// Close the access handle(s).
    /// </summary>
    protected abstract void CloseHandles();

    /// <summary>
    /// Get the device's capabilities.
    /// </summary>
    protected abstract void GetCapabilities();

    /// <summary>
    /// Update the status for each transmit port.
    /// </summary>
    protected abstract void UpdateTransmitPortStatus();

    /// <summary>
    /// Use the device to transmit an IR command.
    /// </summary>
    /// <param name="portMask">A bit-mask indicating which port(s) to transmit on.</param>
    /// <param name="carrierFrequency">The carrier frequency to use when modulating the command.</param>
    /// <param name="command">The command, encoded as a sequence of pulse and space durations.</param>
    protected abstract void Transmit(uint portMask, uint carrierFrequency, int[] command);

    /// <summary>
    /// Get the size of buffer to create for receiving.
    /// </summary>
    protected abstract int GetReceiveBufferSize();

    /// <summary>
    /// Set the receive configuration.
    /// </summary>
    /// <remarks>
    /// From the specifications...
    /// [The IR time-out] is the period of silence necessary before the
    /// firmware will determine that a signal has ended and will stop sending
    /// silence to the host.
    /// </remarks>
    /// <param name="port">The port to receive from.</param>
    /// <param name="timeOut">The receive time-out.</param>
    protected abstract void SetReceiveConfiguration(ReceivePort port, TimeSpan timeOut);

    /// <summary>
    /// Receive IR timing and/or command response data from the device.
    /// </summary>
    /// <param name="receiveBuffer">A buffer to use to hold received data.</param>
    /// <param name="receiveBufferSize">The size of <paramref name="receiveBuffer"/>.</param>
    /// <param name="overlappedBuffer">A buffer containing an OVERLAPPED structure which can be used by an asynchronous implementation.</param>
    /// <param name="completeEvent">An event that can be used by an asynchronous implementation.</param>
    /// <param name="abortEvent">An event that will be signalled when receiving should be aborted.</param>
    /// <param name="timingData">The IR timing data received from the device, if any; <c>null</c> if no data was available.</param>
    protected abstract void Receive(IntPtr receiveBuffer, int receiveBufferSize, IntPtr overlappedBuffer, ManualResetEvent completeEvent, ManualResetEvent abortEvent, out int[] timingData);

    /// <summary>
    /// Learn the carrier frequency for a command.
    /// </summary>
    /// <param name="timingData">The timing data that has been learned so far.</param>
    /// <param name="carrierFrequency">The carrier frequency. The unit is Hertz (Hz).</param>
    /// <returns><c>true</c> if the carrier frequency has been learned (indicating learning is complete), otherwise <c>false</c></returns>
    protected abstract bool LearnCarrierFrequency(IEnumerable<int> timingData, out int carrierFrequency);

    #endregion

    #region private members

    /// <summary>
    /// Start a thread to receive IR timing and/or command response data from
    /// the device.
    /// </summary>
    private void StartReceiveThread()
    {
      if (_receiveThread != null)
      {
        if (_receiveThread.IsAlive)
        {
          return;
        }
        StopReceiveThread();
      }

      this.LogDebug("Microsoft blaster driver: start receive thread");
      _abortEventReceive = new ManualResetEvent(false);
      _receiveThreadState = ReceiveThreadState.Receiving;

      _receiveThread = new Thread(new ThreadStart(Receiver));
      _receiveThread.Name = "Microsoft blaster driver receiver";
      _receiveThread.IsBackground = true;
      _receiveThread.Priority = ThreadPriority.Lowest;
      _receiveThread.Start();
    }

    /// <summary>
    /// Stop the thread that receives data from the device.
    /// </summary>
    private void StopReceiveThread()
    {
      if (_receiveThread != null)
      {
        if (!_receiveThread.IsAlive)
        {
          this.LogWarn("Microsoft blaster driver: aborting old receive thread");
          _receiveThread.Abort();
        }
        else
        {
          this.LogDebug("Microsoft blaster driver: stop receive thread");
          _receiveThreadState = ReceiveThreadState.Stop;
          _abortEventWrite.Set();
          _abortEventReceive.Set();
          if (!_receiveThread.Join((int)Math.Max(TIMEOUT_RECEIVE.TotalMilliseconds, TIMEOUT_LEARN.TotalMilliseconds) * 2))
          {
            this.LogWarn("Microsoft blaster driver: failed to join receive thread, aborting thread");
            _receiveThread.Abort();
          }
          _abortEventWrite.Reset();
        }
        _receiveThread = null;
        if (_abortEventReceive != null)
        {
          _abortEventReceive.Close();
          _abortEventReceive = null;
        }
      }
    }

    /// <summary>
    /// Thread function for receiving data from the device.
    /// </summary>
    private void Receiver()
    {
      this.LogDebug("Microsoft blaster driver: receive thread started");
      ReceivePort receivePort = ReceivePort.None;
      TimeSpan receiveTimeOut = TIMEOUT_RECEIVE;
      Decoder.Decoder decoder = new Decoder.Decoder();

      ManualResetEvent receiveCompleteEvent = new ManualResetEvent(false);
      IntPtr overlappedBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NativeOverlapped)));
      int receiveBufferSize = GetReceiveBufferSize();
      IntPtr receiveBuffer = Marshal.AllocHGlobal(receiveBufferSize);

      try
      {
        NativeOverlapped overlapped = new NativeOverlapped();
        overlapped.EventHandle = receiveCompleteEvent.SafeWaitHandle.DangerousGetHandle();
        Marshal.StructureToPtr(overlapped, overlappedBuffer, false);

        while (_receiveThreadState != ReceiveThreadState.Stop)
        {
          // Change receive port if necessary.
          ReceivePort receivePortTarget = ReceivePort.Receive;
          TimeSpan receiveTimeOutTarget = TIMEOUT_RECEIVE;
          if (_receiveThreadState != ReceiveThreadState.Receiving)
          {
            receivePortTarget = ReceivePort.Learn;
            receiveTimeOutTarget = TIMEOUT_LEARN;
          }
          if (!_availablePortsReceive.HasFlag(receivePortTarget))
          {
            receivePortTarget = _availablePortsReceive;
          }
          if (receivePortTarget != ReceivePort.None && (receivePort != receivePortTarget || receiveTimeOut != receiveTimeOutTarget))
          {
            try
            {
              this.LogDebug("Microsoft blaster driver: set receive configuration, port = {0}, time-out = {1} ms", receivePortTarget, receiveTimeOutTarget.TotalMilliseconds);
              SetReceiveConfiguration(receivePortTarget, receiveTimeOutTarget);
              receivePort = receivePortTarget;
              receiveTimeOut = receiveTimeOutTarget;
            }
            catch
            {
              // SetReceiveConfiguration() can throw. We assume that it will
              // only throw when the underlying write/IOCTL is aborted because
              // the thread is being stopped.
              continue;
            }
          }

          // Receive synchronously (can block).
          receiveCompleteEvent.Reset();
          int[] timingData;
          Receive(receiveBuffer, receiveBufferSize, overlappedBuffer, receiveCompleteEvent, _abortEventReceive, out timingData);

          // Decode or learn from the received timing data.
          if (_receiveThreadState == ReceiveThreadState.Receiving && timingData != null)
          {
            //DebugTimingData(timingData);
            decoder.Decode(timingData);
          }
          else if (_receiveThreadState == ReceiveThreadState.Learning)
          {
            if (timingData != null)
            {
              DebugTimingData(timingData);
              if (_learnedTimingData.Count == 0)
              {
                _learnedTimingData.AddRange(timingData);
              }
              else
              {
                int lastExistingDuration = _learnedTimingData[_learnedTimingData.Count - 1];
                if (Math.Sign(lastExistingDuration) == Math.Sign(timingData[0]))
                {
                  timingData[0] += lastExistingDuration;
                  _learnedTimingData.RemoveAt(_learnedTimingData.Count - 1);
                }
                _learnedTimingData.AddRange(timingData);
              }
            }
            if (_learnedTimingData.Count > 0 && LearnCarrierFrequency(_learnedTimingData, out _learnedCarrierFrequency))
            {
              _receiveThreadState = ReceiveThreadState.LearningComplete;
            }
          }
        }
      }
      catch (ThreadAbortException)
      {
      }
      catch (Exception ex)
      {
        this.LogError(ex, "Microsoft blaster driver: unexpected receive thread exception");
      }
      finally
      {
        receiveCompleteEvent.Close();
        receiveCompleteEvent.Dispose();
        Marshal.FreeHGlobal(receiveBuffer);
        Marshal.FreeHGlobal(overlappedBuffer);
      }

      this.LogDebug("Microsoft blaster driver: receive thread stopped");
    }

    protected uint GetTransmitPortMask(TransmitPort ports)
    {
      // There is uncertainty about the correct format for the port bit-mask.
      // When deciding how to write this code, I looked at:
      // 1. The official specifications - "Remote Control and
      //    Receiver-Transceiver Specifications and Requirements for Windows
      //    Media Center in Windows Operating Systems".
      // 2. Open source projects:
      //      - EventGhost
      //      - IRSS
      //      - Linux kernel driver
      //      - MediaPortal TV Server ServerBlaster plugin
      //      - WinLIRC
      // 3. http://stackoverflow.com/questions/1222156/ir-transmit-params-transmitportmask-values
      //
      // Emulator driver...
      // The specifications say the LSB corresponds with "IR0", etc. However we
      // cannot be sure that "IR0" corresponds with the first output as per any
      // labelling on the device etc. Our code is based on the Linux kernel
      // driver, IRSS and the ServerBlaster plugin... in that order.
      //
      //                              bit = [MSB] 7  6  5  4  3  2  1  0 [LSB]
      //                standard handling =       8  7  6  5  4  3  2  1
      // reversed handling, 1 port device =                         1
      // reversed handling, 2 port device =                      1  2
      // reversed handling, 3 port device =                   1  2  3
      // etc.
      //
      //
      // Port driver...
      // The specifications for TransmitPortMask do not specify which bit
      // is associated with each port. Even if they did, we again cannot be
      // sure that the port referred to as port 1 in the specifications is
      // labelled as such on the device. Our code is based on IRSS and the
      // StackOverflow answer.
      //
      //           bit = [MSB] 31 30 .. 2  1  0 [LSB]
      // 1 port device =                      1
      // 2 port device =                   1  2
      // 3 port device =                1  2  3
      // etc.
      if (_useStandardTransmitPortHandling)
      {
        return (uint)ports;
      }

      TransmitPort portMask = TransmitPort.None;
      TransmitPort reversedPort1 = TransmitPort.None;
      Array allPorts = System.Enum.GetValues(typeof(TransmitPort));
      for (int i = allPorts.Length - 1; i > 0; i--)
      {
        reversedPort1 = (TransmitPort)allPorts.GetValue(i);
        if (_availablePortsTransmit.HasFlag(reversedPort1))
        {
          if (this is DriverEmulator)
          {
            reversedPort1 = (TransmitPort)((uint)reversedPort1 << 1);   // shifted to skip the LSB as above
          }
          break;
        }
      }
      foreach (TransmitPort p in allPorts)
      {
        if (p != TransmitPort.None)
        {
          if (ports.HasFlag(p))
          {
            portMask |= reversedPort1;
          }
          reversedPort1 = (TransmitPort)((uint)reversedPort1 >> 1);
        }
      }
      return (uint)portMask;
    }

    private static void DebugTimingData(int[] data)
    {
      if (data == null || data.Length == 0)
      {
        return;
      }

      StringBuilder row = new StringBuilder();
      for (int position = 0; position < data.Length; position++)
      {
        if (position % 0x10 == 0)
        {
          if (row.Length > 0)
          {
            Log.Debug(row.ToString());
          }
          row.Length = 0;
          row.AppendFormat("{0:X4}|", position);
        }
        int value = data[position];
        if (value > 0)
        {
          row.Append('+');
        }
        else
        {
          row.Append('-');
          value *= -1;
        }
        row.AppendFormat("{0, -4} ", value);
      }
      if (row.Length > 0)
      {
        Log.Debug(row.ToString());
      }
    }

    protected static TransmitPort GetPortByIndex(int index)
    {
      if (index < 0 || index > 31)
      {
        throw new TvException("Transmit port index {0} is out of the supported range (0 - 31).", index);
      }
      return (TransmitPort)((uint)1 << index);
    }

    protected int WaitForAsyncIoCompletion(SafeFileHandle handle, IntPtr overlappedBuffer, ManualResetEvent completeEvent, ManualResetEvent abortEvent, TimeSpan timeOut, out uint byteCount)
    {
      byteCount = 0;

      int error = Marshal.GetLastWin32Error();
      while (true)
      {
        if (error != (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS && error != (int)NativeMethods.SystemErrorCode.ERROR_IO_PENDING)
        {
          this.LogError("Microsoft blaster driver: unexpected asyncrhonous I/O error state, error = {0}", error);
          break;
        }

        if (abortEvent != null)
        {
          int signalledEventIndex = ManualResetEvent.WaitAny(new WaitHandle[2] { completeEvent, abortEvent }, timeOut);
          if (signalledEventIndex == 1)
          {
            this.LogWarn("Microsoft blaster driver: asyncrhonous I/O aborted");
            error = (int)NativeMethods.SystemErrorCode.ERROR_OPERATION_ABORTED;
            abortEvent.Reset();
            break;
          }
          else if (signalledEventIndex == WaitHandle.WaitTimeout)
          {
            this.LogWarn("Microsoft blaster driver: asyncrhonous I/O timed out, time limit = {0} ms", timeOut.TotalMilliseconds);
            error = (int)NativeMethods.SystemErrorCode.ERROR_WAIT_TIMEOUT;
            break;
          }
          else if (signalledEventIndex != 0)
          {
            this.LogError("Microsoft blaster driver: asyncrhonous I/O wait failed, event index = {0}", signalledEventIndex);
            error = signalledEventIndex;
            break;
          }
        }
        else if (!completeEvent.WaitOne(timeOut))
        {
          this.LogWarn("Microsoft blaster driver: asyncrhonous I/O timed out, time limit = {0} ms", timeOut.TotalMilliseconds);
          error = (int)NativeMethods.SystemErrorCode.ERROR_WAIT_TIMEOUT;
          break;
        }

        if (!NativeMethods.GetOverlappedResult(handle, overlappedBuffer, out byteCount, false))
        {
          error = Marshal.GetLastWin32Error();
          this.LogError("Microsoft blaster driver: asyncrhonous I/O get overlapped result returned false, error = {0}", error);
          break;
        }

        return (int)NativeMethods.SystemErrorCode.ERROR_SUCCESS;
      }

      if (!NativeMethods.CancelIo(handle))
      {
        this.LogWarn("Microsoft blaster driver: failed to cancel I/O after error, error = {0}", error);
      }
      return error;
    }

    #endregion
  }
}