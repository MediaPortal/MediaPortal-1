namespace MediaLibrary
{
    #region public interface IMLPluginProperty
    /// <summary>
    /// The IMLPluginProperty object sets the parameters for a plugin property. It is passed as a parameter for the GetProperty function of the IML object
    /// </summary>
    public interface IMLPluginProperty
    {
        #region bool CanTypeChoices
        /// <summary>
        /// If set to True, the property will show as a drop down, but the user will be able to type in new values
        /// </summary>
        /// <value></value>
        bool CanTypeChoices
        {
            get;
            set;
        }
        #endregion

        #region string Caption
        /// <summary>
        /// Sets the property caption
        /// </summary>
        /// <value></value>
        string Caption
        {
            get;
            set;
        }
        #endregion

        #region object[] Choices
        /// <summary>
        /// Sets a string array to be the allowed choices for this property. They will be displayed in a dropdown list by Media's configuration program.
        /// </summary>
        /// <value></value>
        object[] Choices  // TODO: make this string[] (or object as in Media)
        {
            get;
            set;
        }
        #endregion

        #region IMLHashItem Choices2
        /// <summary>
        /// This is an alternative (and better) way to specify a drop down list of possible values for a property. 
        /// The property type must be set to string or int in order for this to work correctly. 
        /// The IMLHashItem should contain a number of keys and values. 
        /// The keys cannot be numeric (or they will be interpreted as indices). 
        /// The values contain the text that will be displayed to the user. 
        /// When SetProperties is called, the value of this property will be the key of the selected choice and not the associated text.
        /// </summary>
        /// <value></value>
        IMLHashItem Choices2
        {
            get;
            set;
        }
        #endregion

        #region string DataType
        /// <summary>
        /// Sets the property type. Media's configuration program will display properties in different ways depending on the data type specified. 
        /// Allowed values are: 
        /// string, int, float, date, time: Allows the user to enter a string of text that will be converted to the type specified 
        /// bool: Specifies a boolean (TRUE/FALSE) value 
        /// section: Allows the user to select a section from the Media Library 
        /// folder: Displays the "Browse for folder" dialog box, where the user can select a folder. 
        /// folderlist: Displays the Folders window, where the user can select a list of folders. The list will be returned to your plugin as variant array of strings. It could also be null. 
        /// stringlist: Displays a window where the user can enter any number of strings. The list will be returned to your plugin as variant array of strings. It could also be null. 
        /// file: Allows the user to select a file
        /// custom: Displays a custom window. Media will call the EditCustomProperty function passing the property name. Your plugin is responsible for showing the window, returning the value to Media and destroying any resources it uses.
        /// password: Lets the user enter a password. The characters type will be converted to asterisks (*)
        /// </summary>
        /// <value></value>
        string DataType
        {
            get;
            set;
        }
        #endregion

        #region object DefaultValue
        /// <summary>
        /// Sets the default value for the property
        /// </summary>
        /// <value></value>
        object DefaultValue
        {
            get;
            set;
        }
        #endregion

        #region string GroupCaption
        /// <summary>
        /// If set will make Media Config display the supplied string as a Group Caption above this property.
        /// </summary>
        /// <value></value>
        string GroupCaption
        {
            get;
            set;
        }
        #endregion

        #region string HelpText
        /// <summary>
        /// Sets the help text to the specified string. This text will be inside a tooltip, under the property name, that will be displayed when the user moves the mouse over the property.
        /// </summary>
        /// <value></value>
        string HelpText
        {
            get;
            set;
        }
        #endregion

        #region bool IsMandatory
        /// <summary>
        /// If a property is set to Mandatory, Media won't let the user leave the field empty. Mandatory properties are displayed in bold by Media's configuration program.
        /// </summary>
        /// <value></value>
        bool IsMandatory
        {
            get;
            set;
        }
        #endregion

        #region string Name
        /// <summary>
        /// Gets the property name. This is an internal value useful only for the developer and will be passed as an IMLHashItem key to the SetProperties method of your plugin.
        /// </summary>
        /// <value></value>
        string Name
        {
            get;
        }
        #endregion

        #region bool CausesValidation
        /// <summary>
        /// If true then after this value is modified, the ValidateProperties function will be called passing 
        /// in both the collection of properties and their associated values.  This allows you to make properties
        /// that are dependent on other property's values.
        /// </summary>
        /// <value></value>
        bool CausesValidation
        {
            get;
            set;
        }
        #endregion
    }
    #endregion
}

