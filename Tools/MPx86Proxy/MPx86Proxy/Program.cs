using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace MPx86Proxy
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam); //Places (posts) a message in the message queue associated with the thread that created the specified window and returns without waiting for the thread to process the message.

        [DllImport("user32.dll")]
        private static extern int RegisterWindowMessage(string message); //Defines a new window message that is guaranteed to be unique throughout the system. The message value can be used when sending or posting messages.

        private const int HWND_BROADCAST = 0xFFFF; //The message is posted to all top-level windows in the system, including disabled or invisible unowned windows, overlapped windows, and pop-up windows. The message is not posted to child windows.

        internal static readonly int WM_ACTIVATEAPP = RegisterWindowMessage("WM_ACTIVATEAPP");

        private const int SW_FORCEMINIMIZE = 11; // Minimizes a window, even if the thread that owns the window is not responding. This flag should only be used when minimizing windows from a different thread.
        private const int SW_HIDE = 0; //Hides the window and activates another window.
        private const int SW_MAXIMIZE = 3; //Maximizes the specified window.
        private const int SW_MINIMIZE = 6; //Minimizes the specified window and activates the next top-level window in the Z order.
        private const int SW_RESTORE = 9; //Activates and displays the window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when restoring a minimized window.
        private const int SW_SHOW = 5; //Activates the window and displays it in its current size and position. 
        private const int SW_SHOWDEFAULT = 10; //Sets the show state based on the SW_ value specified in the STARTUPINFO structure passed to the CreateProcess function by the program that started the application. 
        private const int SW_SHOWMAXIMIZED = 3; //Activates the window and displays it as a maximized window.
        private const int SW_SHOWMINIMIZED = 2; //Activates the window and displays it as a minimized window.
        private const int SW_SHOWMINNOACTIVE = 7; //Displays the window as a minimized window. This value is similar to SW_SHOWMINIMIZED, except the window is not activated.
        private const int SW_SHOWNA = 8; //Displays the window in its current size and position. This value is similar to SW_SHOW, except that the window is not activated.
        private const int SW_SHOWNOACTIVATE = 4; //Displays a window in its most recent size and position. This value is similar to SW_SHOWNORMAL, except that the window is not activated.
        private const int SW_SHOWNORMAL = 1; //Activates and displays a window. If the window is minimized or maximized, the system restores it to its original size and position. An application should specify this flag when displaying the window for the first time.

        private const string GUID = "9a52325f-d4d6-430f-8d4f-e720e29da316";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            bool bCreatedNewMutex = true;
            using (Mutex mutex = new Mutex(true, GUID, out bCreatedNewMutex))//make sure it's an unique identifier (a GUID would be better)
            {
                if (!bCreatedNewMutex)
                {
                    //we tried to create a mutex, but there's already one (bCreatedNewMutex = false - another app created it before)
                    //so there's another instance of this application running

                    // Single instance
                    Process curr = Process.GetCurrentProcess();
                    Process[] procs = Process.GetProcessesByName(curr.ProcessName);
                    foreach (Process p in procs)
                    {
                        if (p.Id != curr.Id)
                        {
                            IntPtr handle = p.MainWindowHandle;
                            if (handle != IntPtr.Zero)
                            {
                                ShowWindowAsync(handle, SW_RESTORE);
                            }
                            else
                            {
                                //can't activate the window, it's not visible and we can't get its handle
                                PostMessage((IntPtr)HWND_BROADCAST, WM_ACTIVATEAPP, (IntPtr)p.Id, IntPtr.Zero); //this message will be sent to MainForm
                            }
                            return;
                        }
                    }
                }
                else
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);

                    MainForm formMain = new MainForm(args);
                    try
                    {
                        Application.Run(formMain);
                    }
                    catch (Exception ex)
                    {
                        //MainForm.Logger.Error("[MainForm] Fatal error: {0} {1} {2}", ex.Message, ex.Source, ex.StackTrace);
                        MessageBox.Show(ex.Message, "Fatal error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }

        }
    }
}
