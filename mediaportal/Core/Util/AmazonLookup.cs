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
				if (index<0 || index>=m_imageList.Count) return String.Empty;
				return (string)m_imageList[index];
			}
    }
    
    public void Search(string searchtag)
    {
			if (searchtag==null) return;
			if (searchtag==String.Empty) return;
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
