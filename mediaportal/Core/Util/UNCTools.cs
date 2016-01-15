using System;
using System.IO;
using System.Management;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using MediaPortal.GUI.Library;
using MediaPortal.Services;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading;

namespace MediaPortal.Util
{
    /// <summary>
    /// A static class to help with resolving a mapped drive path to a UNC network path.
    /// If a local drive path or a UNC network path are passed in, they will just be returned.
    /// Also there is a File/Folder exists function which first pings the UNC server first, if it is not reachable it states that file not exitst.
    /// This is usefull because there is no "long" timeout like on c# File.Exists Function.
    /// </summary>

    public static class UNCTools
    {

        #region Declarations

        [DllImport("wininet", CharSet = CharSet.Auto)]
        static extern bool InternetGetConnectedState(ref ConnectionStatusEnum flags, int dw);

        /// <summary>
        /// enum to hold the possible connection states
        /// </summary>
        [Flags]
        enum ConnectionStatusEnum : int
        {
            INTERNET_CONNECTION_MODEM = 0x1,
            INTERNET_CONNECTION_LAN = 0x2,
            INTERNET_CONNECTION_PROXY = 0x4,
            INTERNET_RAS_INSTALLED = 0x10,
            INTERNET_CONNECTION_OFFLINE = 0x20,
            INTERNET_CONNECTION_CONFIGURED = 0x40
        }

        #endregion

        #region Public functions

        /// <summary>
        /// Resolves the given path to a full UNC path if the path is a mapped drive.
        /// Otherwise, just returns the given path.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns></returns>
        public static string ResolveToUNC(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                Log.Debug("UNCTools: ResolveToUNC: The path argument '" + path + "' was null or whitespace.");
            }

            if (!Path.IsPathRooted(path))
            {
                Log.Debug(
                    string.Format("UNCTools: ResolveToUNC: The path '{0}' was not a rooted path and ResolveToUNC does not support relative paths.",
                        path)
                );
            }

            // Is the path already in the UNC format?
            if (path.StartsWith(@"\\"))
            {
                return path;
            }

            string rootPath = ResolveToRootUNC(path);

            if (path.StartsWith(rootPath))
            {
                return path; // Local drive, no resolving occurred
            }
            else
            {
                return path.Replace(GetDriveLetter(path), rootPath);
            }
        }

        /// <summary>
        /// Resolves the given path to a root UNC path if the path is a mapped drive.
        /// Otherwise, just returns the given path.
        /// </summary>
        /// <param name="path">The path to resolve.</param>
        /// <returns></returns>
        public static string ResolveToRootUNC(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                Log.Debug("UNCTools: ResolveToRootUNC: The path argument was null or whitespace.");
            }

            if (!Path.IsPathRooted(path))
            {
                Log.Debug(
                    string.Format("UNCTools: ResolveToRootUNC: The path '{0}' was not a rooted path and ResolveToRootUNC does not support relative paths.",
                    path)
                );
            }

            if (path.StartsWith(@"\\"))
            {
                return Directory.GetDirectoryRoot(path);
            }

            // Get just the drive letter for WMI call
            string driveletter = GetDriveLetter(path);

            // Query WMI if the drive letter is a network drive, and if so the UNC path for it
            using (System.Management.ManagementObject mo = new ManagementObject())
            {
                mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", driveletter));

                DriveType driveType = (DriveType)((uint)mo["DriveType"]);
                string networkRoot = Convert.ToString(mo["ProviderName"]);

                if (driveType == DriveType.Network)
                {
                    return networkRoot;
                }
                return driveletter + Path.DirectorySeparatorChar;
            }
        }

        /// <summary>
        /// Checks if the given path is a network drive.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns></returns>
        public static bool isNetworkDrive(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                Log.Debug("UNCTools: isNetworkDrive: The path argument was null or whitespace.");
            }

            if (!Path.IsPathRooted(path))
            {
                Log.Debug(
                    string.Format("UNCTools: isNetworkDrive: The path '{0}' was not a rooted path and ResolveToRootUNC does not support relative paths.",
                    path)
                );
            }

            if (path.StartsWith(@"\\"))
            {
                return true;
            }

            // Get just the drive letter for WMI call
            string driveletter = GetDriveLetter(path);

            // Query WMI if the drive letter is a network drive
            using (ManagementObject mo = new ManagementObject())
            {
                mo.Path = new ManagementPath(string.Format("Win32_LogicalDisk='{0}'", driveletter));
                DriveType driveType = (DriveType)((uint)mo["DriveType"]);
                return driveType == DriveType.Network;
            }
        }

        /// <summary>
        /// Given a path will extract just the drive letter with volume separator.
        /// </summary>
        /// <param name="path"></param>
        /// <returns>C:</returns>
        public static string GetDriveLetter(string path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                Log.Debug("UNCTools: GetDriveLetter: The path argument was null or whitespace.");
            }

            if (!Path.IsPathRooted(path))
            {
                Log.Debug(
                    string.Format("UNCTools: GetDriveLetter: The path '{0}' was not a rooted path and GetDriveLetter does not support relative paths.",
                    path)
                );
            }

            if (path.StartsWith(@"\\"))
            {
                Log.Debug("UNCTools: A UNC path was passed to GetDriveLetter, path '" + path + "'");
            }

            return Directory.GetDirectoryRoot(path).Replace(Path.DirectorySeparatorChar.ToString(), "");
        }

      /// <summary>
      /// Check if the host of an UNC file/folder is online and the given filesystem object exists (with user defined ping timeout)
      /// On local files/folders (ex.: c:\temp\1.txt) will be returned true/// 
      /// ex.: bolRes = UNCFileFolderExists("\\MYSERVER\VIDEOS\1.MKV");
      /// ex.: bolRes = UNCFileFolderExists("\\MYSERVER\VIDEOS\");/// 
      /// </summary>
      /// <param name="strFile"></param>
      /// <returns>BOOL</returns>
      public static bool UNCFileFolderExists(string strFile)
      {
        string strUNCPath;
        bool bolExist = false;
        string strType = "";
        try
        {
          //Check if the host of the file/folder is online
          strUNCPath = UNCFileFolderOnline(strFile);
          if (strUNCPath == "")
          {
            return false;
          }

          // get the file attributes for file or directory
          FileAttributes attr = File.GetAttributes(strUNCPath);

          //detect whether its a directory or file
          if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
          {
            //its a folder
            bolExist = Directory.Exists(strUNCPath);           //Does the folder exist?
            strType = "Folder";
          }
          else
          {
            //its a file
            bolExist = File.Exists(strUNCPath);                //Does the file exist?
            strType = "File";
          }


          if (bolExist)
          {
            //File/Folder exists
            Log.Debug("UNCFileFolderExists: " + strType + "'" + strFile + "' exists!");
          }
          else
          {
            //File/Folder doesnt exist
            Log.Info("UNCTools: UNCFileFolderExists: " + strType + "'" + strFile + "' doesn't exists or isnt online!");
          }

        }
        catch (Exception ex)
        {
        }

        //Return the flag
        return bolExist;

      }

        /// <summary>
        /// Check if the host of an UNC file/folder is online, (with user defined ping timeout)
        /// On local files/folders (ex.: c:\temp\1.txt) will be returned true
        /// ex.: strUNCPath = UNCFileFolderOnline("C:\mydir\myfile.ext");
        /// ex.: strUNCPath = UNCFileFolderOnline("C:\mydir\");/// 
        /// </summary>
        /// <param name="strFile"></param>
        /// <returns>the converted UNC Path as string when the file/folder is online</returns>
        /// <returns>empty string when the file/folder is offline</returns>
        public static string UNCFileFolderOnline(string strFile)
        {
            string strUNCPath;
            int iPingAnswers = 0;

            //Resolve given path to UNC
            strUNCPath = ResolveToUNC(strFile);

            //Get Host name
            Uri uri = new Uri(strUNCPath);

            //ping the Host
            if (uri.Host != "")
            {
                //We have an host -> try to ping it

                iPingAnswers = PingHost(uri.Host, 200, 2);
                if (iPingAnswers == 0)
                {
                    //We DONT have received an answer
                    Log.Debug("UNCTools: UNCFileFolderOnline: host '" + uri.Host + "' is not reachable!! , File/Folder '" + strFile + "'");
                    return "";
                }
            }

            //UNC device is online or local file/folder
            return strUNCPath;
        }

        public static bool IsUNCFileFolderOnline(string strFile)
        {
          if (UNCFileFolderOnline(strFile) == string.Empty)
          {
            return false;
          }
          return true;
        }

        //Method UNCCopyFile copies a remote file (strSourceFile) to the given strDestFile
        public static void UNCCopyFile(string strSourceFile, string strDestFile)
        {
            //CopyDB
            try
            {
                if (UNCTools.UNCFileFolderExists(strSourceFile))
                {
                    //UNC host is online and file exists
                    File.Copy(strSourceFile, strDestFile, true);
                    Log.Info("UNCTools: UNCCopyFile: '{0}' to '{1}' ok!", strSourceFile, strDestFile);
                }
                else
                {
                    //UNC host is offline or file doesnt exists
                    Log.Warn("UNCTools: UNCCopyFile: '{0}' DOESNT exists or host is offline! File copy skipped!", strSourceFile);
                }
            }
            catch (Exception ex)
            {
                Log.Error("UNCTools: UNCCopyFile: exception on copy database: '{0}' to '{1}, ex:{2} stack:{3}", strSourceFile, strDestFile, ex.Message, ex.StackTrace);
            }
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Pings an given hostname
        /// </summary>
        private static int PingHost(string strHost_or_IP, int iTimeoutMilliseonds, int iCount)
        {
            //int which returns the answered packages
            int iAnswers = 0;

            //string to hold our return messge
            string returnMessage = string.Empty;

            //IPAddress instance for holding the returned host
            IPAddress address = AsyncDNSReverseLookup(strHost_or_IP);

            //check if we have an ipaddress
            if((strHost_or_IP == "") || (address == null))
            {
                Log.Debug("UNCTools: PingHost: Could not resolve/convert " + strHost_or_IP + " to an IPAddress object!");
                return 0;
            }

            //set the ping options, TTL 128
            PingOptions pingOptions = new PingOptions(128, true);

            //create a new ping instance
            Ping ping = new Ping();

            //32 byte buffer (create empty)
            byte[] buffer = new byte[32];

            //first make sure we actually have an internet connection
            if (HasConnection())
            {
                //here we will ping the host 4 times (standard)
                for (int i = 0; i < iCount; i++)
                {
                    try
                    {
                        //send the ping 4 times to the host and record the returned data.
                        //The Send() method expects 4 items:
                        //1) The IPAddress we are pinging
                        //2) The timeout value
                        //3) A buffer (our byte array)
                        //4) PingOptions
                        PingReply pingReply = ping.Send(address, iTimeoutMilliseonds, buffer, pingOptions);

                        //make sure we dont have a null reply
                        if (!(pingReply == null))
                        {
                            switch (pingReply.Status)
                            {
                                case IPStatus.Success:
                                    //Log.Debug(string.Format("UNCTools: PingHost: Reply from {0}: bytes={1} time={2}ms TTL={3}", pingReply.Address, pingReply.Buffer.Length, pingReply.RoundtripTime, pingReply.Options.Ttl));
                                    iAnswers++;
                                    break;
                                case IPStatus.TimedOut:
                                    Log.Debug("UNCTools: PingHost: Connection has timed out...");
                                    break;
                                default:
                                    Log.Debug(string.Format("PingHost: Ping failed: {0}", pingReply.Status.ToString()));
                                    break;
                            }
                        }
                        else
                            Log.Debug("UNCTools: PingHost: Connection failed for an unknown reason...");
                    }
                    catch (PingException ex)
                    {
                        Log.Debug(string.Format("UNCTools: PingHost: Connection Error: {0}", ex.Message));
                    }
                    catch (SocketException ex)
                    {
                        Log.Debug(string.Format("UNCTools: PingHost: Connection Error: {0}", ex.Message));
                    }
                }
            }
            else
                Log.Debug("UNCTools: PingHost: No Internet connection found...");

            //return the message
            return iAnswers;
        }

        /// <summary>
        /// method for retrieving the IP address from the host/IP string provided
        /// </summary>
        /// <param name="strHost_or_IP">the host/IP we need the IPAddress obj for</param>
        /// <returns></returns>
        private static IPAddress AsyncDNSReverseLookup(string strHost_or_IP)
        {
            var cts = new CancellationTokenSource();
            DateTime dt1;
            double iTimeDiff = 0;

            //IPAddress instance for holding the returned host
            IPAddress address = null;

            //try to get an ip address from the given string
            if (!IPAddress.TryParse(strHost_or_IP, out address))
            {
                //we have no IP address -> make an async dns reverse lookup

                //Start async dns reverse lookup
                try
                {
                    var t1 = Task.Factory.StartNew(_ => DnsReverseLookup(strHost_or_IP),
                                                        TaskCreationOptions.AttachedToParent)
                                         .TimeoutAfter(1000)
                                         .ContinueWith(antecedent =>
                                         {
                                             if (!(antecedent.IsCanceled || antecedent.IsFaulted))
                                                 address = antecedent.Result;
                                         }
                                    , cts.Token);

                    //Make Timestamp now
                    dt1 = DateTime.Now;

                    t1.Wait();

                    //Calc needed milliseconds
                    iTimeDiff = (DateTime.Now - dt1).TotalMilliseconds;
                }
                catch (Exception ex)
                {
                    Log.Debug("UNCTools: AsyncDNSReverseLookup: exception: " + ex.InnerException.Message);
                }
            }


            //is the result ok?
            if (address == null)
            {
                Log.Debug(string.Format("UNCTools: AsyncDNSReverseLookup: dns reverse lookup timeout (" + iTimeDiff.ToString() + " ms)!"));
            }
            else
            {
                Log.Debug(string.Format("UNCTools: AsyncDNSReverseLookup: ip '" + address.ToString() + "' resolved for host '" + strHost_or_IP + "' in " + iTimeDiff.ToString() + " ms"));
            }
            return address;
        }

        //DnsReverseLookup, makes a DnsReverseLookup, call this from a new task to make it faster
        private static IPAddress DnsReverseLookup(string strHost)
        {
            IPAddress ipAddressObj = null;
            try
            {
                ipAddressObj = Dns.GetHostEntry(strHost).AddressList[0];
            }
            catch
            {
            }
            return ipAddressObj;
        }

        /// <summary>
        /// method to check the status of the pinging machines internet connection
        /// </summary>
        /// <returns></returns>
        private static bool HasConnection()
        {
            //instance of our ConnectionStatusEnum
            ConnectionStatusEnum state = 0;

            //call the API
            InternetGetConnectedState(ref state, 0);

            //check the status, if not offline and the returned state
            //isnt 0 then we have a connection
            if (((int)ConnectionStatusEnum.INTERNET_CONNECTION_OFFLINE & (int)state) != 0)
            {
                //return true, we have a connection
                return false;
            }
            //return false, no connection available
            return true;
        }

        #endregion
    }
}
