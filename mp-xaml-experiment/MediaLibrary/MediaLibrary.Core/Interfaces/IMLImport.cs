using System;

namespace MediaLibrary
{
    #region public interface IMLImport
    /// <summary>
    /// The IMLImport object contains information about an import
    /// </summary>
    public interface IMLImport
    {
        #region IMLImport Properties

        #region bool HasSchedule
        /// <summary>
        /// Returns TRUE if the import has a schedule defined
        /// </summary>
        /// <value></value>
        bool HasSchedule
        {
            get;
        }
        #endregion

        #region int ID
        /// <summary>
        /// Returns the import's ID number (assigned by Media)
        /// </summary>
        /// <value></value>
        int ID
        {
            get;
        }
        #endregion

        #region DateTime LastRun
        /// <summary>
        /// Get/Sets the date of the last time the import was run
        /// </summary>
        /// <value></value>
        DateTime LastRun
        {
            get;
            set;
        }
        #endregion

        #region bool MissedLastRun
        /// <summary>
        /// Returns TRUE if the last scheduled run of this import failed or was not executed
        /// </summary>
        /// <value></value>
        bool MissedLastRun
        {
            get;
        }
        #endregion

        #region string Mode
        /// <summary>
        /// Get/Sets the import mode: "Update existing items and add new items" or 
        /// "Delete all items and Re-import"
        /// </summary>
        /// <value></value>
        string Mode
        {
            get;
            set;
        }
        #endregion

        #region string Name
        /// <summary>
        /// Get/Sets the import name
        /// </summary>
        /// <value></value>
        string Name
        {
            get;
            set;
        }
        #endregion

        #region string PluginID
        /// <summary>
        /// Get/Sets the CLSID of the import plugin to execute
        /// </summary>
        /// <value></value>
        string PluginID
        {
            get;
            set;
        }
        #endregion

        #region IMLHashItem PluginProperties
        /// <summary>
        /// Returns the defined properties for the import plugin
        /// </summary>
        /// <value></value>
        IMLHashItem PluginProperties
        {
            get;
        }
        #endregion

        #region bool RunMissed
        /// <summary>
        /// Get/Sets the RunMissed of the IMLImport
        /// </summary>
        /// <value></value>
        bool RunMissed
        {
            get;
            set;
        }
        #endregion

        #region string ScheduleDescription
        /// <summary>
        /// Get/Sets the ScheduleDescription of the IMLImport
        /// </summary>
        /// <value></value>
        string ScheduleDescription
        {
            get;
        }
        #endregion

        #region string ScheduleFrequency
        /// <summary>
        /// Get/Sets the ScheduleFrequency of the IMLImport
        /// </summary>
        /// <value></value>
        string ScheduleFrequency
        {
            get;
            set;
        }
        #endregion

        #region int ScheduleInterval
        /// <summary>
        /// Get/Sets the ScheduleInterval of the IMLImport
        /// </summary>
        /// <value></value>
        int ScheduleInterval
        {
            get;
            set;
        }
        #endregion

        #region DateTime ScheduleTime
        /// <summary>
        /// Get/Sets the import's scheduled time to run
        /// </summary>
        /// <value></value>
        DateTime ScheduleTime
        {
            get;
            set;
        }
        #endregion

        #region string SectionName
        /// <summary>
        /// Get/Sets the section of the Media Library this import uses
        /// </summary>
        /// <value></value>
        string SectionName
        {
            get;
            set;
        }
        #endregion

        #region bool WakeUp
        /// <summary>
        /// Get/Sets the WakeUp of the IMLImport
        /// </summary>
        /// <value></value>
        bool WakeUp
        {
            get;
            set;
        }
        #endregion

        #endregion

        #region IMLImport Methods

        #region bool Run(IMLImportProgress Progress, out string ErrorText)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Progress"></param>
        /// <param name="ErrorText"></param>
        /// <returns></returns>
        bool Run(IMLImportProgress Progress, out string ErrorText);
        #endregion

        #region void Save()
        /// <summary>
        /// 
        /// </summary>
        void Save();

        #endregion

        #endregion
    }
    #endregion
}
