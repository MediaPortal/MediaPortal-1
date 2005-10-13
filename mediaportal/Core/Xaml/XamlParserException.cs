using System;
using System.Xml;

namespace MediaPortal.Xaml
{
	public class XamlParserException : Exception
	{
		#region Constructors

		public XamlParserException(string message, string filename, XmlTextReader reader) : base(string.Format("{0}({1},{2}): {3}", filename, reader.LineNumber, reader.LinePosition, message))
		{
			_lineNumber = reader.LineNumber;
			_linePosition = reader.LinePosition;
		}

		#endregion Constructors

		#region Properties

		public int LineNumber
		{
			get { return _lineNumber; }
		}

		public int LinePosition
		{
			get { return _linePosition; }
		}

		#endregion Properties

		#region Fields

		int							_lineNumber;
		int							_linePosition;

		#endregion Fields
	}
}
