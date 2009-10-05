using System;
using System.Collections.Generic;
using System.Text;

namespace MpeCore.Classes
{
    public class VersionInfo: IComparable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionInfo"/> class.
        /// Version number components are stored in string format
        /// </summary>
        public VersionInfo()
        {
            Major = "0";
            Minor = "0";
            Build = "0";
            Revision = "0";
        }

        public string Major { get; set; }
        public string Minor { get; set; }
        public string Build { get; set; }
        public string Revision { get; set; }

        public override string ToString()
        {
            return Major + "." + Minor + "." + Build + "." + Revision;
        }

        /// <summary>
        /// Compares the current instance with another object of the same type and returns an integer that indicates whether the current instance precedes, follows, or occurs in the same position in the sort order as the other object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the objects being compared. The return value has these meanings: 
        ///                     Value 
        ///                     Meaning 
        ///                     Less than zero 
        ///                     This instance is less than <paramref name="obj"/>. 
        ///                     Zero 
        ///                     This instance is equal to <paramref name="obj"/>. 
        ///                     Greater than zero 
        ///                     This instance is greater than <paramref name="obj"/>. 
        /// </returns>
        /// <param name="obj">An object to compare with this instance. 
        ///                 </param><exception cref="T:System.ArgumentException"><paramref name="obj"/> is not the same type as this instance. 
        ///                 </exception><filterpriority>2</filterpriority>
        public int CompareTo(object obj)
        {
            return this.ToString().CompareTo(obj.ToString());
        }
    }
}
