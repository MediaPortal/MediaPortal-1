using System;
using System.Collections;

namespace MediaLibrary
{
    #region public interface IMLItemList
    /// <summary>
    /// The IMLItemList contains a collection of IMLItem objects representing 
    /// records of a Media Library section.
    /// </summary>
    public interface IMLItemList
    {
        #region IMLItemList Properties

        #region IMLItem this[int Index]
        /// <summary>
        /// Returns the item specified by Index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        IMLItem this[int Index]
        {
            get;
        }
        #endregion

        #endregion

        #region IMLItemList Methods

        #region int Count
        /// <summary>
        /// Returns the number of items in this collection
        /// </summary>
        /// <value></value>
        int Count
        {
            get;
        }
        #endregion

        #region bool IsReadOnly
        /// <summary>
        /// Returns TRUE if this IMLItemList is read only
        /// </summary>
        /// <value></value>
        bool IsReadOnly
        {
            get;
        }
        #endregion

        #endregion
    }
    #endregion
}
