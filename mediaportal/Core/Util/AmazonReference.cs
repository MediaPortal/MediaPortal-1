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

namespace ConsoleApplication2.com.amazon.webservices
{
  using System.Diagnostics;
  using System.Xml.Serialization;
  using System;
  using System.Web.Services.Protocols;
  using System.ComponentModel;
  using System.Web.Services;


  /// <remarks/>
  [System.Diagnostics.DebuggerStepThroughAttribute()]
  [System.ComponentModel.DesignerCategoryAttribute("code")]
  [System.Web.Services.WebServiceBindingAttribute(Name = "AWSECommerceServiceBinding",
    Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (CartAddRequestItem[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (CartCreateRequestItem[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (CartModifyRequestItem[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (string[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (ItemAttributesLanguage[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (BrowseNode[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (TransactionItem[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (object[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (TransactionShipmentPackage[]))]
  [System.Xml.Serialization.XmlIncludeAttribute(typeof (TransactionShipment[]))]
  public class AWSECommerceService : System.Web.Services.Protocols.SoapHttpClientProtocol
  {
    /// <remarks/>
    public AWSECommerceService()
    {
      this.Url = "http://soap.amazon.com/onca/soap?Service=AWSECommerceService";
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("HelpResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public HelpResponse Help(
      [System.Xml.Serialization.XmlElementAttribute("Help",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] Help Help1)
    {
      object[] results = this.Invoke("Help", new object[]
                                               {
                                                 Help1
                                               });
      return ((HelpResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginHelp(Help Help1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("Help", new object[]
                                        {
                                          Help1
                                        }, callback, asyncState);
    }

    /// <remarks/>
    public HelpResponse EndHelp(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((HelpResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("ItemSearchResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public ItemSearchResponse ItemSearch(
      [System.Xml.Serialization.XmlElementAttribute("ItemSearch",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] ItemSearch ItemSearch1)
    {
      object[] results = this.Invoke("ItemSearch", new object[]
                                                     {
                                                       ItemSearch1
                                                     });
      return ((ItemSearchResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginItemSearch(ItemSearch ItemSearch1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("ItemSearch", new object[]
                                              {
                                                ItemSearch1
                                              }, callback, asyncState);
    }

    /// <remarks/>
    public ItemSearchResponse EndItemSearch(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((ItemSearchResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("ItemLookupResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public ItemLookupResponse ItemLookup(
      [System.Xml.Serialization.XmlElementAttribute("ItemLookup",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] ItemLookup ItemLookup1)
    {
      object[] results = this.Invoke("ItemLookup", new object[]
                                                     {
                                                       ItemLookup1
                                                     });
      return ((ItemLookupResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginItemLookup(ItemLookup ItemLookup1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("ItemLookup", new object[]
                                              {
                                                ItemLookup1
                                              }, callback, asyncState);
    }

    /// <remarks/>
    public ItemLookupResponse EndItemLookup(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((ItemLookupResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("BrowseNodeLookupResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public BrowseNodeLookupResponse BrowseNodeLookup(
      [System.Xml.Serialization.XmlElementAttribute("BrowseNodeLookup",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] BrowseNodeLookup BrowseNodeLookup1)
    {
      object[] results = this.Invoke("BrowseNodeLookup", new object[]
                                                           {
                                                             BrowseNodeLookup1
                                                           });
      return ((BrowseNodeLookupResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginBrowseNodeLookup(BrowseNodeLookup BrowseNodeLookup1, System.AsyncCallback callback,
                                                     object asyncState)
    {
      return this.BeginInvoke("BrowseNodeLookup", new object[]
                                                    {
                                                      BrowseNodeLookup1
                                                    }, callback, asyncState);
    }

    /// <remarks/>
    public BrowseNodeLookupResponse EndBrowseNodeLookup(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((BrowseNodeLookupResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("ListSearchResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public ListSearchResponse ListSearch(
      [System.Xml.Serialization.XmlElementAttribute("ListSearch",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] ListSearch ListSearch1)
    {
      object[] results = this.Invoke("ListSearch", new object[]
                                                     {
                                                       ListSearch1
                                                     });
      return ((ListSearchResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginListSearch(ListSearch ListSearch1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("ListSearch", new object[]
                                              {
                                                ListSearch1
                                              }, callback, asyncState);
    }

    /// <remarks/>
    public ListSearchResponse EndListSearch(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((ListSearchResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("ListLookupResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public ListLookupResponse ListLookup(
      [System.Xml.Serialization.XmlElementAttribute("ListLookup",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] ListLookup ListLookup1)
    {
      object[] results = this.Invoke("ListLookup", new object[]
                                                     {
                                                       ListLookup1
                                                     });
      return ((ListLookupResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginListLookup(ListLookup ListLookup1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("ListLookup", new object[]
                                              {
                                                ListLookup1
                                              }, callback, asyncState);
    }

    /// <remarks/>
    public ListLookupResponse EndListLookup(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((ListLookupResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("CustomerContentSearchResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public CustomerContentSearchResponse CustomerContentSearch(
      [System.Xml.Serialization.XmlElementAttribute("CustomerContentSearch",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] CustomerContentSearch
        CustomerContentSearch1)
    {
      object[] results = this.Invoke("CustomerContentSearch", new object[]
                                                                {
                                                                  CustomerContentSearch1
                                                                });
      return ((CustomerContentSearchResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginCustomerContentSearch(CustomerContentSearch CustomerContentSearch1,
                                                          System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("CustomerContentSearch", new object[]
                                                         {
                                                           CustomerContentSearch1
                                                         }, callback, asyncState);
    }

    /// <remarks/>
    public CustomerContentSearchResponse EndCustomerContentSearch(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((CustomerContentSearchResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("CustomerContentLookupResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public CustomerContentLookupResponse CustomerContentLookup(
      [System.Xml.Serialization.XmlElementAttribute("CustomerContentLookup",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] CustomerContentLookup
        CustomerContentLookup1)
    {
      object[] results = this.Invoke("CustomerContentLookup", new object[]
                                                                {
                                                                  CustomerContentLookup1
                                                                });
      return ((CustomerContentLookupResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginCustomerContentLookup(CustomerContentLookup CustomerContentLookup1,
                                                          System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("CustomerContentLookup", new object[]
                                                         {
                                                           CustomerContentLookup1
                                                         }, callback, asyncState);
    }

    /// <remarks/>
    public CustomerContentLookupResponse EndCustomerContentLookup(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((CustomerContentLookupResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("SimilarityLookupResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public SimilarityLookupResponse SimilarityLookup(
      [System.Xml.Serialization.XmlElementAttribute("SimilarityLookup",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] SimilarityLookup SimilarityLookup1)
    {
      object[] results = this.Invoke("SimilarityLookup", new object[]
                                                           {
                                                             SimilarityLookup1
                                                           });
      return ((SimilarityLookupResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginSimilarityLookup(SimilarityLookup SimilarityLookup1, System.AsyncCallback callback,
                                                     object asyncState)
    {
      return this.BeginInvoke("SimilarityLookup", new object[]
                                                    {
                                                      SimilarityLookup1
                                                    }, callback, asyncState);
    }

    /// <remarks/>
    public SimilarityLookupResponse EndSimilarityLookup(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((SimilarityLookupResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("SellerLookupResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public SellerLookupResponse SellerLookup(
      [System.Xml.Serialization.XmlElementAttribute("SellerLookup",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] SellerLookup SellerLookup1)
    {
      object[] results = this.Invoke("SellerLookup", new object[]
                                                       {
                                                         SellerLookup1
                                                       });
      return ((SellerLookupResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginSellerLookup(SellerLookup SellerLookup1, System.AsyncCallback callback,
                                                 object asyncState)
    {
      return this.BeginInvoke("SellerLookup", new object[]
                                                {
                                                  SellerLookup1
                                                }, callback, asyncState);
    }

    /// <remarks/>
    public SellerLookupResponse EndSellerLookup(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((SellerLookupResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("CartGetResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public CartGetResponse CartGet(
      [System.Xml.Serialization.XmlElementAttribute("CartGet",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] CartGet CartGet1)
    {
      object[] results = this.Invoke("CartGet", new object[]
                                                  {
                                                    CartGet1
                                                  });
      return ((CartGetResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginCartGet(CartGet CartGet1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("CartGet", new object[]
                                           {
                                             CartGet1
                                           }, callback, asyncState);
    }

    /// <remarks/>
    public CartGetResponse EndCartGet(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((CartGetResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("CartAddResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public CartAddResponse CartAdd(
      [System.Xml.Serialization.XmlElementAttribute("CartAdd",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] CartAdd CartAdd1)
    {
      object[] results = this.Invoke("CartAdd", new object[]
                                                  {
                                                    CartAdd1
                                                  });
      return ((CartAddResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginCartAdd(CartAdd CartAdd1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("CartAdd", new object[]
                                           {
                                             CartAdd1
                                           }, callback, asyncState);
    }

    /// <remarks/>
    public CartAddResponse EndCartAdd(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((CartAddResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("CartCreateResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public CartCreateResponse CartCreate(
      [System.Xml.Serialization.XmlElementAttribute("CartCreate",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] CartCreate CartCreate1)
    {
      object[] results = this.Invoke("CartCreate", new object[]
                                                     {
                                                       CartCreate1
                                                     });
      return ((CartCreateResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginCartCreate(CartCreate CartCreate1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("CartCreate", new object[]
                                              {
                                                CartCreate1
                                              }, callback, asyncState);
    }

    /// <remarks/>
    public CartCreateResponse EndCartCreate(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((CartCreateResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("CartModifyResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public CartModifyResponse CartModify(
      [System.Xml.Serialization.XmlElementAttribute("CartModify",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] CartModify CartModify1)
    {
      object[] results = this.Invoke("CartModify", new object[]
                                                     {
                                                       CartModify1
                                                     });
      return ((CartModifyResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginCartModify(CartModify CartModify1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("CartModify", new object[]
                                              {
                                                CartModify1
                                              }, callback, asyncState);
    }

    /// <remarks/>
    public CartModifyResponse EndCartModify(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((CartModifyResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("CartClearResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public CartClearResponse CartClear(
      [System.Xml.Serialization.XmlElementAttribute("CartClear",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] CartClear CartClear1)
    {
      object[] results = this.Invoke("CartClear", new object[]
                                                    {
                                                      CartClear1
                                                    });
      return ((CartClearResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginCartClear(CartClear CartClear1, System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("CartClear", new object[]
                                             {
                                               CartClear1
                                             }, callback, asyncState);
    }

    /// <remarks/>
    public CartClearResponse EndCartClear(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((CartClearResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("TransactionLookupResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public TransactionLookupResponse TransactionLookup(
      [System.Xml.Serialization.XmlElementAttribute("TransactionLookup",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] TransactionLookup
        TransactionLookup1)
    {
      object[] results = this.Invoke("TransactionLookup", new object[]
                                                            {
                                                              TransactionLookup1
                                                            });
      return ((TransactionLookupResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginTransactionLookup(TransactionLookup TransactionLookup1,
                                                      System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("TransactionLookup", new object[]
                                                     {
                                                       TransactionLookup1
                                                     }, callback, asyncState);
    }

    /// <remarks/>
    public TransactionLookupResponse EndTransactionLookup(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((TransactionLookupResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("SellerListingSearchResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public SellerListingSearchResponse SellerListingSearch(
      [System.Xml.Serialization.XmlElementAttribute("SellerListingSearch",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] SellerListingSearch
        SellerListingSearch1)
    {
      object[] results = this.Invoke("SellerListingSearch", new object[]
                                                              {
                                                                SellerListingSearch1
                                                              });
      return ((SellerListingSearchResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginSellerListingSearch(SellerListingSearch SellerListingSearch1,
                                                        System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("SellerListingSearch", new object[]
                                                       {
                                                         SellerListingSearch1
                                                       }, callback, asyncState);
    }

    /// <remarks/>
    public SellerListingSearchResponse EndSellerListingSearch(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((SellerListingSearchResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("SellerListingLookupResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public SellerListingLookupResponse SellerListingLookup(
      [System.Xml.Serialization.XmlElementAttribute("SellerListingLookup",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] SellerListingLookup
        SellerListingLookup1)
    {
      object[] results = this.Invoke("SellerListingLookup", new object[]
                                                              {
                                                                SellerListingLookup1
                                                              });
      return ((SellerListingLookupResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginSellerListingLookup(SellerListingLookup SellerListingLookup1,
                                                        System.AsyncCallback callback, object asyncState)
    {
      return this.BeginInvoke("SellerListingLookup", new object[]
                                                       {
                                                         SellerListingLookup1
                                                       }, callback, asyncState);
    }

    /// <remarks/>
    public SellerListingLookupResponse EndSellerListingLookup(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((SellerListingLookupResponse)(results[0]));
    }

    /// <remarks/>
    [System.Web.Services.Protocols.SoapDocumentMethodAttribute("http://soap.amazon.com",
      Use = System.Web.Services.Description.SoapBindingUse.Literal,
      ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
    [return:
      System.Xml.Serialization.XmlElementAttribute("MultiOperationResponse",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")]
    public MultiOperationResponse MultiOperation(
      [System.Xml.Serialization.XmlElementAttribute("MultiOperation",
        Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")] MultiOperation MultiOperation1)
    {
      object[] results = this.Invoke("MultiOperation", new object[]
                                                         {
                                                           MultiOperation1
                                                         });
      return ((MultiOperationResponse)(results[0]));
    }

    /// <remarks/>
    public System.IAsyncResult BeginMultiOperation(MultiOperation MultiOperation1, System.AsyncCallback callback,
                                                   object asyncState)
    {
      return this.BeginInvoke("MultiOperation", new object[]
                                                  {
                                                    MultiOperation1
                                                  }, callback, asyncState);
    }

    /// <remarks/>
    public MultiOperationResponse EndMultiOperation(System.IAsyncResult asyncResult)
    {
      object[] results = this.EndInvoke(asyncResult);
      return ((MultiOperationResponse)(results[0]));
    }
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Help
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public HelpRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public HelpRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class HelpRequest
  {
    /// <remarks/>
    public string About;

    /// <remarks/>
    public HelpRequestHelpType HelpType;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HelpTypeSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum HelpRequestHelpType
  {
    /// <remarks/>
    Operation,

    /// <remarks/>
    ResponseGroup,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class MultiOperationResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    public HelpResponse HelpResponse;

    /// <remarks/>
    public ItemSearchResponse ItemSearchResponse;

    /// <remarks/>
    public ItemLookupResponse ItemLookupResponse;

    /// <remarks/>
    public ListSearchResponse ListSearchResponse;

    /// <remarks/>
    public ListLookupResponse ListLookupResponse;

    /// <remarks/>
    public CustomerContentSearchResponse CustomerContentSearchResponse;

    /// <remarks/>
    public CustomerContentLookupResponse CustomerContentLookupResponse;

    /// <remarks/>
    public SimilarityLookupResponse SimilarityLookupResponse;

    /// <remarks/>
    public SellerLookupResponse SellerLookupResponse;

    /// <remarks/>
    public CartGetResponse CartGetResponse;

    /// <remarks/>
    public CartAddResponse CartAddResponse;

    /// <remarks/>
    public CartCreateResponse CartCreateResponse;

    /// <remarks/>
    public CartModifyResponse CartModifyResponse;

    /// <remarks/>
    public CartClearResponse CartClearResponse;

    /// <remarks/>
    public TransactionLookupResponse TransactionLookupResponse;

    /// <remarks/>
    public SellerListingSearchResponse SellerListingSearchResponse;

    /// <remarks/>
    public SellerListingLookupResponse SellerListingLookupResponse;

    /// <remarks/>
    public BrowseNodeLookupResponse BrowseNodeLookupResponse;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class OperationRequest
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Header", IsNullable = false)] public HTTPHeadersHeader[]
      HTTPHeaders;

    /// <remarks/>
    public string RequestId;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Argument", IsNullable = false)] public ArgumentsArgument[]
      Arguments;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Error", IsNullable = false)] public ErrorsError[] Errors;

    /// <remarks/>
    public System.Single RequestProcessingTime;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool RequestProcessingTimeSpecified;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class HTTPHeadersHeader
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()] public string Name;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()] public string Value;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ArgumentsArgument
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()] public string Name;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()] public string Value;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ErrorsError
  {
    /// <remarks/>
    public string Code;

    /// <remarks/>
    public string Message;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class HelpResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Information")] public Information[] Information;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Information
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("OperationInformation")] public OperationInformation[]
      OperationInformation;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroupInformation")] public ResponseGroupInformation[]
      ResponseGroupInformation;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Request
  {
    /// <remarks/>
    public string IsValid;

    /// <remarks/>
    public HelpRequest HelpRequest;

    /// <remarks/>
    public BrowseNodeLookupRequest BrowseNodeLookupRequest;

    /// <remarks/>
    public ItemSearchRequest ItemSearchRequest;

    /// <remarks/>
    public ItemLookupRequest ItemLookupRequest;

    /// <remarks/>
    public ListSearchRequest ListSearchRequest;

    /// <remarks/>
    public ListLookupRequest ListLookupRequest;

    /// <remarks/>
    public CustomerContentSearchRequest CustomerContentSearchRequest;

    /// <remarks/>
    public CustomerContentLookupRequest CustomerContentLookupRequest;

    /// <remarks/>
    public SimilarityLookupRequest SimilarityLookupRequest;

    /// <remarks/>
    public CartGetRequest CartGetRequest;

    /// <remarks/>
    public CartAddRequest CartAddRequest;

    /// <remarks/>
    public CartCreateRequest CartCreateRequest;

    /// <remarks/>
    public CartModifyRequest CartModifyRequest;

    /// <remarks/>
    public CartClearRequest CartClearRequest;

    /// <remarks/>
    public TransactionLookupRequest TransactionLookupRequest;

    /// <remarks/>
    public SellerListingSearchRequest SellerListingSearchRequest;

    /// <remarks/>
    public SellerListingLookupRequest SellerListingLookupRequest;

    /// <remarks/>
    public SellerLookupRequest SellerLookupRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Error", IsNullable = false)] public ErrorsError[] Errors;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class BrowseNodeLookupRequest
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("BrowseNodeId")] public string[] BrowseNodeId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemSearchRequest
  {
    /// <remarks/>
    public string Actor;

    /// <remarks/>
    public string Artist;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("AudienceRating")] public AudienceRating[] AudienceRating;

    /// <remarks/>
    public string Author;

    /// <remarks/>
    public string Brand;

    /// <remarks/>
    public string BrowseNode;

    /// <remarks/>
    public string City;

    /// <remarks/>
    public string Composer;

    /// <remarks/>
    public Condition Condition;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool ConditionSpecified;

    /// <remarks/>
    public string Conductor;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string Count;

    /// <remarks/>
    public string Cuisine;

    /// <remarks/>
    public DeliveryMethod DeliveryMethod;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool DeliveryMethodSpecified;

    /// <remarks/>
    public string Director;

    /// <remarks/>
    public string FutureLaunchDate;

    /// <remarks/>
    public string ISPUPostalCode;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string ItemPage;

    /// <remarks/>
    public string Keywords;

    /// <remarks/>
    public string Manufacturer;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string MaximumPrice;

    /// <remarks/>
    public string MerchantId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string MinimumPrice;

    /// <remarks/>
    public string MusicLabel;

    /// <remarks/>
    public string Neighborhood;

    /// <remarks/>
    public string Orchestra;

    /// <remarks/>
    public string PostalCode;

    /// <remarks/>
    public string Power;

    /// <remarks/>
    public string Publisher;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    public string SearchIndex;

    /// <remarks/>
    public string Sort;

    /// <remarks/>
    public string State;

    /// <remarks/>
    public string TextStream;

    /// <remarks/>
    public string Title;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum AudienceRating
  {
    /// <remarks/>
    G,

    /// <remarks/>
    PG,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("PG-13")] PG13,

    /// <remarks/>
    R,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("NC-17")] NC17,

    /// <remarks/>
    NR,

    /// <remarks/>
    Unrated,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("6")] Item6,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("12")] Item12,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("16")] Item16,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("18")] Item18,

    /// <remarks/>
    FamilyViewing,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum Condition
  {
    /// <remarks/>
    All,

    /// <remarks/>
    New,

    /// <remarks/>
    Used,

    /// <remarks/>
    Collectible,

    /// <remarks/>
    Refurbished,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum DeliveryMethod
  {
    /// <remarks/>
    Ship,

    /// <remarks/>
    ISPU,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemLookupRequest
  {
    /// <remarks/>
    public Condition Condition;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool ConditionSpecified;

    /// <remarks/>
    public DeliveryMethod DeliveryMethod;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool DeliveryMethodSpecified;

    /// <remarks/>
    public string FutureLaunchDate;

    /// <remarks/>
    public ItemLookupRequestIdType IdType;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool IdTypeSpecified;

    /// <remarks/>
    public string ISPUPostalCode;

    /// <remarks/>
    public string MerchantId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string OfferPage;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ItemId")] public string[] ItemId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string ReviewPage;

    /// <remarks/>
    public string SearchIndex;

    /// <remarks/>
    public string SearchInsideKeywords;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string VariationPage;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum ItemLookupRequestIdType
  {
    /// <remarks/>
    ASIN,

    /// <remarks/>
    UPC,

    /// <remarks/>
    SKU,

    /// <remarks/>
    EAN,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ListSearchRequest
  {
    /// <remarks/>
    public string City;

    /// <remarks/>
    public string Email;

    /// <remarks/>
    public string FirstName;

    /// <remarks/>
    public string LastName;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string ListPage;

    /// <remarks/>
    public ListSearchRequestListType ListType;

    /// <remarks/>
    public string Name;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    public string State;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum ListSearchRequestListType
  {
    /// <remarks/>
    WishList,

    /// <remarks/>
    WeddingRegistry,

    /// <remarks/>
    BabyRegistry,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ListLookupRequest
  {
    /// <remarks/>
    public Condition Condition;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool ConditionSpecified;

    /// <remarks/>
    public DeliveryMethod DeliveryMethod;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool DeliveryMethodSpecified;

    /// <remarks/>
    public string ISPUPostalCode;

    /// <remarks/>
    public string ListId;

    /// <remarks/>
    public ListLookupRequestListType ListType;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool ListTypeSpecified;

    /// <remarks/>
    public string MerchantId;

    /// <remarks/>
    public string ProductGroup;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string ProductPage;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    public string Sort;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum ListLookupRequestListType
  {
    /// <remarks/>
    WishList,

    /// <remarks/>
    Listmania,

    /// <remarks/>
    WeddingRegistry,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CustomerContentSearchRequest
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string CustomerPage;

    /// <remarks/>
    public string Email;

    /// <remarks/>
    public string Name;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CustomerContentLookupRequest
  {
    /// <remarks/>
    public string CustomerId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string ReviewPage;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SimilarityLookupRequest
  {
    /// <remarks/>
    public Condition Condition;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool ConditionSpecified;

    /// <remarks/>
    public DeliveryMethod DeliveryMethod;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool DeliveryMethodSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ItemId")] public string[] ItemId;

    /// <remarks/>
    public string ISPUPostalCode;

    /// <remarks/>
    public string MerchantId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    public SimilarityLookupRequestSimilarityType SimilarityType;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool SimilarityTypeSpecified;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum SimilarityLookupRequestSimilarityType
  {
    /// <remarks/>
    Intersection,

    /// <remarks/>
    Random,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartGetRequest
  {
    /// <remarks/>
    public string CartId;

    /// <remarks/>
    public string HMAC;

    /// <remarks/>
    public string MergeCart;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartAddRequest
  {
    /// <remarks/>
    public string CartId;

    /// <remarks/>
    public string HMAC;

    /// <remarks/>
    public string MergeCart;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Item", IsNullable = false)] public CartAddRequestItem[] Items;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartAddRequestItem
  {
    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    public string OfferListingId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string Quantity;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string ListItemId;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartCreateRequest
  {
    /// <remarks/>
    public string MergeCart;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Item", IsNullable = false)] public CartCreateRequestItem[] Items;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartCreateRequestItem
  {
    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    public string OfferListingId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string Quantity;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string ListItemId;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartModifyRequest
  {
    /// <remarks/>
    public string CartId;

    /// <remarks/>
    public string HMAC;

    /// <remarks/>
    public string MergeCart;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Item", IsNullable = false)] public CartModifyRequestItem[] Items;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartModifyRequestItem
  {
    /// <remarks/>
    public CartModifyRequestItemAction Action;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool ActionSpecified;

    /// <remarks/>
    public string CartItemId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string Quantity;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum CartModifyRequestItemAction
  {
    /// <remarks/>
    MoveToCart,

    /// <remarks/>
    SaveForLater,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartClearRequest
  {
    /// <remarks/>
    public string CartId;

    /// <remarks/>
    public string HMAC;

    /// <remarks/>
    public string MergeCart;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TransactionLookupRequest
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("TransactionId")] public string[] TransactionId;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerListingSearchRequest
  {
    /// <remarks/>
    public string BrowseNode;

    /// <remarks/>
    public string Country;

    /// <remarks/>
    public string Keywords;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string ListingPage;

    /// <remarks/>
    public SellerListingSearchRequestOfferStatus OfferStatus;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool OfferStatusSpecified;

    /// <remarks/>
    public string PostalCode;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    public string SearchIndex;

    /// <remarks/>
    public string SellerId;

    /// <remarks/>
    public SellerListingSearchRequestShipOption ShipOption;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool ShipOptionSpecified;

    /// <remarks/>
    public string Sort;

    /// <remarks/>
    public string Title;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum SellerListingSearchRequestOfferStatus
  {
    /// <remarks/>
    Open,

    /// <remarks/>
    Closed,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum SellerListingSearchRequestShipOption
  {
    /// <remarks/>
    ShipTo,

    /// <remarks/>
    ShipFrom,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerListingLookupRequest
  {
    /// <remarks/>
    public string Id;

    /// <remarks/>
    public SellerListingLookupRequestIdType IdType;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool IdTypeSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum SellerListingLookupRequestIdType
  {
    /// <remarks/>
    Exchange,

    /// <remarks/>
    Listing,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerLookupRequest
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ResponseGroup")] public string[] ResponseGroup;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("SellerId")] public string[] SellerId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string FeedbackPage;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class OperationInformation
  {
    /// <remarks/>
    public string Name;

    /// <remarks/>
    public string Description;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Parameter", IsNullable = false)] public string[] RequiredParameters;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Parameter", IsNullable = false)] public string[]
      AvailableParameters;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("ResponseGroup", IsNullable = false)] public string[]
      DefaultResponseGroups;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("ResponseGroup", IsNullable = false)] public string[]
      AvailableResponseGroups;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ResponseGroupInformation
  {
    /// <remarks/>
    public string Name;

    /// <remarks/>
    public string CreationDate;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Operation", IsNullable = false)] public string[] ValidOperations;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Element", IsNullable = false)] public string[] Elements;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemSearchResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Items")] public Items[] Items;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Items
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalResults;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("SearchIndex", IsNullable = false)] public
      SearchResultsMapSearchIndex[] SearchResultsMap;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Item")] public Item[] Item;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SearchResultsMapSearchIndex
  {
    /// <remarks/>
    public string IndexName;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string Results;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string Pages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")] public string RelevanceRank;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ASIN")] public string[] ASIN;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Item
  {
    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Error", IsNullable = false)] public ErrorsError[] Errors;

    /// <remarks/>
    public string DetailPageURL;

    /// <remarks/>
    public string SalesRank;

    /// <remarks/>
    public Image SmallImage;

    /// <remarks/>
    public Image MediumImage;

    /// <remarks/>
    public Image LargeImage;

    /// <remarks/>
    public ItemAttributes ItemAttributes;

    /// <remarks/>
    public OfferSummary OfferSummary;

    /// <remarks/>
    public Offers Offers;

    /// <remarks/>
    public VariationSummary VariationSummary;

    /// <remarks/>
    public Variations Variations;

    /// <remarks/>
    public CustomerReviews CustomerReviews;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)] public EditorialReview[] EditorialReviews;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("SimilarProduct", IsNullable = false)] public
      SimilarProductsSimilarProduct[] SimilarProducts;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Accessory", IsNullable = false)] public AccessoriesAccessory[]
      Accessories;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Disc", IsNullable = false)] public TracksDisc[] Tracks;

    /// <remarks/>
    public BrowseNodes BrowseNodes;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("ListmaniaList", IsNullable = false)] public
      ListmaniaListsListmaniaList[] ListmaniaLists;

    /// <remarks/>
    public SearchInside SearchInside;

    /// <remarks/>
    public PromotionalTag PromotionalTag;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Image
  {
    /// <remarks/>
    public string URL;

    /// <remarks/>
    public DecimalWithUnits Height;

    /// <remarks/>
    public DecimalWithUnits Width;

    /// <remarks/>
    public string IsVerified;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class DecimalWithUnits
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()] public string Units;

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()] public System.Decimal Value;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemAttributes
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Actor")] public string[] Actor;

    /// <remarks/>
    public Address Address;

    /// <remarks/>
    public DecimalWithUnits AmazonMaximumAge;

    /// <remarks/>
    public DecimalWithUnits AmazonMinimumAge;

    /// <remarks/>
    public string ApertureModes;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Artist")] public string[] Artist;

    /// <remarks/>
    public string AspectRatio;

    /// <remarks/>
    public string AudienceRating;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("AudioFormat")] public string[] AudioFormat;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Author")] public string[] Author;

    /// <remarks/>
    public string BackFinding;

    /// <remarks/>
    public string BandMaterialType;

    /// <remarks/>
    public string BatteriesIncluded;

    /// <remarks/>
    public NonNegativeIntegerWithUnits Batteries;

    /// <remarks/>
    public string BatteryDescription;

    /// <remarks/>
    public string BatteryType;

    /// <remarks/>
    public string BezelMaterialType;

    /// <remarks/>
    public string Binding;

    /// <remarks/>
    public string Brand;

    /// <remarks/>
    public string CalendarType;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("CameraManualFeatures")] public string[] CameraManualFeatures;

    /// <remarks/>
    public DecimalWithUnits CaseDiameter;

    /// <remarks/>
    public string CaseMaterialType;

    /// <remarks/>
    public DecimalWithUnits CaseThickness;

    /// <remarks/>
    public string CaseType;

    /// <remarks/>
    public string CDRWDescription;

    /// <remarks/>
    public string ChainType;

    /// <remarks/>
    public string ClaspType;

    /// <remarks/>
    public string ClothingSize;

    /// <remarks/>
    public string Color;

    /// <remarks/>
    public string Compatibility;

    /// <remarks/>
    public string ComputerHardwareType;

    /// <remarks/>
    public string ComputerPlatform;

    /// <remarks/>
    public string Connectivity;

    /// <remarks/>
    public DecimalWithUnits ContinuousShootingSpeed;

    /// <remarks/>
    public string Country;

    /// <remarks/>
    public string CPUManufacturer;

    /// <remarks/>
    public DecimalWithUnits CPUSpeed;

    /// <remarks/>
    public string CPUType;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Creator")] public ItemAttributesCreator[] Creator;

    /// <remarks/>
    public string Cuisine;

    /// <remarks/>
    public DecimalWithUnits DelayBetweenShots;

    /// <remarks/>
    public string Department;

    /// <remarks/>
    public string DeweyDecimalNumber;

    /// <remarks/>
    public string DialColor;

    /// <remarks/>
    public string DialWindowMaterialType;

    /// <remarks/>
    public DecimalWithUnits DigitalZoom;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Director")] public string[] Director;

    /// <remarks/>
    public DecimalWithUnits DisplaySize;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string DrumSetPieceQuantity;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string DVDLayers;

    /// <remarks/>
    public string DVDRWDescription;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string DVDSides;

    /// <remarks/>
    public string EAN;

    /// <remarks/>
    public string Edition;

    /// <remarks/>
    public string ESRBAgeRating;

    /// <remarks/>
    public string ExternalDisplaySupportDescription;

    /// <remarks/>
    public string FabricType;

    /// <remarks/>
    public string FaxNumber;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Feature")] public string[] Feature;

    /// <remarks/>
    public StringWithUnits FirstIssueLeadTime;

    /// <remarks/>
    public string FloppyDiskDriveDescription;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Format")] public string[] Format;

    /// <remarks/>
    public string GemType;

    /// <remarks/>
    public string GraphicsCardInterface;

    /// <remarks/>
    public string GraphicsDescription;

    /// <remarks/>
    public DecimalWithUnits GraphicsMemorySize;

    /// <remarks/>
    public string GuitarAttribute;

    /// <remarks/>
    public string GuitarBridgeSystem;

    /// <remarks/>
    public string GuitarPickThickness;

    /// <remarks/>
    public string GuitarPickupConfiguration;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string HardDiskCount;

    /// <remarks/>
    public NonNegativeIntegerWithUnits HardDiskSize;

    /// <remarks/>
    public bool HasAutoFocus;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HasAutoFocusSpecified;

    /// <remarks/>
    public bool HasBurstMode;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HasBurstModeSpecified;

    /// <remarks/>
    public bool HasInCameraEditing;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HasInCameraEditingSpecified;

    /// <remarks/>
    public bool HasRedEyeReduction;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HasRedEyeReductionSpecified;

    /// <remarks/>
    public bool HasSelfTimer;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HasSelfTimerSpecified;

    /// <remarks/>
    public bool HasTripodMount;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HasTripodMountSpecified;

    /// <remarks/>
    public bool HasVideoOut;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HasVideoOutSpecified;

    /// <remarks/>
    public bool HasViewfinder;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool HasViewfinderSpecified;

    /// <remarks/>
    public string HoursOfOperation;

    /// <remarks/>
    public string IncludedSoftware;

    /// <remarks/>
    public bool IncludesMp3Player;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool IncludesMp3PlayerSpecified;

    /// <remarks/>
    public string Ingredients;

    /// <remarks/>
    public string InstrumentKey;

    /// <remarks/>
    public bool IsAutographed;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool IsAutographedSpecified;

    /// <remarks/>
    public string ISBN;

    /// <remarks/>
    public bool IsFragile;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool IsFragileSpecified;

    /// <remarks/>
    public bool IsLabCreated;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool IsLabCreatedSpecified;

    /// <remarks/>
    public bool IsMemorabilia;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool IsMemorabiliaSpecified;

    /// <remarks/>
    public NonNegativeIntegerWithUnits ISOEquivalent;

    /// <remarks/>
    public string IssuesPerYear;

    /// <remarks/>
    public ItemAttributesItemDimensions ItemDimensions;

    /// <remarks/>
    public string KeyboardDescription;

    /// <remarks/>
    public string Label;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Language", IsNullable = false)] public ItemAttributesLanguage[]
      Languages;

    /// <remarks/>
    public string LegalDisclaimer;

    /// <remarks/>
    public string LineVoltage;

    /// <remarks/>
    public Price ListPrice;

    /// <remarks/>
    public string MacroFocusRange;

    /// <remarks/>
    public string MagazineType;

    /// <remarks/>
    public string MalletHardness;

    /// <remarks/>
    public string Manufacturer;

    /// <remarks/>
    public string ManufacturerLaborWarrantyDescription;

    /// <remarks/>
    public DecimalWithUnits ManufacturerMaximumAge;

    /// <remarks/>
    public DecimalWithUnits ManufacturerMinimumAge;

    /// <remarks/>
    public string ManufacturerPartsWarrantyDescription;

    /// <remarks/>
    public string MaterialType;

    /// <remarks/>
    public DecimalWithUnits MaximumAperture;

    /// <remarks/>
    public string MaximumColorDepth;

    /// <remarks/>
    public NonNegativeIntegerWithUnits MaximumFocalLength;

    /// <remarks/>
    public NonNegativeIntegerWithUnits MaximumHighResolutionImages;

    /// <remarks/>
    public NonNegativeIntegerWithUnits MaximumHorizontalResolution;

    /// <remarks/>
    public string MaximumLowResolutionImages;

    /// <remarks/>
    public DecimalWithUnits MaximumResolution;

    /// <remarks/>
    public DecimalWithUnits MaximumShutterSpeed;

    /// <remarks/>
    public NonNegativeIntegerWithUnits MaximumVerticalResolution;

    /// <remarks/>
    public DecimalWithUnits MaximumWeightRecommendation;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string MemorySlotsAvailable;

    /// <remarks/>
    public string MetalStamp;

    /// <remarks/>
    public string MetalType;

    /// <remarks/>
    public string MiniMovieDescription;

    /// <remarks/>
    public NonNegativeIntegerWithUnits MinimumFocalLength;

    /// <remarks/>
    public DecimalWithUnits MinimumShutterSpeed;

    /// <remarks/>
    public string Model;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string ModelYear;

    /// <remarks/>
    public string ModemDescription;

    /// <remarks/>
    public DecimalWithUnits MonitorSize;

    /// <remarks/>
    public DecimalWithUnits MonitorViewableDiagonalSize;

    /// <remarks/>
    public string MouseDescription;

    /// <remarks/>
    public string MusicalStyle;

    /// <remarks/>
    public string NativeResolution;

    /// <remarks/>
    public string Neighborhood;

    /// <remarks/>
    public string NetworkInterfaceDescription;

    /// <remarks/>
    public string NotebookDisplayTechnology;

    /// <remarks/>
    public string NotebookPointingDeviceDescription;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfDiscs;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfIssues;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfItems;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfKeys;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfPearls;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfRapidFireShots;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfStones;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfStrings;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string NumberOfTracks;

    /// <remarks/>
    public DecimalWithUnits OpticalZoom;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string OutputWattage;

    /// <remarks/>
    public ItemAttributesPackageDimensions PackageDimensions;

    /// <remarks/>
    public string PearlLustre;

    /// <remarks/>
    public string PearlMinimumColor;

    /// <remarks/>
    public string PearlShape;

    /// <remarks/>
    public string PearlStringingMethod;

    /// <remarks/>
    public string PearlSurfaceBlemishes;

    /// <remarks/>
    public string PearlType;

    /// <remarks/>
    public string PearlUniformity;

    /// <remarks/>
    public string PhoneNumber;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("PhotoFlashType")] public string[] PhotoFlashType;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("PictureFormat")] public string[] PictureFormat;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Platform")] public string[] Platform;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string PriceRating;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string ProcessorCount;

    /// <remarks/>
    public string ProductGroup;

    /// <remarks/>
    public string PromotionalTag;

    /// <remarks/>
    public string PublicationDate;

    /// <remarks/>
    public string Publisher;

    /// <remarks/>
    public string ReadingLevel;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string RecorderTrackCount;

    /// <remarks/>
    public string RegionCode;

    /// <remarks/>
    public string RegionOfOrigin;

    /// <remarks/>
    public string ReleaseDate;

    /// <remarks/>
    public string RemovableMemory;

    /// <remarks/>
    public string ResolutionModes;

    /// <remarks/>
    public string RingSize;

    /// <remarks/>
    public NonNegativeIntegerWithUnits RunningTime;

    /// <remarks/>
    public NonNegativeIntegerWithUnits SecondaryCacheSize;

    /// <remarks/>
    public string SettingType;

    /// <remarks/>
    public string Size;

    /// <remarks/>
    public string SizePerPearl;

    /// <remarks/>
    public string SkillLevel;

    /// <remarks/>
    public string SoundCardDescription;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string SpeakerCount;

    /// <remarks/>
    public string SpeakerDescription;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("SpecialFeatures")] public string[] SpecialFeatures;

    /// <remarks/>
    public string StoneClarity;

    /// <remarks/>
    public string StoneColor;

    /// <remarks/>
    public string StoneCut;

    /// <remarks/>
    public string StoneShape;

    /// <remarks/>
    public DecimalWithUnits StoneWeight;

    /// <remarks/>
    public string Studio;

    /// <remarks/>
    public NonNegativeIntegerWithUnits SubscriptionLength;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("SupportedImageType")] public string[] SupportedImageType;

    /// <remarks/>
    public DecimalWithUnits SystemBusSpeed;

    /// <remarks/>
    public NonNegativeIntegerWithUnits SystemMemorySizeMax;

    /// <remarks/>
    public NonNegativeIntegerWithUnits SystemMemorySize;

    /// <remarks/>
    public string SystemMemoryType;

    /// <remarks/>
    public string TheatricalReleaseDate;

    /// <remarks/>
    public string Title;

    /// <remarks/>
    public DecimalWithUnits TotalDiamondWeight;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalExternalBaysFree;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalFirewirePorts;

    /// <remarks/>
    public DecimalWithUnits TotalGemWeight;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalInternalBaysFree;

    /// <remarks/>
    public DecimalWithUnits TotalMetalWeight;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalNTSCPALPorts;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalParallelPorts;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPCCardSlots;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPCISlotsFree;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalSerialPorts;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalSVideoOutPorts;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalUSB2Ports;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalUSBPorts;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalVGAOutPorts;

    /// <remarks/>
    public string UPC;

    /// <remarks/>
    public string VariationDenomination;

    /// <remarks/>
    public string VariationDescription;

    /// <remarks/>
    public string Warranty;

    /// <remarks/>
    public string WatchMovementType;

    /// <remarks/>
    public DecimalWithUnits WaterResistanceDepth;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string
      WirelessMicrophoneFrequency;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Address
  {
    /// <remarks/>
    public string Name;

    /// <remarks/>
    public string Address1;

    /// <remarks/>
    public string Address2;

    /// <remarks/>
    public string Address3;

    /// <remarks/>
    public string City;

    /// <remarks/>
    public string State;

    /// <remarks/>
    public string PostalCode;

    /// <remarks/>
    public string Country;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class NonNegativeIntegerWithUnits
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()] public string Units;

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute(DataType = "nonNegativeInteger")] public string Value;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemAttributesCreator
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()] public string Role;

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()] public string Value;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class StringWithUnits
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()] public string Units;

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()] public string Value;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemAttributesItemDimensions
  {
    /// <remarks/>
    public DecimalWithUnits Height;

    /// <remarks/>
    public DecimalWithUnits Length;

    /// <remarks/>
    public DecimalWithUnits Weight;

    /// <remarks/>
    public DecimalWithUnits Width;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemAttributesLanguage
  {
    /// <remarks/>
    public string Name;

    /// <remarks/>
    public string Type;

    /// <remarks/>
    public string AudioFormat;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Price
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")] public string Amount;

    /// <remarks/>
    public string CurrencyCode;

    /// <remarks/>
    public string FormattedPrice;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemAttributesPackageDimensions
  {
    /// <remarks/>
    public DecimalWithUnits Height;

    /// <remarks/>
    public DecimalWithUnits Length;

    /// <remarks/>
    public DecimalWithUnits Weight;

    /// <remarks/>
    public DecimalWithUnits Width;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class OfferSummary
  {
    /// <remarks/>
    public Price LowestNewPrice;

    /// <remarks/>
    public Price LowestUsedPrice;

    /// <remarks/>
    public Price LowestCollectiblePrice;

    /// <remarks/>
    public Price LowestRefurbishedPrice;

    /// <remarks/>
    public string TotalNew;

    /// <remarks/>
    public string TotalUsed;

    /// <remarks/>
    public string TotalCollectible;

    /// <remarks/>
    public string TotalRefurbished;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Offers
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalOffers;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalOfferPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Offer")] public Offer[] Offer;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Offer
  {
    /// <remarks/>
    public Merchant Merchant;

    /// <remarks/>
    public Seller Seller;

    /// <remarks/>
    public Image SmallImage;

    /// <remarks/>
    public Image MediumImage;

    /// <remarks/>
    public Image LargeImage;

    /// <remarks/>
    public OfferAttributes OfferAttributes;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("OfferListing")] public OfferListing[] OfferListing;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Merchant
  {
    /// <remarks/>
    public string MerchantId;

    /// <remarks/>
    public string Name;

    /// <remarks/>
    public string GlancePage;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Seller
  {
    /// <remarks/>
    public string SellerId;

    /// <remarks/>
    public string SellerName;

    /// <remarks/>
    public string Nickname;

    /// <remarks/>
    public string GlancePage;

    /// <remarks/>
    public string About;

    /// <remarks/>
    public string MoreAbout;

    /// <remarks/>
    public SellerLocation Location;

    /// <remarks/>
    public System.Decimal AverageFeedbackRating;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool AverageFeedbackRatingSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalFeedback;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalFeedbackPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Feedback", IsNullable = false)] public SellerFeedbackFeedback[]
      SellerFeedback;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerLocation
  {
    /// <remarks/>
    public string City;

    /// <remarks/>
    public string State;

    /// <remarks/>
    public string Country;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerFeedbackFeedback
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string Rating;

    /// <remarks/>
    public string Comment;

    /// <remarks/>
    public string Date;

    /// <remarks/>
    public string RatedBy;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class OfferAttributes
  {
    /// <remarks/>
    public string Condition;

    /// <remarks/>
    public string SubCondition;

    /// <remarks/>
    public string ConditionNote;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class OfferListing
  {
    /// <remarks/>
    public string OfferListingId;

    /// <remarks/>
    public string ExchangeId;

    /// <remarks/>
    public Price Price;

    /// <remarks/>
    public Price SalePrice;

    /// <remarks/>
    public string Availability;

    /// <remarks/>
    public Address ISPUStoreAddress;

    /// <remarks/>
    public string ISPUStoreHours;

    /// <remarks/>
    public bool IsEligibleForSuperSaverShipping;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool IsEligibleForSuperSaverShippingSpecified;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class VariationSummary
  {
    /// <remarks/>
    public Price LowestPrice;

    /// <remarks/>
    public Price HighestPrice;

    /// <remarks/>
    public Price LowestSalePrice;

    /// <remarks/>
    public Price HighestSalePrice;

    /// <remarks/>
    public string SingleMerchantId;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Variations
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalVariations;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalVariationPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Item")] public Item[] Item;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CustomerReviews
  {
    /// <remarks/>
    public System.Decimal AverageRating;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool AverageRatingSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalReviews;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalReviewPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Review")] public Review[] Review;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Review
  {
    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string Rating;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string HelpfulVotes;

    /// <remarks/>
    public string CustomerId;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalVotes;

    /// <remarks/>
    public string Date;

    /// <remarks/>
    public string Summary;

    /// <remarks/>
    public string Content;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class EditorialReview
  {
    /// <remarks/>
    public string Source;

    /// <remarks/>
    public string Content;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SimilarProductsSimilarProduct
  {
    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    public string Title;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class AccessoriesAccessory
  {
    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    public string Title;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TracksDisc
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Track")] public TracksDiscTrack[] Track;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")] public string Number;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TracksDiscTrack
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "positiveInteger")] public string Number;

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()] public string Value;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class BrowseNodes
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("BrowseNode")] public BrowseNode[] BrowseNode;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class BrowseNode
  {
    /// <remarks/>
    public string BrowseNodeId;

    /// <remarks/>
    public string Name;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)] public BrowseNode[] Children;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)] public BrowseNode[] Ancestors;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ListmaniaListsListmaniaList
  {
    /// <remarks/>
    public string ListId;

    /// <remarks/>
    public string ListName;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SearchInside
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalExcerpts;

    /// <remarks/>
    public SearchInsideExcerpt Excerpt;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SearchInsideExcerpt
  {
    /// <remarks/>
    public string Checksum;

    /// <remarks/>
    public string PageType;

    /// <remarks/>
    public string PageNumber;

    /// <remarks/>
    public string SequenceNumber;

    /// <remarks/>
    public string Text;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class PromotionalTag
  {
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("PromotionalTag")] public string PromotionalTag1;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemLookupResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Items")] public Items[] Items;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ListSearchResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Lists")] public Lists[] Lists;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Lists
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalResults;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("List")] public List[] List;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class List
  {
    /// <remarks/>
    public string ListId;

    /// <remarks/>
    public string ListURL;

    /// <remarks/>
    public string RegistryNumber;

    /// <remarks/>
    public string ListName;

    /// <remarks/>
    public ListListType ListType;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalItems;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPages;

    /// <remarks/>
    public string DateCreated;

    /// <remarks/>
    public string OccasionDate;

    /// <remarks/>
    public string CustomerName;

    /// <remarks/>
    public string PartnerName;

    /// <remarks/>
    public string AdditionalName;

    /// <remarks/>
    public string Comment;

    /// <remarks/>
    public Image Image;

    /// <remarks/>
    public System.Decimal AverageRating;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()] public bool AverageRatingSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalVotes;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalTimesRead;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("ListItem")] public ListItem[] ListItem;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public enum ListListType
  {
    /// <remarks/>
    WishList,

    /// <remarks/>
    WeddingRegistry,

    /// <remarks/>
    BabyRegistry,

    /// <remarks/>
    Listmania,
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ListItem
  {
    /// <remarks/>
    public string ListItemId;

    /// <remarks/>
    public string DateAdded;

    /// <remarks/>
    public string Comment;

    /// <remarks/>
    public string QuantityDesired;

    /// <remarks/>
    public string QuantityReceived;

    /// <remarks/>
    public Item Item;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ListLookupResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Lists")] public Lists[] Lists;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CustomerContentSearchResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Customers")] public Customers[] Customers;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Customers
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalResults;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Customer")] public Customer[] Customer;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Customer
  {
    /// <remarks/>
    public string CustomerId;

    /// <remarks/>
    public string Nickname;

    /// <remarks/>
    public string Birthday;

    /// <remarks/>
    public string WishListId;

    /// <remarks/>
    public CustomerLocation Location;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("CustomerReviews")] public CustomerReviews[] CustomerReviews;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CustomerLocation
  {
    /// <remarks/>
    public string City;

    /// <remarks/>
    public string State;

    /// <remarks/>
    public string Country;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CustomerContentLookupResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Customers")] public Customers[] Customers;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SimilarityLookupResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Items")] public Items[] Items;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerLookupResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Sellers")] public Sellers[] Sellers;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Sellers
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalResults;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Seller")] public Seller[] Seller;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartGetResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Cart")] public Cart[] Cart;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Cart
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    public string CartId;

    /// <remarks/>
    public string HMAC;

    /// <remarks/>
    public string URLEncodedHMAC;

    /// <remarks/>
    public string PurchaseURL;

    /// <remarks/>
    public Price SubTotal;

    /// <remarks/>
    public CartItems CartItems;

    /// <remarks/>
    public SavedForLaterItems SavedForLaterItems;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("SimilarProduct", IsNullable = false)] public
      SimilarProductsSimilarProduct[] SimilarProducts;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartItems
  {
    /// <remarks/>
    public Price SubTotal;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("CartItem")] public CartItem[] CartItem;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartItem
  {
    /// <remarks/>
    public string CartItemId;

    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    public string ExchangeId;

    /// <remarks/>
    public string MerchantId;

    /// <remarks/>
    public string SellerId;

    /// <remarks/>
    public string SellerNickname;

    /// <remarks/>
    public string Quantity;

    /// <remarks/>
    public string Title;

    /// <remarks/>
    public string ProductGroup;

    /// <remarks/>
    public string ListOwner;

    /// <remarks/>
    public string ListType;

    /// <remarks/>
    public Price Price;

    /// <remarks/>
    public Price ItemTotal;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SavedForLaterItems
  {
    /// <remarks/>
    public Price SubTotal;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("SavedForLaterItem")] public CartItem[] SavedForLaterItem;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartAddResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Cart")] public Cart[] Cart;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartCreateResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Cart")] public Cart[] Cart;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartModifyResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Cart")] public Cart[] Cart;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartClearResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Cart")] public Cart[] Cart;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TransactionLookupResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Transactions")] public Transactions[] Transactions;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Transactions
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalResults;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Transaction")] public Transaction[] Transaction;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class Transaction
  {
    /// <remarks/>
    public string TransactionId;

    /// <remarks/>
    public string SellerId;

    /// <remarks/>
    public string Condition;

    /// <remarks/>
    public string TransactionDate;

    /// <remarks/>
    public string TransactionDateEpoch;

    /// <remarks/>
    public string SellerName;

    /// <remarks/>
    public string PayingCustomerId;

    /// <remarks/>
    public string OrderingCustomerId;

    /// <remarks/>
    public TransactionTotals Totals;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)] public TransactionItem[] TransactionItems;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Shipment", IsNullable = false)] public TransactionShipment[]
      Shipments;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TransactionTotals
  {
    /// <remarks/>
    public Price Total;

    /// <remarks/>
    public Price Subtotal;

    /// <remarks/>
    public Price Tax;

    /// <remarks/>
    public Price ShippingCharge;

    /// <remarks/>
    public Price Promotion;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TransactionItem
  {
    /// <remarks/>
    public string TransactionItemId;

    /// <remarks/>
    public string Quantity;

    /// <remarks/>
    public Price UnitPrice;

    /// <remarks/>
    public Price TotalPrice;

    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute(IsNullable = false)] public TransactionItem[] ChildTransactionItems;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TransactionShipment
  {
    /// <remarks/>
    public string Condition;

    /// <remarks/>
    public string DeliveryMethod;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("TransactionItemId", IsNullable = false)] public object[]
      ShipmentItems;

    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Package", IsNullable = false)] public TransactionShipmentPackage[]
      Packages;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TransactionShipmentPackage
  {
    /// <remarks/>
    public string TrackingNumber;

    /// <remarks/>
    public string CarrierName;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerListingSearchResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("SellerListings")] public SellerListings[] SellerListings;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerListings
  {
    /// <remarks/>
    public Request Request;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalResults;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute(DataType = "nonNegativeInteger")] public string TotalPages;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("SellerListing")] public SellerListing[] SellerListing;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerListing
  {
    /// <remarks/>
    public string ExchangeId;

    /// <remarks/>
    public string ListingId;

    /// <remarks/>
    public string ASIN;

    /// <remarks/>
    public string Title;

    /// <remarks/>
    public Price Price;

    /// <remarks/>
    public string StartDate;

    /// <remarks/>
    public string EndDate;

    /// <remarks/>
    public string Status;

    /// <remarks/>
    public string Quantity;

    /// <remarks/>
    public string QuantityAllocated;

    /// <remarks/>
    public string Condition;

    /// <remarks/>
    public string SubCondition;

    /// <remarks/>
    public string ConditionNote;

    /// <remarks/>
    public string Availability;

    /// <remarks/>
    public string FeaturedCategory;

    /// <remarks/>
    public Seller Seller;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerListingLookupResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("SellerListings")] public SellerListings[] SellerListings;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class BrowseNodeLookupResponse
  {
    /// <remarks/>
    public OperationRequest OperationRequest;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("BrowseNodes")] public BrowseNodes[] BrowseNodes;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class MultiOperation
  {
    /// <remarks/>
    public Help Help;

    /// <remarks/>
    public ItemSearch ItemSearch;

    /// <remarks/>
    public ItemLookup ItemLookup;

    /// <remarks/>
    public ListSearch ListSearch;

    /// <remarks/>
    public ListLookup ListLookup;

    /// <remarks/>
    public CustomerContentSearch CustomerContentSearch;

    /// <remarks/>
    public CustomerContentLookup CustomerContentLookup;

    /// <remarks/>
    public SimilarityLookup SimilarityLookup;

    /// <remarks/>
    public SellerLookup SellerLookup;

    /// <remarks/>
    public CartGet CartGet;

    /// <remarks/>
    public CartAdd CartAdd;

    /// <remarks/>
    public CartCreate CartCreate;

    /// <remarks/>
    public CartModify CartModify;

    /// <remarks/>
    public CartClear CartClear;

    /// <remarks/>
    public TransactionLookup TransactionLookup;

    /// <remarks/>
    public SellerListingSearch SellerListingSearch;

    /// <remarks/>
    public SellerListingLookup SellerListingLookup;

    /// <remarks/>
    public BrowseNodeLookup BrowseNodeLookup;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemSearch
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public ItemSearchRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public ItemSearchRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ItemLookup
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public ItemLookupRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public ItemLookupRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ListSearch
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public ListSearchRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public ListSearchRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class ListLookup
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public ListLookupRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public ListLookupRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CustomerContentSearch
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public CustomerContentSearchRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public CustomerContentSearchRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CustomerContentLookup
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public CustomerContentLookupRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public CustomerContentLookupRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SimilarityLookup
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public SimilarityLookupRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public SimilarityLookupRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerLookup
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public SellerLookupRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public SellerLookupRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartGet
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public CartGetRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public CartGetRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartAdd
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public CartAddRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public CartAddRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartCreate
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public CartCreateRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public CartCreateRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartModify
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public CartModifyRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public CartModifyRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class CartClear
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public CartClearRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public CartClearRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class TransactionLookup
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public TransactionLookupRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public TransactionLookupRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerListingSearch
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public SellerListingSearchRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public SellerListingSearchRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class SellerListingLookup
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public SellerListingLookupRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public SellerListingLookupRequest[] Request;
  }

  /// <remarks/>
  [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://webservices.amazon.com/AWSECommerceService/2005-03-23")
  ]
  public class BrowseNodeLookup
  {
    /// <remarks/>
    public string SubscriptionId;

    /// <remarks/>
    public string AssociateTag;

    /// <remarks/>
    public string Validate;

    /// <remarks/>
    public string XMLEscaping;

    /// <remarks/>
    public BrowseNodeLookupRequest Shared;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Request")] public BrowseNodeLookupRequest[] Request;
  }
}