using System;
using System.Text;

namespace MediaPortal.Player
{
	/// <summary>
	/// Summary description for IExternalPlayer.
	/// </summary>
	public abstract class IExternalPlayer : IPlayer, MediaPortal.GUI.Library.ISetupForm
	{
        private bool m_enabled = false;
 
        public bool Enabled
        {
          get
          {
            return m_enabled;
          }
          set
          {
            m_enabled = value;
          }
        }

        #region ISetupForm Members
        public string PluginName()
        {
          return PlayerName;
        }
		    public bool DefaultEnabled()
		    {
			    return false;
		    }
        public virtual string Description()
        {
          string[] exts = GetAllSupportedExtensions();
          StringBuilder strExts = new StringBuilder();
          strExts.Append("External Player for: ");
          for(int i = 0; i < exts.Length; i++)
          {
            if(i > 0)
              strExts.Append(',');
            strExts.Append(exts[i]);
          }
          return strExts.ToString();
        }
      
        public string Author()
        {
          return AuthorName;
        }

        public virtual void ShowPlugin()
        {
          ; //nothing to show
        }

        public bool HasSetup()
        {
          return true;
        }
        public bool CanEnable()
        {
          return true;
        }

        public virtual int GetWindowId()
        {
          return -1;
        }

        public virtual bool  GetHome(out string strButtonText, out string strButtonImage, 
                                     out string strButtonImageFocus, out string strPictureImage)
        {
          strButtonText="";
          strButtonImage="";
          strButtonImageFocus="";
          strPictureImage="";
          return false;
        }
        #endregion

        /// <summary>
        /// This method returns the name of the external player
        /// </summary>
        /// <returns>string representing the name of the external player</returns>
        public abstract string PlayerName
        {
          get;
        }

        /// <summary>
        /// This method returns the version number of the plugin
        /// </summary>
        public abstract string VersionNumber
        {
          get;
        }

        /// <summary>
        /// This method returns the author of the external player
        /// </summary>
        /// <returns></returns>
        public abstract string AuthorName
        {
          get;
        }

        /// <summary>
        /// Returns all the extensions that the external player supports.  
        /// The return value is an array of extensions of the form: .wma, .mp3, etc...
        /// </summary>
        /// <returns>array of strings of extensions in the form: .wma, .mp3, etc..</returns>
        public abstract string[] GetAllSupportedExtensions();

        /// <summary>
        /// Returns true or false depending if the filename passed is supported or not.
        /// The filename could be just the filename or the complete path of a file.
        /// </summary>
        /// <param name="filename">a fully qualified path and filename or just the filename</param>
        /// <returns>true or false if the file is supported by the player</returns>
        public abstract bool SupportsFile(string filename);
	}
}
