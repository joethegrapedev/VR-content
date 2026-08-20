using UnityEngine;
using UnityEngine.Serialization;

namespace Autohand
{
	[DefaultExecutionOrder(1000)]
	public class HandTeleportGuard : MonoBehaviour
	{
		[Header("Helps prevent hand from passing through static collision boundries")]
		public Hand hand;

		[Header("Guard Settings")]
		[Tooltip("The mask of things the guarding will ignore, if left on default or empty, will default to ignoring recommended Auto Hand layers")]
		public LayerMask ignoreMask;

		[Tooltip("The amount of distance change required in one frame or fixed udpate to activate the teleport guard")]
		public float buffer = 0.1f;

		[Tooltip("Whether this should always run or only run when activated by the teleporter")]
		public bool alwaysRun;

		[Tooltip("If true hands wont teleport return when past the max distance if something is in the way")]
		[FormerlySerializedAs("strict")]
		public bool ignoreMaxHandDistance;

		private Vector3 deltaHandPos;

		private Vector3 deltaHandFixedPos;

		private void Awake()
		{
			if (hand == null && (bool)GetComponent<Hand>())
			{
				hand = GetComponent<Hand>();
			}
			if ((int)ignoreMask == 0)
			{
				ignoreMask = LayerMask.GetMask(Hand.grabbableLayerNameDefault, Hand.grabbingLayerName, Hand.rightHandLayerName, Hand.leftHandLayerName, "HandPlayer");
			}
			else
			{
				ignoreMask = (int)ignoreMask | LayerMask.GetMask(Hand.rightHandLayerName, Hand.leftHandLayerName);
			}
		}

		private void Update()
		{
			if (!(hand == null) && hand.gameObject.activeInHierarchy && alwaysRun)
			{
				float num = Vector3.Distance(hand.palmTransform.position, deltaHandPos);
				if ((ignoreMaxHandDistance || (!ignoreMaxHandDistance && num < hand.maxFollowDistance)) && num > buffer)
				{
					TeleportProtection(deltaHandPos, hand.palmTransform.position);
				}
				deltaHandPos = hand.palmTransform.position;
			}
		}

		private void FixedUpdate()
		{
			if (!(hand == null) && hand.gameObject.activeInHierarchy && alwaysRun)
			{
				float num = Vector3.Distance(hand.palmTransform.position, deltaHandFixedPos);
				if ((ignoreMaxHandDistance || (!ignoreMaxHandDistance && num < hand.maxFollowDistance)) && num > buffer)
				{
					TeleportProtection(deltaHandFixedPos, hand.palmTransform.position);
				}
				deltaHandFixedPos = hand.palmTransform.position;
			}
		}

		public void TeleportProtection(Vector3 fromPos, Vector3 toPos)
		{
			if (hand == null || hand.transform == null)
			{
				return;
			}
			RaycastHit[] array = Physics.RaycastAll(fromPos, toPos - fromPos, Vector3.Distance(fromPos, toPos), ~(int)ignoreMask);
			Vector3 vector = Vector3.zero;
			RaycastHit[] array2 = array;
			foreach (RaycastHit raycastHit in array2)
			{
				if (raycastHit.transform != hand.transform)
				{
					vector = fromPos;
					break;
				}
			}
			if (vector != Vector3.zero)
			{
				hand.SetHandLocation(vector, hand.transform.rotation);
			}
		}
	}
}
