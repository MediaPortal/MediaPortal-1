namespace MediaLibrary
{
    /// <summary>
    /// The IMLHashItem is a generic object that can contain any number of values and it's used by Media as a means to pass or return data between objects.
    /// </summary>
    public interface IMLHashItem
    {
        #region the old IMLHashItem member

        /// <summary>
        /// Gets or sets the value of the IMLHashItemEntry associated with Key. If the IMLHashItemEntry does not exist, it's created.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object this[string key]
        {
            get;
            set;
        }

        /// <summary>
        /// Clears the data contained
        /// </summary>
        void Clear();

        /// <summary>
        /// Checks if the IMLHashItem object contains the specified Key
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        bool Contains(string Key);

        /// <summary>
        /// Copies all the data from the specified Source IMLHashItem
        /// </summary>
        /// <param name="Source"></param>
        void CopyFrom(IMLHashItem Source);

        /// <summary>
        /// Return the number of data items contained in the IMLHashItem object
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Returns the value of the IMLHashItemEntry associated with Key. If the IMLHashItemEntry does not exist, Get returns the Default object.
        /// </summary>
        /// <param name="Key"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        object Get(string Key, object Default);

        #endregion

        #region new methods

        /// <summary>
        /// Array of keys that have associated Values with them.
        /// Returns an ICollection containing the keys in the IMLHashItem.
        /// The order of the keys in the ICollection is unspecified, 
        /// but it is the same order as the associated values in the 
        /// ICollection returned by the Values method.
        /// </summary>
        /// <returns>An ICollection containing the keys in the IMLHashItem. </returns>
        System.Collections.ICollection Keys
        {
            get;
        }

        /// <summary>
        /// Finds the item with a numerical index in the (array)list and returns its value. 
        /// This method is only valid as long as MLHashItem uses an ArrayList or a List<>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        object this[int index]
        {
            get;
        }

        /// <summary>
        /// Gets an ICollection containing the values in the Hashtable.
        /// The order of the values in the ICollection is unspecified, 
        /// but it is the same order as the associated values in the 
        /// ICollection returned by the Keys method.
        /// </summary>
        /// <returns>An ICollection containing the values in the Hashtable. </returns>
        System.Collections.ICollection Values
        {
            get;
        }

        /// <summary>
        /// Copies all the data from IMLHashItem Source points to
        /// </summary>
        /// <param name="Source"></param>        
        void CopyFrom(System.IntPtr Source);

        #endregion
    }
}