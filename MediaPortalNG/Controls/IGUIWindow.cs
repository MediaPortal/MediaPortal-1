using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;

namespace MediaPortal
{
    public interface IGUIWindow
    {
 
        /// <summary>
        /// Gets the GUIWindow-ID
        /// </summary>
        int ID
        {
            get;
        }

        int DefaultControl
        {
            get;
        }


    }
}
