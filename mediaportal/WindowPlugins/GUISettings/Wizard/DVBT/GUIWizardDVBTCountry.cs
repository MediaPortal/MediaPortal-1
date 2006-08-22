using System;
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using MediaPortal.GUI.Library;
namespace WindowPlugins.GUISettings.Wizard.DVBT
{
	/// <summary>
	/// Summary description for GUIWizardDVBTCountry.
	/// </summary>
	public class GUIWizardDVBTCountry : GUIWindow
	{
		[SkinControlAttribute(24)]			protected GUIListControl listCountries=null;
		public GUIWizardDVBTCountry()
		{
			GetID=(int)GUIWindow.Window.WINDOW_WIZARD_DVBT_COUNTRY;
		}
    
		public override bool Init()
		{
			return Load (GUIGraphicsContext.Skin+@"\wizard_tvcard_dvbt_country.xml");
		}
		protected override void OnPageLoad()
		{
			base.OnPageLoad ();
			LoadCountries();
		}

		void LoadCountries()
		{
			listCountries.Clear();
            XmlDocument doc = new XmlDocument();
            doc.Load(_config.Get(MediaPortal.Utils.Services.Config.Options.BasePath) + "Tuningparameters/dvbt.xml");
            XPathNavigator nav = doc.CreateNavigator();

            // Ensure we are at the root node
            nav.MoveToRoot();
            XPathExpression expr = nav.Compile("/dvbt/country");
            // Add an XSLT based sort
            expr.AddSort("@name", XmlSortOrder.Ascending, XmlCaseOrder.None, "", XmlDataType.Text);
            IEnumerator enumerator = nav.Select(expr).GetEnumerator();
            while (enumerator.MoveNext())
            {
                XPathNavigator nodeCountry = (XPathNavigator)enumerator.Current;
                XPathNavigator nameNode = nodeCountry.SelectSingleNode("@name");
                string name = nameNode.Value;
                GUIListItem item = new GUIListItem();
                item.IsFolder = false;
                item.Label = name;
                listCountries.Add(item);
            }
		}
		protected override void OnClicked(int controlId, GUIControl control, MediaPortal.GUI.Library.Action.ActionType actionType)
		{
			if (control==listCountries)
			{
				GUIListItem item=listCountries.SelectedListItem;
				DoScan(item.Label);
				GUIWindowManager.ActivateWindow((int)GUIWindow.Window.WINDOW_WIZARD_DVBT_SCAN);
				return;
			}
			base.OnClicked (controlId, control, actionType);
		}

		void DoScan(string country)
		{
			GUIPropertyManager.SetProperty("#WizardCountry",country);
		}
	}
}
