using System;
using System.Collections.Generic;
using System.Text;

namespace WindowPlugins.VideoEditor
{
	enum EditType
	{
		Join,
		Cut,
		Convert,
		Compress,
	}
	class EditSettings
	{
		string fileName;
		object settings;
		bool deleteAfter;
		EditType type;

		public EditSettings(object setting)
		{
			this.settings = setting;
		}

		public object Settings
		{
			get
			{
				return settings;
			}
		}
		public string FileName
		{
			get
			{
				return fileName;
			}
			set
			{
				fileName = value;
			}
		}
		public bool DeleteAfter
		{
			get
			{
				return deleteAfter;
			}
			set
			{
				deleteAfter = value;
			}
		}
		public EditType Type
		{
			get
			{
				return type;
			}
			set
			{
				type = value;
			}
		}
	}
}
