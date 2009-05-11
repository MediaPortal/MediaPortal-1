#region Copyright (C) 2005-2008 Team MediaPortal

/* 
 *	Copyright (C) 2005-2008 Team MediaPortal
 *	http://www.team-mediaportal.com
 *
 *  This Program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2, or (at your option)
 *  any later version.
 *   
 *  This Program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 *  GNU General Public License for more details.
 *   
 *  You should have received a copy of the GNU General Public License
 *  along with GNU Make; see the file COPYING.  If not, write to
 *  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA. 
 *  http://www.gnu.org/copyleft/gpl.html
 *
 */

#endregion

using System;
using System.Drawing;
using System.ComponentModel;
using System.Globalization;

namespace MediaPortal.Drawing
{
	public class BrushConverter : TypeConverter
	{
		#region Methods

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type t)
		{
			if(t == typeof(string))
				return true;

			if(t == typeof(Brush))
				return true;

			return base.CanConvertFrom(context, t);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if(value is Brush)
				return value;

			if(value is string)
			{
				StringTokenizer tokens = new StringTokenizer((string)value);
			
				if(tokens.Count == 1)
					return ParseSolidColor(tokens[0]);

				if(tokens.Count == 0)
					return base.ConvertFrom(context, culture, value);

				// http://winfx.msdn.microsoft.com/library/default.asp?url=/library/en-us/cpref/html/C_System_Windows_Media_LinearGradientBrush_ctor_2_ecb3b488.asp

				if(string.Compare(tokens[0], "HorizontalGradient", true) == 0)
					return ParseHorizontalGradient(tokens);

				if(string.Compare(tokens[0], "LinearGradient", true) == 0)
					return ParseLinearGradient(tokens);

				if(string.Compare(tokens[0], "VerticalGradient", true) == 0)
					return ParseVerticalGradient(tokens);

				throw new NotImplementedException();
			}

			return base.ConvertFrom(context, culture, value);
		}

		private Brush ParseHorizontalGradient(StringTokenizer tokens)
		{
			if(tokens.Count != 3)
				throw new ArithmeticException("Expecting 'HorizontalGradient StartColor EndColor'");

			Color sc = Color.FromName(tokens[1]);

			if(sc.A == 0 && sc.R == 0 && sc.G == 0 && sc.B == 0)
				sc = ColorTranslator.FromHtml(tokens[1]);

			Color ec = Color.FromName(tokens[2]);

			if(ec.A == 0 && ec.R == 0 && ec.G == 0 && ec.B == 0)
				ec = ColorTranslator.FromHtml(tokens[2]);

//			return new LinearGradientBrush(sc, ec, new Point(0, 0.5), new Point(1, 0.5));
			throw new NotImplementedException();
		}

		private Brush ParseLinearGradient(StringTokenizer tokens)
		{
			if(tokens.Count != 5)
				throw new ArithmeticException("Expecting 'LinearGradient StartPoint EndPoint StartColor EndColor'");

			Color sc = Color.FromName(tokens[3]);

			if(sc.A == 0 && sc.R == 0 && sc.G == 0 && sc.B == 0)
				sc = ColorTranslator.FromHtml(tokens[3]);

			Color ec = Color.FromName(tokens[4]);

			if(ec.A == 0 && ec.R == 0 && ec.G == 0 && ec.B == 0)
				ec = ColorTranslator.FromHtml(tokens[4]);

			string[] spxy = tokens[1].Split(',');
			string[] epxy = tokens[2].Split(',');

			Point sp = new Point(Convert.ToDouble(spxy[0], CultureInfo.CurrentCulture), Convert.ToDouble(spxy[1], CultureInfo.CurrentCulture));
			Point ep = new Point(Convert.ToDouble(epxy[0], CultureInfo.CurrentCulture), Convert.ToDouble(epxy[1], CultureInfo.CurrentCulture));

			throw new NotImplementedException();
//			return new LinearGradientBrush(sc, ec, sp, ep);
		}

		private Brush ParseSolidColor(string text)
		{
			Color color = Color.FromName(text);

			if(color.A == 0 && color.R == 0 && color.G == 0 && color.B == 0)
				return new SolidColorBrush(ColorTranslator.FromHtml(text));

			return new SolidColorBrush(color);
		}

		private Brush ParseVerticalGradient(StringTokenizer tokens)
		{
			if(tokens.Count != 3)
				throw new ArithmeticException("Expecting 'VerticalGradient UpperColor LowerColor'");

			Color uc = Color.FromName(tokens[1]);

			if(uc.A == 0 && uc.R == 0 && uc.G == 0 && uc.B == 0)
				uc = ColorTranslator.FromHtml(tokens[1]);

			Color lc = Color.FromName(tokens[2]);

			if(lc.A == 0 && lc.R == 0 && lc.G == 0 && lc.B == 0)
				lc = ColorTranslator.FromHtml(tokens[2]);

			throw new NotImplementedException();
//			return new LinearGradientBrush(uc, lc, new Point(0.5, 0), new Point(0.5, 1));
		}

		#endregion Methods
	}
}
