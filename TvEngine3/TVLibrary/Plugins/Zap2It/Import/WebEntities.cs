/* 
 *	Copyright (C) 2006 Team MediaPortal
 *	http://www.team-mediaportal.com
 *  Written by Jonathan Bradshaw <jonathan@nrgup.net>
 * 
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */
using System;
using System.Collections.ObjectModel;

namespace Zap2it.WebEntities
{
    /// <summary>
    /// Defines a Lineup for Zap2it Web Interface
    /// </summary>
    public class WebLineup
    {
        private string name;
        private string type;
        private string udl_id;
        private string zipCode;
        private string lineup_id;

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Gets or sets the udl_ id.
        /// </summary>
        /// <value>The udl_ id.</value>
        public string Udl_Id
        {
            get { return udl_id; }
            set { udl_id = value; }
        }

        /// <summary>
        /// Gets or sets the zip code.
        /// </summary>
        /// <value>The zip code.</value>
        public string ZipCode
        {
            get { return zipCode; }
            set { zipCode = value; }
        }

        /// <summary>
        /// Gets or sets the lineup id.
        /// </summary>
        /// <value>The lineup id.</value>
        public string LineupId
        {
            get { return lineup_id; }
            set { lineup_id = value; }
        }
        private Uri formAction;

        /// <summary>
        /// Gets or sets the form action.
        /// </summary>
        /// <value>The form action.</value>
        public Uri FormAction
        {
            get { return formAction; }
            set { formAction = value; }
        }

        public override string ToString()
        {
            return this.name;
        }
    }

    /// <summary>
    /// Defines a collection of web lineups keyed on the internal lineup ID
    /// </summary>
    public class WebLineupCollection : KeyedCollection<string, WebLineup>
    {
        public WebLineupCollection() : base() { }

        protected override string GetKeyForItem(WebLineup item)
        {
            return item.LineupId;
        }
    }

    /// <summary>
    /// Defines a channel for Zap2it web interface
    /// </summary>
    public class WebChannel
    {
        private string id;
        private string station;
        private string channelNum;
        private bool enabled;

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        /// <value>The id.</value>
        public string Id
        {
            get { return id; }
            set { id = value; }
        }

        /// <summary>
        /// Gets or sets the station.
        /// </summary>
        /// <value>The station.</value>
        public string Station
        {
            get { return station; }
            set { station = value; }
        }

        /// <summary>
        /// Gets or sets the channel num.
        /// </summary>
        /// <value>The channel num.</value>
        public string ChannelNum
        {
            get { return channelNum; }
            set { channelNum = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="T:WebChannel"/> is enabled.
        /// </summary>
        /// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
    }

    /// <summary>
    /// Defines a collection of channels for the web interface keyed on the internal ID
    /// </summary>
    public class WebChannelCollection : KeyedCollection<string, WebChannel>
    {
        public WebChannelCollection() : base() { }

        protected override string GetKeyForItem(WebChannel item)
        {
            return item.Id;
        }
    }

}
