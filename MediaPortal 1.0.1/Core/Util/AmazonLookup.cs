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
using System.IO;
using System.Collections;
using System.Net;
using ConsoleApplication2.com.amazon.webservices;

namespace MediaPortal.Util
{
	/// <summary>
	/// 
	/// </summary>
	public class AmazonImageSearch
	{
    ArrayList m_imageList = new ArrayList();

		public AmazonImageSearch()
		{
		}

    public int Count
    {
      get { return m_imageList.Count;}
    }

    public string this[int index]
    {
      get { 
				if (index<0 || index>=m_imageList.Count) return string.Empty;
				return (string)m_imageList[index];
			}
    }
    
    public void Search(string searchtag)
    {
			if (searchtag==null) return;
			if (searchtag==string.Empty) return;
      m_imageList.Clear();
			AWSECommerceService amazonService = new AWSECommerceService();
			ItemSearch itemSearch = new ItemSearch();
			ItemSearchRequest itemSearchRequest = new ItemSearchRequest();
			ItemSearchResponse itemSearchResponse;

			itemSearch.SubscriptionId="1CTB4YKNPBV7EK9SQVG2";
			itemSearch.AssociateTag = "";
			itemSearchRequest.Keywords = searchtag;
			itemSearchRequest.SearchIndex = "DVD";
			itemSearchRequest.ResponseGroup = new string [] { "Large", "Images", "ItemAttributes", "OfferFull" };
			itemSearchRequest.ItemPage = "1";
			itemSearch.Request = new ItemSearchRequest[1] {itemSearchRequest};
			//send the query
			try
			{
				itemSearchResponse = amazonService.ItemSearch(itemSearch);
			}
			catch (Exception )
			{
				return;
			}

			Items[] itemsResponse = itemSearchResponse.Items; 
			if (itemsResponse==null) return;
			for (int i=0; i < itemsResponse.Length;++i)
			{
				Items itemList=itemsResponse[i];
				if (itemList==null) continue;
				if (itemList.Item==null) continue;

				for (int x=0; x < itemList.Item.Length;++x)
				{
					Item item=itemList.Item[x];
					if (item==null) continue;
					Image image = item.LargeImage;
					if (image!=null)
					{
						if (image.URL!=null && image.URL.Length>0)
						{
							m_imageList.Add(image.URL);
						}
					}
				}
			}
    }

	}
}
