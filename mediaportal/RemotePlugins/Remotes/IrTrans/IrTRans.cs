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
using System.Net.Sockets;
using System.Runtime.InteropServices;
using MediaPortal.Configuration;
using MediaPortal.GUI.Library;
using MediaPortal.Profile;

namespace MediaPortal.InputDevices
{
  /// <summary>
  /// Support for remote commands transmitted via the IRTrans Server.
  /// Remote buttons are defined in the XML file.
  /// The name of the remote to load is defined in the Configuration. It os prefixed with "IrTrans"
  /// </summary>
  public class IrTrans
  {
    #region Variables and Constants

    private InputHandler irtransHandler;
    private Socket m_Socket;
    private IAsyncResult m_asynResult;
    private AsyncCallback pfnCallBack;
    private bool irTransEnabled = false;
    private string remoteKeyFile = "";
    private string remoteModel = "";
    private int irTransServerPort = 21000;
    private bool logVerbose = false;

    #endregion

    #region Enums and Structure

    public enum IrTransStatus
    {
      STATUS_MESSAGE = 1,
      STATUS_TIMING = 2,
      STATUS_DEVICEMODE = 3,
      STATUS_RECEIVE = 4,
      STATUS_LEARN = 5,
      STATUS_REMOTELIST = 6,
      STATUS_COMMANDLIST = 7,
      STATUS_TRANSLATE = 8,
      STATUS_FUNCTION = 9,
      STATUS_DEVICEMODEEX = 10,
      STATUS_DEVICEDATA = 11,
      STATUS_LCDDATA = 12,
      STATUS_FUNCTIONEX = 13,
      STATUS_DEVICEMODEEXN = 14,
      STATUS_IRDB = 15,
      STATUS_TRANSLATIONFILE = 16,
      STATUS_IRDBFILE = 17,
      STATUS_BUSLIST = 18,
      STATUS_LEARNDIRECT = 19
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NETWORKRECV
    {
      public UInt32 clientid;
      public Int16 statuslen;
      public Int16 statustype;
      public Int16 adress;
      public UInt16 command_num;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)] public string remote;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)] public string command;
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 200)] public string data;
    }

    #endregion

    public IrTrans() {}

    /// <summary>
    /// Read Port number of IRTrans Server and Remote Key file as specified on Configuration
    /// and if IrTrans support enabled connect to the IRTRans Server.
    /// </summary>
    /// <param name="hwnd"></param>
    public void Init(IntPtr hwnd)
    {
      using (Settings xmlreader = new MPSettings())
      {
        irTransEnabled = xmlreader.GetValueAsBool("remote", "IRTrans", false);
        remoteModel = xmlreader.GetValueAsString("remote", "IRTransRemoteModel", "mediacenter");
        irTransServerPort = xmlreader.GetValueAsInt("remote", "IRTransServerPort", 21000);
        logVerbose = xmlreader.GetValueAsBool("remote", "IRTransVerboseLog", false);
      }
      if (irTransEnabled)
      {
        // Build the XML File name to be used for Button mapping.
        remoteKeyFile = "IrTrans " + remoteModel;
        irtransHandler = new InputHandler(remoteKeyFile);
      }
      else
      {
        return;
      }
      // Now Connect to the IRTransServer 
      if (Connect_IrTrans(irTransServerPort))
      {
        if (logVerbose)
        {
          Log.Info("IRTrans: Connection established.");
        }
        // Now Wait for data to be sent
        WaitForData();
      }
      else
      {
        return;
      }
    }

    /// <summary>
    /// Close the connection to IRTrans when MP is terminated
    /// </summary>
    public void DeInit()
    {
      if (!irTransEnabled)
      {
        return;
      }

      try
      {
        m_Socket.Close();
        if (logVerbose)
        {
          Log.Info("IRTrans: Connection closed.");
        }
      }
      catch (SocketException se)
      {
        if (logVerbose)
        {
          Log.Info("IRTrans: Exception on closing socket. {0}", se.Message);
        }
      }
    }


    /// <summary>
    /// Establishes a connection to the IRTransServer on "localhost" at the given Port.
    /// Default Port is 21000, but could have been changed in Configuration.
    /// </summary>
    /// <param name="connectionPort"></param>
    /// <returns></returns>
    private bool Connect_IrTrans(int connectionPort)
    {
      try
      {
        m_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        m_Socket.Connect("localhost", connectionPort);
        // Send Client id to Server
        int clientID = 0;
        byte[] sendData = BitConverter.GetBytes(clientID);
        m_Socket.Send(sendData, sendData.Length, SocketFlags.None);
        return true;
      }
      catch (SocketException)
      {
        Log.Info("IRTrans: Could not connect to server - server not started?");
        return false;
      }
    }

    /// <summary>
    /// Establishes an Async Callback Procedure that receives data being sent via the IRTrans Server.
    /// </summary>
    private void WaitForData()
    {
      try
      {
        if (pfnCallBack == null)
        {
          pfnCallBack = new AsyncCallback(OnDataReceived);
        }
        CSocketPacket socketPkt = new CSocketPacket();
        socketPkt.thisSocket = m_Socket;
        // Now start to listen for any data.
        m_asynResult = m_Socket.BeginReceive(socketPkt.receiveBuffer, 0, socketPkt.receiveBuffer.Length,
                                             SocketFlags.None, pfnCallBack, socketPkt);
      }
      catch (SocketException se)
      {
        if (logVerbose)
        {
          Log.Info("IRTrans: Error on receive from socket: {0}", se.Message);
        }
      }
    }

    /// <summary>
    /// Class that receives the Socketid and databuffer on receive
    /// </summary>
    public class CSocketPacket
    {
      public Socket thisSocket;
      public byte[] receiveBuffer = new byte[350];
    }

    /// <summary>
    /// Method is called, whenever a IR Comand is received via the IRTrans Server connection.
    /// </summary>
    /// <param name="asyn"></param>
    private void OnDataReceived(IAsyncResult asyn)
    {
      try
      {
        CSocketPacket theSockId = (CSocketPacket)asyn.AsyncState;
        //Do an end receive first
        int bytesReceived = 0;
        bytesReceived = theSockId.thisSocket.EndReceive(asyn);
        // Map the received data to the structure
        IntPtr ptrReceive = Marshal.AllocHGlobal(bytesReceived);
        Marshal.Copy(theSockId.receiveBuffer, 0, ptrReceive, bytesReceived);
        NETWORKRECV netrecv = (NETWORKRECV)Marshal.PtrToStructure(ptrReceive, typeof (NETWORKRECV));
        if (logVerbose)
        {
          Log.Info("IRTrans: Command Start --------------------------------------------");
          Log.Info("IRTrans: Client       = {0}", netrecv.clientid);
          Log.Info("IRTrans: Status       = {0}", (IrTransStatus)netrecv.statustype);
          Log.Info("IRTrans: Remote       = {0}", netrecv.remote);
          Log.Info("IRTrans: Command Num. = {0}", netrecv.command_num.ToString());
          Log.Info("IRTrans: Command      = {0}", netrecv.command);
          Log.Info("IRTrans: Data         = {0}", netrecv.data);
          Log.Info("IRTrans: Command End ----------------------------------------------");
        }

        // Do an action only on Receive and if the command came from the selected Remote
        if ((IrTransStatus)netrecv.statustype == IrTransStatus.STATUS_RECEIVE)
        {
          if (netrecv.remote.Trim() == remoteModel)
          {
            try
            {
              if (irtransHandler.MapAction(netrecv.command.Trim()))
              {
                if (logVerbose)
                {
                  Log.Info("IRTrans: Action mapped");
                }
              }
              else
              {
                if (logVerbose)
                {
                  Log.Info("IRTrans: Action not mapped");
                }
              }
            }
            catch (Exception ex)
            {
              if (logVerbose)
              {
                Log.Info("IRTrans: Exception in IRTranshandler: {0}", ex.Message);
              }
            }
          }
        }

        Marshal.FreeHGlobal(ptrReceive);
        WaitForData();
      }
      catch (ObjectDisposedException) {}
      catch (SocketException se)
      {
        if (logVerbose)
        {
          Log.Info("IRTrans: Error on receive from socket: {0}", se.Message);
        }
      }
    }
  }
}