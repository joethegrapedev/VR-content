using UnityEngine;

namespace Autohand.Demo
{
	public class CubeBreak : MonoBehaviour
	{
		public float force = 10f;

		private Vector3[] offsets = new Vector3[8]
		{
			new Vector3(0.25f, 0.25f, 0.25f),
			new Vector3(-0.25f, 0.25f, 0.25f),
			new Vector3(0.25f, 0.25f, -0.25f),
			new Vector3(-0.25f, 0.25f, -0.25f),
			new Vector3(0.25f, -0.25f, 0.25f),
			new Vector3(-0.25f, -0.25f, 0.25f),
			new Vector3(0.25f, -0.25f, -0.25f),
			new Vector3(-0.25f, -0.25f, -0.25f)
		};

		[ContextMenu("Break")]
		public void Break()
		{
			for (int i = 0; i < 8; i++)
			{
				GameObject gameObject = Object.Instantiate(base.gameObject, base.transform.position, base.transform.rotation);
				FixedJoint[] components = gameObject.GetComponents<FixedJoint>();
				for (int j = 0; j < components.Length; j++)
				{
					Object.Destroy(components[j]);
				}
				try
				{
					gameObject.transform.parent = base.transform;
				}
				catch
				{
				}
				gameObject.transform.localPosition += offsets[i];
				gameObject.transform.parent = null;
				gameObject.transform.localScale = base.transform.localScale / 2f;
				gameObject.layer = LayerMask.NameToLayer(Hand.grabbableLayerNameDefault);
				Rigidbody component = gameObject.GetComponent<Rigidbody>();
				component.ResetCenterOfMass();
				component.ResetInertiaTensor();
				component.velocity = GetComponent<Rigidbody>().velocity;
				component.AddRelativeForce(base.transform.rotation * (offsets[i] * force), ForceMode.Impulse);
				component.AddRelativeTorque(base.transform.rotation * (offsets[i] * force + Vector3.one * (Random.value / 3f)), ForceMode.Impulse);
				component.mass /= 2f;
				gameObject.GetComponent<Grabbable>().jointBreakForce /= 2f;
				if (gameObject.transform.localScale.x < 0.03f)
				{
					gameObject.GetComponent<Grabbable>().singleHandOnly = true;
				}
			}
			Object.Destroy(base.gameObject);
		}
	}
}
