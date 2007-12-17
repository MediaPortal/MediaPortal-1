using System;
using System.Collections.Generic;
using System.Text;

namespace MediaLibrary.Settings
{
    public class MLPluginProperty : IMLPluginProperty
    {
        bool _CanTypeChoices;
        string _Caption;
        object[] _Choices;  //string[]
        IMLHashItem _Choices2;
        string _DataType;
        object _DefaultValue;
        object _Value;
        string _GroupCaption;
        string _HelpText;
        bool _IsMandatory;
        string _Name;
        private bool _CausesValidation;

//        ___
        
        public MLPluginProperty()
        {
            _CanTypeChoices = false;
            _Caption = "Property";
            _Choices = null;  //string[]
            _Choices2 = new MLHashItem();
            _DataType = "string";
            _DefaultValue = null;
            _GroupCaption = null;
            _HelpText = "";
            _IsMandatory = false;
            _Name = "";
        }

        public static readonly string NoPropName = "#+§invalid&";
        
        public bool CanTypeChoices
        {
            get
            {
                return _CanTypeChoices;
            }
            set
            {
                _CanTypeChoices = value;
            }
        }

        public string Caption
        {
            get
            {
                return _Caption;
            }
            set
            {
                _Caption = value;
            }
        }

        public object[] Choices  //string[]
        {
            get
            {
                return _Choices;
            }
            set
            {                
                _Choices = value;
            }
        }

        public IMLHashItem Choices2
        {
            get
            {
                return _Choices2;
            }
            set
            {
                _Choices2 = value;
            }
        }

        public string DataType
        {
            get
            {
                return _DataType;
            }
            set
            {
                _DataType = value;
            }
        }

        public object DefaultValue
        {
            get
            {
                return _DefaultValue;
            }
            set
            {
                _DefaultValue = value;
            }
        }

        public string GroupCaption
        {
            get
            {
                return _GroupCaption;
            }
            set
            {
                _GroupCaption = value;
            }
        }

        public string HelpText
        {
            get
            {
                return _HelpText;
            }
            set
            {
                _HelpText = value;
            }
        }

        public bool IsMandatory
        {
            get
            {
                return _IsMandatory;
            }
            set
            {
                _IsMandatory = value;
            }
        }

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        // this contains the actual value
        public object Value
        {
            get
            {
                return _Value;
            }
            set
            {
                _Value = value;
            }
        }

        

        public bool CausesValidation
        {
            get { return _CausesValidation; }
            set { _CausesValidation = value; }
        }

        
    }
}
