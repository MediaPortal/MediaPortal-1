/*
 * The business object used for testing validations.
 * Copyright (C) 2005 Clayton Harbour
 * 
 * This library is free software; you can redistribute it and/or modify it 
 * under the terms of the GNU Lesser General Public License 2.1 or later, as
 * published by the Free Software Foundation. See the included License.txt
 * or http://www.gnu.org/copyleft/lesser.html for details.
 *
 * $Id: Numbers.cs $
 */

using System;

namespace Gentle.Framework.Tests
{
	/// <summary>
	/// Summary description for ManyNumbers.
	/// </summary>
	[TableName( "PropertyHolder" )]
	public class Numbers : Persistent
	{
		private Int16 nInt16;
		private Int32 nInt32;
		private int nInt;
		private Int64 nInt64;
		private long nLong;
		private float nFloat;
		private Double nDblO;
		private double nDouble;

		[RangeValidator( Min = 20, Max = 100 )]
		public Int16 NInt16
		{
			get { return nInt16; }
			set { nInt16 = value; }
		}
		[RangeValidator( Max = 100 )]
		public Int32 NInt32
		{
			get { return nInt32; }
			set { nInt32 = value; }
		}
		[RangeValidator( Min = 20, Max = 100 )]
		public int NInt
		{
			get { return nInt; }
			set { nInt = value; }
		}
		[RangeValidator( Min = 100 )]
		public Int64 NInt64
		{
			get { return nInt64; }
			set { nInt64 = value; }
		}
		[RangeValidator( Min = 20, Max = 100 )]
		public long NLong
		{
			get { return nLong; }
			set { nLong = value; }
		}
		[RangeValidator( Min = 20.3, Max = 100.3 )]
		public float NFloat
		{
			get { return nFloat; }
			set { nFloat = value; }
		}
		[RangeValidator( Max = 100.4 )]
		public Double NDblO
		{
			get { return nDblO; }
			set { nDblO = value; }
		}
		[RangeValidator( Min = 20.5, Max = 100.5 )]
		public double NDouble
		{
			get { return nDouble; }
			set { nDouble = value; }
		}
	}
}