using System;
using System.Collections;

namespace MediaLibrary
{
    #region public interface IMLViewStep
    /// <summary>
    /// The IMLViewStep object lets you manage view steps
    /// </summary>
    public interface IMLViewStep
    {
        #region int ViewID
        /// <summary>
        /// Gets the ViewID of the associated view
        /// </summary>
        /// <value></value>
        int ViewID
        {
            get;
        }
        #endregion

        #region int ViewStepID
        /// <summary>
        /// Gets the ID current step
        /// </summary>
        /// <value></value>
        int ViewStepID
        {
            get;
        }
        #endregion

        #region string GroupFunction
        /// <summary>
        /// Returns/sets the Group Function for this view step
        /// </summary>
        /// <value></value>
        string GroupFunction
        {
            get;
            set;
        }
        #endregion

        #region string GroupTag
        /// <summary>
        /// Returns/sets the Group Tag for this view step
        /// </summary>
        /// <value></value>
        string GroupTag
        {
            get;
            set;
        }
        #endregion

        #region string Mode
        /// <summary>
        /// Returns/sets the Mode for this view step
        /// </summary>
        /// <value></value>
        string Mode
        {
            get;
            set;
        }
        #endregion

        #region bool SortAscending
        /// <summary>
        /// Returns/sets if this view step will be sorted in ascending order
        /// </summary>
        /// <value></value>
        bool SortAscending
        {
            get;
            set;
        }
        #endregion

        #region string SortTag
        /// <summary>
        /// Returns/sets if the Sort Tag for this view step
        /// </summary>
        /// <value></value>
        string SortTag
        {
            get;
            set;
        }
        #endregion

        #region string SortType
        /// <summary>
        /// Returns/sets the Sort Type for this view step
        /// </summary>
        /// <value></value>
        string SortType
        {
            get;
            set;
        }
        #endregion

    }
    #endregion
}
