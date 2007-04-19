using System;
using System.Collections;
using System.Data;

namespace MediaLibrary
{
    #region public interface IMLSection
    /// <summary>
    /// The IMLSection object provides access to a Media Library section
    /// </summary>
    public interface IMLSection
    {
        #region IMLSection Properties

        #region string FileName
        /// <summary>
        /// Returns the filename where the section is stored
        /// </summary>
        /// <value></value>
        string FileName
        {
            get;
        }
        #endregion

        #region int ItemCount
        /// <summary>
        /// Returns the number of items present in the section
        /// </summary>
        /// <value></value>
        int ItemCount
        {
            get;
        }
        #endregion

        #region string Name
        /// <summary>
        /// Returns the section's name
        /// </summary>
        /// <value></value>
        string Name
        {
            get;
        }
        #endregion

        #region int ViewCount
        /// <summary>
        /// Returns the number of views defined for this section
        /// </summary>
        /// <value></value>
        int ViewCount
        {
            get;
        }
        #endregion

        #endregion

        #region IMLSection Methods

        #region Old Media Methods

        #region IMLItem AddNewItem(string ItemName, string ItemSource)
        /// <summary>
        /// Adds an item to the database, returning an IMLItem that references it
        /// </summary>
        /// <param name="ItemName"></param>
        /// <param name="ItemSource"></param>
        /// <returns></returns>
        IMLItem AddNewItem(string ItemName, string ItemSource);
        #endregion

        #region void DeleteItem(IMLItem Item)
        /// <summary>
        /// Deletes the given IMLItem from the secti
        /// </summary>
        /// <param name="Item"></param>
        void DeleteItem(IMLItem Item);
        #endregion

        #region void DeleteAllItems()
        /// <summary>
        /// Empties the section by deleting all items
        /// </summary>
        void DeleteAllItems();
        #endregion

        #region IMLItem Items(int Index)
        /// <summary>
        /// Returns the item specified by Index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        IMLItem Items(int Index);
        #endregion

        #region IMLItemList GetReadOnlyItems()
        /// <summary>
        /// Returns a read-only IMLItemList containing all 
        /// the items of the section. Use this function when 
        /// you do not plan to change item values, use this 
        /// function as this method is faster and more efficient 
        /// than using the Items collection.
        /// </summary>
        /// <returns></returns>
        IMLItemList GetReadOnlyItems();
        #endregion

        #region IMLViewNavigator GetViewNavigator()
        /// <summary>
        /// Returns an IMLViewNavigator object that lets you trasverse 
        /// the database using a specific view
        /// </summary>
        /// <returns></returns>
        IMLViewNavigator GetViewNavigator();
        #endregion

        #region bool AddNewTag(string TagName)
        /// <summary>
        /// Adds a new tag to the section, returning TRUE if the tag could be succesfully created
        /// </summary>
        /// <param name="TagName"></param>
        /// <returns></returns>
        bool AddNewTag(string TagName);
        #endregion

        #region bool DeleteTag(string TagName)
        /// <summary>
        /// Deletes the tag TagName from the section. Returns TRUE is the operation was succesful.
        /// </summary>
        /// <param name="TagName"></param>
        /// <returns></returns>
        bool DeleteTag(string TagName);
        #endregion

        #region bool RenameTag(string OldTagName, string NewTagName)
        /// <summary>
        /// Renames a tag from OldTagName to NewTagName. Returns TRUE if the operation was succesful.
        /// </summary>
        /// <param name="OldTagName"></param>
        /// <param name="NewTagName"></param>
        /// <returns></returns>
        bool RenameTag(string OldTagName, string NewTagName);
        #endregion

        #region string[] GetTagNames()
        /// <summary>
        /// Returns a string array containing the names of all tags present in the section
        /// </summary>
        /// <returns></returns>
        string[] GetTagNames();
        #endregion

        #region string[] GetTagValues(string TagName)
        /// <summary>
        /// Returns a string array containing the values for the tag specified by TagName
        /// </summary>
        /// <param name="TagName"></param>
        /// <returns></returns>
        string[] GetTagValues(string TagName);
        #endregion

        #region IMLView AddNewView(string ViewName)
        /// <summary>
        /// Adds a view to the database, returning an IMLView that references it
        /// </summary>
        /// <param name="ViewName"></param>
        /// <returns></returns>
        IMLView AddNewView(string ViewName);
        #endregion

        #region void DeleteView(IMLView View)
        /// <summary>
        /// Deletes the given IMLView from the section
        /// </summary>
        /// <param name="View"></param>
        void DeleteView(IMLView View);
        #endregion

        #region void DeleteAllViews()
        /// <summary>
        /// Deletes all views associated with this section
        /// </summary>
        void DeleteAllViews();
        #endregion

        #region void RefreshViews()
        /// <summary>
        /// Drops the views that the section may have cached so that next 
        /// time you read them, they will be reloaded
        /// </summary>
        void RefreshViews();
        #endregion

        #region IMLView Views(int Index)
        /// <summary>
        /// Returns the view specified by Index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        IMLView Views(int Index);
        #endregion

        #region bool BeginUpdate()
        /// <summary>
        /// Marks the begin of a database transaction. All the updates will be 
        /// queued until either CancelUpdate or EndUpdate is called.
        /// </summary>
        /// <returns></returns>
        bool BeginUpdate();
        #endregion

        #region void CancelUpdate()
        /// <summary>
        /// Cancels the pending update. The database will be reverted to the state 
        /// it was before the call to the BeginUpdate method.
        /// </summary>
        void CancelUpdate();
        #endregion

        #region void EndUpdate()
        /// <summary>
        /// Ends the current transaction and commits the changes to the database
        /// </summary>
        void EndUpdate();
        #endregion

        #region IMLItem FindItemByExternalID(string ExternalID)
        /// <summary>
        /// Search for an item by External ID. This function will return Null if no item is found.
        /// </summary>
        /// <param name="ExternalID"></param>
        /// <returns></returns>
        IMLItem FindItemByExternalID(string ExternalID);
        #endregion

        #region IMLItem FindItemByID(int ItemID)
        /// <summary>
        /// Search for an item by its ID value. This function will return Null if no item is found.
        /// </summary>
        /// <param name="ItemID"></param>
        /// <returns></returns>
        IMLItem FindItemByID(int ItemID);
        #endregion

        #region IMLItem FindItemByLocation(string Location)
        /// <summary>
        /// Search for an item by Location. This function will return Null if no item is found.
        /// </summary>
        /// <param name="Location"></param>
        /// <returns></returns>
        IMLItem FindItemByLocation(string Location);
        #endregion

        #region int[] GetAllItemIDs()
        /// <summary>
        /// Returns an array containing all the ID values for the section
        /// </summary>
        /// <returns></returns>
        int[] GetAllItemIDs();
        #endregion

        #region IMLItemList SearchAll(string SearchString)
        /// <summary>
        /// Searchs for SearchString in all fields of the section, returning 
        /// an IMLItemList object containing a collection of all items found 
        /// or Null if none was located.
        /// </summary>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        IMLItemList SearchAll(string SearchString);
        #endregion

        #region IMLItemList SearchByImage(string SearchString)
        /// <summary>
        /// Searchs for SearchString in the Image field, returning an 
        /// IMLItemList object containing a collection of all items found or 
        /// Null if none was located.
        /// </summary>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        IMLItemList SearchByImage(string SearchString);
        #endregion

        #region IMLItemList SearchByLocation(string SearchString)
        /// <summary>
        /// Searchs for SearchString in the Location field, returning an 
        /// IMLItemList object containing a collection of all items found or 
        /// Null if none was located.
        /// </summary>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        IMLItemList SearchByLocation(string SearchString);
        #endregion

        #region IMLItemList SearchByTag(string TagName, string SearchString)
        /// <summary>
        /// Searchs for SearchString in the tag TagName, returning an IMLItemList 
        /// object containing a collection of all items found or Null if none was located.
        /// </summary>
        /// <param name="TagName"></param>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        IMLItemList SearchByTag(string TagName, string SearchString);
        #endregion

        #region IMLItemList SearchByTitle(string SearchString)
        /// <summary>
        /// Searchs for SearchString in the Name field, returning an IMLItemList object 
        /// containing a collection of all items found or Null if none was located.
        /// </summary>
        /// <param name="SearchString"></param>
        /// <returns></returns>
        IMLItemList SearchByTitle(string SearchString);
        #endregion

        #endregion

        #region New Methods

        #region void RefreshItems()
        /// <summary>
        /// 
        /// </summary>
        void RefreshItems();
        #endregion

        IMLItemList CustomSearch(string Filter, string GroupBy, string GroupFunc, string OrderBy, string OrderType, bool Asc);


        IMLDataSet GetDataSet();

        #endregion

        #endregion
    }
    #endregion
}
