using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MPx86Proxy.Controls
{
    public class EventTableMessage
    {
        public EventTableMessageTypeEnum Type { get; internal set; }
        public Icon Icon { get; internal set; }
        public string Text { get; internal set; }
        public DateTime TimeStamp { get; internal set; }

        public EventTableMessage(string strText, EventTableMessageTypeEnum type, Icon icon, DateTime ts)
        {
            this.Text = strText;
            this.Type = type;
            this.Icon = icon;
            this.TimeStamp = ts;
        }
    }
}
