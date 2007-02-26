using System;
using System.Collections.Generic;
using System.Text;

namespace DirecTV
{
  public class Command
  {
    public byte command;
    public int bytesToSend;
    public int bytesToReceive;
    public byte[] dataToSend;

    public Command(byte command)
      : this(command, 0, 0)
    { }
    public Command(byte command, int bytesToSend)
      : this(command, bytesToSend, 0)
    { }
    public Command(byte command, int bytesToSend, int bytesToReceive)
      : this(command, bytesToSend, bytesToReceive, new byte[bytesToSend])
    { }
    public Command(byte command, int bytesToSend, int bytesToReceive, byte[] dataToSend)
    {
      this.command = command;
      this.bytesToSend = bytesToSend;
      this.bytesToReceive = bytesToReceive;
      this.dataToSend = dataToSend;
    }
    public Command(Command cmd)
    {
      this.command = cmd.command;
      this.bytesToSend = cmd.bytesToSend;
      this.bytesToReceive = cmd.bytesToReceive;
      this.dataToSend = cmd.dataToSend;
    }
    public Command Clone()
    {
      Command cmd = new Command(this);
      return cmd;
    }
  }
}
