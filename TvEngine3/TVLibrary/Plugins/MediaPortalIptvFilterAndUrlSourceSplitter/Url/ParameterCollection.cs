using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    /// <summary>
    /// Represents collection of parameters for MediaPortal IPTV Source Filter.
    /// </summary>
    internal class ParameterCollection : Collection<Parameter>
    {
        #region Private fields
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="ParameterCollection"/> class.
        /// </summary>
        public ParameterCollection()
            : base()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets filter parameters.
        /// </summary>
        public virtual String FilterParameters
        {
            get
            {
                StringBuilder builder = new StringBuilder();

                foreach (var parameter in this)
                {
                    builder.AppendFormat((builder.Length == 0) ? "{0}" : "{1}{0}", parameter.FormatParameter(ParameterCollection.ParameterSeparator), ParameterCollection.ParameterSeparator);
                }

                return builder.ToString();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Inserts a parameter into collection at specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The parameter to insert.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <paramref name="item"/> is <see langword="null"/>.</para>
        /// </exception>
        protected override void InsertItem(int index, Parameter item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            base.InsertItem(index, item);
        }

        /// <summary>
        /// Replaces the parameter at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the parameter to replace.param>
        /// <param name="item">The new value for the parameter at the specified index.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>The <paramref name="item"/> is <see langword="null"/>.</para>
        /// </exception>
        protected override void SetItem(int index, Parameter item)
        {
            base.SetItem(index, item);
        }

        /// <summary>
        /// Gets parameters from URL.
        /// </summary>
        /// <param name="url">The url to extract parameters.</param>
        /// <returns>The collection of parameters.</returns>
        public static ParameterCollection GetParameters(String url)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }

            ParameterCollection result = new ParameterCollection();
            int index = url.IndexOf(SimpleUrl.ParameterSeparator);

            if (index != (-1))
            {
                url = url.Substring(index + SimpleUrl.ParameterSeparator.Length);

                String[] splitted = url.Split(new String[] { ParameterCollection.ParameterSeparator }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var paramValue in splitted)
                {
                    String[] paramValueSplitted = paramValue.Split(new String[] { Parameter.ParameterAssign }, StringSplitOptions.None);

                    result.Add(new Parameter(paramValueSplitted[0], System.Web.HttpUtility.UrlDecode(paramValueSplitted[1])));
                }
            }

            return result;
        }

        #endregion

        #region Constants

        /// <summary>
        /// Specifies parameter separator for MediaPortal IPTV Source Filter.
        /// </summary>
        public static String ParameterSeparator = "&";

        #endregion
    }
}
