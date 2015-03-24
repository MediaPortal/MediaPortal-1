using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents parameter for MediaPortal IPTV Source Filter.
    /// </summary>
    internal class Parameter
    {
        #region Private fields

        private String name;
        private String value;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="Parameter"/> class.
        /// </summary>
        /// <param name="name">The name of parameter.</param>
        /// <param name="value">The value of parameter.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="name"/> is <see langword="null"/>.</para>
        /// <para>- or -</para>
        /// <para>The <see cref="value"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The <see cref="name"/> is empty string ("").</para>
        /// </exception>
        public Parameter(String name, String value)
        {
            this.Name = name;
            this.Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the name of parameter.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="Name"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The <see cref="Name"/> is empty string ("").</para>
        /// </exception>
        public String Name
        {
            get { return this.name; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Name");
                }

                if (value == String.Empty)
                {
                    throw new ArgumentException("Cannot be empty string.", "Name");
                }

                this.name = value;
            }
        }

        /// <summary>
        /// Gets or sets the value of parameter.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="Value"/> is <see langword="null"/>.</para>
        /// </exception>
        public String Value
        {
            get { return this.value; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("Name");
                }

                this.value = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Formats parameter name and value into one string.
        /// </summary>
        /// <param name="parameterSeparator">The separator between parameters.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <paramref name="parameterSeparator"/> is <see langword="null"/>.</para>
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <para>The <paramref name="parameterSeparator"/> contains invalid characters.</para>
        /// </exception>
        /// <returns>
        /// A <see cref="System.String"/> which represent parameter for MediaPortal IPTV Source Filter.
        /// </returns>
        public virtual String FormatParameter(String parameterSeparator)
        {
            if (parameterSeparator == null)
            {
                throw new ArgumentNullException("parameterSeparator");
            }

            if (parameterSeparator.Contains(Parameter.ParameterAssign))
            {
                throw new ArgumentException("Argument contains invalid characters.", "parameterSeparator");
            }

            return String.Format("{0}{1}{2}", this.Name, Parameter.ParameterAssign, System.Web.HttpUtility.UrlEncode(this.Value).Replace("+", "%20"));
        }

        #endregion

        #region Constants

        /// <summary>
        /// Specifies string which assign parameter value to parameter name.
        /// </summary>
        public static String ParameterAssign = "=";

        #endregion
    }
}
