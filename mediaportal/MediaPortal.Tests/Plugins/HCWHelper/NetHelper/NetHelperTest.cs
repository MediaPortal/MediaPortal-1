#region Copyright (C) 2006 Team MediaPortal - Author: mPod

/* 
 *	Copyright (C) 2006 Team MediaPortal - Author: mPod
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
using System.Collections.Generic;
using System.Text;
using System.Threading;
using NUnit.Framework;
using NetHelper;

namespace MediaPortal.Tests.Plugins.HCWHelper.NetHelper
{
  [TestFixture]
  [Category("NetHelper")]
  public class NetHelperTest
  {
    int udpPort = 2110;

    //[Test]
    //public void ListenForIncoming()
    //{
    //  Connection connection = new Connection(true);
    //  Assert.IsTrue(connection.Start(udpPort));
    //  connection.Stop();
    //  connection = null;
    //  Thread.Sleep(2000);
    //}

    [Test]
    public void SendMessage()
    {
      Connection connection = new Connection(true);
      Assert.IsTrue(connection.Send(udpPort, "CMD", "0", DateTime.Now));
    }

  }
}
