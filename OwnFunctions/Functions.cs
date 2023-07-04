#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace OwnFunctions
{
	public static class Functions
	{
		/// <summary>
		/// return sinus dependent on x and z direction
		/// </summary>
		/// <param name="x"></param>
		/// <param name="z"></param>
		/// <returns>2-dimensional sinus</returns>
		public static double Sin2D(double x, double z)
		{
			return Math.Sin(x) + Math.Sin(z);
		}

		public static double Fun(double x, double z, double t)
		{
			return Math.Sin(x * x + t) + Math.Cos(z * x + t);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="x"></param>
		/// <returns>positive part of x</returns>
		public static double Pos(double x)
		{
			if (x > 0)
			{
				return x;
			}
			else
				return -x;
		}
	}
}
