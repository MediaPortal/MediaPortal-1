using System;
using System.Windows.Forms;
using MediaPortal.GUI.Library;
namespace MediaPortal.Configuration
{
	/// <summary>
	/// Summary description for Startup.
	/// </summary>
	public class Startup
	{
		enum StartupMode
		{
			Normal,
			Wizard
		}
		StartupMode startupMode = StartupMode.Normal;

		string sectionsConfiguration = String.Empty;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="arguments"></param>
		public Startup(string[] arguments)
		{
			if (!System.IO.File.Exists("mediaportal.xml"))
				startupMode = StartupMode.Wizard;
																										
			else if (arguments!=null)
			{
				foreach(string argument in arguments)
				{
					string trimmedArgument = argument.ToLower();

					if(trimmedArgument.StartsWith("/wizard"))
					{
						startupMode = StartupMode.Wizard;
					}

					if(trimmedArgument.StartsWith("/section"))
					{
						string[] subArguments = argument.Split('=');

						if(subArguments.Length >= 2)
						{
							sectionsConfiguration = subArguments[1];
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void Start()
		{
			Form applicationForm = null;

			switch(startupMode)
			{
				case StartupMode.Normal:
          Log.Write("Create new standard setup");
					applicationForm = new SettingsForm();
					break;

        case StartupMode.Wizard:
          Log.Write("Create new wizard setup");
					applicationForm = new WizardForm(sectionsConfiguration);
					break;
			}

      
      Log.Write("determine proxy servers");
      // One time setup for proxy servers
      // also set credentials to allow use with firewalls that require them
      // this means we can use the .NET internet objects and not have
      // to worry about proxies elsewhere in the code
      System.Net.WebProxy proxy = System.Net.WebProxy.GetDefaultProxy();
      proxy.Credentials  = System.Net.CredentialCache.DefaultCredentials;
      System.Net.GlobalProxySelection.Select = proxy;

			if(applicationForm != null)
			{
        
        Log.Write("start application");
				Application.Run(applicationForm);
			}
		}

		[STAThread]
		public static void Main(string[] arguments)
		{
			try
			{

				try
				{
					System.IO.Directory.CreateDirectory(@"thumbs");
					System.IO.Directory.CreateDirectory(@"thumbs\music");
					System.IO.Directory.CreateDirectory(@"thumbs\music\albums");
					System.IO.Directory.CreateDirectory(@"thumbs\music\artists");
					System.IO.Directory.CreateDirectory(@"thumbs\music\genre");
					System.IO.Directory.CreateDirectory(@"thumbs\pictures");
					System.IO.Directory.CreateDirectory(@"thumbs\radio");
					System.IO.Directory.CreateDirectory(@"thumbs\tv");
					System.IO.Directory.CreateDirectory(@"thumbs\tv\logos");
					System.IO.Directory.CreateDirectory(@"thumbs\tv\shows");
					System.IO.Directory.CreateDirectory(@"thumbs\videos");
					System.IO.Directory.CreateDirectory(@"thumbs\videos\genre");
					System.IO.Directory.CreateDirectory(@"thumbs\videos\title");
					System.IO.Directory.CreateDirectory(@"thumbs\videos\actors");
				}
				catch(Exception){}

				AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
				Application.EnableVisualStyles();
				Application.DoEvents();

					new Startup(arguments).Start();
			}
			finally
			{
				GC.Collect();
			}
		}

		private static System.Reflection.Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			if(args.Name.IndexOf(".resources") < 0)
			{
				MessageBox.Show("Failed to locate assembly '" + args.Name + "'." + Environment.NewLine + "Note that the configuration program must be executed from/reside in the MediaPortal folder, the execution will now end.", "MediaPortal", MessageBoxButtons.OK, MessageBoxIcon.Error);
				Application.Exit();
			}
				
			return null;
		}
	}
}
