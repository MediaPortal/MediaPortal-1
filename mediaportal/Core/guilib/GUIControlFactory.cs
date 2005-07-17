using System;
using System.ComponentModel;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace MediaPortal.GUI.Library
{
	/// <summary>
	/// Creates new GUIControls based on Skin Xml data, and handles
	/// the skin references.xml file which contains default properties for most controls.
	/// </summary>
	public class GUIControlFactory
	{
		#region Constructors

		private GUIControlFactory() // NON-CREATABLE
		{
		}
		
		#endregion Constructors

		#region Methods

		public static void LoadReferences(string referenceFile)
		{
			try
			{		
				if (m_referenceNodesByControlType != null)
					return;
				Log.Write("  Loading references from {0}", referenceFile);
				m_referenceNodesByControlType = new Hashtable();
				XmlDocument doc = new XmlDocument();
				doc.Load(referenceFile);
				// Check the root element
				if (doc.DocumentElement == null || doc.DocumentElement.Name != "controls")
					return;

				ReadSkinSizeFromReferenceFile(doc);

				XmlNodeList list = doc.DocumentElement.SelectNodes("/controls/control");
				foreach (XmlNode controlNode in list)
				{
					if (GetControlType(controlNode) != null)
						m_referenceNodesByControlType[GetControlType(controlNode)] = controlNode;
				}
			}
			catch (Exception ex)
			{
				Log.Write("exception loading references {0} err:{1} stack:{2}",
					referenceFile, ex.Message, ex.StackTrace);
			}
		}

		
		/// <summary>
		/// Deletes all reference nodes from memory (Use this to change skins in runtime).
		/// </summary>
		public static void ClearReferences()
		{
			m_referenceNodesByControlType = null;
		}

		private static void ReadSkinSizeFromReferenceFile(XmlDocument doc)
		{
			GUIGraphicsContext.SkinSize = new Size(720, 576);
			XmlNode nodeSkinWidth = doc.DocumentElement.SelectSingleNode("/controls/skin/width/text()");
			XmlNode nodeSkinHeight = doc.DocumentElement.SelectSingleNode("/controls/skin/height/text()");
			if (nodeSkinWidth != null && nodeSkinHeight != null)
			{
				try
				{
					int iWidth = Convert.ToInt16(nodeSkinWidth.Value);
					int iHeight = Convert.ToInt16(nodeSkinHeight.Value);
					Log.Write("  original skin size:{0}x{1}", iWidth, iHeight);
					GUIGraphicsContext.SkinSize = new Size(iWidth, iHeight);
				}
				catch (FormatException) // Size values were invalid.
				{
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="guiControlType">The type of control you wish to update.</param>
		/// <returns>A hashtable which contains the FieldInfo objects for every
		/// updatable field, indexed by their corresponding Xml Element name. </returns>
		static Hashtable GetFieldsToUpdate(Type guiControlType)
		{
			// Lazy Initializiation...
			if (m_reflectionCacheByControlType.ContainsKey(guiControlType)) 
				return (Hashtable)m_reflectionCacheByControlType[guiControlType]; 
			
			Hashtable fieldsTable = new Hashtable();
			FieldInfo[] allFields = guiControlType.GetFields(
				BindingFlags.Instance 
				|BindingFlags.NonPublic
				|BindingFlags.FlattenHierarchy
				|BindingFlags.Public);
			foreach (FieldInfo field in allFields)
			{
				if (field.IsDefined(typeof(XMLSkinElementAttribute), false))
				{
					XMLSkinElementAttribute atrb = (XMLSkinElementAttribute)
						field.GetCustomAttributes(typeof(XMLSkinElementAttribute), false)[0];
					fieldsTable[atrb.XmlElementName] = field;
				}
			}
			m_reflectionCacheByControlType[guiControlType] = fieldsTable;
			return fieldsTable;
						
		}

		private static object ConvertXmlStringToObject(string valueName, string valueText, Type type)
		{
			try
			{
				try
				{
					if(type == typeof (string))
						return valueText;
				}
				catch (Exception) 
				{ 
					return String.Empty;
				}

				try
				{
					if(type == typeof(int) || type == typeof(long))
					{
						switch(valueName.ToLower())
						{
							case "textcolor":
							case "colorkey":
							case "colordiffuse":
								if(valueText.Length > 0)
								{
									// not very pretty but we can improve on this by requesting that RGB or ARGB
									// colors are specified with '#'
									if(valueText[0] != '#' && valueText.Length == 6 || valueText.Length == 8)
										valueText = '#' + valueText;
								}

								return ColorTranslator.FromHtml(valueText).ToArgb();
						}
					}

					if (type == typeof (int))
						return System.Int32.Parse(valueText);
					if (type == typeof (long))
						return System.Int64.Parse(valueText, NumberStyles.HexNumber);
				}
				catch(Exception)
				{
					return 0;
				}
				try
				{
					if (type == typeof (bool))
						if (valueText == "off" || valueText == "no" || valueText == "disabled") 
							return false;
						else 
							return true;
				}
				catch(Exception)
				{
					return false;
				}
				try
				{
					if (type == typeof (GUIControl.Alignment))
					{
						switch (valueText)
						{
							case "right" :
								return GUIControl.Alignment.ALIGN_RIGHT;
							case "center" :
								return GUIControl.Alignment.ALIGN_CENTER;
							default:
								return GUIControl.Alignment.ALIGN_LEFT;
						}
					}
				}
				catch(Exception)
				{
					return GUIControl.Alignment.ALIGN_LEFT;
				}

				try
				{
					if (type == typeof (GUIControl.eOrientation))
					{
						switch (valueText)
						{
							case "horizontal" :
								return GUIControl.eOrientation.Horizontal;
							case "vertical" :
								return GUIControl.eOrientation.Vertical;
							default:
								return GUIControl.eOrientation.Horizontal;
						}
					}
				}
				catch(Exception)
				{
					return GUIControl.Alignment.ALIGN_LEFT;
				}


				try
				{
					if (type == typeof (Animator.AnimationType))
					{
						switch (valueText.ToLower())
						{
							case "flyinfromleft":
								return Animator.AnimationType.FlyInFromLeft;
							case "flyinfromright":
								return Animator.AnimationType.FlyInFromRight;
							case "flyinfromtop":
								return Animator.AnimationType.FlyInFromTop;
							case "flyinfrombottom":
								return Animator.AnimationType.FlyInFromBottom;
							case "zoominfrommiddle":
								return Animator.AnimationType.ZoomInFromMiddle;
						}
					}
				}
				catch(Exception)
				{
					return Animator.AnimationType.FlyInFromLeft;
				}
				try
				{
					if (type == typeof(GUISpinControl.SpinType))
					{
						switch (valueText.ToLower())
						{
							case "int": 
								return GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
							case "float":
								return GUISpinControl.SpinType.SPIN_CONTROL_TYPE_FLOAT;
							default: 
								return GUISpinControl.SpinType.SPIN_CONTROL_TYPE_TEXT;
						}
					}
				}
				catch(Exception)
				{
					return GUISpinControl.SpinType.SPIN_CONTROL_TYPE_INT;
				}
			
				return null;
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static GUIControl Create(int dwParentId, XmlNode pControlNode, IDictionary defines)
		{
			Type typeOfControlToCreate = GetControlType(pControlNode);
			if (typeOfControlToCreate == null)
				return null;

			object[] ctorParams = { dwParentId };			
			GUIControl control = (GUIControl)
				Activator.CreateInstance(typeOfControlToCreate,ctorParams);
			
			XmlNode referenceNode = 
				(XmlNode) m_referenceNodesByControlType[typeOfControlToCreate];
			
			if (referenceNode != null)
				UpdateControlWithXmlData(control,typeOfControlToCreate, referenceNode, defines);
			
			UpdateControlWithXmlData(control,typeOfControlToCreate, pControlNode, defines);
			
			control.ScaleToScreenResolution();
			AddSubitemsToControl(pControlNode,control);
			control.FinalizeConstruction();
			

			if (typeOfControlToCreate == typeof(GUIGroup))
			{
				GUIGroup group = (GUIGroup) control;
				XmlNodeList nodeList = pControlNode.SelectNodes("control");
				foreach (XmlNode subControlNode in nodeList)
				{
					GUIControl subControl = Create(dwParentId, subControlNode, defines);
					group.AddControl(subControl);
				}
			}

			if (typeOfControlToCreate == typeof (GUIFacadeControl))
			{
				GUIFacadeControl facade = (GUIFacadeControl) control;
				XmlNodeList nodeList = pControlNode.SelectNodes("control");
				foreach (XmlNode subControlNode in nodeList)
				{
					GUIControl subControl = Create(dwParentId, subControlNode, defines);
					if (subControl is GUIListControl)
					{
						GUIListControl list = subControl as GUIListControl;
						if ( list.SubType=="album")
							facade.AlbumListView = list;
						else
							facade.ListView = list;
					}
					if (subControl is GUIThumbnailPanel)
						facade.ThumbnailView = subControl as GUIThumbnailPanel;
					if (subControl is GUIFilmstripControl)
						facade.FilmstripView = subControl as GUIFilmstripControl;
				}
			}

			return control;

		}

		private static void UpdateControlWithXmlData(GUIControl control, 
			Type controlType,
			XmlNode pControlNode, IDictionary defines)
		{
			Hashtable fieldsThatCanBeUpdated = GetFieldsToUpdate(controlType);
	
			XmlNodeList childNodes = pControlNode.ChildNodes;
			foreach (XmlNode element in childNodes)
			{
				FieldInfo correspondingField =
					fieldsThatCanBeUpdated[element.Name] as FieldInfo;

				if (correspondingField != null)
				{
					string text = element.InnerText;

					if(defines.Contains(text))
						text = (string)defines[text];

					object newValue = ConvertXmlStringToObject(element.Name, text,
						correspondingField.FieldType);
			
					try
					{
						correspondingField.SetValue(control, newValue);
					}
					catch (Exception e)
					{
						Log.Write("Couldn't place {0}, which is {1} in {2}. Exception:{3}",  
							newValue, newValue.GetType(), correspondingField,e) ;
					}

				}
			}

		}

		private static void AddSubitemsToControl(XmlNode subItemsNode, GUIControl control)
		{
			XmlNodeList subNodes=subItemsNode.SelectNodes("subitems/subitem/text()");
			foreach (XmlNode subNode in subNodes)
			{
				string strSubItem = subNode.Value;
				if (Char.IsDigit(strSubItem[0]))
				{
					GUILocalizeStrings.LocalizeLabel(ref strSubItem);
				}
				control.AddSubItem(strSubItem);
			}
		}

		private static Type GetControlType(XmlNode controlNode)
		{
			XmlNode typeText = controlNode.SelectSingleNode("type/text()");
			if (typeText == null || typeText.Value == "")
				return null;
			string xmlTypeName = typeText.Value;
			switch (xmlTypeName)
			{
				case ("image"):
					return typeof (GUIImage);
				case ("imagelist"):
					return typeof (GUIImageList);
				case ("slider"):
					return typeof (GUISliderControl);
				case ("fadelabel"):
					return typeof (GUIFadeLabel);
				case ("label"):
					return typeof (GUILabelControl);
				case ("button"):
					return typeof (GUIButtonControl);
				case ("updownbutton"):
					return typeof (GUIUpDownButton);
				case ("button3part"):
					return typeof (GUIButton3PartControl);
				case ("statusbar"):
					return typeof (GUIStatusbarControl);
				case ("progress"):
					return typeof (GUIProgressControl);
				case ("tvprogress"):
					return typeof (GUITVProgressControl);
				case ("hscrollbar"):
					return typeof (GUIHorizontalScrollbar);
				case ("vscrollbar"):
					return typeof (GUIverticalScrollbar);
				case ("textbox"):
					return typeof (GUITextControl);
				case ("textboxscrollup"):
					return typeof (GUITextScrollUpControl);
				case ("thumbnailpanel"):
					return typeof (GUIThumbnailPanel);
				case ("spincontrol"):
					return typeof (GUISpinControl);
				case ("checkmark"):
					return typeof (GUICheckMarkControl);
				case ("selectbutton"):
					return typeof (GUISelectButtonControl);
				case ("listcontrol"):
					return typeof (GUIListControl);
				case ("updownlistcontrol"):
					return typeof (GUIUpDownListControl);
				case ("checklistcontrol"):
					return typeof (GUICheckListControl);
				case ("togglebutton"):
					return typeof (GUIToggleButtonControl);
				case ("group"):
					return typeof (GUIGroup);
				case ("videowindow"):
					return typeof (GUIVideoControl);
				case ("facadeview"):
					return typeof (GUIFacadeControl);
				case ("filmstrip"):
					return typeof (GUIFilmstripControl);
				case ("smsinput"):
					return typeof (GUISMSInputControl);
				default:
					Type t = (Type)m_hashCustomControls[xmlTypeName];

					if(t == null)
					{
						Log.Write("ERROR: unknown control:<{0}>",xmlTypeName);
						return null;
					}

					return t;
			}
		}

		public static void RegisterControl(string strName, Type t)
		{
			m_hashCustomControls[strName] = t;
		}

		#endregion Methods

		#region Fields

		/// <summary>
		/// Contains all of the reference nodes, indexed by control Type.
		/// </summary>
		static Hashtable			m_referenceNodesByControlType = null;

		static Hashtable			m_hashCustomControls = new Hashtable();

		/// <summary>
		/// A hashtable which contains the reflection results for every control.
		/// </summary>
		static Hashtable m_reflectionCacheByControlType = new Hashtable(20);

		#endregion Fields
	}
}