using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

namespace Calandiel.Utilities
{
	public static class TimeUtility
	{
		private static Stopwatch sw;
		public static bool ShouldRestart()
		{
			if(sw == null)
			{
				sw = new Stopwatch();
				sw.Start();
			}
			if(sw.ElapsedMilliseconds > 16)
			{
				sw.Restart();
				return true;
			}
			return false;
		}
	}
}