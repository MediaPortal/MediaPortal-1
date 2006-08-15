using System;
using System.Text;
using System.Xml.Serialization;

namespace ProcessPlugins.ExternalDisplay.Setting
{
    [Serializable]
    public class TextProgressBar : Value
    {
        private char startChar = '[';
        private char endChar = ']';
        private char valueChar = '#';
        private char fillChar = '-';
        private int length = 8;
        private Property valueProperty;
        private Property targetProperty;
        private double targetValue = -1;

        public TextProgressBar()
        {
        }

        public TextProgressBar(string valueProperty, string targetProperty, int length)
        {
            this.length = length;
            this.valueProperty = new Property(valueProperty);
            this.targetProperty = new Property(targetProperty);
        }

        /// <summary>
        /// The character to start the progress bar with.
        /// </summary>
        /// <remarks>
        /// Default value is [
        /// </remarks>
        [XmlAttribute]
        public string StartChar
        {
            get { return new string(startChar,1); }
            set
            {
                if (value != null && value.Length > 0)
                {
                    startChar = value[0];
                }
            }
        }

        /// <summary>
        /// The character to end the progress bar with.
        /// </summary>
        /// <remarks>
        /// Default value is ]
        /// </remarks>
        [XmlAttribute]
        public string EndChar
        {
            get { return new string(endChar,1); }
            set
            {
                if (value != null && value.Length > 0)
                {
                    endChar = value[0];
                }
            }
        }

        /// <summary>
        /// The character to draw the progress bar's value with.
        /// </summary>
        /// <remarks>
        /// Default value is #
        /// </remarks>
        [XmlAttribute]
        public string ValueChar
        {
            get { return new string(valueChar,1); }
            set
            {
                if (value != null && value.Length > 0)
                {
                    valueChar = value[0];
                }
            }
        }

        /// <summary>
        /// The character to fill the rest of the progress bar with.
        /// </summary>
        /// <remarks>
        /// Default value is -
        /// </remarks>
        [XmlAttribute]
        public string FillChar
        {
            get { return new string(fillChar,1); }
            set
            {
                if (value != null && value.Length > 0)
                {
                    fillChar = value[0];
                }
            }
        }

        /// <summary>
        /// The number of characters the complete progress bar should be.
        /// </summary>
        /// <remarks>This number includes the begin- and end characters.</remarks>
        [XmlAttribute]
        public int Length
        {
            get { return length; }
            set { length = value; }
        }
        

        /// <summary>
        /// The property that holds the value to draw.
        /// </summary>
        /// <remarks>Only properties holding time- and number values are supported.</remarks>
        [XmlElement]
        public Property ValueProperty
        {
            get { return valueProperty; }
            set { valueProperty = value; }
        }

        /// <summary>
        /// The property that holds the value that represents a completely filled progress bar.
        /// </summary>
        /// <remarks>Only properties holding time- and number values are supported.</remarks>
        [XmlElement]
        public Property TargetProperty
        {
            get { return targetProperty; }
            set { targetProperty = value; }
        }

        /// <summary>
        /// Evaluates the properties and returns the progress bar.
        /// </summary>
        /// <returns>A <see cref="string"/> containing the complete progress bar.</returns>
        public override string Evaluate()
        {
            double currentValue = ConvertToInt(valueProperty.Evaluate());
            int barLength = (int) (currentValue <= 0 ? 0 : (currentValue/TargetValue)*(length - 2));
            StringBuilder b = new StringBuilder(length);
            b.Append(startChar);
            b.Append(valueChar, barLength);
            b.Append(fillChar, length - 2 - barLength);
            b.Append(endChar);
            return b.ToString();
        }

        /// <summary>
        /// Tries to convert the passed <see cref="string"/> to a <see cref="double"/>.
        /// </summary>
        /// <param name="stringValue">The value to convert.</param>
        /// <returns>The result of the conversion.</returns>
        private double ConvertToInt(string stringValue)
        {
            DateTime dateResult;
            double result;

            if (stringValue == null || stringValue.Length == 0)
            {
                result = 0;
            }
            else if (DateTime.TryParse(stringValue, out dateResult))
            {
                result = dateResult.TimeOfDay.TotalSeconds;
            }
            else if (!Double.TryParse(stringValue, out result))
            {
                result = Convert.ToInt32(stringValue);
            }
            return result;
        }

        /// <value>
        /// Holds the cached target value (bar is full)
        /// </value>
        private double TargetValue
        {
            get
            {
                if (targetValue < 0)
                {
                    targetValue = ConvertToInt(targetProperty.Evaluate());
                }
                return targetValue;
            }
        }
    }
}