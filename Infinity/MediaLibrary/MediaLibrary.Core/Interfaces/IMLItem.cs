using System;

namespace MediaLibrary
{
    /// <summary>
    /// The IMLItem object represents a record of a Media Library section
    /// </summary>
    public interface IMLItem
	{
        #region DateTime DateCreated
        /// <summary>
        /// Gets the Date the Item was added to the library
        /// </summary>
        /// <value></value>
        DateTime DateCreated
        {
            get;
        }

        #region DateTime DateChanged
        /// <summary>
        /// Gets the Date the Item was last modified in the library
        /// </summary>
        /// <value></value>
        DateTime DateChanged
        {
            get;
        }
        #endregion

        #region string ExternalID
        /// <summary>
        /// Returns/sets the External ID value. The External ID field contains a special 
        /// value inserted by the import plugin used to identify that record. When doing 
        /// an update the import plugin can check this value and check it against the 
        /// external data to see if the item needs to be reimported or updated.
        /// </summary>
        /// <value></value>
        string ExternalID
        {
            get;
            set;
        }
        #endregion

        #region int ID
        /// <summary>
        /// Returns the ID of the item. The id is used internally to give each item a 
        /// unique integer identifier.
        /// </summary>
        /// <value></value>
        int ID
        {
            get;
        }
        #endregion

        #region string ImageFile
        /// <summary>
        /// Returns/sets the Image field value. The image is a file name or URL to an 
        /// image that is associated with the item. For example, with music items, the 
        /// image may represent the cover art for the item’s album. In a section full 
        /// of contacts, the image may be a picture of each contact.
        /// </summary>
        /// <value></value>
        string ImageFile
        {
            get;
            set;
        }
        #endregion

        #region string Location
        /// <summary>
        /// Returns/sets the item's Location. The location of an item is a bit of a 
        /// misnomer. In music sections, for example, it is expected to be the name 
        /// of the actual file that this item represents. In other cases, it can be 
        /// an identifier that is used to link the item to an external source of data. 
        /// For example, if you are importing data from a DVD cataloging application, 
        /// you may put the ID of the DVD in the cataloging application as the 
        /// location of the item.
        /// </summary>
        /// <value></value>
        string Location
        {
            get;
            set;
        }
        #endregion

        #region string Name
        /// <summary>
        /// Return the item's name. The name is a short text description of the item.
        /// </summary>
        /// <value></value>
        string Name
        {
            get;
            set;
        }
        #endregion

        #region IMLHashItem Tags
        /// <summary>
        /// Returns an IMLHashItem objtect containing the tags and their values 
        /// associated to the IMLItem object. Tags are a list of user defined 
        /// attributes.
        /// </summary>
        /// <value></value>
        IMLHashItem Tags
        {
            get;
        }
        #endregion

        #region DateTime TimeStamp
        /// <summary>
        /// Returns/sets the item's time stamp. The time stamp is used by import 
        /// plug-ins to keep track of the last time the item was imported. This 
        /// allows them to update only items that have changed since that date.
        /// </summary>
        /// <value></value>
        DateTime TimeStamp
        {
            get;
            set;
        }
        #endregion

		#endregion

        #region void SaveTags()
        /// <summary>
        /// Saves the changes made to the IMLItem object to the database
        /// </summary>
        void SaveTags();
        #endregion
	}
}
