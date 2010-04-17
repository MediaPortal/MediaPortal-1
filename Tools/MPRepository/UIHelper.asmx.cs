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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using MPRepository.Controller;
using MPRepository.Items;


namespace MPRepository.Web
{
  /// <summary>
  /// Summary description for UIHelper
  /// </summary>
  [WebService(Namespace = "http://tempuri.org/")]
  [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
  [ToolboxItem(false)]
  [System.Web.Script.Services.ScriptService]
  public class UIHelper : System.Web.Services.WebService
  {

    [WebMethod]
    public string GetTags()
    {

      MPRSession session = MPRController.StartSession();
      IList<MPTag> tags = MPRController.RetrieveAll<MPTag>(session);

      string result = "";

      foreach (MPTag tag in tags)
      {
        result += System.String.Format("{0}:{1}:{2};", tag.Id, tag.Name, tag.Description);
      }

      MPRController.EndSession(session, true);

      return result;
    }

    [WebMethod]
    public string GetTypes()
    {

      MPRSession session = MPRController.StartSession();
      IList<MPItemType> types = MPRController.RetrieveAll<MPItemType>(session);

      string result = "";

      foreach (MPItemType type in types)
      {
        result += System.String.Format("{0}:{1}:{2};", type.Id, type.Name, type.Description);
      }

      MPRController.EndSession(session, true);

      return result;
    }

    [WebMethod]
    public string GetCategories()
    {

      MPRSession session = MPRController.StartSession();
      IList<MPCategory> categories = MPRController.RetrieveAll<MPCategory>(session);

      string result = "";

      foreach (MPCategory category in categories)
      {
        result += System.String.Format("{0}:{1}:{2};", category.Id, category.Name, category.Type);
      }

      MPRController.EndSession(session, true);

      return result;
    }

    [WebMethod]
    public string GetCategories(Int64 typeId)
    {

      MPRSession session = MPRController.StartSession();
      IList<MPCategory> categories = MPRController.RetrieveByForeignKey<MPCategory>(session, "Type", typeId);

      string result = "";

      foreach (MPCategory category in categories)
      {
        result += System.String.Format("{0}:{1}:{2};", category.Id, category.Name, category.Type);
      }

      MPRController.EndSession(session, true);

      return result;
    }


  }
}
