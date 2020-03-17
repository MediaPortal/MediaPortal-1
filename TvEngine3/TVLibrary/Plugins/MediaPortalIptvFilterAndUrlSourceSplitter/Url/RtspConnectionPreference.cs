using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TvEngine.MediaPortalIptvFilterAndUrlSourceSplitter.Url
{
    public partial class RtspConnectionPreference : UserControl
    {
        #region Private fields

        private int multicastPreference = RtspUrl.DefaultRtspMulticastPreference;
        private int udpPreference = RtspUrl.DefaultRtspUdpPreference;
        private int sameConnectionPreference = RtspUrl.DefaultRtspSameConnectionTcpPreference;

        #endregion

        #region Constructors

        public RtspConnectionPreference()
        {
            InitializeComponent();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Specifies UDP multicast connection preference.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="MulticastPreference"/> is lower than zero.</para>
        /// </exception>
        public int MulticastPreference
        {
            get { return this.multicastPreference; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("MulticastPreference", value, "Cannot be lower than zero.");
                }

                this.multicastPreference = value;
            }
        }

        /// <summary>
        /// Specifies UDP connection preference.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="UdpPreference"/> is lower than zero.</para>
        /// </exception>
        public int UdpPreference
        {
            get { return this.udpPreference; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("UdpPreference", value, "Cannot be lower than zero.");
                }

                this.udpPreference = value;
            }
        }

        /// <summary>
        /// Specifies same connection preference.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// <para>The <see cref="SameConnectionPreference"/> is lower than zero.</para>
        /// </exception>
        public int SameConnectionPreference
        {
            get { return this.sameConnectionPreference; }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("SameConnectionPreference", value, "Cannot be lower than zero.");
                }

                this.sameConnectionPreference = value;
            }
        }

        #endregion

        #region Methods

        private void RtspConnectionPreference_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 3; i++)
            {
                if (i == this.SameConnectionPreference)
                {
                    this.listBoxRtspConnectionPreference.Items.Add(RtspConnectionPreference.RtspSameConnectionPreference);
                    continue;
                }

                if (i == this.UdpPreference)
                {
                    this.listBoxRtspConnectionPreference.Items.Add(RtspConnectionPreference.RtspUdpConnectionPreference);
                    continue;
                }

                if (i == this.MulticastPreference)
                {
                    this.listBoxRtspConnectionPreference.Items.Add(RtspConnectionPreference.RtspUdpMulticastConnectionPreference);
                    continue;
                }
            }
        }

        private void buttonRtspUp_Click(object sender, EventArgs e)
        {
            if (this.listBoxRtspConnectionPreference.SelectedIndex > 0)
            {
                String previousItem = (String)this.listBoxRtspConnectionPreference.Items[this.listBoxRtspConnectionPreference.SelectedIndex - 1];
                String item = (String)this.listBoxRtspConnectionPreference.Items[this.listBoxRtspConnectionPreference.SelectedIndex];

                this.listBoxRtspConnectionPreference.Items[this.listBoxRtspConnectionPreference.SelectedIndex - 1] = item;
                this.listBoxRtspConnectionPreference.Items[this.listBoxRtspConnectionPreference.SelectedIndex] = previousItem;

                this.listBoxRtspConnectionPreference.SelectedIndex--;
                this.UpdatePreferences();
            }
        }

        private void buttonDown_Click(object sender, EventArgs e)
        {
            if (this.listBoxRtspConnectionPreference.SelectedIndex < (this.listBoxRtspConnectionPreference.Items.Count - 1))
            {
                String nextItem = (String)this.listBoxRtspConnectionPreference.Items[this.listBoxRtspConnectionPreference.SelectedIndex + 1];
                String item = (String)this.listBoxRtspConnectionPreference.Items[this.listBoxRtspConnectionPreference.SelectedIndex];

                this.listBoxRtspConnectionPreference.Items[this.listBoxRtspConnectionPreference.SelectedIndex + 1] = item;
                this.listBoxRtspConnectionPreference.Items[this.listBoxRtspConnectionPreference.SelectedIndex] = nextItem;

                this.listBoxRtspConnectionPreference.SelectedIndex++;
                this.UpdatePreferences();
            }
        }

        private void UpdatePreferences()
        {
            for (int i = 0; i < 3; i++)
            {
                if (((String)this.listBoxRtspConnectionPreference.Items[i]) == RtspConnectionPreference.RtspSameConnectionPreference)
                {
                    this.SameConnectionPreference = i;
                    continue;
                }

                if (((String)this.listBoxRtspConnectionPreference.Items[i]) == RtspConnectionPreference.RtspUdpConnectionPreference)
                {
                    this.UdpPreference = i;
                    continue;
                }

                if (((String)this.listBoxRtspConnectionPreference.Items[i]) == RtspConnectionPreference.RtspUdpMulticastConnectionPreference)
                {
                    this.MulticastPreference = i;
                    continue;
                }
            }
        }

        #endregion

        #region Constants

        private static readonly String RtspSameConnectionPreference = "same connection";

        private static readonly String RtspUdpConnectionPreference = "UDP";

        private static readonly String RtspUdpMulticastConnectionPreference = "UDP multicast";

        #endregion
    }
}
