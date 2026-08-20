using NaughtyAttributes;
using UnityEngine;

namespace Autohand
{
	public class HandProjector : MonoBehaviour
	{
		[Header("References")]
		public Hand hand;

		[Tooltip("This should be a copy of the hand with the desired visual setup for your projection hand")]
		public Hand handProjection;

		[Tooltip("The Object(s) under your Hand that contain the MeshRenderer Component(s)")]
		public Transform[] handProjectionVisuals;

		[Tooltip("Smoothing speed, turning too high could cause jitters")]
		public float speed = 15f;

		[Tooltip("If true everything in the hand Vvisuals will be disabled/hidden when projection hand is showing")]
		public bool hideHand;

		[ShowIf("hideHand")]
		[Tooltip("The Object(s) under your main hand (not the projection hand) that contain the MeshRenderer Component(s)")]
		public Transform[] handVisuals;

		[Tooltip("Should the projection interpolate between the hand pose and the projected grab pose based on the grip input axis")]
		public bool useGrabTransition;

		[EnableIf("useGrabTransition")]
		[Tooltip("This offsets the grab transistion by this percent when active [0-1 range]")]
		public float grabTransitionOffset;

		[EnableIf("useGrabTransition")]
		[Tooltip("This sets the position of the hand based on its [(gripAxis + grabTransitionOffset) * grabDistanceMultiplyer] -> gripAxis is set on the HandControllerLink component on the main hand")]
		public float grabDistanceMultiplyer = 2f;

		[Tooltip("This sets the pose of the hand based on its [(gripAxis + grabTransitionOffset) * grabDistanceMultiplyer] -> gripAxis is set on the HandControllerLink component on the main hand")]
		[EnableIf("useGrabTransition")]
		public float grabTransitionMultiplyer = 2f;

		[DisableIf("useGrabTransition")]
		[Tooltip("This offsets the highlight by this percent when active [0-1 range]")]
		public float grabPercent = 1f;

		[Header("Events")]
		public UnityHandGrabEvent OnStartProjection;

		public UnityHandGrabEvent OnEndProjection;

		private HandPoseData lastProjectionPose;

		private HandPoseData newProjectionPose;

		private Vector3 lastProjectionPosition;

		private Quaternion lastProjectionRotation;

		private Grabbable target;

		private float startMass;

		private float minGrabTime;

		private float currAmount;

		private bool tryingGrab;

		private void OnEnable()
		{
			if (handProjection.body == null)
			{
				handProjection.body = handProjection.GetComponent<Rigidbody>();
			}
			if (hand.body == null)
			{
				hand.body = hand.GetComponent<Rigidbody>();
			}
			handProjection.body.detectCollisions = false;
			handProjection.body.mass = 0f;
			handProjection.enableMovement = false;
			handProjection.usingHighlight = false;
			handProjection.disableIK = true;
			handProjection.followPositionStrength = 0f;
			handProjection.followRotationStrength = 0f;
			handProjection.swayStrength = 0f;
			handProjection.disableIK = true;
			handProjection.usingHighlight = false;
			handProjection.usingPoseAreas = false;
			startMass = hand.body.mass;
			minGrabTime = hand.minGrabTime;
			lastProjectionPosition = hand.transform.localPosition;
			lastProjectionRotation = hand.transform.localRotation;
			lastProjectionPose = hand.GetHandPose();
			hand.OnBeforeGrabbed += OnBeforeGrab;
			hand.OnGrabbed += OnGrab;
			hand.OnBeforeReleased += OnRelease;
			hand.OnTriggerGrab += OnTriggerGrab;
		}

		private void OnDisable()
		{
			ShowProjection(show: false);
			hand.OnBeforeGrabbed -= OnBeforeGrab;
			hand.OnGrabbed -= OnGrab;
			hand.OnBeforeReleased -= OnRelease;
			hand.OnTriggerGrab -= OnTriggerGrab;
		}

		private void OnTriggerGrab(Hand hand, Grabbable grab)
		{
			tryingGrab = true;
		}

		private void OnBeforeGrab(Hand hand, Grabbable grab)
		{
			if (hideHand)
			{
				lastProjectionPose.SetFingerPose(hand);
				hand.transform.position = handProjection.transform.position;
				hand.transform.rotation = handProjection.transform.rotation;
				hand.body.position = hand.transform.position;
				hand.body.rotation = hand.transform.rotation;
				hand.minGrabTime = 0f;
			}
			ShowProjection(show: false);
		}

		private void OnGrab(Hand hand, Grabbable grab)
		{
			if (useGrabTransition)
			{
				hand.minGrabTime = minGrabTime;
			}
		}

		private void OnRelease(Hand hand, Grabbable grab)
		{
			lastProjectionPose = hand.GetHandPose();
			lastProjectionPose.SetFingerPose(handProjection);
			lastProjectionPosition = hand.transform.localPosition;
			lastProjectionRotation = hand.transform.localRotation;
			handProjection.transform.position = hand.transform.position;
			handProjection.transform.rotation = hand.transform.rotation;
			handProjection.body.position = handProjection.transform.position;
			handProjection.body.rotation = handProjection.transform.rotation;
			if (hideHand)
			{
				hand.minGrabTime = minGrabTime;
				for (int i = 0; i < handProjection.fingers.Length; i++)
				{
					handProjection.fingers[i].SetCurrentFingerBend(hand.fingers[i].GetLastHitBend());
				}
			}
		}

		private void LateUpdate()
		{
			if (tryingGrab && hand.GetTriggerAxis() < 0.35f)
			{
				tryingGrab = false;
			}
			SetTarget(hand.lookingAtObj);
			ShowProjection(IsProjectionActive());
		}

		private void OnProjectionStart(Hand projectionHand, Grabbable target)
		{
			OnStartProjection?.Invoke(projectionHand, target);
		}

		private void OnProjectionEnd(Hand projectionHand, Grabbable target)
		{
			OnEndProjection?.Invoke(projectionHand, target);
		}

		private void ShowProjection(bool show)
		{
			for (int i = 0; i < handProjectionVisuals.Length; i++)
			{
				handProjectionVisuals[i].gameObject.SetActive(show);
			}
			if (hideHand)
			{
				for (int j = 0; j < handVisuals.Length; j++)
				{
					handVisuals[j].gameObject.SetActive(!show);
				}
				if (show)
				{
					hand.body.mass = 0f;
				}
				else
				{
					hand.body.mass = startMass;
				}
			}
			if (show)
			{
				RaycastHit highlightHit = hand.GetHighlightHit();
				if (highlightHit.collider != null)
				{
					if (!hand.CanGrab(target))
					{
						ShowProjection(show: false);
						return;
					}
					float num = (useGrabTransition ? hand.grabCurve.Evaluate(hand.GetTriggerAxis() * grabTransitionMultiplyer + grabTransitionOffset) : grabPercent);
					currAmount = Mathf.MoveTowards(currAmount, num, Time.deltaTime * speed);
					float num2 = Mathf.Lerp(speed, speed / 4f, hand.GetTriggerAxis());
					if (hideHand)
					{
						hand.body.mass = Mathf.Lerp(startMass, 0f, Mathf.Pow(num * 2f, 2f));
					}
					handProjection.transform.localPosition = hand.transform.localPosition;
					handProjection.transform.localRotation = hand.transform.localRotation;
					GrabbablePose grabPose;
					if ((bool)(grabPose = handProjection.GetGrabPose(highlightHit.collider.transform, target)))
					{
						grabPose.SetHandPose(handProjection, isProjection: true);
					}
					else
					{
						handProjection.transform.position -= handProjection.palmTransform.forward * 0.08f;
						handProjection.body.position = handProjection.transform.position;
						handProjection.AutoPose(highlightHit, target);
					}
					newProjectionPose = handProjection.GetHandPose();
					Vector3 b;
					Quaternion b2;
					if (useGrabTransition && (target.grabType == HandGrabType.GrabbableToHand || (target.grabType == HandGrabType.Default && hand.grabType == GrabType.GrabbableToHand)))
					{
						b = hand.transform.localPosition;
						b2 = hand.transform.localRotation;
					}
					else
					{
						b = Vector3.Lerp(hand.transform.localPosition, handProjection.transform.localPosition, currAmount * grabDistanceMultiplyer);
						b2 = Quaternion.Lerp(hand.transform.localRotation, handProjection.transform.localRotation, currAmount * grabDistanceMultiplyer);
					}
					if (grabPose == null)
					{
						Finger[] fingers = handProjection.fingers;
						foreach (Finger obj in fingers)
						{
							obj.SetFingerBend(Mathf.Clamp01(obj.GetLastHitBend() - 0.1f));
						}
					}
					else
					{
						Finger[] fingers = handProjection.fingers;
						for (int k = 0; k < fingers.Length; k++)
						{
							fingers[k].SetFingerBend(handProjection.gripOffset);
						}
					}
					HandPoseData to = HandPoseData.LerpPose(hand.GetHandPose(), newProjectionPose, Mathf.Clamp01(currAmount - 0.33f) * 1.5f);
					HandPoseData.LerpPose(lastProjectionPose, to, speed * Time.deltaTime).SetFingerPose(handProjection);
					if (hand.GetTriggerAxis() > 0.1f || !hideHand)
					{
						handProjection.transform.localPosition = Vector3.Lerp(lastProjectionPosition, b, num2 * Time.deltaTime);
						handProjection.transform.localRotation = Quaternion.Lerp(lastProjectionRotation, b2, num2 * Time.deltaTime);
					}
					else
					{
						handProjection.transform.localPosition = hand.transform.localPosition;
						handProjection.transform.localRotation = hand.transform.localRotation;
						lastProjectionPose = hand.GetHandPose();
						lastProjectionPose.SetFingerPose(handProjection);
					}
					handProjection.body.position = handProjection.transform.position;
					handProjection.body.rotation = handProjection.transform.rotation;
					lastProjectionPosition = handProjection.transform.localPosition;
					lastProjectionRotation = handProjection.transform.localRotation;
					lastProjectionPose = handProjection.GetHandPose();
				}
				else if (!hand.IsGrabbing())
				{
					handProjection.transform.localPosition = hand.transform.localPosition;
					handProjection.transform.localRotation = hand.transform.localRotation;
					lastProjectionPosition = hand.transform.localPosition;
					lastProjectionRotation = hand.transform.localRotation;
					lastProjectionPose = hand.GetHandPose();
					lastProjectionPose.SetFingerPose(handProjection);
				}
			}
			else if (useGrabTransition)
			{
				handProjection.transform.localPosition = hand.transform.localPosition;
				handProjection.transform.localRotation = hand.transform.localRotation;
			}
		}

		private void SetTarget(Grabbable newTarget)
		{
			if (newTarget != null && !hand.CanGrab(newTarget))
			{
				newTarget = null;
			}
			if (hand.holdingObj != null || newTarget == null)
			{
				if (target != null)
				{
					OnProjectionEnd(handProjection, target);
					lastProjectionPosition = hand.transform.localPosition;
					lastProjectionRotation = hand.transform.localRotation;
					handProjection.transform.position = hand.transform.position;
					handProjection.transform.rotation = hand.transform.rotation;
					handProjection.body.position = handProjection.transform.position;
					handProjection.body.rotation = handProjection.transform.rotation;
				}
				target = null;
			}
			if (newTarget != target)
			{
				if (target != null)
				{
					OnProjectionEnd(handProjection, target);
				}
				target = newTarget;
				OnProjectionStart(handProjection, target);
			}
		}

		private bool IsProjectionActive()
		{
			if (target != null && hand.holdingObj == null && !hand.IsGrabbing())
			{
				return !tryingGrab;
			}
			return false;
		}
	}
}
