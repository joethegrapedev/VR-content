using System;
using UnityEngine;

namespace Autohand
{
	public class AutoHeaderAttribute : PropertyAttribute
	{
		public int count;

		public int depth;

		public string label;

		public string tooltip;

		public string toggleBool;

		public Type type;

		public AutoHeaderAttribute(string label, int count = 0, int depth = 0)
		{
			this.count = count;
			this.depth = depth;
			this.label = label;
		}

		public AutoHeaderAttribute(string label, string tooltip, string toggleName, Type classType, int count = 0, int depth = 0)
		{
			this.count = count;
			this.depth = depth;
			this.label = label;
			this.tooltip = tooltip;
		}
	}
}
