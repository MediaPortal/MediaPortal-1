using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Pipes;
using System.Collections;

using Mediaportal.TV.Server.TVLibrary.Interfaces.Logging;

namespace Mediaportal.TV.Server.TVLibrary.SatIp
{
  class FilterCommunication
  {
    // Constants for the Protocol over the Named Pipe
    public static readonly UInt32 SATIP_PROT_ADDPID = 0;
    public static readonly UInt32 SATIP_PROT_DELPID = 1;
    public static readonly UInt32 SATIP_PROT_SYNCPID = 2;
    public static readonly UInt32 SATIP_PROT_CLIENTIP = 3;
    public static readonly UInt32 SATIP_PROT_CLIENTPORT = 4;
    public static readonly UInt32 SATIP_PROT_STARTSTREAM = 5;
    public static readonly UInt32 SATIP_PROT_STOPSTREAM = 6;
    public static readonly UInt32 SATIP_PROT_NEWSLOT = 7;
    
    private Byte[] _Bytes = new Byte[0];
    private int _slot;
    private string _pipeName;
    private NamedPipeClientStream _namedPipeClientStream;
    
    public FilterCommunication(string pipeName, int slot)
    {
      _pipeName = pipeName;
      _slot = slot;
    }

    public void addClientPort(int sendPort)
    {
      addToQueue(SATIP_PROT_CLIENTPORT);
      addToQueue(_slot);
      addToQueue(sendPort);
    }

    public void addClientIp(string IP)
    {
      addToQueue(SATIP_PROT_CLIENTIP);
      addToQueue(_slot);
      addToQueue(4); // IP Version

      string[] IP_parts = IP.Split(new Char[] {'.'});
      foreach (string part in IP_parts)
      {
        addToQueue(part);
      }
    }

    public void requestNewSlot()
    {
      addToQueue(SATIP_PROT_NEWSLOT);
      addToQueue(0);
      addToQueue(_slot); // Slot that we request
    }

    public void addSyncPids(ArrayList pids)
    {
      addToQueue(SATIP_PROT_SYNCPID);
      addToQueue(_slot);
      addToQueue(pids.Count); // how many pids we have

      foreach (int pid in pids)
      {
        addToQueue(pid);
      }
    }

    public void send()
    {
      _namedPipeClientStream = new NamedPipeClientStream(_pipeName); // TODO: chaneg pipe name
      _namedPipeClientStream.Connect();
      
      if (_namedPipeClientStream != null && _namedPipeClientStream.IsConnected)
      {
        this.LogInfo("Connected to pipe with name: {0}", _pipeName);
        BinaryWriter writer = new BinaryWriter(_namedPipeClientStream);
        writer.Write(_Bytes);
        writer.Flush();
      }
      else
      {
        this.LogError("Couldn't connect to filter with pipename: {0}", _pipeName);
      }
      _namedPipeClientStream.Close();
    }

    private void addToQueue(UInt32 part)
    {
      Array.Resize(ref _Bytes, _Bytes.Length + 4);

      _Bytes[_Bytes.Length - 4] = (byte)(part >> 24); // Command
      _Bytes[_Bytes.Length - 3] = (byte)(part >> 16);
      _Bytes[_Bytes.Length - 2] = (byte)(part >> 8);
      _Bytes[_Bytes.Length - 1] = (byte)(part);
    }

    private void addToQueue(int part)
    {
      UInt32 partIntern = (UInt32)part;
      addToQueue(partIntern);
    }

    private void addToQueue(string part)
    {
      int value;
      if (!int.TryParse(part, out value))
      {
        this.LogError("Error parsing string to int");
        return;
      }

      UInt32 partIntern = (UInt32)value;
      addToQueue(partIntern);
    }
  }
}
