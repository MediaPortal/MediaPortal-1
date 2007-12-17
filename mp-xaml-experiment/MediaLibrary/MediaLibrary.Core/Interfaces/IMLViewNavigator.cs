using System;
using System.IO;
using System.Net;
using System.Data;
using System.Text;
using System.Threading;
using System.Collections;

namespace MediaLibrary
{
    #region public interface IMLViewNavigator
    /// <summary>
    /// The IMLViewNavigator object lets you navigate through your items in a hierarchical manner
    /// </summary>
    public interface IMLViewNavigator
    {
        #region IMLViewNavigator Properties

        #region Old Media Properties

        #region bool AtBottom
        /// <summary>
        /// If TRUE the View Navigator is at the bottom of the hierarchy, that is, at the 
        /// last step defined for the view.
        /// </summary>
        /// <value></value>
        bool AtBottom
        {
            get;
        }
        #endregion

        #region bool AtTop
        /// <summary>
        /// If TRUE the View Navigator is at the top of the hierarchy, that is, at the first 
        /// step defined for the view.
        /// </summary>
        /// <value></value>
        bool AtTop
        {
            get;
        }
        #endregion

        #region bool AtViews
        /// <summary>
        /// Returns TRUE if the view navigator is currently showing the list of views.
        /// </summary>
        /// <value></value>
        bool AtViews
        {
            get;
        }
        #endregion

        #region string BlankChoiceText
        /// <summary>
        /// This is a string to use when there are blank values in the view.
        /// </summary>
        /// <value></value>
        string BlankChoiceText
        {
            get;
            set;
        }
        #endregion

        #region bool CanGoBack
        /// <summary>
        /// Returns TRUE if the navigator can go back one step.
        /// </summary>
        /// <value></value>
        bool CanGoBack
        {
            get;
        }
        #endregion

        #region int Count
        /// <summary>
        /// The number of current choices.
        /// </summary>
        /// <value></value>
        int Count
        {
            get;
        }
        #endregion

        #region string CriteriaPath
        /// <summary>
        /// This is mostly for debugging purposes, as it shows the current "path" and the 
        /// criteria used so far. You can see the value of this property in the views tester 
        /// in the configuration application.
        /// </summary>
        /// <value></value>
        string CriteriaPath
        {
            get;
        }
        #endregion

        #region string CurrentMode
        /// <summary>
        /// Returns the current mode as defined in the view steps or the default mode if there 
        /// is none.
        /// </summary>
        /// <value></value>
        string CurrentMode
        {
            get;
        }
        #endregion

        #region string CurrentTag
        /// <summary>
        /// Returns the name of the current tag that is being displayed by the navigator or 
        /// "views" if the navigator is showing a list of views.
        /// </summary>
        /// <value></value>
        string CurrentTag
        {
            get;
        }
        #endregion

        #region string CurrentView
        /// <summary>
        /// Returns the name of the current view in use or an empty string if the navigator is 
        /// showing a list of views.
        /// </summary>
        /// <value></value>
        string CurrentView
        {
            get;
        }
        #endregion

        #region string DebugText
        /// <summary>
        /// For debugging purposes.
        /// </summary>
        /// <value></value>
        string DebugText
        {
            get;
        }
        #endregion

        #region string DefaultImage
        /// <summary>
        /// An image to use when there are no images.
        /// </summary>
        /// <value></value>
        string DefaultImage { get; set; }
        #endregion

        #region string DefaultMode
        /// <summary>
        /// The default mode to use when the current step does not have a mode.
        /// </summary>
        /// <value></value>
        string DefaultMode { get; set; }
        #endregion

        #region IMLHashItem Filters
        /// <summary>
        /// For debugging purposes.
        /// </summary>
        /// <value></value>
        IMLHashItem Filters { get; }
        #endregion

        #region IMLItemList Items
        /// <summary>
        /// If the navigator is at the bottom, this is the list of items that it is showing. 
        /// Do not access this property if not at the bottom.
        /// </summary>
        /// <value></value>
        IMLItemList Items { get; }
        #endregion

        #region string LastImage
        /// <summary>
        /// The navigator attempts to track the image for the last choice made. This allows 
        /// us to display, for example, the cover for the current album when looking at its 
        /// tracks.
        /// </summary>
        /// <value></value>
        string LastImage { get; }
        #endregion

        #region int Level
        /// <summary>
        /// Gets the current level of the navigator. At the top, it is 0.
        /// </summary>
        /// <value></value>
        int Level { get; }
        #endregion

        #region string SectionName
        /// <summary>
        /// The name of the section that the navigator was created from.
        /// </summary>
        /// <value></value>
        string SectionName { get; }
        #endregion

        #region bool SkipSingleChoice
        /// <summary>
        /// Get/set whether the navigator should skip single choices. This can be very useful to skip over unknown values in views. If set to TRUE, for example, and going from artist to album, the navigator will skip the album step if there is only one album.
        /// </summary>
        /// <value></value>
        bool SkipSingleChoice { get; set; }
        #endregion

        #region string Subtitle
        /// <summary>
        /// Returns a subtitle for display purposes. This is usually the tag value for the next to last choice made.
        /// </summary>
        /// <value></value>
        string Subtitle { get; }
        #endregion

        #region string Title
        /// <summary>
        /// Returns a title for display purposes. This is usually the tag value for the last choice made.
        /// </summary>
        /// <value></value>
        string Title { get; }
        #endregion

        #region string ViewsMode
        /// <summary>
        /// Sets the mode that is returned by CurrentMode when showing the list of views.
        /// </summary>
        /// <value></value>
        string ViewsMode { get; set; }
        #endregion

        #endregion

        #region New Properties

        #region string SortBy
        /// <summary>
        /// Returns the Sort by tag for the current view step
        /// </summary>
        /// <value></value>
        string SortBy
        {
            get;
        }
        #endregion

        #region bool SortAsc
        /// <summary>
        /// Returns TRUE if the current view step is sorted ascending
        /// </summary>
        /// <value></value>
        bool SortAsc
        {
            get;
        }
        #endregion

        #region bool EnableFilters
        /// <summary>
        /// Enables/Disables the usage of custom filters
        /// </summary>
        /// <value></value>
        bool EnableFilters
        {
            get;
            set;
        }
        #endregion

        #endregion

        #endregion

        #region IMLViewNavigator Methods

        #region Old Media Methods

        #region bool Back()
        /// <summary>
        /// Moves the view navigator back one step and returns TRUE if the move was succesful. If the view navigator is at the top, nothing will happen and the result will be FALSE.
        /// </summary>
        /// <returns></returns>
        bool Back();
        #endregion

        #region string Choices(int Index)
        /// <summary>
        /// This is a collection of strings representing the current choices offered by the view navigator. If the navigator is at the top of the hierarchy, for example, the choices will be a list of the names of the views for the section.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        string Choices(int Index);
        #endregion

        #region string Images(int Index)
        /// <summary>
        /// This is a collection of images for the current choices. If the navigator is not at the bottom, the images are pseudo-randomly selected.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        string Images(int Index);
        #endregion

        #region IMLItemList GetAllItemsAtOrBelowHere()
        /// <summary>
        /// Will return all the media items at or below the current step. If at the top, it will return all items in the section. The resulting item list is read-only.
        /// </summary>
        /// <returns></returns>
        IMLItemList GetAllItemsAtOrBelowHere();
        #endregion

        #region IMLItemList GetAllItemsForChoice(int Index)
        /// <summary>
        /// Returns all items below the choice specified. The resulting item list is read-only.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        IMLItemList GetAllItemsForChoice(int Index);
        #endregion

        #region bool Select(int Index)
        /// <summary>
        /// Advances the navigator to the next step based on the item selected. Returns TRUE if the navigator advanced or FALSE if it is already at the bottom.
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        bool Select(int Index);
        #endregion

        #endregion

        #region New Methods

        #region void Refresh()
        /// <summary>
        /// 
        /// </summary>
        void Refresh();
        #endregion

        #region void AddCustomFilter(string ActionName, string filter)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ActionName"></param>
        /// <param name="filter"></param>
        void AddCustomFilter(string ActionName, string filter);
        #endregion

        #endregion

        #endregion
    }
    #endregion
}
