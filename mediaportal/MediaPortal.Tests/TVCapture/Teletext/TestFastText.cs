#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
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

using System.IO;
using System.Reflection;
using MediaPortal.TV.Teletext;
using NUnit.Framework;

namespace MediaPortal.Tests.Teletext
{
  [TestFixture]
  public class TestFastText
  {
    [Test]
    public void Decode()
    {
      Assembly assm = Assembly.GetExecutingAssembly();
      string[] names = assm.GetManifestResourceNames();
      Stream stream = assm.GetManifestResourceStream("MediaPortal.Tests.TVCapture.Teletext.FastTextPage");
      BinaryReader reader = new BinaryReader(stream);
      byte[] byPage = new byte[stream.Length];
      reader.Read(byPage, 0, (int) stream.Length);

      FastTextDecoder decoder = new FastTextDecoder();
      decoder.Decode(byPage);
      Assert.AreEqual(decoder.Red, 0x123);
      Assert.AreEqual(decoder.Green, 0x456);
      Assert.AreEqual(decoder.Yellow, 0x678);
      Assert.AreEqual(decoder.Blue, 0x789);
      Assert.AreEqual(decoder.White, 0x874);
    }
  }
}