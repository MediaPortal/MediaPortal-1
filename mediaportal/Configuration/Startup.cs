using System;
using System.Windows.Forms;

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

		/// <summary>
		/// 
		/// </summary>
		public void Start()
		{
			Form applicationForm = null;

			switch(startupMode)
			{
				case StartupMode.Normal:
					applicationForm = new SettingsForm();
					break;

				case StartupMode.Wizard:
					applicationForm = new WizardForm(sectionsConfiguration);
					break;
			}

      // One time setup for proxy servers
      // also set credentials to allow use with firewalls that require them
      // this means we can use the .NET internet objects and not have
      // to worry about proxies elsewhere in the code
      System.Net.WebProxy proxy = System.Net.WebProxy.GetDefaultProxy();
      proxy.Credentials  = System.Net.CredentialCache.DefaultCredentials;
      System.Net.GlobalProxySelection.Select = proxy;

			if(applicationForm != null)
			{
				Application.Run(applicationForm);
			}
		}

		[STAThread]
		public static void Main(string[] arguments)
		{
			try
			{
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
