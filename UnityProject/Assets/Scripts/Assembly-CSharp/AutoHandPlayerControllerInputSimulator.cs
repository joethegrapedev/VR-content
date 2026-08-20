using System.Collections.Generic;
using Autohand;
using Autohand.Demo;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using UnityEngine.XR.Management;

[DefaultExecutionOrder(-4)]
public class AutoHandPlayerControllerInputSimulator : MonoBehaviour
{
	private enum Move
	{
		noInput = 0,
		body = 1,
		head = 2,
		bodyAndHead = 3,
		leftHand = 4,
		rightHand = 5,
		bothHands = 6
	}

	[Tooltip("Auto-populates, if not assigned.")]
	public AutoHandPlayer player;

	public float headHeight = 1.8f;

	[Header("Status indicators:")]
	[ReadOnly]
	[Tooltip("This just shows whether the simulator is working. It is based on whether the the Mock HMD is running. (only updated on start). If this is false, it means that this simulator is not doing anything (literally nothing at all).")]
	[SerializeField]
	private bool isSimulating;

	[ReadOnly]
	[SerializeField]
	private Move currentlyMoving = Move.bodyAndHead;

	[AutoToggleHeader("Advanced Options", 0, 0)]
	public bool ignoreMe;

	[Header("Adjustments:")]
	[ShowIf("ignoreMe")]
	[Tooltip("Locks cursor to screen, escape the screen by hitting ESC-key")]
	public bool cursorLock = true;

	[ShowIf("ignoreMe")]
	public Vector3 leftHandStartOffset = new Vector3(-0.2f, 1.5f, 0.3f);

	[ShowIf("ignoreMe")]
	public Vector3 rightHandStartOffset = new Vector3(0.2f, 1.5f, 0.3f);

	[ShowIf("ignoreMe")]
	[Range(100f, 1500f)]
	public float mouseLookSensitivity = 800f;

	[ShowIf("ignoreMe")]
	[Range(0.1f, 2f)]
	public float ScrollHandSpeed = 1f;

	[ShowIf("ignoreMe")]
	[Range(0.5f, 6f)]
	public float handMovementSpeed = 3f;

	[ShowIf("ignoreMe")]
	[Tooltip("This will ensure that the regular XRHandPlayerControllerLink is disabled and stays disabled while using this simulator. Reasons to use: If you enable/disable the XRHandPlayerControllerLinkwhile the game is running, it might overtake control and block the commands from this script.")]
	[SerializeField]
	private bool disableXRControllerLink;

	[ShowIf("ignoreMe")]
	[Tooltip("Disables pushing, climbing and platforms options on AutoHandPlayer at startup. These might interfere with the function, though not consistently.")]
	public bool disableInterferringFeatures;

	[ShowIf("ignoreMe")]
	[Tooltip("Auto-populates if the reference is on this gameobject. Otherwise set it here.")]
	[SerializeField]
	private MonoBehaviour xRHandPlayerControllerLink;

	[AutoToggleHeader("Key Setup", 0, 0)]
	[SerializeField]
	private bool ignoreMe2;

	[ShowIf("ignoreMe2")]
	public KeyCode controlLeftHandKeyCode = KeyCode.Q;

	[ShowIf("ignoreMe2")]
	public KeyCode controlRightHandKeyCode = KeyCode.E;

	[ShowIf("ignoreMe2")]
	public KeyCode forwardKeyCode = KeyCode.W;

	[ShowIf("ignoreMe2")]
	public KeyCode leftKeyCode = KeyCode.A;

	[ShowIf("ignoreMe2")]
	public KeyCode backKeyCode = KeyCode.S;

	[ShowIf("ignoreMe2")]
	public KeyCode rightKeyCode = KeyCode.D;

	[ShowIf("ignoreMe2")]
	public KeyCode mouseGrabKeyCode = KeyCode.Mouse0;

	[ShowIf("ignoreMe2")]
	public KeyCode crouchKeyCode = KeyCode.LeftControl;

	[ShowIf("ignoreMe2")]
	public KeyCode resetHandKeyCode = KeyCode.R;

	[ShowIf("ignoreMe2")]
	public KeyCode mouseLookKeyCode = KeyCode.Mouse1;

	[ShowIf("ignoreMe2")]
	[EnableIf("cursorLock")]
	public KeyCode escapeFPSKeyCode = KeyCode.Escape;

	[AutoToggleHeader("Events", 0, 0)]
	[SerializeField]
	private bool ignorMe3;

	[ShowIf("ignorMe3")]
	public UnityHandEvent grabEvent;

	[ShowIf("ignorMe3")]
	public UnityHandEvent releaseEvent;

	private SimulatedPoseDriver leftPoser;

	private SimulatedPoseDriver rightPoser;

	private SimulatedPoseDriver headPoser;

	private Vector2 screenSize;

	private Vector2 previousMousePos = Vector2.zero;

	private bool firstFrame = true;

	private bool controlLeftHand;

	private bool controlRightHand;

	private bool forwardKey;

	private bool backKey;

	private bool leftKey;

	private bool rightKey;

	private bool mouseGrabKey;

	private bool mouseLookKey;

	private bool crouchKey;

	private bool resetHandKey;

	private bool mouseLookKeyDown;

	private bool escapeFPSKeyDown;

	private Vector2 movementInputs = Vector2.zero;

	private Vector2 mouseDeltaPosition = Vector2.zero;

	private Vector2 mouseScrollDelta = Vector2.zero;

	private void Start()
	{
		string text = XRGeneralSettings.Instance?.Manager?.activeLoader.name;
		if (!text.Contains("Mock"))
		{
			Debug.Log("From AutohandSim: an active XR system was found, aborting simulation (XR system was: [" + text + "])");
			isSimulating = false;
			return;
		}
		bool flag = false;
		_ = XRGeneralSettings.Instance?.Manager?.activeLoader.name;
		List<XRDisplaySubsystem> list = new List<XRDisplaySubsystem>();
		SubsystemManager.GetInstances(list);
		foreach (XRDisplaySubsystem item in list)
		{
			if (item.running && !item.subsystemDescriptor.id.Contains("Mock"))
			{
				flag = true;
			}
		}
		if (flag)
		{
			return;
		}
		isSimulating = true;
		TrackedPoseDriver[] array = Object.FindObjectsOfType<TrackedPoseDriver>();
		foreach (TrackedPoseDriver trackedPoseDriver in array)
		{
			if (trackedPoseDriver.poseSource == TrackedPoseDriver.TrackedPose.Center)
			{
				headPoser = new GameObject("HeadDriver").AddComponent<SimulatedPoseDriver>();
				headPoser.transform.position = new Vector3(0f, headHeight, 0f);
				trackedPoseDriver.poseProviderComponent = headPoser;
			}
			if (trackedPoseDriver.poseSource == TrackedPoseDriver.TrackedPose.LeftPose)
			{
				leftPoser = new GameObject("LeftDriver").AddComponent<SimulatedPoseDriver>();
				leftPoser.transform.position = leftHandStartOffset;
				trackedPoseDriver.poseProviderComponent = leftPoser;
			}
			if (trackedPoseDriver.poseSource == TrackedPoseDriver.TrackedPose.RightPose)
			{
				rightPoser = new GameObject("RightDriver").AddComponent<SimulatedPoseDriver>();
				rightPoser.transform.position = rightHandStartOffset;
				trackedPoseDriver.poseProviderComponent = rightPoser;
			}
		}
		player = GetComponent<AutoHandPlayer>();
		if (grabEvent == null)
		{
			grabEvent = new UnityHandEvent();
		}
		if (releaseEvent == null)
		{
			releaseEvent = new UnityHandEvent();
		}
		if (disableXRControllerLink)
		{
			xRHandPlayerControllerLink = Object.FindObjectOfType<XRHandPlayerControllerLink>();
		}
		if (disableXRControllerLink && xRHandPlayerControllerLink != null)
		{
			xRHandPlayerControllerLink.enabled = false;
		}
		if (disableInterferringFeatures)
		{
			player.allowBodyPushing = false;
			player.allowClimbing = false;
			player.allowClimbingMovement = false;
			player.allowPlatforms = false;
		}
		Invoke("MoveHeadToStartTracking", 0.2f);
		Debug.LogWarning("Thanks for checking out Autohand simulator! Unfortunalely, you appear to have the new input system as the default input system. Currently, it is not supported by this simulator, so in order to use it, you'll have to enable the old/legacy input system.You can do that by going into Edit >> Project Settings >> Player >> and change the [Active Input Handling] to [Legacy] or [Both].");
	}
}
