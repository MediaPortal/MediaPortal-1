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
using Iesi.Collections.Generic;
using System.Text;
using NHibernate;
using NHibernate.Criterion;
using MPRepository.Items;
using MPRepository.Support;
using MPRepository.Controller;
using NUnit.Framework;

namespace MPRepository.Tests
{
  [TestFixture]
  public class ItemTests
  {

    [Test]
    public void TestTagsAdd()
    {
      string[,] tags = 
      {
        { "video", "video tag" },
        { "music", "music tag" },
        { "picture", "picture tag" },
        { "test1",  "test tag" },
      };

      using (ISession session = DatabaseHelper.GetCurrentSession())
      {
        using (ITransaction transaction = session.BeginTransaction())
        {
          for (int i = 0; i <= tags.GetUpperBound(0); i++)
          {
            MPTag tag = new MPTag();
            tag.Name = tags[i,0];
            tag.Description = tags[i,1];
            session.SaveOrUpdate(tag);
          }
          transaction.Commit();
        }
      }

    }

    [Test]
    public void TestTagsParse()
    {
      //string tagsList = "video,music,test2,test3";
      string tagsList = "video,music,test1";
      MPRSession session = MPRController.StartSession();
      ISet<MPTag> tags = MPRController.GetTags(session, tagsList);

      Assert.That(tags.Count, Is.EqualTo(3));

      foreach (MPTag tag in tags)
      {
        System.Console.WriteLine("{0} : {1}", tag.Id, tag.Name);
      }

      MPRController.EndSession(session, true);

    }


    [Test]
    public void TestTypeAdd()
    {
      string[,] types = 
      {
        { "Codecs", "Different multimedia codecs." },
        { "Drivers", "Drivers for different hardware" },
        { "HTPC Customization", "Customize your HTPC the way you like it" },
        { "Plugins", "Plugins for extended functionality" },
        { "Skins", "Download skins to make Media Portal look the way YOU want it." },
        { "System Utilities", "System utillties to aid your way." },
      };

      using (ISession session = DatabaseHelper.GetCurrentSession())
      {
        using (ITransaction transaction = session.BeginTransaction())
        {
          for (int i = 0; i <= types.GetUpperBound(0); i++)
          {
            MPItemType type = new MPItemType();
            type.Name = types[i, 0];
            type.Description = types[i, 1];
            session.SaveOrUpdate(type);
          }
          transaction.Commit();
        }
      }

    }

    [Test]
    public void TestCategoryAdd()
    {
      
      string[,] categories = 
      { // type,     category,      description

        { "Codecs",  "Audio",  "Codecs for audio" },
        { "Codecs",  "Video",  "Codecs for video" },
        { "Codecs",  "Audio & Video",  "Codecs for both audio and video" },

        { "Plugins", "Audio/Radio", "Plugins for audio" },
        { "Plugins", "Automation", "Automating MediaPortal tasks" },
        { "Plugins", "EPG/TV", "EPG - Electronic Program Guide programs/grabbers" },
        { "Plugins", "Web", "Web stuff for Media Portal" },
        { "Plugins", "Video/Movies", "Plugins for video" },

        { "Skins", "16:10", "Skins created to use on a 16:10 (widescreen) television/monitor." },
        { "Skins", "16:9", "Skins created to use on a 16:9 (widescreen) television/monitor." },
        { "Skins", "4:3", "Skins created to use on a 4:3(non-widescreen) television/monitor." },
        { "Skins", "Tools", "Skin tools" },

      };
      

      using (ISession session = DatabaseHelper.GetCurrentSession())
      {
        using (ITransaction transaction = session.BeginTransaction())
        {
          string prevType = null;
          MPItemType mptype = null;
          for (int i = 0; i <= categories.GetUpperBound(0); i++)
          {
            string type = categories[i, 0];

            if (type != prevType)
            { // load new type
              mptype = session
                .CreateQuery("from MPItemType mptype where mptype.Name=?")
                .SetString(0, type)
                .UniqueResult<MPItemType>();
              prevType = type;
            }

            Assert.NotNull(mptype);
            Assert.That(mptype.Name, Is.EqualTo(type));

            MPCategory category = new MPCategory();
            category.Type = mptype;
            category.Name = categories[i, 1];
            category.Description = categories[i, 2];
            session.SaveOrUpdate(category);
          }
          transaction.Commit();
        }
      }

    }

    [Test]
    public void TestGetAllTypes()
    {
      MPRSession session = MPRController.StartSession();
      IList<MPItemType> types = MPRController.RetrieveAll<MPItemType>(session);
      foreach (MPItemType type in types)
      {
        System.Console.WriteLine("{0} : {1} : {2}", type.Id, type.Name, type.Description);
      }
      MPRController.EndSession(session, true);
    }

    [Test]
    public void GetCategoriesForType()
    {
      Int64 typeId = 4;

      MPRSession session = MPRController.StartSession();

      IList<MPCategory> categories = MPRController.RetrieveByForeignKey<MPCategory>(session, "Type", typeId);

      foreach (MPCategory category in categories)
      {
        System.Console.WriteLine("{0}:{1}:{2};", category.Id, category.Name, category.Description);
      }

      MPRController.EndSession(session, true);
    }

    [Test]
    public void GetCategoriesForIdList()
    {
      MPRSession session = MPRController.StartSession();
      List<Int64> ids = new List<Int64>();
      ids.Add(5); ids.Add(7); ids.Add(8);
      IList<MPCategory> categories = MPRController.RetrieveByIdList<MPCategory>(session, ids);

      Assert.That(categories.Count, Is.EqualTo(3));

      foreach (MPCategory category in categories)
      {
        System.Console.WriteLine("{0} : {1} : {2}", category.Id, category.Name, category.Description);
      }
       
    }

  }
}
