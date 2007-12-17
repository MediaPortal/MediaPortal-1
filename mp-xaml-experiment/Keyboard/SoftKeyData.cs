using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace MCEControls
{
    /// <summary>
    /// A generic list of type List&lt;SoftKeyData&gt;
    /// workaround due to XAML doesn't support generic type.
    /// </summary>
    public class SoftKeyDataList : List<SoftKeyData> { }

    /// <summary>
    /// data definition class of SoftKey
    /// </summary>
    public class SoftKeyData
    {
        //-------------------------------------------------------------------
        //
        //  Constructors
        //
        //-------------------------------------------------------------------

        //-------------------------------------------------------------------
        //
        //  Public Properties
        //
        //-------------------------------------------------------------------

        #region Public Properties

        public List<NameKeyPair> NameKeyPairs
        {
            get { return _nameKeyPairs; }
        }

        [XmlAttribute]
        [DefaultValue(ControlKey.None)]
        public ControlKey ControlKey
        {
            get { return _controlKey; }
            set { _controlKey = value; }
        }

        [XmlAttribute]
        [DefaultValue(0)]
        public int Column
        {
            get { return _column; }
            set { _column = value; }
        }

        [XmlAttribute]
        [DefaultValue(0)]
        public int Row
        {
            get { return _row; }
            set { _row = value; }
        }

        [XmlAttribute]
        [DefaultValue(1)]
        public int ColumnSpan
        {
            get { return _columnSpan; }
            set { _columnSpan = value; }
        }

        [XmlAttribute]
        [DefaultValue(1)]
        public int RowSpan
        {
            get { return _rowSpan; }
            set { _rowSpan = value; }
        }

        #endregion

        //-------------------------------------------------------------------
        //
        //  Private Fields
        //
        //-------------------------------------------------------------------

        #region Private Fields

        private ControlKey _controlKey;
        private int _column;
        private int _row;
        private int _columnSpan = 1;
        private int _rowSpan = 1;
        private List<NameKeyPair> _nameKeyPairs = new List<NameKeyPair>(2);

        #endregion
    }

}
