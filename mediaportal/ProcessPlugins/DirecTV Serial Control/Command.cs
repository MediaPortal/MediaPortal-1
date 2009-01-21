/* 
 *	Copyright (C) 2005-2009 Team MediaPortal - micheloe, patrick, diehard2
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
    {
    }

    public Command(byte command, int bytesToSend)
      : this(command, bytesToSend, 0)
    {
    }

    public Command(byte command, int bytesToSend, int bytesToReceive)
      : this(command, bytesToSend, bytesToReceive, new byte[bytesToSend])
    {
    }

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