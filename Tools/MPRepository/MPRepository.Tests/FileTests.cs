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
using System.Collections.Generic;
using System.Text;
using System.IO;
using MPRepository.Storage;
using MPRepository.Support;
using NUnit.Framework;
using NHibernate;

namespace MPRepository.Tests
{

  [TestFixture]
  public class FileTests
  {

    [Test]
    public void TestLocation()
    {
      string location  = FileManager.GetSaveLocation("test.mpe1");
      System.Console.WriteLine("Save location is {0}", location);

      Assert.That(location, Is.Not.Null);

      bool dirExists = Directory.Exists(Path.GetDirectoryName(location));

      Assert.That(dirExists, Is.True);

    }

    [Test]
    public void TestAddFile()
    {
      string sourceFile = @"c:\tmp\2\ViewModeSwitcher.mpe1";

      using (ISession session = DatabaseHelper.GetCurrentSession())
      {
        using (ITransaction transaction = session.BeginTransaction())
        {
          MPFile file = new MPFile();
          file.Location = sourceFile;
          session.SaveOrUpdate(file);
          transaction.Commit();
        }
        DatabaseHelper.CloseSession(session);
      }

    }


  }
}
