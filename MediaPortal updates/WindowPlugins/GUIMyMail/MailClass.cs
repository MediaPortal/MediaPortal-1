/* 
 *	Copyright (C) 2005-2006 Team MediaPortal
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

using System;
using System.Windows.Forms;
using System.Collections;
using System.Net.Sockets;
using System.Drawing.Design;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using MediaPortal.Dialogs;
using MediaPortal.GUI.Library;
using System.ComponentModel;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using System.Windows.Forms.Design;

namespace MyMail
{
  /// <summary>
  /// this a mail-client in a class
  /// to read a mailbox 
  /// </summary>

  public class MailClass
  {
    const int m_buffSize = 2048; // buffer for rec. data
    const string _CRLF_ = "\r\n";
    const System.Net.Sockets.SocketFlags noFlags = System.Net.Sockets.SocketFlags.None;
    MailBox m_mb;
    IPEndPoint m_endPoint;
    Socket m_mailSocket;//
    NetworkStream m_mailSocketStream;
    SslStream m_mailTLSStream;

    static string m_emptyBuffer = new string((char)0, 2048);// 2kbyte socket buffer
    //System.Net.Sockets.TcpClient m_mailReciever=new TcpClient(); 
    int m_imailCount;
    int m_currAction = -1; //
    int m_mailAction;
    int m_mailNumber;
    int m_ierrorNumber;
    string m_mailFolder;
    string m_attachmentFolder;
    string m_errorMessage;
    int m_mailNumberSize;
    string m_recMailData;
    System.Windows.Forms.Timer m_timeOutTimer = new System.Windows.Forms.Timer();
    ArrayList m_knownMails = new ArrayList();
    System.Security.Cryptography.MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
    byte[] m_mailBuffer = new Byte[m_buffSize];
    Byte m_TLSSecured = MailBox.NO_SSL; //NO_SSL: user/password combination won't be encrypted, STLS: use explicit SSL, SSL_PORT: use implicit SSL; The authentication method is chosen during mailbox configuration
    Boolean m_TLS_OK = false; //Are we actually protected under a SSL layer?
    public const Boolean UntrustedRootOK = true; //Accept that the server to securely connect to has produced its own certificate
    public const Boolean CertNameMistmatchOK = false; //Accept that the server name (domain name) to securely connect to may mismatch the one provided in the certificate
    public const Boolean CertRevokedOK = true; //Allow expired certificates to be used by the server

    private struct MailMultiPart
    {
      public int lineStart;
      public int lineEnd;
      public string contentID;
      public string boundary;
      public string contentType;
      public string fileName;
      public string name;
      public string transferEncoding;
      public string contentDisposition;
    }
    public struct MailAttachment
    {
      public string attPath;
      public string attFileName;
      public int attKind; // 1 - image, 2 - audio, 3 - application rel.
    }
    // 
    enum SocketError
    {
      ERROR_NO_ERROR = 0,
      ERROR_TIMEOUT,
      ERROR_NO_DATA,
      ERROR_CONNECTION_ERR,
      ERROR_WRONG_DATA,
      ERROR_SERVER_NOT_READY,
      ERROR_UNKNOWN_MSG_FROM_SERVER = 999
    }

    enum MailAction
    {
      MAIL_MB_CONNECTED = 1,
      MAIL_SEND_CAPABILITIES,
      MAIL_SEND_STARTTLS,
      MAIL_ACTION_WAIT_SERVER_CERTIFICATE,
      MAIL_SEND_USER,
      MAIL_SEND_PASS,
      MAIL_SEND_STAT,
      MAIL_SEND_RETR,
      MAIL_SEND_QUIT,
      MAIL_SEND_LIST,
      MAIL_SEND_DELE,
      MAIL_SEND_RETR_LIST,
      MAIL_SEND_PERFORM,
      MAIL_ACTION_INVALID,
      MAIL_ACTION_READ_ON,
      MAIL_ACTION_READ_LIST

    }

    enum InformNumber
    {
      INFORM_CONNECTED = 8030,
      INFORM_GETTING_MAIL,
      INFORM_LOGOUT,
      INFORM_GETTING_MAIL_PROGRESS
    }
    // our events
    // an event to set diverse data
    public delegate void InformEventHandler(int informNumber, object informObject);
    public event InformEventHandler InformUser;
    //
    // timout handler
    public delegate void GotMailDataEventHandler(object mailObject, string mailData, int mailAction);
    public event GotMailDataEventHandler GotMailData;
    //
    // get mail known state
    //
    public MailClass()
    {
      m_timeOutTimer.Tick += new EventHandler(TimeOut);
    }
    ~MailClass()
    {
      //m_timeOutTimer.Tick-=new EventHandler(TimeOut);
    }
    // getting all mails from an mailbox
    public void ReadMailBox(ref MailBox mailbox)
    {
      IPAddress[] addr = new IPAddress[0];
      if (ServerExists(mailbox, ref addr) == true)
      {
        System.Net.IPEndPoint ePoint = new IPEndPoint(addr[0], mailbox.Port);
        //mailbox.ClearMailList();
        m_mb = mailbox;
        m_mailNumber = 0;
        m_endPoint = ePoint;
        m_TLSSecured = mailbox.TLS;
        m_mailSocket = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.IP);
        m_ierrorNumber = (int)SocketError.ERROR_NO_ERROR;
        m_errorMessage = "";
        m_mailFolder = mailbox.MailboxFolder;
        m_attachmentFolder = mailbox.AttachmentFolder;
        m_timeOutTimer.Interval = 60000; // set timeout to 15 seconds
        m_timeOutTimer.Start();
        AsyncCallback callback = new AsyncCallback(ConnectCallback);
        // Begin Asyncronous Connection
        m_mailSocket.BeginConnect(ePoint, callback, m_mailSocket);
        m_mailAction = (int)MailAction.MAIL_SEND_STAT;
        m_mailBuffer.Initialize();

      }
      else
      {
        GUIDialogOK dlgOK = (GUIDialogOK)GUIWindowManager.GetWindow((int)GUIWindow.Window.WINDOW_DIALOG_OK);
        if (dlgOK != null)
        {
          string serverString = GUILocalizeStrings.Get(8014);
          string mailBoxString = GUILocalizeStrings.Get(8015);
          dlgOK.SetHeading(8013);
          dlgOK.SetLine(1, string.Format(serverString, mailbox.ServerAddress));
          dlgOK.SetLine(2, string.Format(mailBoxString, mailbox.BoxLabel));
          dlgOK.SetLine(3, "");
          dlgOK.DoModal(8000);

        }
        m_ierrorNumber = (int)SocketError.ERROR_SERVER_NOT_READY;
        m_mb.MailCount = 0;
        Log.Write("mymail: server connecting problem {0}", m_mb.ServerAddress);
        InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
      }
    }
    // delete an mail from the server

    private void ConnectCallback(IAsyncResult ar)
    {
      try
      {
        Socket sock1 = (Socket)ar.AsyncState;
        if (sock1.Connected)
        {
          InformUser((int)InformNumber.INFORM_CONNECTED, "");
          m_timeOutTimer.Stop();

          m_mailSocketStream = new NetworkStream(m_mailSocket, false);

          if (m_TLSSecured == MailBox.SSL_PORT)
          {

            AsyncCallback recieveData = new AsyncCallback(OnRecievedData);

            try
            {
              m_mailTLSStream = new SslStream(m_mailSocketStream, false, this.RemoteCertificateValidationCallback);
              m_mailTLSStream.AuthenticateAsClient(m_mb.ServerAddress, null, System.Security.Authentication.SslProtocols.Ssl2 | System.Security.Authentication.SslProtocols.Ssl3 | System.Security.Authentication.SslProtocols.Tls, !CertRevokedOK);

              m_TLS_OK = true;

              m_currAction = (int)MailAction.MAIL_MB_CONNECTED;
              m_mailTLSStream.BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, m_mailTLSStream);

              Log.Write("mymail: connected to server {0} using SSL", m_mb.ServerAddress);

            }
            catch //(Exception ee)
            {
              /*
              Log.Write("mymail: SSL connection attempt to dedicated port failed: {0}", ee.Message);

              m_mailTLSStream = null;
              m_TLS_OK = false;

              m_ierrorNumber = (int)SocketError.ERROR_CONNECTION_ERR;
              m_currAction = (int)MailAction.MAIL_SEND_QUIT;
              m_mailSocketStream.BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, m_mailSocketStream);
              */
            }


          }
          else
          {
            AsyncCallback recieveData = new AsyncCallback(OnRecievedData);

            m_TLS_OK = false;

            m_currAction = (int)MailAction.MAIL_MB_CONNECTED;
            m_mailSocketStream.BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, m_mailSocketStream);

            Log.Write("mymail: connected to server {0}", m_mb.ServerAddress);

          }

        }
      }
      catch
      {
        //
      }
    }

    private void OnRecievedData(IAsyncResult ar)
    {
      if (m_currAction == (int)MailAction.MAIL_ACTION_WAIT_SERVER_CERTIFICATE)
      {

        if (m_TLSSecured == MailBox.STLS && m_TLS_OK)
        {
          Log.Write("mymail: re-connected to server {0} using SSL", m_mb.ServerAddress);
          InteractServer((int)MailAction.MAIL_SEND_USER);
        }
        else
        {
          Log.Write("mymail: SSL re-connection attempt using STARTTLS command failed: Explicit TLS issued w/out any effective SSL layer protection");
          m_ierrorNumber = (int)SocketError.ERROR_CONNECTION_ERR;
          InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
        }
        return;
      }

      int bytesCount;
      if (m_TLSSecured != MailBox.NO_SSL && m_TLS_OK)
        bytesCount = ((SslStream)ar.AsyncState).EndRead(ar);
      else
        bytesCount = ((NetworkStream)ar.AsyncState).EndRead(ar);
      System.String strData = System.Text.Encoding.ASCII.GetString(m_mailBuffer, 0, bytesCount);
      m_timeOutTimer.Stop();

      try
      {

        if (bytesCount == 0 && m_currAction == (int)MailAction.MAIL_ACTION_READ_ON)
        {
          Log.Write("mymail: there is an recieve error. no data was send from server");
          m_errorMessage = "Error. No Mail-End indicator found";
          m_ierrorNumber = (int)SocketError.ERROR_WRONG_DATA;
          InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
        }
        // getting mailbox list

        // getting mail number x
        if (bytesCount > 0 && m_currAction == (int)MailAction.MAIL_ACTION_READ_ON)
        {

          m_recMailData += strData;

          double percentDone = m_recMailData.Length * 100 / m_mailNumberSize;
          if (percentDone > 100f) percentDone = 100f;
          string percentDoneText = " (" + Convert.ToString(percentDone) + "%)";
          InformUser((int)InformNumber.INFORM_GETTING_MAIL, Convert.ToString((m_imailCount + 1) - m_mailNumber) + "/" + Convert.ToString(m_imailCount) + percentDoneText);

          if (m_recMailData.EndsWith(_CRLF_ + "." + _CRLF_) == false)
          {

            AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
            //m_timeOutTimer.Start();
            m_mailBuffer = System.Text.Encoding.ASCII.GetBytes(m_emptyBuffer);
            m_timeOutTimer.Start();
            if (recieveData != null)
              if (m_TLSSecured != MailBox.NO_SSL && m_TLS_OK)
                ((SslStream)ar.AsyncState).BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, (SslStream)ar.AsyncState);
              else
                ((NetworkStream)ar.AsyncState).BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, (NetworkStream)ar.AsyncState);
          }
          else
          {

            Log.Write("mymail: recieved message number {0}", Convert.ToString(m_mailNumber));
            try
            {
              SaveEMail(m_recMailData, m_mailNumber);
            }
            catch
            {
              Log.Write("mymail: there was an error creating the mail nr. {0}", Convert.ToString(m_mailNumber));
            }
            m_recMailData = "";
            m_mailNumber--;

            if (m_mailNumber <= 0)
            {
              m_mailBuffer = System.Text.Encoding.ASCII.GetBytes(m_emptyBuffer);
              Log.Write("mymail: all messages transfered. count: {0}", Convert.ToString(m_imailCount));
              m_ierrorNumber = (int)SocketError.ERROR_NO_ERROR;
              InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
            }
            else
            {
              InformUser((int)InformNumber.INFORM_GETTING_MAIL, Convert.ToString((m_imailCount + 1) - m_mailNumber) + "/" + Convert.ToString(m_imailCount));
              Log.Write("mymail: getting message number {0}", Convert.ToString(m_mailNumber));
              m_mailAction = (int)MailAction.MAIL_SEND_LIST;
              InteractServer((int)MailAction.MAIL_SEND_LIST);
            }
            return;
          }
        }


        if (bytesCount > 0 && m_currAction != (int)MailAction.MAIL_ACTION_READ_ON)
        {

          string strRecieved = strData;

          if (strRecieved.StartsWith("-ERR"))
          {
            m_errorMessage = strRecieved.Substring(4, strRecieved.Length - 4);
            m_ierrorNumber = (int)SocketError.ERROR_UNKNOWN_MSG_FROM_SERVER;
            InteractServer((int)MailAction.MAIL_SEND_QUIT);
          }
          if (strRecieved.Substring(0, 3).Equals("+OK"))
          {
            switch (m_currAction)
            {
              case (int)MailAction.MAIL_SEND_LIST:

                string size = strRecieved;
                size = size.Replace(_CRLF_, "");
                string[] strSeg = size.Split(new char[] { ' ' });
                int count = int.Parse(strSeg[strSeg.Length - 1]);

                if (count > 0 && IsMailInList(size) == false)
                {
                  AppendMailToList(size);
                  m_mailNumberSize = count;
                  m_mailAction = (int)MailAction.MAIL_SEND_RETR;
                  InteractServer((int)MailAction.MAIL_SEND_PERFORM); // get the next mail
                }
                else
                {
                  Log.Write("mymail: message number {0} has already been downloaded or is malformed", Convert.ToString(m_mailNumber));
                  m_mailNumber--;
                  if (m_mailNumber <= 0)
                  {
                    m_ierrorNumber = (int)SocketError.ERROR_NO_ERROR;
                    InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
                  }
                  else
                    InteractServer((int)MailAction.MAIL_SEND_LIST);
                }
                break;
              case (int)MailAction.MAIL_MB_CONNECTED: // send the user and begin login or first issue a STLS command
                if (m_TLSSecured == MailBox.STLS && !m_TLS_OK)
                  InteractServer((int)MailAction.MAIL_SEND_CAPABILITIES);
                else
                  InteractServer((int)MailAction.MAIL_SEND_USER);
                break;
              case (int)MailAction.MAIL_SEND_CAPABILITIES: // check server capabilities

                Boolean is_STLS_Authorized = false;
                //A t'on à faire à un serveur qui peut utiliser une couche SSL/TLS?
                Regex regexEndOfLine = new Regex(_CRLF_);
                string[] caps = regexEndOfLine.Split(strRecieved);
                for (int i = 0; i < caps.Length; i++)
                  if (caps[i].Substring(0, 4).Equals("STLS"))
                  {
                    is_STLS_Authorized = true;
                    InteractServer((int)MailAction.MAIL_SEND_STARTTLS);
                    break;
                  }
                //Non? alors on arrête là...
                if (!is_STLS_Authorized)
                {
                  Log.Write("mymail: Server does not advertise STLS. No secure connection will be issued.");
                  m_ierrorNumber = (int)SocketError.ERROR_CONNECTION_ERR;
                  InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
                }
                break;
              case (int)MailAction.MAIL_SEND_STARTTLS: // SSL/TLS via standard POP3 port

                AsyncCallback recieveData_TLS = new AsyncCallback(OnRecievedData);
                try
                {
                  m_mailTLSStream = new SslStream((NetworkStream)ar.AsyncState, false, this.RemoteCertificateValidationCallback);
                  m_TLS_OK = true;
                  m_currAction = (int)MailAction.MAIL_ACTION_WAIT_SERVER_CERTIFICATE;
                  m_mailTLSStream.BeginAuthenticateAsClient(m_mb.ServerAddress, null, System.Security.Authentication.SslProtocols.Ssl2 | System.Security.Authentication.SslProtocols.Ssl3 | System.Security.Authentication.SslProtocols.Tls, !CertRevokedOK, recieveData_TLS, m_mailTLSStream);
                }
                catch (Exception ee)
                {
                  m_mailSocket.Blocking = false;

                  Log.Write("mymail: SSL connection attempt using STARTTLS command failed: {0}", ee.Message);

                  m_mailTLSStream = null;
                  m_TLS_OK = false;

                  m_ierrorNumber = (int)SocketError.ERROR_CONNECTION_ERR;
                  m_currAction = (int)MailAction.MAIL_SEND_QUIT;
                  m_mailSocketStream.BeginRead(m_mailBuffer, 0, m_buffSize, recieveData_TLS, m_mailSocketStream);
                }
                break;
              case (int)MailAction.MAIL_SEND_USER: // send password
                InteractServer((int)MailAction.MAIL_SEND_PASS);
                break;
              case (int)MailAction.MAIL_SEND_PASS: // if the pass is sended we perform our action
                InteractServer((int)MailAction.MAIL_SEND_PERFORM);
                break;

              case (int)MailAction.MAIL_SEND_PERFORM: // we quit now

                if (m_mailAction == (int)MailAction.MAIL_SEND_STAT) // return the mail count from the inbox
                {

                  // get the mails count 
                  strRecieved = strRecieved.Replace(_CRLF_, "");
                  try
                  {
                    m_imailCount = int.Parse(Regex.Replace(strRecieved, @"^.*\+OK[ |	]+([0-9]+)[ |	]+.*$", "$1"));
                  }
                  catch
                  {
                    m_imailCount = 0;
                  }
                  //									m_imailCount=m_imailCount;
                  m_mb.MailCount = m_imailCount;
                  Log.Write("mymail: there are {0} messages in the mailbox {1}", Convert.ToString(m_imailCount), m_mb.BoxLabel);
                  if (m_imailCount > 0)
                  {
                    //m_lastMailToRecieve=m_mb.LastCheckCount;
                    m_mailNumber = m_imailCount;
                    //m_mb.LastCheckCount=m_imailCount;
                    InformUser((int)InformNumber.INFORM_GETTING_MAIL, Convert.ToString((m_imailCount + 1) - m_mailNumber) + "/" + Convert.ToString(m_imailCount));
                    InteractServer((int)MailAction.MAIL_SEND_LIST);
                    // starting out with getting a list
                    //m_mailAction=(int)MailAction.MAIL_SEND_RETR_LIST;
                    //InteractServer((int)MailAction.MAIL_SEND_PERFORM);
                  }
                  else
                  {
                    m_ierrorNumber = 2;// no mails on server
                    m_mb.MailCount = CountMail(m_mb);
                    InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
                  }
                }
                if (m_mailAction == (int)MailAction.MAIL_SEND_RETR) // return the mail content (if its size is greater than the actual buffer size, will pursue using MAIL_ACTION_READ_ON command on next buffer filling pass)
                {
                  //m_recMailData = "";
                  m_recMailData = strRecieved;
                  //QUICK FIX: mail bodies whose size is lower than buffer length don't pass MAIL_ACTION_READ_ON related procedure through MAIL_SEND_RETR related procedure. 
                  if (strRecieved.EndsWith(_CRLF_ + "." + _CRLF_))
                  {
                    Log.Write("mymail: recieved message number {0}", Convert.ToString(m_mailNumber));
                    try
                    {
                      SaveEMail(m_recMailData, m_mailNumber);
                    }
                    catch
                    {
                      Log.Write("mymail: there was an error creating the mail nr. {0}", Convert.ToString(m_mailNumber));
                    }
                    m_recMailData = "";
                    m_mailNumber--;
                    if (m_mailNumber <= 0)
                    {
                      m_mailBuffer = System.Text.Encoding.ASCII.GetBytes(m_emptyBuffer);
                      Log.Write("mymail: all messages transfered. count: {0}", Convert.ToString(m_imailCount));
                      m_ierrorNumber = (int)SocketError.ERROR_NO_ERROR;
                      InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
                    }
                    else
                    {
                      InformUser((int)InformNumber.INFORM_GETTING_MAIL, Convert.ToString((m_imailCount + 1) - m_mailNumber) + "/" + Convert.ToString(m_imailCount));
                      Log.Write("mymail: getting message number {0}", Convert.ToString(m_mailNumber));
                      m_mailAction = (int)MailAction.MAIL_SEND_LIST;
                      InteractServer((int)MailAction.MAIL_SEND_LIST);
                    }
                  }
                  else
                  {
                    m_currAction = (int)MailAction.MAIL_ACTION_READ_ON;
                    AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
                    if (m_TLSSecured != MailBox.NO_SSL && m_TLS_OK)
                      ((SslStream)ar.AsyncState).BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, (SslStream)ar.AsyncState);
                    else
                      ((NetworkStream)ar.AsyncState).BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, (NetworkStream)ar.AsyncState);
                  }
                }
                if (m_mailAction == (int)MailAction.MAIL_SEND_DELE) // return the mail count from the inbox
                  InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server

                break;
              case (int)MailAction.MAIL_SEND_QUIT:
                m_mb.MailCount = CountMail(m_mb);
                m_timeOutTimer.Stop();
                m_mailSocket.Close();
                m_mailSocketStream = null;
                m_mailTLSStream = null;
                if (m_ierrorNumber != 0)
                {
                  Log.Write("mymail: an error occured. errornumber {0} on mailbox {1}", Convert.ToString(m_ierrorNumber), m_mb.ServerAddress);
                  Log.Write("mymail: an error occured. errormessage from server {0}", m_errorMessage);
                }
                GotMailData(m_errorMessage, "Ready", m_ierrorNumber); // ready
                break;
            }
          } // else we are ready
        }
      }
      catch
      {
        Log.Write("mymail: recieve error. mail number {0} on mailbox {1}", Convert.ToString(m_mailNumber), m_mb.BoxLabel);
        InteractServer((int)MailAction.MAIL_SEND_QUIT); // logout from the server
      }
    }
    // sending some request to the mailserver
    // and set the required mailAction
    // for example: if we sended the user we send the pass...
    private int InteractServer(int action)
    {
      Socket sock1 = m_mailSocket;
      byte[] toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("");
      if (sock1.Connected == true)
      {
        switch (action)
        {
          case (int)MailAction.MAIL_SEND_LIST: // send user
            toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("list " + Convert.ToString(m_mailNumber) + _CRLF_);
            break;
          case (int)MailAction.MAIL_SEND_CAPABILITIES: //check capabilities (to this date, only used to verify if server supports STARTTLS)
            toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("capa" + _CRLF_);
            break;
          case (int)MailAction.MAIL_SEND_STARTTLS: //authenticate through explicit SSL/TLS
            toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("stls" + _CRLF_);
            break;
          case (int)MailAction.MAIL_SEND_USER: // send user
            toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("user " + m_mb.Username + _CRLF_);
            break;
          case (int)MailAction.MAIL_SEND_PASS: // send pass
            toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("pass " + m_mb.Password + _CRLF_);
            break;
          case (int)MailAction.MAIL_SEND_PERFORM: // send depends on what we request in Connect()
            if (m_mailAction == (int)MailAction.MAIL_SEND_STAT)
              toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("stat" + _CRLF_);
            if (m_mailAction == (int)MailAction.MAIL_SEND_RETR)
              toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("retr " + Convert.ToString(m_mailNumber) + _CRLF_);
            if (m_mailAction == (int)MailAction.MAIL_SEND_DELE)
              toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("dele " + Convert.ToString(m_mailNumber) + _CRLF_);
            if (m_mailAction == (int)MailAction.MAIL_SEND_RETR_LIST)
              toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("list" + _CRLF_);
            break;
          case (int)MailAction.MAIL_SEND_QUIT:
            toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("QUIT" + _CRLF_);
            break;
          default:
            toSend = System.Text.ASCIIEncoding.ASCII.GetBytes("");
            break;
        }
      }
      //m_timeOutTimer.Start();
      if (sock1.Connected == true && action > 0 && toSend.Length > 1)
      {
        m_currAction = action;
        AsyncCallback recieveData = new AsyncCallback(OnRecievedData);
        sock1.Blocking = false;
        //Log.Write(System.Text.Encoding.ASCII.GetString(toSend));
        //sock1.Poll(15000,System.Net.Sockets.SelectMode.SelectRead);
        //sock1.Send(toSend,0,toSend.Length,noFlags);
        if (m_TLSSecured != MailBox.NO_SSL && m_TLS_OK)
          m_mailTLSStream.Write(toSend, 0, toSend.Length);
        else
          m_mailSocketStream.Write(toSend, 0, toSend.Length);
        m_mailBuffer = System.Text.Encoding.ASCII.GetBytes(m_emptyBuffer);
        m_timeOutTimer.Start();
        //sock1.BeginReceive( m_mailBuffer, 0, m_buffSize, noFlags, recieveData , sock1 );
        if (m_TLSSecured != MailBox.NO_SSL && m_TLS_OK)
          m_mailTLSStream.BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, m_mailTLSStream);
        else
          m_mailSocketStream.BeginRead(m_mailBuffer, 0, m_buffSize, recieveData, m_mailSocketStream);
      }
      return 0;
    }

    private bool RemoteCertificateValidationCallback(Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
      SslPolicyErrors errors = sslPolicyErrors;

      if (((errors & SslPolicyErrors.RemoteCertificateChainErrors) ==
            SslPolicyErrors.RemoteCertificateChainErrors) && UntrustedRootOK)
      {
        errors -= SslPolicyErrors.RemoteCertificateChainErrors;
      }

      if (((errors & SslPolicyErrors.RemoteCertificateNameMismatch) ==
            SslPolicyErrors.RemoteCertificateNameMismatch) && CertNameMistmatchOK)
      {
        errors -= SslPolicyErrors.RemoteCertificateNameMismatch;
      }

      if (errors == SslPolicyErrors.None)
        return true;
      else
        return false;

      //Le serveur doit s'authentifier sans erreur
    }

    private void TimeOut(object sender, System.EventArgs e)
    {

      m_mailSocket.Close();
      m_ierrorNumber = (int)SocketError.ERROR_TIMEOUT; // error timeout
      GotMailData(m_errorMessage, "Ready", m_ierrorNumber); // ready

    }
    public void SetMailboxPath(string mailbox, string attachments)
    {
      m_mailFolder = mailbox;
      m_attachmentFolder = attachments;
    }
    public Socket GetSocket
    {
      get { return m_mailSocket; }
    }
    // get a mail
    protected virtual bool ServerExists(MailBox mb)
    {
      try
      {
        IPHostEntry hostIP = Dns.GetHostEntry(mb.ServerAddress);
        IPAddress[] addr = hostIP.AddressList;
      }
      catch
      {
        return false;
      }
      return true;
    }
    protected virtual bool ServerExists(MailBox mb, ref IPAddress[] addr)
    {
      try
      {
        IPHostEntry hostIP = Dns.GetHostEntry(mb.ServerAddress);
        string[] aliases = hostIP.Aliases;
        addr = hostIP.AddressList;
      }
      catch
      {
        return false;
      }
      return true;
    }
    // email body line
    bool IsBodyLine(string line)
    {
      return false;
    }
    // parse the mail
    public eMail ParseMailText(string mailText, bool decodeBody)
    {
      eMail retMail = new eMail();
      bool isMultipartMail = false;
      string mailBoundary = "";
      string multipartType = "";
      string headerCheckText = "";
      int bodyBlockOffset = 0;
      ArrayList headerBlock = new ArrayList();
      ArrayList bodyBlock = new ArrayList();
      retMail.AttachmentsPath = m_attachmentFolder;
      retMail.MailboxPath = m_mailFolder;
      string singlePartType = "";
      Regex mailSeparator = new Regex(@_CRLF_);
      InformUser(8025, "");
      try
      {
        string[] lines = mailSeparator.Split(mailText);
        // first getting the header-block
        int counter = 0;
        foreach (string line in lines)
        {
          if (line == "") break; // the first empty line indicates header end
          headerBlock.Add(line);
          headerCheckText += line;
          counter += 1;
        }
        // header ready
        byte[] mailID = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(headerCheckText), 0, headerCheckText.Length);
        retMail.MailID = System.Text.Encoding.ASCII.GetString(mailID);
        //
        bodyBlockOffset = counter + 1;
        if (decodeBody == true)
          for (int i = counter + 1; i < lines.Length; i++)
            bodyBlock.Add(lines[i]);
        // getting the body block
        foreach (string line in headerBlock)
        {
          string tmpText = "";
          if (Regex.Match(line, "^.*boundary=[\"]*([^\"]*).*$").Success)
            mailBoundary = Regex.Replace(line, "^.*boundary=[\"]*([^\"]*).*$", "$1");


          if (Regex.Match(line, @"^Content-Type: (.*)$").Success)
          {
            tmpText = Regex.Replace(line, @"^Content-Type: (.*)$", "$1");
            if (Regex.Match(tmpText, "^multipart/.*").Success)
            { isMultipartMail = true; multipartType = tmpText; }
            else
              singlePartType = Regex.Replace(tmpText, @"(.*);.*", "$1");
          }
          // get sender
          // get sender

          if (line.Length > 5)
            if (line.Substring(0, 5).Equals("From:"))
            {
              tmpText = Regex.Replace(line, @"^From:.*[ |<]([a-z|A-Z|0-9|\.|\-|_]+@[a-z|A-Z|0-9|\.|\-|_]+).*$", "$1");
              if (tmpText != "" && retMail.From.Equals(""))
              { retMail.From = tmpText; continue; }
            }
          // get to
          if (line.Length > 3)
            if (line.Substring(0, 3).Equals("To:"))
            {
              tmpText = Regex.Replace(line, @"^To:.*[ |<]([a-z|A-Z|0-9|\.|\-|_]+@[a-z|A-Z|0-9|\.|\-|_]+).*$", "$1");
              if (tmpText != "" && retMail.To.Equals(""))
              { retMail.To = tmpText; continue; }
            }
          // get subject
          if (line.Length > 8)
            if (line.Substring(0, 8).Equals("Subject:"))
            {
              tmpText = Regex.Replace(line, @"^Subject: (.*)$", "$1");
              if (tmpText != "" && retMail.Subject.Equals(""))
              {
                tmpText = Regex.Replace(tmpText, @".*=\?.*\?.*\?(.*)\?=$", "$1");
                tmpText = Regex.Replace(tmpText, @"_", " ");
                retMail.Subject += tmpText;
                retMail.Subject = TextToIntText(retMail.Subject);
                retMail.Subject.TrimEnd(new char[] { ' ' });
                continue;
              }
            }
          if (retMail.Subject != "" && Regex.Match(line, @".*=\?.*\?.*\?").Success)
          {
            tmpText = Regex.Replace(line, @".*=\?.*\?.*\?(.*)\?=$", "$1");
            tmpText = Regex.Replace(tmpText, @"_", " ");
            retMail.Subject += tmpText;
            retMail.Subject.TrimEnd(new char[] { ' ' });
            retMail.Subject = TextToIntText(retMail.Subject);
          }

        }
        if (isMultipartMail == true && decodeBody == true)//isMultipartMail==true && 
        {
          ArrayList boundarys = new ArrayList();
          ArrayList multiParts = new ArrayList();
          bool getAll = GetAllBoundarys(bodyBlock, mailBoundary, ref boundarys);
          GetAllMultiParts(bodyBlock, boundarys, ref multiParts);// we set all the parts from the mail
          if (multiParts.Count > 0)
          {
            int stepCount = 0;
            foreach (MailMultiPart mp in multiParts)
            {
              stepCount++;
              string mimeType = "";
              string mimeSubType = "";
              string contentType = mp.contentType.Replace(";", "");
              string[] splitter = contentType.Split(new char[] { '/' });
              InformUser(8025, " (step " + Convert.ToString(stepCount) + "/" + Convert.ToString(multiParts.Count) + ")");
              if (splitter.Length > 0)
              {
                string path = m_attachmentFolder + @"\";
                mimeType = splitter[0];
                mimeSubType = splitter[1];
                string tmpData = "";
                byte[] binData = null;
                MailAttachment mailAtt = new MailAttachment();
                System.IO.FileStream fs = null;
                string tmpName = "";
                if (mimeType != "image" && mimeType != "multipart")
                {
                  string[] splitt = mp.fileName.Split(new char[] { '.' });
                  if (splitt.Length > 1)
                  {
                    switch (splitt[splitt.Length - 1].ToLower())
                    {
                      case "jpg":
                      case "gif":
                      case "png":
                      case "tiff":
                      case "bmp":
                      case "jpeg":
                        mimeType = "application";
                        mimeSubType = splitt[splitt.Length - 1].ToLower();
                        break;
                      default:
                        //
                        break;
                    }
                  }
                  else
                  {
                    splitt = mp.fileName.Split(new char[] { '.' });
                    if (splitt.Length > 1)
                    {
                      switch (splitt[splitt.Length - 1].ToLower())
                      {
                        case "jpg":
                        case "gif":
                        case "png":
                        case "tiff":
                        case "bmp":
                        case "jpeg":
                          mimeType = "application";
                          mimeSubType = splitt[splitt.Length - 1].ToLower();
                          break;
                        default:
                          //
                          break;
                      }
                    }

                  }
                }
                switch (mimeType)
                {

                  case "text": // here are the plain and html body we want
                    if (mp.contentDisposition == "") // not an attachment
                    {
                      if (System.IO.File.Exists(path + "mail-body.html"))
                        System.IO.File.Delete(path + "mail-body.html"); // we kill the temp html-body
                      //
                      if (mimeSubType.StartsWith("plain"))
                        retMail.Body = GetMultiPartText(mailText, mp, bodyBlockOffset);// bodyBlock,mp :get text format
                      if (mimeSubType.StartsWith("html"))
                      {
                        retMail.HTML = GetMultiPartText(mailText, mp, bodyBlockOffset);// get text format
                        tmpData = GetMultiPartText(mailText, mp, bodyBlockOffset);
                        foreach (MailMultiPart mp1 in multiParts)
                          if (mp1.contentID != "")
                          {
                            tmpData = tmpData.Replace("cid:", "");
                            if (tmpData.IndexOf(mp1.contentID) >= 0)
                              tmpData = tmpData.Replace(mp1.contentID, "file:///" + path + mp1.name);
                          }
                        binData = Encoding.ASCII.GetBytes(tmpData);
                        fs = new System.IO.FileStream(path + "mail-body.html", System.IO.FileMode.CreateNew);
                        if (mp.name.Length > 0)
                          tmpName = mp.name;
                        else
                          tmpName = mp.fileName;
                        mailAtt.attFileName = tmpName;
                        mailAtt.attPath = path;
                        // not an real attachment, its for show in browser
                        // so we dont add an ref yet
                        fs.Write(binData, 0, binData.Length);
                        fs.Close();
                      }
                    }
                    if (mp.contentDisposition.StartsWith("attachment")) // not an attachment
                    {
                      tmpData = GetMultiPartText(mailText, mp, bodyBlockOffset);
                      tmpData = TextToIntText(tmpData);
                      binData = Encoding.ASCII.GetBytes(tmpData);
                      if (System.IO.File.Exists(path + mp.fileName))
                        System.IO.File.Delete(path + mp.fileName);
                      fs = new System.IO.FileStream(path + mp.fileName, System.IO.FileMode.CreateNew);
                      mailAtt.attFileName = mp.fileName;
                      mailAtt.attPath = path;
                      retMail.AddAttachment(mailAtt);
                      fs.Write(binData, 0, binData.Length);
                      fs.Close();
                    }
                    break;
                  case "image":
                    // we generate a bitmap to save it to disk
                    if (mp.name.Length > 0)
                      tmpName = mp.name;
                    else
                      tmpName = mp.fileName;
                    if (System.IO.File.Exists(path + tmpName) == false)
                    {
                      binData = GetMultiPartRaw(mailText, mp, bodyBlockOffset);
                      binData = GetBinaryBase64(binData);
                      System.IO.MemoryStream stream = new System.IO.MemoryStream(binData);
                      System.Drawing.Bitmap pic = new System.Drawing.Bitmap(stream);
                      if (pic != null)
                        pic.Save(path + tmpName);
                    }
                    mailAtt.attFileName = tmpName;
                    mailAtt.attPath = path;
                    mailAtt.attKind = 1; // image
                    retMail.AddAttachment(mailAtt);
                    break;
                  // we save other attachments to mail-folder
                  case "audio":
                  case "video":
                  case "application":
                    switch (mimeType)
                    {
                      case "audio":
                        mailAtt.attKind = 2;
                        break;
                      case "application":
                        mailAtt.attKind = 3;
                        break;
                      case "video":
                        mailAtt.attKind = 4;
                        break;
                      default:
                        mailAtt.attKind = 0;
                        break;
                    }

                    if (System.IO.File.Exists(path + mp.fileName) == false)
                    {
                      binData = GetMultiPartRaw(mailText, mp, bodyBlockOffset);
                      binData = GetBinaryBase64(binData);
                      fs = new System.IO.FileStream(path + mp.fileName, System.IO.FileMode.CreateNew);
                      fs.Write(binData, 0, binData.Length);
                      fs.Close();
                    }
                    mailAtt.attFileName = mp.fileName;
                    mailAtt.attPath = path;
                    retMail.AddAttachment(mailAtt);
                    break;
                  // other attachments ignored	
                }
              }
            }
          }

        }
        else
        {
          string[] splitter = singlePartType.Split(new char[] { '/' });
          if (splitter.Length > 1)
          {
            if (splitter[1].StartsWith("plain"))
              foreach (string line in bodyBlock)
                retMail.Body += line + _CRLF_;
            else
            {
              retMail.Body = "[only HTML in message!]" + _CRLF_;
              foreach (string line in bodyBlock)
                retMail.Body += line + _CRLF_;

            }
          }
          else
          {
            foreach (string line in bodyBlock)
              retMail.Body += line + _CRLF_;
          }
        }
        retMail.Body = TextToIntText(retMail.Body);
        retMail.HTML = TextToIntText(retMail.HTML);
        // TODO: body extracting improvement
        //retMail.Body=GetPlainMailBody(bodyBlock,isMultipartMail,mailBoundary);
        //retMail.HTML=GetHTMLMailBody(bodyBlock,isMultipartMail,mailBoundary);
      }
      catch (Exception)
      { retMail.Body = ""; retMail.HTML = ""; }
      return retMail;
    }
    // save mail
    public int GetEMailList(string folderName, ref System.IO.FileInfo[] theMailFileList)
    {
      try
      {
        System.IO.DirectoryInfo dInfo = new System.IO.DirectoryInfo(folderName);
        System.IO.FileInfo[] fInfoList = null;

        ArrayList list = new ArrayList();
        fInfoList = dInfo.GetFiles("*.mail");
        foreach (System.IO.FileInfo fInfo in fInfoList)
          list.Add(fInfo);
        //ArrayList list=new ArrayList();
        list.Sort(new CompareModificationTime());
        theMailFileList = new System.IO.FileInfo[list.Count];
        int c = 0;
        foreach (System.IO.FileInfo fInfo in list)
        {
          theMailFileList.SetValue(fInfo, c);
          c++;
        }
        int count = ((Array)theMailFileList).GetLength(0);
        return count;
      }
      catch (Exception)
      {
        return 0;
      }
    }
    public string LoadEMail(string fileName)
    {
      string mailText = "";
      if (System.IO.File.Exists(fileName) == true)
      {
        System.IO.StreamReader sr = new System.IO.StreamReader(fileName);
        mailText = sr.ReadToEnd();
        sr.Close();
        return mailText;
      }
      return "";
    }
    private void SaveEMail(string mailText, int mailNum)
    {
      string fileName = "";
      System.IO.DirectoryInfo dinf = new System.IO.DirectoryInfo(m_mb.MailboxFolder);
      System.IO.FileInfo[] fInfo = dinf.GetFiles("*.mail");
      string tmpPath = "";
      int n = fInfo.Length;
      do
      {
        n++;
        tmpPath = m_mb.MailboxFolder + @"\mail_message_" + Convert.ToString(n) + ".mail";
        if (n > 99999)
          break;
      }
      while (System.IO.File.Exists(tmpPath) == true);
      fileName = m_mb.MailboxFolder + @"\mail_message_" + Convert.ToString(fInfo.Length) + ".mail";
      Log.Write("mymail: saved message to file named {0}", fileName);
      if (CheckMailFileExists(mailText) == true) return;
      System.IO.StreamWriter sw = new System.IO.StreamWriter(fileName);
      sw.Write(mailText);
      sw.Close();
    }
    private bool CheckMailFileExists(string mailText)
    {
      string md5Check = GetMD5(mailText);
      System.IO.FileInfo[] mailFiles = null;
      int count = GetEMailList(m_mb.MailboxFolder, ref mailFiles);
      foreach (System.IO.FileInfo fInfo in mailFiles)
      {
        string tmpCheckTest = LoadEMail(m_mb.MailboxFolder + @"\" + fInfo.Name);
        if (md5Check.Equals(GetMD5(tmpCheckTest)) == true)
        {
          return true;
        }
      }
      return false;
    }
    public string GetMD5(string headerText)
    {
      string headerCheckText = "";
      Regex mailSeparator = new Regex(@_CRLF_);
      try
      {
        string[] lines = mailSeparator.Split(headerText);
        // first getting the header-block
        foreach (string line in lines)
        {
          if (line == "") break; // the first empty line indicates header end
          headerCheckText += line;
        }
        // header ready
        byte[] mailID = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(headerCheckText), 0, headerCheckText.Length);
        return System.Text.Encoding.ASCII.GetString(mailID);
      }
      catch
      {
        return "";
      }
    }
    public void SetMailToKnownState(string mailText)
    {
      string mailMD5 = GetMD5(mailText);
      byte[] buffer = Encoding.ASCII.GetBytes(mailMD5);
      string fileName = m_mailFolder + @"\knownmails.txt";
      System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Append);
      fs.Write(buffer, 0, buffer.Length);
      fs.WriteByte(13);
      fs.WriteByte(10);
      fs.Close();
    }
    //
    private bool IsMailInList(string mailText)
    {
      mailText = mailText.Replace(_CRLF_, "");
      string fileName = m_mailFolder + @"\transferList.txt";
      string listText = "";
      try
      {
        System.IO.TextReader tr = (System.IO.TextReader)System.IO.File.OpenText(fileName);
        listText = tr.ReadToEnd();
        tr.Close();

        Regex splitter = new Regex(@_CRLF_);
        string[] lines = splitter.Split(listText);
        foreach (string line in lines)
          if (mailText.Equals(line) == true)
            return true;
        return false;
      }
      catch
      {
        return false;
      }
    }
    //
    private void AppendMailToList(string text)
    {
      byte[] buffer = Encoding.ASCII.GetBytes(text);
      string fileName = m_mailFolder + @"\transferList.txt";
      System.IO.FileStream fs = new System.IO.FileStream(fileName, System.IO.FileMode.Append);
      fs.Write(buffer, 0, buffer.Length);
      fs.WriteByte(13);
      fs.WriteByte(10);
      fs.Close();
    }
    //
    public bool IsMailKnown(string fileName)
    {
      string mailText = LoadEMail(fileName);
      string compareChecksum = GetMD5(mailText);
      try
      {
        System.IO.FileStream fs = new System.IO.FileStream(m_mailFolder + @"\knownmails.txt", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.None);
        string kmText = "";
        byte[] knownMails = new Byte[(int)fs.Length];
        fs.Read(knownMails, 0, (int)fs.Length);
        fs.Close();
        kmText = Encoding.ASCII.GetString(knownMails);
        Regex splitter = new Regex(@_CRLF_);
        string[] lines = splitter.Split(kmText);
        foreach (string line in lines)
          if (compareChecksum.Equals(line))
            return true;
        return false;
      }
      catch
      {
        return false;
      }


    }
    //
    public int CountMail(MailBox mb)
    {
      System.IO.FileInfo[] theFileList = null;
      int count = GetEMailList(mb.MailboxFolder, ref theFileList);
      return count;
    }
    public int CountNewMail(MailBox mb)
    {
      System.IO.FileInfo[] theFileList = null;
      int countNewMails = 0;
      SetMailboxPath(mb.MailboxFolder, mb.AttachmentFolder);
      int count = GetEMailList(mb.MailboxFolder, ref theFileList);
      if (count > 0)
      {
        foreach (System.IO.FileInfo fInfo in theFileList)
        {
          if (IsMailKnown(mb.MailboxFolder + @"\" + fInfo.Name) == false)
            countNewMails++;
        }

      }
      return countNewMails;
    }
    // multi parts
    bool GetAllMultiParts(ArrayList bodyBlock, ArrayList boundarys, ref ArrayList parts)
    {
      //MailMultiPart mpSegment=null;
      parts.Clear();
      string content = "";
      string fileName = "";
      string name = "";
      int startLine = -1;
      int endLine = -1;
      int firstLine = -1;
      string contTransferEncoding = "";
      string contentID = "";
      string contDisposition = "";
      bool headerEnd = false;
      foreach (string line in boundarys)
      {
        startLine = -1;
        endLine = -1;
        firstLine = -1;
        content = "";
        fileName = "";
        contentID = "";
        contTransferEncoding = "";
        contDisposition = "";
        name = "";
        headerEnd = false;
        for (int i = 0; i < bodyBlock.Count; i++)
        {


          string text = (string)bodyBlock[i];
          if (text.StartsWith("--" + line))
          {
            if (startLine != -1)
            {
              endLine = i - 1;
              MailMultiPart mpSegment = new MailMultiPart();
              mpSegment.boundary = line;
              mpSegment.lineEnd = i - 1;
              mpSegment.lineStart = startLine;
              mpSegment.contentType = content;
              mpSegment.contentID = contentID;
              mpSegment.transferEncoding = contTransferEncoding;
              mpSegment.contentDisposition = contDisposition;
              if (fileName == "")
                fileName = "tmp-" + Convert.ToString(bodyBlock.GetHashCode()) + ".bin"; // generate a filename
              mpSegment.fileName = fileName;
              mpSegment.name = name;
              headerEnd = false;
              fileName = "";
              name = "";
              content = "";
              contTransferEncoding = "";
              contentID = "";
              contDisposition = "";
              startLine = -1;
              endLine = -1;
              parts.Add(mpSegment); // add it
            }
            if (startLine == -1)
              startLine = i + 1;
          }
          if (startLine != -1 && text == "")
            headerEnd = true;
          // start to examine the header data
          if (startLine != -1 && headerEnd == false)
          {
            if (text.IndexOf("Content-Type:") >= 0 && content == "")
              content = Regex.Replace(text, @"^Content-Type: (.*)$", "$1");
            // filename
            if (Regex.Match(text, "^[ |	]+filename=[\"]*([^\"]*).*$").Success && fileName == "")
              fileName = Regex.Replace(text, "^[ |	]+filename=[\"]*([^\"]*).*$", "$1");
            // name
            if (Regex.Match(text, "^[ |	]+name=[\"]*([^\"]*).*$").Success && name == "")
              name = Regex.Replace(text, "^[ |	]+name=[\"]*([^\"]*).*$", "$1");
            // disposition
            if (text.IndexOf("Content-Disposition:") >= 0 && contDisposition == "")
              contDisposition = Regex.Replace(text, @"^Content-Disposition: (.*)$", "$1");
            // encoding
            if (text.IndexOf("Content-Transfer-Encoding:") >= 0 && contTransferEncoding == "")
              contTransferEncoding = Regex.Replace(text, @"^Content-Transfer-Encoding: (.*)$", "$1");
            // content id
            if (text.IndexOf("Content-ID:") >= 0 && contentID == "")
              contentID = Regex.Replace(text, @"^Content-ID:.*[<](.*)[>]$", "$1");

            if (text == "" && firstLine == -1) // header end
              firstLine = i + 1;
          }
        }
      }
      return true;
    }

    //
    bool GetAllBoundarys(ArrayList lineBlock, string mainBoundary, ref ArrayList bounds)
    {
      int lineCount = 0;
      int startLine = -1;
      bounds.Clear();
      bounds.Add(mainBoundary);

      foreach (string line in lineBlock)
      {
        if (line.StartsWith("--" + mainBoundary))
        {
          startLine = lineCount;
          break;
        }
        lineCount++;
      }
      if (startLine == -1)
        return false;
      for (int i = startLine; i < lineBlock.Count; i++)
      {
        string line = (string)lineBlock[i];
        if (Regex.Match(line, "^.*boundary=[\"]*([^\"]*).*$").Success)
        {
          bounds.Add(Regex.Replace(line, "^.*boundary=[\"]*([^\"]*).*$", "$1"));
        }
      }
      return true;
    }
    //
    string GetBoundary(ArrayList dataBlock)
    {
      string boundary = "";

      foreach (string line in dataBlock)
      {
        if (Regex.Match(line, "^.*boundary=[\"]*([^\"]*).*$").Success)
        {
          boundary = Regex.Replace(line, "^.*boundary=[\"]*([^\"]*).*$", "$1");
          break;
        }
      }
      return boundary;
    }
    // this functions return the data from parts, still improving something here 
    string GetMultiPartText(string rawMail, MailMultiPart mp, int offset)
    {
      int start = -1;
      int startPos = 0;
      int endPos = 0;

      string[] lines = new Regex(@_CRLF_).Split(rawMail);

      startPos = rawMail.IndexOf(lines[mp.lineStart + offset]);

      for (int i = mp.lineStart + offset; i < mp.lineEnd + offset; i++)
      {
        string line = lines[i];
        startPos += line.Length;
        if (line == "")
        {
          start = i + 1;
          break;
        }
      }
      startPos += 2; // add 2 bytes for \r\n
      if (start == -1)
        return "";
      startPos = rawMail.IndexOf(lines[start], startPos);
      endPos = rawMail.IndexOf(lines[mp.lineEnd + offset + 1], startPos + 1);
      endPos -= 1;

      return rawMail.Substring(startPos, endPos - startPos);



    }
    byte[] GetMultiPartRaw(string rawMail, MailMultiPart mp, int offset)
    {

      int start = -1;
      int startPos = 0;
      int endPos = 0;

      string[] lines = new Regex(@_CRLF_).Split(rawMail);
      for (int i = mp.lineStart + offset; i < mp.lineEnd + offset; i++)
      {
        string line = lines[i];
        if (line == "")
        {
          start = i + 1;
          break;
        }
      }
      if (start == -1)
        return new byte[] { 0 };
      startPos = rawMail.IndexOf(lines[start]);
      endPos = rawMail.IndexOf(lines[mp.lineEnd + offset + 1], startPos + 1);
      endPos -= 1;
      byte[] partText = new Byte[endPos - startPos];

      Encoding.ASCII.GetBytes(rawMail, startPos, endPos - startPos, partText, 0);


      return partText;

    }
    //
    //
    private string TextToIntText(string theText)
    {
      string[] lines = theText.Split(new char[] { '=' });
      string transText = "";
      foreach (string line in lines)
      {
        string lineEnd = line;
        int val = -1;
        try
        {
          val = Convert.ToInt16("0x" + line.Substring(0, 2), 16);
        }
        catch
        {
        }
        if (val >= 0 && val <= 255 && line.Substring(0, 2).ToUpper() == line.Substring(0, 2))
        {
          lineEnd = ((char)val) + line.Substring(2, line.Length - 2);

        }
        transText += lineEnd;
      }
      return transText;
    }
    // base64 decoding
    public byte[] GetBinaryBase64(byte[] text)
    {
      try
      {
        string tmpStr = Encoding.ASCII.GetString(text);
        byte[] binaryDecoded = Convert.FromBase64String(tmpStr);
        return binaryDecoded;
      }
      catch
      {
        string tmpStr = Encoding.ASCII.GetString(text);
        return Encoding.ASCII.GetBytes(tmpStr);
      }
    }
  }

  // the eMail class
  public class eMail
  {
    string m_mailFrom;
    string m_mailTo;
    string m_mailSubject;
    string m_mailBody;
    string m_mailHTMLBody;
    string m_id;
    bool m_isSetAsRead;
    bool m_isSetForDelete;
    int m_mailNumberOnServer;
    string m_mailboxPath;
    string m_attachmentsPath;
    ArrayList m_attachments = new ArrayList();

    public eMail()
    {
      m_mailFrom = "";
      m_mailTo = "";
      m_mailSubject = "";
      m_id = "";
      m_mailBody = "";
      m_mailHTMLBody = "";
      m_isSetAsRead = false;
      m_mailboxPath = "";
      m_attachmentsPath = "";
      m_mailNumberOnServer = -1;
    }
    public void AddAttachment(MailClass.MailAttachment att)
    {
      m_attachments.Add(att);
    }
    public void GetAttachmentList(ref ArrayList theList)
    {
      theList = (ArrayList)m_attachments.Clone();
    }
    public int MailNumberOnServer
    {
      get { return m_mailNumberOnServer; }
      set { m_mailNumberOnServer = value; }
    }
    public string MailboxPath
    {
      get { return m_mailboxPath; }
      set { m_mailboxPath = value; }
    }
    public string AttachmentsPath
    {
      get { return m_attachmentsPath; }
      set { m_attachmentsPath = value; }
    }
    public bool IsSetForDelete
    {
      get { return m_isSetForDelete; }
      set { m_isSetForDelete = value; }
    }
    public bool SetRead
    {
      get { return m_isSetAsRead; }
      set { m_isSetAsRead = value; }
    }
    public string From
    {
      get { return m_mailFrom; }
      set { m_mailFrom = value; }
    }
    public string To
    {
      get { return m_mailTo; }
      set { m_mailTo = value; }
    }
    public string Subject
    {
      get { return m_mailSubject; }
      set { m_mailSubject = value; }
    }
    public string Body
    {
      get { return m_mailBody; }
      set { m_mailBody = value; }
    }
    public string HTML
    {
      get { return m_mailHTMLBody; }
      set { m_mailHTMLBody = value; }
    }

    public string MailID
    {
      get { return m_id; }
      set { m_id = value; }
    }

  }
  // the mailbox class
  public class MailBox
  {
    string m_mailBoxLabel;
    string m_userName;
    string m_passWord;
    string m_server;
    int m_mailsCount;
    int m_serverPort;
    string m_mailBoxPath;
    string m_attachmentsPath;
    int m_lastCheckCount;
    bool m_enabled = true;
    public const byte NO_SSL = 0;
    public const byte SSL_PORT = 1;
    public const byte STLS = 2;
    byte m_TLS;

    public MailBox(string label, string userName, string passWord, string server, int port, byte TLS, string mailboxFolder, string attachmentsFolder)
    {
      m_mailBoxLabel = label;
      m_userName = userName;
      m_passWord = passWord;
      m_server = server;
      m_serverPort = port; // by default port 110 is used
      m_mailsCount = 0;
      m_attachmentsPath = attachmentsFolder;
      m_mailBoxPath = mailboxFolder;
      m_lastCheckCount = 0;
      m_enabled = true;
      m_TLS = TLS;

      // Check if MailboxFolder directory exist
      System.IO.DirectoryInfo dInfoMail = new System.IO.DirectoryInfo(m_mailBoxPath);
      if (!dInfoMail.Exists)
      {
        dInfoMail.Create(); // Creating the mail directory
        Log.Write("Mailbox {0} created mail folder: {1}", label, m_mailBoxPath);
      }

      // Check if Attachments directory exist
      System.IO.DirectoryInfo dInfoAtt = new System.IO.DirectoryInfo(m_attachmentsPath);
      if (!dInfoAtt.Exists)
      {
        dInfoAtt.Create(); // Creating the mail directory
        Log.Write("Mailbox {0} created attachemnts folder: {1}", label, m_attachmentsPath);
      }
    }

    //[Editor(typeof(FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public string AttachmentFolder
    {
      get { return m_attachmentsPath; }
      set { m_attachmentsPath = value; }
    }

    [BrowsableAttribute(false)]
    public int LastCheckCount
    {
      get { return m_lastCheckCount; }
      set { m_lastCheckCount = value; }
    }

    //[Editor(typeof(FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
    public string MailboxFolder
    {
      get { return m_mailBoxPath; }
      set { m_mailBoxPath = value; }
    }
    public int Port
    {
      get { return m_serverPort; }
      set { m_serverPort = value; }
    }
    public byte TLS
    {
      get { return m_TLS; }
      set { m_TLS = value; }
    }
    public string Username
    {
      get { return m_userName; }
      set { m_userName = value; }
    }
    [BrowsableAttribute(false)]
    public int MailCount
    {
      get { return m_mailsCount; }
      set { m_mailsCount = value; } // no need to set
    }
    [BrowsableAttribute(false)]
    public string Password
    {
      get { return m_passWord; }
      set { m_passWord = value; }
    }

    public string ServerAddress
    {
      get { return m_server; }
      set { m_server = value; }
    }
    public string BoxLabel
    {
      get { return m_mailBoxLabel; }
      set { m_mailBoxLabel = value; }
    }

    public bool Enabled
    {
      get { return m_enabled; }
      set { m_enabled = value; }
    }

    public override string ToString()
    {
      if (m_mailBoxLabel != null)
        if (m_mailBoxLabel != "")
          return m_mailBoxLabel;
      return base.ToString();
    }

  }
  public class CompareModificationTime : System.Collections.IComparer
  {
    public int Compare(object objA, object objB)// : System.Collections.IComparer
    {
      System.IO.FileInfo fInfoA = (System.IO.FileInfo)objA;
      System.IO.FileInfo fInfoB = (System.IO.FileInfo)objB;
      if (fInfoA.LastWriteTime > fInfoB.LastWriteTime) return -1;
      if (fInfoA.LastWriteTime == fInfoB.LastWriteTime) return 0;
      if (fInfoA.LastWriteTime < fInfoB.LastWriteTime) return 1;
      return 0;
    }

  }
}
