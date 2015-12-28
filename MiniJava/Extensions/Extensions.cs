using System;
using System.Collections.Generic;

namespace MiniJava
{
	public static class Extensions {

		public static HashSet<T> ToHashSet<T>(this IEnumerable<T> items)
		{
			return new HashSet<T>(items);
		}
	}
		
}

