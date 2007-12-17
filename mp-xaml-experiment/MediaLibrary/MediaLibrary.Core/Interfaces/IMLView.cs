
using System;
using System.Collections;

namespace MediaLibrary
{
    #region public interface IMLView
    /// <summary>
    /// The IMLView object lets you manage Media Library views
    /// </summary>
    public interface IMLView : IComparable
    {
        #region IMLView Properties

        #region Old Media Properties

        #region string Filter
        /// <summary>
        /// Get/Sets the Filter of the IMLView
        /// </summary>
        /// <value></value>
        string Filter
        {
            get;
            set;
        }
        #endregion

        #region string Name
        /// <summary>
        /// Get/Sets the Name of the view
        /// </summary>
        /// <value></value>
        string Name
        {
            get;
            set;
        }
        #endregion

        #region int Count
        /// <summary>
        /// Returns the number of steps in the current view
        /// </summary>
        /// <value></value>
        int Count
        {
            get;
        }
        #endregion

        #region int ID
        /// <summary>
        /// Returns the view's ID
        /// </summary>
        /// <value></value>
        int ID
        {
            get;
        }
        #endregion

        #endregion

        #region New Properties

        #region string Custom1
        /// <summary>
        /// Get/Sets the Custom1 value for the view
        /// </summary>
        /// <value></value>
        string Custom1
        {
            get;
            set;
        }
        #endregion

        #region string Custom2
        /// <summary>
        /// Get/Sets the Custom2 value for the view
        /// </summary>
        /// <value></value>
        string Custom2
        {
            get;
            set;
        }
        #endregion

        #endregion

        #endregion

        #region IMLView Methods

        #region Old Media Methods

        #region IMLViewStep Steps(int Index)
        /// <summary>
        /// Returns the view step specified by Index
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        IMLViewStep Steps(int Index);
        #endregion

        #region IMLViewStep AddNewStep(string GroupTag)
        /// <summary>
        /// Adds a new step, based in the Group Tag GroupTag, to the current view
        /// </summary>
        /// <param name="GroupTag"></param>
        /// <returns></returns>
        IMLViewStep AddNewStep(string GroupTag);
        #endregion

        bool DeleteStep(IMLViewStep Step);

        #region void DeleteAllSteps()
        /// <summary>
        /// Deletes all steps from the current view
        /// </summary>
        /// <returns></returns>
        void DeleteAllSteps();
        #endregion

        #endregion

        #region New Methods

        #region bool MoveDown()
        /// <summary>
        /// Moves view down 1 index position.  Returns TRUE if successful
        /// </summary>
        /// <returns></returns>
        bool MoveDown();
        #endregion

        #region bool MoveUp()
        /// <summary>
        /// Moves view up 1 index position.  Returns TRUE if successful
        /// </summary>
        /// <returns></returns>
        bool MoveUp();
        #endregion

        #region new int CompareTo(object obj)
        /// <summary>
        /// Compares the current instance with another object of the same type.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with this instance.
        /// </param>
        /// <returns>A 32-bit signed integer that indicates the relative order 
        /// of the objects being compared. The return value has these meanings: 
        /// <table>
        /// 		<tr><th>Value</th><th>Meaning</th></tr>
        /// 		<tr><td>Less than zero</td><td>This instance is less than <i>obj</i>.</td></tr>
        /// 		<tr><td>Zero</td><td>This instance is equal to <i>obj</i>.</td></tr>
        /// 		<tr><td>Greater than zero</td><td>This instance is greater than <i>obj</i>.</td></tr>
        /// 	</table>
        /// </returns>
        new int CompareTo(object obj);
        #endregion

        #endregion

        #endregion
    }
    #endregion
}
