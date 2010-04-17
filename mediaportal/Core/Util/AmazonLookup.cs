#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.IO;
using System.Collections;
using System.Net;
using ConsoleApplication2.com.amazon.webservices;
using MediaPortal.ExtensionMethods;

namespace MediaPortal.Util
{
  /// <summary>
  /// 
  /// </summary>
  public class AmazonImageSearch
  {
    private ArrayList m_imageList = new ArrayList();

    public AmazonImageSearch() {}

    public int Count
    {
      get { return m_imageList.Count; }
    }

    public string this[int index]
    {
      get
      {
        if (index < 0 || index >= m_imageList.Count) return string.Empty;
        return (string)m_imageList[index];
      }
    }

    public void Search(string searchtag)
    {
      if (searchtag == null) return;
      if (searchtag == string.Empty) return;
      m_imageList.DisposeAndClearList();
      AWSECommerceService amazonService = new AWSECommerceService();
      ItemSearch itemSearch = new ItemSearch();
      ItemSearchRequest itemSearchRequest = new ItemSearchRequest();
      ItemSearchResponse itemSearchResponse;

      itemSearch.SubscriptionId = "1CTB4YKNPBV7EK9SQVG2";
      itemSearch.AssociateTag = "";
      itemSearchRequest.Keywords = searchtag;
      itemSearchRequest.SearchIndex = "DVD";
      itemSearchRequest.ResponseGroup = new string[] {"Large", "Images", "ItemAttributes", "OfferFull"};
      itemSearchRequest.ItemPage = "1";
      itemSearch.Request = new ItemSearchRequest[1] {itemSearchRequest};
      //send the query
      try
      {
        itemSearchResponse = amazonService.ItemSearch(itemSearch);
      }
      catch (Exception)
      {
        return;
      }

      Items[] itemsResponse = itemSearchResponse.Items;
      if (itemsResponse == null) return;
      for (int i = 0; i < itemsResponse.Length; ++i)
      {
        Items itemList = itemsResponse[i];
        if (itemList == null) continue;
        if (itemList.Item == null) continue;

        for (int x = 0; x < itemList.Item.Length; ++x)
        {
          Item item = itemList.Item[x];
          if (item == null) continue;
          Image image = item.LargeImage;
          if (image != null)
          {
            if (image.URL != null && image.URL.Length > 0)
            {
              m_imageList.Add(image.URL);
            }
          }
        }
      }
    }
  }
}