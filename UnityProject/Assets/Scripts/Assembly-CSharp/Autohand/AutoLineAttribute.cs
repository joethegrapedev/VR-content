using System;
using UnityEngine;

namespace Autohand
{
	public class AutoLineAttribute : PropertyAttribute
	{
		public int count;

		public int depth;

		public string tooltip;

		public string toggleBool;

		public Type type;

		public AutoLineAttribute(int count = 0, int depth = 0)
		{
			this.count = count;
			this.depth = depth;
		}

		public AutoLineAttribute(string tooltip, string toggleName, Type classType, int count = 0, int depth = 0)
		{
			this.count = count;
			this.depth = depth;
			this.tooltip = tooltip;
		}
	}
}
