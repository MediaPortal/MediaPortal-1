using System.ComponentModel;
using System.Xml.Serialization;

namespace MCEControls
{   
    /// <summary>
    /// structure of DisplayName-Key pair data for SoftKey(Data)
    /// </summary>
    public struct NameKeyPair
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        #region Constructors

        public NameKeyPair(char key)
            : this(null, key)
        {
        }

        public NameKeyPair(string displayName, char key)
        {
            _displayName = displayName;
            _key = key;
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        [XmlAttribute]
        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value; }
        }

        [XmlAttribute]
        [TypeConverter(typeof(CharConverter))]
        [DefaultValue('\0')]
        public char Key
        {
            get { return _key; }
            set { _key = value; }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------

        #region Private Fields

        private char _key;
        private string _displayName;

        #endregion
    }
}
