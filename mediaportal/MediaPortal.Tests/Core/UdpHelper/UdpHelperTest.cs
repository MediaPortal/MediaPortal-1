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
using NUnit.Framework;
using UdpHelper;

namespace MediaPortal.Tests.Plugins.HCWHelper.NetHelper
{
  [TestFixture]
  [Category("UdpHelper")]
  public class NetHelperTest
  {
    private int udpPort = 2110;

    [SetUp]
    public void Init() {}

    [Test]
    public void SendMessage()
    {
      Connection connection = new Connection(true);
      Assert.IsTrue(connection.Send(udpPort, "CMD", "0", DateTime.Now));
    }

    [Test]
    public void Start()
    {
      Connection connection = new Connection(true);
      Assert.IsTrue(connection.Start(udpPort + 1));
      connection.Stop();
    }
  }
}