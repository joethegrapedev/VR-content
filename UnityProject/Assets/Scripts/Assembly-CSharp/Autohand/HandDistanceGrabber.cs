using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Autohand
{
	[DefaultExecutionOrder(2)]
	[HelpURL("https://earnestrobot.notion.site/Distance-Grabbing-19e4e8b14f00428295eca75fca752787")]
	public class HandDistanceGrabber : MonoBehaviour
	{
		[Header("Hands")]
		[Tooltip("The primaryHand used to trigger pulling or flicking")]
		public Hand primaryHand;

		[Tooltip("This is important for catch assistance")]
		public Hand secondaryHand;

		[Header("Pointing Options")]
		public Transform forwardPointer;

		public LineRenderer line;

		[Space]
		public float maxRange = 5f;

		[Tooltip("Defaults to grabbable on start if none")]
		public LayerMask layers;

		[Space]
		public Material defaultTargetedMaterial;

		[Tooltip("The highlight material to use when pulling")]
		public Material defaultSelectedMaterial;

		[Header("Pull Options")]
		public bool useInstantPull;

		[Tooltip("If false will default to distance pull, set pullGrabDistance to 0 for instant pull on select")]
		public bool useFlickPull;

		[Tooltip("The magnitude of your hands angular velocity for \"flick\" to start")]
		[ShowIf("useFlickPull")]
		public float flickThreshold = 7f;

		[Tooltip("The amount you need to move your hand from the select position to trigger the grab")]
		[HideIf("useFlickPull")]
		public float pullGrabDistance = 0.1f;

		[Space]
		[Tooltip("If this is true the object will be grabbed when entering the radius")]
		public bool instantGrabAssist = true;

		[Tooltip("The radius around of thrown object")]
		public float catchAssistRadius = 0.2f;

		[AutoToggleHeader("Show Events", 0, 0)]
		public bool showEvents = true;

		[ShowIf("showEvents")]
		public UnityHandGrabEvent OnPull;

		[ShowIf("showEvents")]
		public UnityHandEvent StartPoint;

		[ShowIf("showEvents")]
		public UnityHandEvent StopPoint;

		[ShowIf("showEvents")]
		[Tooltip("Targeting is started when object is highlighted")]
		public UnityHandGrabEvent StartTarget;

		[ShowIf("showEvents")]
		public UnityHandGrabEvent StopTarget;

		[Tooltip("Selecting is started when grab is selected on highlight object")]
		[ShowIf("showEvents")]
		public UnityHandGrabEvent StartSelect;

		[ShowIf("showEvents")]
		public UnityHandGrabEvent StopSelect;

		private List<CatchAssistData> catchAssisted;

		private DistanceGrabbable targetingDistanceGrabbable;

		private DistanceGrabbable selectingDistanceGrabbable;

		private float catchAssistSeconds = 3f;

		private bool pointing;

		private bool pulling;

		private Vector3 startPullPosition;

		private RaycastHit hit;

		private Quaternion lastRotation;

		private RaycastHit selectionHit;

		private float selectedEstimatedRadius;

		private float startLookAssist;

		private bool lastInstantPull;

		private GameObject _hitPoint;

		private Coroutine catchAssistRoutine;

		private DistanceGrabbable catchAsistGrabbable;

		private CatchAssistData catchAssistData;

		private GameObject hitPoint
		{
			get
			{
				if (!base.gameObject.activeInHierarchy)
				{
					return null;
				}
				if (_hitPoint == null)
				{
					_hitPoint = new GameObject();
					_hitPoint.name = "Distance Hit Point";
					return _hitPoint;
				}
				return _hitPoint;
			}
		}

		private void Start()
		{
			catchAssisted = new List<CatchAssistData>();
			if ((int)layers == 0)
			{
				layers = LayerMask.GetMask(Hand.grabbableLayerNameDefault);
			}
			if (useInstantPull)
			{
				SetInstantPull();
			}
		}

		private void OnEnable()
		{
			primaryHand.OnTriggerGrab += TryCatchAssist;
			if (secondaryHand != null)
			{
				secondaryHand.OnTriggerGrab += TryCatchAssist;
			}
			primaryHand.OnBeforeGrabbed += delegate
			{
				StopPointing();
				CancelSelect();
			};
		}

		private void OnDisable()
		{
			primaryHand.OnTriggerGrab -= TryCatchAssist;
			if (secondaryHand != null)
			{
				secondaryHand.OnTriggerGrab -= TryCatchAssist;
			}
			primaryHand.OnBeforeGrabbed -= delegate
			{
				StopPointing();
				CancelSelect();
			};
			if (catchAssistRoutine == null)
			{
				return;
			}
			StopCoroutine(catchAssistRoutine);
			catchAssistRoutine = null;
			Grabbable grabbable = catchAsistGrabbable.grabbable;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Remove(grabbable.OnGrabEvent, (HandGrabEvent)delegate
			{
				if (catchAssisted.Contains(catchAssistData))
				{
					catchAssisted.Remove(catchAssistData);
				}
			});
			DistanceGrabbable distanceGrabbable = catchAsistGrabbable;
			distanceGrabbable.OnPullCanceled = (HandGrabEvent)Delegate.Remove(distanceGrabbable.OnPullCanceled, (HandGrabEvent)delegate
			{
				if (catchAssisted.Contains(catchAssistData))
				{
					catchAssisted.Remove(catchAssistData);
				}
			});
		}

		private void Update()
		{
			CheckDistanceGrabbable();
			if (lastInstantPull != useInstantPull)
			{
				if (useInstantPull)
				{
					useFlickPull = false;
					pullGrabDistance = 0f;
				}
				lastInstantPull = useInstantPull;
			}
		}

		private void OnDestroy()
		{
			UnityEngine.Object.Destroy(hitPoint);
		}

		public void SetInstantPull()
		{
			useInstantPull = true;
		}

		public void SetPull(float distance)
		{
			useInstantPull = false;
			useFlickPull = false;
			pullGrabDistance = distance;
		}

		public void SetFlickPull(float threshold)
		{
			useInstantPull = false;
			useFlickPull = true;
			flickThreshold = threshold;
		}

		private void CheckDistanceGrabbable()
		{
			if (!pulling && pointing && primaryHand.holdingObj == null)
			{
				bool flag = Physics.SphereCast(forwardPointer.position, 0.03f, forwardPointer.forward, out hit, maxRange, layers);
				if (flag)
				{
					GrabbableChild component2;
					if (hit.transform.CanGetComponent<DistanceGrabbable>(out var component))
					{
						if (targetingDistanceGrabbable == null || component.GetInstanceID() != targetingDistanceGrabbable.GetInstanceID())
						{
							StartTargeting(component);
						}
					}
					else if (hit.transform.CanGetComponent<GrabbableChild>(out component2))
					{
						if (component2.grabParent.transform.CanGetComponent<DistanceGrabbable>(out component) && (targetingDistanceGrabbable == null || component.GetInstanceID() != targetingDistanceGrabbable.GetInstanceID()))
						{
							StartTargeting(component);
						}
					}
					else if (targetingDistanceGrabbable != null && hit.transform.gameObject.GetInstanceID() != targetingDistanceGrabbable.gameObject.GetInstanceID())
					{
						StopTargeting();
					}
				}
				else
				{
					StopTargeting();
				}
				if (line != null)
				{
					if (flag)
					{
						line.positionCount = 2;
						line.SetPositions(new Vector3[2] { forwardPointer.position, hit.point });
					}
					else
					{
						line.positionCount = 2;
						line.SetPositions(new Vector3[2]
						{
							forwardPointer.position,
							forwardPointer.position + forwardPointer.forward * maxRange
						});
					}
				}
			}
			else if (pulling && primaryHand.holdingObj == null)
			{
				if (useFlickPull)
				{
					TryFlickPull();
				}
				else
				{
					TryDistancePull();
				}
			}
			else if (targetingDistanceGrabbable != null)
			{
				StopTargeting();
			}
		}

		public virtual void StartPointing()
		{
			pointing = true;
			StartPoint?.Invoke(primaryHand);
		}

		public virtual void StopPointing()
		{
			pointing = false;
			if (line != null)
			{
				line.positionCount = 0;
				line.SetPositions(new Vector3[0]);
			}
			StopPoint?.Invoke(primaryHand);
			StopTargeting();
		}

		public virtual void StartTargeting(DistanceGrabbable target)
		{
			if (target.enabled && primaryHand.CanGrab(target.grabbable))
			{
				if (targetingDistanceGrabbable != null)
				{
					StopTargeting();
				}
				targetingDistanceGrabbable = target;
				targetingDistanceGrabbable?.grabbable.Highlight(primaryHand, GetTargetedMaterial(targetingDistanceGrabbable));
				targetingDistanceGrabbable?.StartTargeting?.Invoke(primaryHand, target.grabbable);
				StartTarget?.Invoke(primaryHand, target.grabbable);
			}
		}

		public virtual void StopTargeting()
		{
			targetingDistanceGrabbable?.grabbable.Unhighlight(primaryHand, GetTargetedMaterial(targetingDistanceGrabbable));
			targetingDistanceGrabbable?.StopTargeting?.Invoke(primaryHand, targetingDistanceGrabbable.grabbable);
			if (targetingDistanceGrabbable != null)
			{
				StopTarget?.Invoke(primaryHand, targetingDistanceGrabbable.grabbable);
			}
			else if (selectingDistanceGrabbable != null)
			{
				StopTarget?.Invoke(primaryHand, selectingDistanceGrabbable.grabbable);
			}
			targetingDistanceGrabbable = null;
		}

		public virtual void SelectTarget()
		{
			if (targetingDistanceGrabbable != null)
			{
				pulling = true;
				startPullPosition = primaryHand.transform.localPosition;
				lastRotation = base.transform.rotation;
				selectionHit = hit;
				if (catchAssistRoutine == null)
				{
					hitPoint.transform.position = selectionHit.point;
					hitPoint.transform.parent = selectionHit.transform;
				}
				selectingDistanceGrabbable = targetingDistanceGrabbable;
				selectedEstimatedRadius = Vector3.Distance(hitPoint.transform.position, selectingDistanceGrabbable.transform.position);
				selectingDistanceGrabbable.grabbable.Unhighlight(primaryHand, GetTargetedMaterial(selectingDistanceGrabbable));
				selectingDistanceGrabbable.grabbable.Highlight(primaryHand, GetSelectedMaterial(selectingDistanceGrabbable));
				selectingDistanceGrabbable?.StartSelecting?.Invoke(primaryHand, selectingDistanceGrabbable.grabbable);
				targetingDistanceGrabbable?.StopTargeting?.Invoke(primaryHand, selectingDistanceGrabbable.grabbable);
				targetingDistanceGrabbable = null;
				StartSelect?.Invoke(primaryHand, selectingDistanceGrabbable.grabbable);
				StopPointing();
			}
		}

		public virtual void CancelSelect()
		{
			StopTargeting();
			pulling = false;
			selectingDistanceGrabbable?.grabbable.Unhighlight(primaryHand, GetSelectedMaterial(selectingDistanceGrabbable));
			selectingDistanceGrabbable?.StopSelecting?.Invoke(primaryHand, selectingDistanceGrabbable.grabbable);
			if (selectingDistanceGrabbable != null)
			{
				StopSelect?.Invoke(primaryHand, selectingDistanceGrabbable.grabbable);
			}
			selectingDistanceGrabbable = null;
		}

		public virtual void ActivatePull()
		{
			if (!selectingDistanceGrabbable)
			{
				return;
			}
			OnPull?.Invoke(primaryHand, selectingDistanceGrabbable.grabbable);
			selectingDistanceGrabbable.OnPull?.Invoke(primaryHand, selectingDistanceGrabbable.grabbable);
			if (selectingDistanceGrabbable.instantPull)
			{
				selectingDistanceGrabbable.grabbable.body.velocity = Vector3.zero;
				selectingDistanceGrabbable.grabbable.body.angularVelocity = Vector3.zero;
				selectionHit.point = hitPoint.transform.position;
				if (selectingDistanceGrabbable.grabbable.placePoint != null)
				{
					selectingDistanceGrabbable.grabbable.placePoint.Remove();
				}
				primaryHand.Grab(selectionHit, selectingDistanceGrabbable.grabbable);
				CancelSelect();
				selectingDistanceGrabbable?.CancelTarget();
			}
			else if (selectingDistanceGrabbable.grabType == DistanceGrabType.Velocity)
			{
				catchAssistRoutine = StartCoroutine(StartCatchAssist(selectingDistanceGrabbable, selectedEstimatedRadius));
				catchAsistGrabbable = selectingDistanceGrabbable;
				if (selectingDistanceGrabbable.grabbable.placePoint != null)
				{
					selectingDistanceGrabbable.grabbable.placePoint.Remove();
				}
				selectingDistanceGrabbable.SetTarget(primaryHand.palmTransform);
			}
			else if (selectingDistanceGrabbable.grabType == DistanceGrabType.Linear)
			{
				selectingDistanceGrabbable.grabbable.body.velocity = Vector3.zero;
				selectingDistanceGrabbable.grabbable.body.angularVelocity = Vector3.zero;
				selectionHit.point = hitPoint.transform.position;
				if (selectingDistanceGrabbable.grabbable.placePoint != null)
				{
					selectingDistanceGrabbable.grabbable.placePoint.Remove();
				}
				primaryHand.Grab(selectionHit, selectingDistanceGrabbable.grabbable, GrabType.GrabbableToHand);
				CancelSelect();
				selectingDistanceGrabbable?.CancelTarget();
			}
			CancelSelect();
		}

		private void TryDistancePull()
		{
			if (Vector3.Distance(startPullPosition, primaryHand.transform.localPosition) > pullGrabDistance)
			{
				ActivatePull();
			}
		}

		private void TryFlickPull()
		{
			Quaternion quaternion = base.transform.rotation * Quaternion.Inverse(lastRotation);
			lastRotation = base.transform.rotation;
			float angle = 0f;
			Vector3 axis = Vector3.zero;
			quaternion.ToAngleAxis(out angle, out axis);
			angle *= MathF.PI / 180f;
			if (((axis * angle * (1f / Time.deltaTime)).magnitude > flickThreshold || useInstantPull) && (bool)selectingDistanceGrabbable)
			{
				ActivatePull();
			}
		}

		private Material GetSelectedMaterial(DistanceGrabbable grabbable)
		{
			if (grabbable.ignoreHighlights)
			{
				return null;
			}
			if (!(grabbable.selectedMaterial != null))
			{
				return defaultSelectedMaterial;
			}
			return grabbable.selectedMaterial;
		}

		private Material GetTargetedMaterial(DistanceGrabbable grabbable)
		{
			if (grabbable.ignoreHighlights)
			{
				return null;
			}
			if (!(grabbable.selectedMaterial != null))
			{
				return defaultTargetedMaterial;
			}
			return grabbable.targetedMaterial;
		}

		private void TryCatchAssist(Hand hand, Grabbable grab)
		{
			for (int i = 0; i < catchAssisted.Count; i++)
			{
				if (Vector3.Distance(hand.palmTransform.position + hand.palmTransform.forward * catchAssistRadius, catchAssisted[i].grab.transform.position) - catchAssisted[i].estimatedRadius < catchAssistRadius && Physics.SphereCast(new Ray(hand.palmTransform.position, hitPoint.transform.position - hand.palmTransform.position), 0.03f, out var hitInfo, catchAssistRadius * 2f, LayerMask.GetMask(Hand.grabbableLayerNameDefault, Hand.grabbingLayerName)) && hitInfo.transform.gameObject == catchAssisted[i].grab.gameObject)
				{
					catchAssisted[i].grab.body.velocity = Vector3.zero;
					catchAssisted[i].grab.body.angularVelocity = Vector3.zero;
					hand.Grab(hitInfo, catchAssisted[i].grab);
					CancelSelect();
				}
			}
		}

		private IEnumerator StartCatchAssist(DistanceGrabbable grab, float estimatedRadius)
		{
			catchAssistData = new CatchAssistData(grab.grabbable, catchAssistRadius);
			catchAssisted.Add(catchAssistData);
			Grabbable grabbable = grab.grabbable;
			grabbable.OnGrabEvent = (HandGrabEvent)Delegate.Combine(grabbable.OnGrabEvent, (HandGrabEvent)delegate
			{
				if (catchAssisted.Contains(catchAssistData))
				{
					catchAssisted.Remove(catchAssistData);
				}
			});
			DistanceGrabbable distanceGrabbable = grab;
			distanceGrabbable.OnPullCanceled = (HandGrabEvent)Delegate.Combine(distanceGrabbable.OnPullCanceled, (HandGrabEvent)delegate
			{
				if (catchAssisted.Contains(catchAssistData))
				{
					catchAssisted.Remove(catchAssistData);
				}
			});
			if (instantGrabAssist)
			{
				bool cancelInstantGrab = false;
				float time = 0f;
				primaryHand.OnTriggerRelease += delegate
				{
					cancelInstantGrab = true;
				};
				while (time < catchAssistSeconds && !cancelInstantGrab)
				{
					time += Time.fixedDeltaTime;
					if (TryCatch(primaryHand))
					{
						break;
					}
					yield return new WaitForEndOfFrame();
				}
				primaryHand.OnTriggerRelease -= delegate
				{
					cancelInstantGrab = true;
				};
			}
			else
			{
				yield return new WaitForSeconds(catchAssistSeconds);
			}
			Grabbable grabbable2 = grab.grabbable;
			grabbable2.OnGrabEvent = (HandGrabEvent)Delegate.Remove(grabbable2.OnGrabEvent, (HandGrabEvent)delegate
			{
				if (catchAssisted.Contains(catchAssistData))
				{
					catchAssisted.Remove(catchAssistData);
				}
			});
			DistanceGrabbable distanceGrabbable2 = grab;
			distanceGrabbable2.OnPullCanceled = (HandGrabEvent)Delegate.Remove(distanceGrabbable2.OnPullCanceled, (HandGrabEvent)delegate
			{
				if (catchAssisted.Contains(catchAssistData))
				{
					catchAssisted.Remove(catchAssistData);
				}
			});
			if (catchAssisted.Contains(catchAssistData))
			{
				catchAssisted.Remove(catchAssistData);
			}
			catchAssistRoutine = null;
			bool TryCatch(Hand hand)
			{
				if (Vector3.Distance(hand.palmTransform.position + hand.palmTransform.forward * catchAssistRadius, grab.transform.position) - estimatedRadius < catchAssistRadius)
				{
					RaycastHit[] array = Physics.SphereCastAll(new Ray(hand.palmTransform.position, hitPoint.transform.position - hand.palmTransform.position), 0.03f, catchAssistRadius * 2f, LayerMask.GetMask(Hand.grabbableLayerNameDefault, Hand.grabbingLayerName));
					for (int i = 0; i < array.Length; i++)
					{
						if (array[i].transform.gameObject == grab.gameObject)
						{
							grab.grabbable.body.velocity = Vector3.zero;
							grab.grabbable.body.angularVelocity = Vector3.zero;
							hand.Grab(array[i], grab.grabbable);
							grab.CancelTarget();
							CancelSelect();
							return true;
						}
					}
				}
				return false;
			}
		}

		private void OnDrawGizmosSelected()
		{
			if ((bool)primaryHand)
			{
				Gizmos.DrawWireSphere(primaryHand.palmTransform.position + primaryHand.palmTransform.forward * catchAssistRadius * 4f / 5f + primaryHand.palmTransform.up * catchAssistRadius * 1f / 4f, catchAssistRadius);
			}
		}
	}
}
