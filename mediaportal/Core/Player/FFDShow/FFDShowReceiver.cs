#region Copyright (C) 2005-2010 Team MediaPortal

// Copyright (C) 2005-2010 Team MediaPortal
// http://www.team-mediaportal.com
// 
// MediaPortal is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// MediaPortal is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MediaPortal. If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;



namespace FFDShow
{
    /// <summary>
    /// This class is instantiated and used by FFDShowAPI to receive the answers from FFDShow
    /// It must derivate from System.Windows.Forms.Form because FFDShow communication API is based on
    /// windows messages. This class is not used directly by user.
    /// </summary>
    public class FFDShowReceiver : System.Windows.Forms.Form
    {

        /// <summary>
        /// The CopyData Constant for SendMessage
        /// </summary>
        public const Int32 WM_COPYDATA = 0x004A;

       
        [StructLayout(LayoutKind.Sequential)]
        internal struct COPYDATASTRUCT
        {
            internal UIntPtr dwData;
            internal uint cbData;
            internal IntPtr lpData;
        }

        /// <summary>
        /// Received string
        /// </summary>
        private String receivedString = null;
        /// <summary>
        /// Received type : identifier of the requested parameter
        /// </summary>
        private Int32 receivedType = 0;

        /// <summary>
        /// Thread instance of FFDShowAPI waiting for the response
        /// </summary>
        private Thread parentThread;

        /// <summary>
        /// Gets or sets the string received from FFDShow
        /// </summary>
        public String ReceivedString
        {
            get
            {
                return receivedString;
            }
            set
            {
                receivedString = value;
            }
        }

        /// <summary>
        /// Gets or sets the identifier of the value to retrieve
        /// </summary>
        public Int32 ReceivedType
        {
            get
            {
                return receivedType;
            }
            set
            {
                receivedType = value;
            }
        }


        #region Constructors
        /// <summary>
        /// FFDShowReceiver constructor
        /// </summary>
        /// <param name="parentThread">FFDShowAPI thread to be interrupted once the response is received</param>
        public FFDShowReceiver(Thread parentThread)
        {
            this.parentThread = parentThread;
        }
        #endregion Constructors

        /// <summary>
        /// Main method that receives window messages
        /// We handle only for WM_COPYDATA messages
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_COPYDATA)
            {
                try
                {
                    COPYDATASTRUCT cd = new COPYDATASTRUCT();
                    cd = (COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(COPYDATASTRUCT));

#if UNICODE
                    string returnedData = Marshal.PtrToStringUni(cd.lpData);
#else
                    string returnedData = Marshal.PtrToStringAnsi(cd.lpData);
#endif
                    receivedString = returnedData;
                    receivedType = (int)cd.dwData.ToUInt32();

                    /*if (receivedString != null)
                        Debug.WriteLine("Receiver got " + receivedType + " " + receivedString);
                    else
                        Debug.WriteLine("Receiver got " + receivedType + " NULL");
                    Debug.Flush();*/

                    if (parentThread != null && parentThread.ThreadState == System.Threading.ThreadState.WaitSleepJoin)
                        parentThread.Interrupt();
                    //resetEvent.Set();
                }
                catch (Exception)
                {
                    /*Debug.Write(e.StackTrace.ToString());
                    Debug.Flush();*/
                }
            }
            base.WndProc(ref m);
        }
    }
}
