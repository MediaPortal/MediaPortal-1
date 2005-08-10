using System;
using System.Collections;

namespace MediaPortal.Dispatcher
{
	internal class JobComparer : IComparer
	{
		#region Methods

		public int Compare(object l, object r)
		{
			if(l is Job == false)
				throw new ArgumentException("argument l is not of type Job.");

			if(r is Job == false)
				throw new ArgumentException("argument r is not of type Job.");

			return Compare((Job)l, (Job)r);
		}

		public int Compare(Job l, Job r)
		{
			return DateTime.Compare(l.Next, r.Next);
		}

		#endregion Methods
	}
}