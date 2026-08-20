using UnityEngine;

namespace Autohand
{
	[DefaultExecutionOrder(1)]
	public class GrabbableChild : MonoBehaviour
	{
		public Grabbable grabParent;

		private void Start()
		{
			grabParent.SetGrabbableChild(this);
			if (base.gameObject.layer == LayerMask.NameToLayer("Default") || LayerMask.LayerToName(base.gameObject.layer) == "")
			{
				base.gameObject.layer = LayerMask.NameToLayer(Hand.grabbableLayerNameDefault);
			}
		}
	}
}
