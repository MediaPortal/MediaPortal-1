using System.Collections;

namespace MediaLibrary
{    
    /// <summary>
    /// Item lists allows you to get several items of data in one call. They are used to get information from the Media Library, to pass several items in one message and everywhere you need to have a collection of items.
    /// </summary>
	public interface IMLHashItemList : IEnumerable
	{
        /// <summary>
        /// Provides access to the items collection by returning the IMLHashItem corresponding to the specified Index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
		IMLHashItem this[int Index]
		{
			get;
		}

        /// <summary>
        /// Adds a reference to the specified IMLHashItem to the Item List.
        /// </summary>
        /// <param name="Item"></param>
		void Add(IMLHashItem Item );

        /// <summary>
        /// Adds all the IMLHashItem objects from another IMLHashItemList. Each item from the source list is copied and the list is not cleared beforehand.
        /// </summary>
        /// <param name="SourceList"></param>
		void AddFromList(IMLHashItemList SourceList);

        /// <summary>
        /// Creates a new IMLHashItem and adds it to the Item List.
        /// </summary>
        /// <returns></returns>
		IMLHashItem AddNew();

        /// <summary>
        /// Clears the list
        /// </summary>
		void Clear();

        /// <summary>
        /// Returns the number of items this list has
        /// </summary>
		int Count
		{
			get;
		}

        /// <summary>
        /// Creates a deep copy of the current IMLHashItemList
        /// </summary>
        /// <returns></returns>
		IMLHashItemList CreateCopy();

        /// <summary>
        /// Swaps the position of the two IMLHashItems
        /// </summary>
        /// <param name="OldIndex"></param>
        /// <param name="NewIndex"></param>
		void Exchange( int OldIndex, int NewIndex );

        /// <summary>
        /// Returns the IMLHashItem associated with the specified Key/Value pair or NULL if a matching one is not found.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        IMLHashItem FindItem(string Key, object Value);

        /// <summary>
        /// Returns the index number corresponding to the IMLHashItem associated with the specified Key/Value pair. If none is found, the return value will be -1.
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        int FindItemIndex(string Key, object Value);

        /// <summary>
        /// Removes from the list the IMLHashItem corresponding to the specified Index
        /// </summary>
        /// <param name="Index"></param>
        void Remove(int Index);

        #region New members

        /// <summary>
        /// Returns the index number corresponding to the IMLHashItem. If Item is not part of the IMLHashItemList, the return value will be -1.
        /// </summary>
        /// <param name="Item"></param>
        /// <returns></returns>
        /// 
        int FindItemIndex(IMLHashItem Item);

        /// <summary>
        /// Removes  the IMLHashItem from the list
        /// </summary>
        /// <param name="Item"></param>
        void Remove(IMLHashItem Item);

        /// <summary>
        /// Sorts the items of the list by comparing the properties SortKey of each IMLHashItem.
        /// </summary>
        /// <param name="SortKey"></param>
        /// <param name="SortAscending"></param>
        void Sort(string SortKey, bool SortAscending);

        #endregion
    }


	
}