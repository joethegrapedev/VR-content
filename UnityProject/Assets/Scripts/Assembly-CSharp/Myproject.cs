using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class Myproject : IInputActionCollection2, IInputActionCollection, IEnumerable<InputAction>, IEnumerable, IDisposable
{
	public struct PlayerActions
	{
		private Myproject m_Wrapper;

		public InputAction Move => m_Wrapper.m_Player_Move;

		public InputAction Look => m_Wrapper.m_Player_Look;

		public InputAction Fire => m_Wrapper.m_Player_Fire;

		public bool enabled => Get().enabled;

		public PlayerActions(Myproject wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Player;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(PlayerActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IPlayerActions instance)
		{
			if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
			{
				Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
				Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
				Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
				Look.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
				Look.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
				Look.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
				Fire.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
				Fire.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
				Fire.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnFire;
			}
			m_Wrapper.m_PlayerActionsCallbackInterface = instance;
			if (instance != null)
			{
				Move.started += instance.OnMove;
				Move.performed += instance.OnMove;
				Move.canceled += instance.OnMove;
				Look.started += instance.OnLook;
				Look.performed += instance.OnLook;
				Look.canceled += instance.OnLook;
				Fire.started += instance.OnFire;
				Fire.performed += instance.OnFire;
				Fire.canceled += instance.OnFire;
			}
		}
	}

	public struct UIActions
	{
		private Myproject m_Wrapper;

		public InputAction Navigate => m_Wrapper.m_UI_Navigate;

		public InputAction Submit => m_Wrapper.m_UI_Submit;

		public InputAction Cancel => m_Wrapper.m_UI_Cancel;

		public InputAction Point => m_Wrapper.m_UI_Point;

		public InputAction Click => m_Wrapper.m_UI_Click;

		public InputAction ScrollWheel => m_Wrapper.m_UI_ScrollWheel;

		public InputAction MiddleClick => m_Wrapper.m_UI_MiddleClick;

		public InputAction RightClick => m_Wrapper.m_UI_RightClick;

		public InputAction TrackedDevicePosition => m_Wrapper.m_UI_TrackedDevicePosition;

		public InputAction TrackedDeviceOrientation => m_Wrapper.m_UI_TrackedDeviceOrientation;

		public bool enabled => Get().enabled;

		public UIActions(Myproject wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_UI;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(UIActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IUIActions instance)
		{
			if (m_Wrapper.m_UIActionsCallbackInterface != null)
			{
				Navigate.started -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
				Navigate.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
				Navigate.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnNavigate;
				Submit.started -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
				Submit.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
				Submit.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnSubmit;
				Cancel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
				Cancel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
				Cancel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnCancel;
				Point.started -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
				Point.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
				Point.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnPoint;
				Click.started -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
				Click.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
				Click.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnClick;
				ScrollWheel.started -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
				ScrollWheel.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
				ScrollWheel.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnScrollWheel;
				MiddleClick.started -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
				MiddleClick.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
				MiddleClick.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnMiddleClick;
				RightClick.started -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
				RightClick.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
				RightClick.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnRightClick;
				TrackedDevicePosition.started -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
				TrackedDevicePosition.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
				TrackedDevicePosition.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDevicePosition;
				TrackedDeviceOrientation.started -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.performed -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.canceled -= m_Wrapper.m_UIActionsCallbackInterface.OnTrackedDeviceOrientation;
			}
			m_Wrapper.m_UIActionsCallbackInterface = instance;
			if (instance != null)
			{
				Navigate.started += instance.OnNavigate;
				Navigate.performed += instance.OnNavigate;
				Navigate.canceled += instance.OnNavigate;
				Submit.started += instance.OnSubmit;
				Submit.performed += instance.OnSubmit;
				Submit.canceled += instance.OnSubmit;
				Cancel.started += instance.OnCancel;
				Cancel.performed += instance.OnCancel;
				Cancel.canceled += instance.OnCancel;
				Point.started += instance.OnPoint;
				Point.performed += instance.OnPoint;
				Point.canceled += instance.OnPoint;
				Click.started += instance.OnClick;
				Click.performed += instance.OnClick;
				Click.canceled += instance.OnClick;
				ScrollWheel.started += instance.OnScrollWheel;
				ScrollWheel.performed += instance.OnScrollWheel;
				ScrollWheel.canceled += instance.OnScrollWheel;
				MiddleClick.started += instance.OnMiddleClick;
				MiddleClick.performed += instance.OnMiddleClick;
				MiddleClick.canceled += instance.OnMiddleClick;
				RightClick.started += instance.OnRightClick;
				RightClick.performed += instance.OnRightClick;
				RightClick.canceled += instance.OnRightClick;
				TrackedDevicePosition.started += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.performed += instance.OnTrackedDevicePosition;
				TrackedDevicePosition.canceled += instance.OnTrackedDevicePosition;
				TrackedDeviceOrientation.started += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.performed += instance.OnTrackedDeviceOrientation;
				TrackedDeviceOrientation.canceled += instance.OnTrackedDeviceOrientation;
			}
		}
	}

	public struct KeyboardActions
	{
		private Myproject m_Wrapper;

		public InputAction Keys => m_Wrapper.m_Keyboard_Keys;

		public bool enabled => Get().enabled;

		public KeyboardActions(Myproject wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Keyboard;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(KeyboardActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IKeyboardActions instance)
		{
			if (m_Wrapper.m_KeyboardActionsCallbackInterface != null)
			{
				Keys.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnKeys;
				Keys.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnKeys;
				Keys.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnKeys;
			}
			m_Wrapper.m_KeyboardActionsCallbackInterface = instance;
			if (instance != null)
			{
				Keys.started += instance.OnKeys;
				Keys.performed += instance.OnKeys;
				Keys.canceled += instance.OnKeys;
			}
		}
	}

	public struct OculusActions
	{
		private Myproject m_Wrapper;

		public InputAction Controller => m_Wrapper.m_Oculus_Controller;

		public bool enabled => Get().enabled;

		public OculusActions(Myproject wrapper)
		{
			m_Wrapper = wrapper;
		}

		public InputActionMap Get()
		{
			return m_Wrapper.m_Oculus;
		}

		public void Enable()
		{
			Get().Enable();
		}

		public void Disable()
		{
			Get().Disable();
		}

		public static implicit operator InputActionMap(OculusActions set)
		{
			return set.Get();
		}

		public void SetCallbacks(IOculusActions instance)
		{
			if (m_Wrapper.m_OculusActionsCallbackInterface != null)
			{
				Controller.started -= m_Wrapper.m_OculusActionsCallbackInterface.OnController;
				Controller.performed -= m_Wrapper.m_OculusActionsCallbackInterface.OnController;
				Controller.canceled -= m_Wrapper.m_OculusActionsCallbackInterface.OnController;
			}
			m_Wrapper.m_OculusActionsCallbackInterface = instance;
			if (instance != null)
			{
				Controller.started += instance.OnController;
				Controller.performed += instance.OnController;
				Controller.canceled += instance.OnController;
			}
		}
	}

	public interface IPlayerActions
	{
		void OnMove(InputAction.CallbackContext context);

		void OnLook(InputAction.CallbackContext context);

		void OnFire(InputAction.CallbackContext context);
	}

	public interface IUIActions
	{
		void OnNavigate(InputAction.CallbackContext context);

		void OnSubmit(InputAction.CallbackContext context);

		void OnCancel(InputAction.CallbackContext context);

		void OnPoint(InputAction.CallbackContext context);

		void OnClick(InputAction.CallbackContext context);

		void OnScrollWheel(InputAction.CallbackContext context);

		void OnMiddleClick(InputAction.CallbackContext context);

		void OnRightClick(InputAction.CallbackContext context);

		void OnTrackedDevicePosition(InputAction.CallbackContext context);

		void OnTrackedDeviceOrientation(InputAction.CallbackContext context);
	}

	public interface IKeyboardActions
	{
		void OnKeys(InputAction.CallbackContext context);
	}

	public interface IOculusActions
	{
		void OnController(InputAction.CallbackContext context);
	}

	private readonly InputActionMap m_Player;

	private IPlayerActions m_PlayerActionsCallbackInterface;

	private readonly InputAction m_Player_Move;

	private readonly InputAction m_Player_Look;

	private readonly InputAction m_Player_Fire;

	private readonly InputActionMap m_UI;

	private IUIActions m_UIActionsCallbackInterface;

	private readonly InputAction m_UI_Navigate;

	private readonly InputAction m_UI_Submit;

	private readonly InputAction m_UI_Cancel;

	private readonly InputAction m_UI_Point;

	private readonly InputAction m_UI_Click;

	private readonly InputAction m_UI_ScrollWheel;

	private readonly InputAction m_UI_MiddleClick;

	private readonly InputAction m_UI_RightClick;

	private readonly InputAction m_UI_TrackedDevicePosition;

	private readonly InputAction m_UI_TrackedDeviceOrientation;

	private readonly InputActionMap m_Keyboard;

	private IKeyboardActions m_KeyboardActionsCallbackInterface;

	private readonly InputAction m_Keyboard_Keys;

	private readonly InputActionMap m_Oculus;

	private IOculusActions m_OculusActionsCallbackInterface;

	private readonly InputAction m_Oculus_Controller;

	private int m_KeyboardMouseSchemeIndex = -1;

	private int m_GamepadSchemeIndex = -1;

	private int m_TouchSchemeIndex = -1;

	private int m_JoystickSchemeIndex = -1;

	private int m_XRSchemeIndex = -1;

	public InputActionAsset asset { get; }

	public InputBinding? bindingMask
	{
		get
		{
			return asset.bindingMask;
		}
		set
		{
			asset.bindingMask = value;
		}
	}

	public ReadOnlyArray<InputDevice>? devices
	{
		get
		{
			return asset.devices;
		}
		set
		{
			asset.devices = value;
		}
	}

	public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

	public IEnumerable<InputBinding> bindings => asset.bindings;

	public PlayerActions Player => new PlayerActions(this);

	public UIActions UI => new UIActions(this);

	public KeyboardActions Keyboard => new KeyboardActions(this);

	public OculusActions Oculus => new OculusActions(this);

	public InputControlScheme KeyboardMouseScheme
	{
		get
		{
			if (m_KeyboardMouseSchemeIndex == -1)
			{
				m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard&Mouse");
			}
			return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
		}
	}

	public InputControlScheme GamepadScheme
	{
		get
		{
			if (m_GamepadSchemeIndex == -1)
			{
				m_GamepadSchemeIndex = asset.FindControlSchemeIndex("Gamepad");
			}
			return asset.controlSchemes[m_GamepadSchemeIndex];
		}
	}

	public InputControlScheme TouchScheme
	{
		get
		{
			if (m_TouchSchemeIndex == -1)
			{
				m_TouchSchemeIndex = asset.FindControlSchemeIndex("Touch");
			}
			return asset.controlSchemes[m_TouchSchemeIndex];
		}
	}

	public InputControlScheme JoystickScheme
	{
		get
		{
			if (m_JoystickSchemeIndex == -1)
			{
				m_JoystickSchemeIndex = asset.FindControlSchemeIndex("Joystick");
			}
			return asset.controlSchemes[m_JoystickSchemeIndex];
		}
	}

	public InputControlScheme XRScheme
	{
		get
		{
			if (m_XRSchemeIndex == -1)
			{
				m_XRSchemeIndex = asset.FindControlSchemeIndex("XR");
			}
			return asset.controlSchemes[m_XRSchemeIndex];
		}
	}

	public Myproject()
	{
		asset = InputActionAsset.FromJson("{\r\n    \"name\": \"My project\",\r\n    \"maps\": [\r\n        {\r\n            \"name\": \"Player\",\r\n            \"id\": \"623fed6a-2377-470f-bf59-bfc431b18651\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Move\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"5be196e0-74f9-4a00-8fdd-7d2e7c7b4cd5\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Look\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"ef935e35-50cd-4c04-8802-7358dd3aaeb0\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Fire\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"02e010cd-ff37-4600-a698-b9fa4a51d2c8\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"978bfe49-cc26-4a3d-ab7b-7d7a29327403\",\r\n                    \"path\": \"<Gamepad>/leftStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"WASD\",\r\n                    \"id\": \"00ca640b-d935-4593-8157-c05846ea39b3\",\r\n                    \"path\": \"Dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"e2062cb9-1b15-46a2-838c-2f8d72a0bdd9\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"8180e8bd-4097-4f4e-ab88-4523101a6ce9\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"320bffee-a40b-4347-ac70-c210eb8bc73a\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"1c5327b5-f71c-4f60-99c7-4e737386f1d1\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"d2581a9b-1d11-4566-b27d-b92aff5fabbc\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"2e46982e-44cc-431b-9f0b-c11910bf467a\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"fcfe95b8-67b9-4526-84b5-5d0bc98d6400\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"77bff152-3580-4b21-b6de-dcd0c7e41164\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1635d3fe-58b6-4ba9-a4e2-f4b964f6b5c8\",\r\n                    \"path\": \"<XRController>/{Primary2DAxis}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"XR\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3ea4d645-4504-4529-b061-ab81934c3752\",\r\n                    \"path\": \"<Joystick>/stick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Move\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c1f7a91b-d0fd-4a62-997e-7fb9b69bf235\",\r\n                    \"path\": \"<Gamepad>/rightStick\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8c8e490b-c610-4785-884f-f04217b23ca4\",\r\n                    \"path\": \"<Pointer>/delta\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse;Touch\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3e5f5442-8668-4b27-a940-df99bad7e831\",\r\n                    \"path\": \"<Joystick>/{Hatswitch}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Look\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"143bb1cd-cc10-4eca-a2f0-a3664166fe91\",\r\n                    \"path\": \"<Gamepad>/rightTrigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"05f6913d-c316-48b2-a6bb-e225f14c7960\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"886e731e-7071-4ae4-95c0-e61739dad6fd\",\r\n                    \"path\": \"<Touchscreen>/primaryTouch/tap\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Touch\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"ee3d0cd2-254e-47a7-a8cb-bc94d9658c54\",\r\n                    \"path\": \"<Joystick>/trigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8255d333-5683-4943-a58a-ccb207ff1dce\",\r\n                    \"path\": \"<XRController>/{PrimaryAction}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"XR\",\r\n                    \"action\": \"Fire\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"UI\",\r\n            \"id\": \"dd00986e-b3a9-4d03-8ce6-1dd10024fd04\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Navigate\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"792ff18d-6bad-44f4-ada5-ea462555e85d\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Submit\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"fb44743f-121c-4045-9cba-39c839c91281\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Cancel\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"ca5793eb-dce0-4a97-a5ca-70c7cc8d7bef\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"Point\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"77c93751-8e24-4200-9059-a50d995d8288\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"Click\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"97ae45c0-c343-47ba-95be-9188c3a7cba3\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                },\r\n                {\r\n                    \"name\": \"ScrollWheel\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"71be1843-e63f-4f0d-96ea-1881009b0441\",\r\n                    \"expectedControlType\": \"Vector2\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"MiddleClick\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"2cae26dc-cd4a-42a5-84ad-3535d65a989e\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"RightClick\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"10ac5157-a194-411c-a22e-f6d461c880b2\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TrackedDevicePosition\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"7ca679f0-d1b1-46d2-bf2a-1d0ce6a6842c\",\r\n                    \"expectedControlType\": \"Vector3\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                },\r\n                {\r\n                    \"name\": \"TrackedDeviceOrientation\",\r\n                    \"type\": \"PassThrough\",\r\n                    \"id\": \"9a405037-a36d-4b94-9296-94c800697ac1\",\r\n                    \"expectedControlType\": \"Quaternion\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"Gamepad\",\r\n                    \"id\": \"809f371f-c5e2-4e7a-83a1-d867598f40dd\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"14a5d6e8-4aaf-4119-a9ef-34b8c2c548bf\",\r\n                    \"path\": \"<Gamepad>/leftStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"9144cbe6-05e1-4687-a6d7-24f99d23dd81\",\r\n                    \"path\": \"<Gamepad>/rightStick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"2db08d65-c5fb-421b-983f-c71163608d67\",\r\n                    \"path\": \"<Gamepad>/leftStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"58748904-2ea9-4a80-8579-b500e6a76df8\",\r\n                    \"path\": \"<Gamepad>/rightStick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"8ba04515-75aa-45de-966d-393d9bbd1c14\",\r\n                    \"path\": \"<Gamepad>/leftStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"712e721c-bdfb-4b23-a86c-a0d9fcfea921\",\r\n                    \"path\": \"<Gamepad>/rightStick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"fcd248ae-a788-4676-a12e-f4d81205600b\",\r\n                    \"path\": \"<Gamepad>/leftStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"1f04d9bc-c50b-41a1-bfcc-afb75475ec20\",\r\n                    \"path\": \"<Gamepad>/rightStick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fb8277d4-c5cd-4663-9dc7-ee3f0b506d90\",\r\n                    \"path\": \"<Gamepad>/dpad\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Gamepad\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"Joystick\",\r\n                    \"id\": \"e25d9774-381c-4a61-b47c-7b6b299ad9f9\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"3db53b26-6601-41be-9887-63ac74e79d19\",\r\n                    \"path\": \"<Joystick>/stick/up\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"0cb3e13e-3d90-4178-8ae6-d9c5501d653f\",\r\n                    \"path\": \"<Joystick>/stick/down\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"0392d399-f6dd-4c82-8062-c1e9c0d34835\",\r\n                    \"path\": \"<Joystick>/stick/left\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"942a66d9-d42f-43d6-8d70-ecb4ba5363bc\",\r\n                    \"path\": \"<Joystick>/stick/right\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Joystick\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"Keyboard\",\r\n                    \"id\": \"ff527021-f211-4c02-933e-5976594c46ed\",\r\n                    \"path\": \"2DVector\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": true,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"563fbfdd-0f09-408d-aa75-8642c4f08ef0\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"up\",\r\n                    \"id\": \"eb480147-c587-4a33-85ed-eb0ab9942c43\",\r\n                    \"path\": \"<Keyboard>/upArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"2bf42165-60bc-42ca-8072-8c13ab40239b\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"down\",\r\n                    \"id\": \"85d264ad-e0a0-4565-b7ff-1a37edde51ac\",\r\n                    \"path\": \"<Keyboard>/downArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"74214943-c580-44e4-98eb-ad7eebe17902\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"left\",\r\n                    \"id\": \"cea9b045-a000-445b-95b8-0c171af70a3b\",\r\n                    \"path\": \"<Keyboard>/leftArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"8607c725-d935-4808-84b1-8354e29bab63\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"right\",\r\n                    \"id\": \"4cda81dc-9edd-4e03-9d7c-a71a14345d0b\",\r\n                    \"path\": \"<Keyboard>/rightArrow\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Navigate\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": true\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"9e92bb26-7e3b-4ec4-b06b-3c8f8e498ddc\",\r\n                    \"path\": \"*/{Submit}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse;Gamepad;Touch;Joystick;XR\",\r\n                    \"action\": \"Submit\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"82627dcc-3b13-4ba9-841d-e4b746d6553e\",\r\n                    \"path\": \"*/{Cancel}\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse;Gamepad;Touch;Joystick;XR\",\r\n                    \"action\": \"Cancel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"c52c8e0b-8179-41d3-b8a1-d149033bbe86\",\r\n                    \"path\": \"<Mouse>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e1394cbc-336e-44ce-9ea8-6007ed6193f7\",\r\n                    \"path\": \"<Pen>/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5693e57a-238a-46ed-b5ae-e64e6e574302\",\r\n                    \"path\": \"<Touchscreen>/touch*/position\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Touch\",\r\n                    \"action\": \"Point\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4faf7dc9-b979-4210-aa8c-e808e1ef89f5\",\r\n                    \"path\": \"<Mouse>/leftButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8d66d5ba-88d7-48e6-b1cd-198bbfef7ace\",\r\n                    \"path\": \"<Pen>/tip\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"47c2a644-3ebc-4dae-a106-589b7ca75b59\",\r\n                    \"path\": \"<Touchscreen>/touch*/press\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"Touch\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"bb9e6b34-44bf-4381-ac63-5aa15d19f677\",\r\n                    \"path\": \"<XRController>/trigger\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"XR\",\r\n                    \"action\": \"Click\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"38c99815-14ea-4617-8627-164d27641299\",\r\n                    \"path\": \"<Mouse>/scroll\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"ScrollWheel\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"24066f69-da47-44f3-a07e-0015fb02eb2e\",\r\n                    \"path\": \"<Mouse>/middleButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"MiddleClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4c191405-5738-4d4b-a523-c6a301dbf754\",\r\n                    \"path\": \"<Mouse>/rightButton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \";Keyboard&Mouse\",\r\n                    \"action\": \"RightClick\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7236c0d9-6ca3-47cf-a6ee-a97f5b59ea77\",\r\n                    \"path\": \"<XRController>/devicePosition\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"XR\",\r\n                    \"action\": \"TrackedDevicePosition\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"23e01e3a-f935-4948-8d8b-9bcac77714fb\",\r\n                    \"path\": \"<XRController>/deviceRotation\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"XR\",\r\n                    \"action\": \"TrackedDeviceOrientation\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Keyboard\",\r\n            \"id\": \"46bc369b-f913-46f2-b94e-a4cf48ba363a\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Keys\",\r\n                    \"type\": \"Value\",\r\n                    \"id\": \"624c46db-101e-4a78-aad7-3fd143495217\",\r\n                    \"expectedControlType\": \"Key\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": true\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cce202f9-e686-47d2-819c-c698ac8bd647\",\r\n                    \"path\": \"<Keyboard>/capsLock\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fc88d90f-8f5c-45bb-8a9b-ddf33abaaba2\",\r\n                    \"path\": \"<Keyboard>/backspace\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=2)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1e7afb1f-ec16-4e30-985b-a2bf39de9dc9\",\r\n                    \"path\": \"<Keyboard>/space\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=3)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"cb0c9cd5-c0e0-4536-ac9e-a0917abc8acf\",\r\n                    \"path\": \"<Keyboard>/q\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=4)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"549d8ceb-a847-4c4e-8290-befeeb02a256\",\r\n                    \"path\": \"<Keyboard>/w\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=5)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5bd52e04-aa0a-45d6-b6cd-6ad82efeed0e\",\r\n                    \"path\": \"<Keyboard>/e\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=6)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"db884b45-444a-4b93-bdfb-cc9eeeb2ced6\",\r\n                    \"path\": \"<Keyboard>/r\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=7)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7796f5c6-5109-4574-81a8-2c2bc05454e9\",\r\n                    \"path\": \"<Keyboard>/t\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=8)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"6a24d611-8609-47cd-b833-b6bf5b4d4bf4\",\r\n                    \"path\": \"<Keyboard>/y\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=9)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"4cb872d1-7f26-4fbd-8956-c649546703f0\",\r\n                    \"path\": \"<Keyboard>/u\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=10)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3be70f29-6c99-4305-9ee7-e4a27dc9787e\",\r\n                    \"path\": \"<Keyboard>/i\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=11)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"097a1fed-028b-4d12-9121-8db023168927\",\r\n                    \"path\": \"<Keyboard>/o\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=12)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3fd7883e-2cd2-4a20-8afb-3aa58316b6ae\",\r\n                    \"path\": \"<Keyboard>/p\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=13)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"3a2e531c-c3b3-424e-a0f9-a72f58d2f4f0\",\r\n                    \"path\": \"<Keyboard>/a\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=14)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"8d571c14-57d5-4ebd-997b-258ab37777b3\",\r\n                    \"path\": \"<Keyboard>/s\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=15)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"66b88f6a-4c89-4a01-b962-fcbbe9adadea\",\r\n                    \"path\": \"<Keyboard>/d\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=16)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"46ca554d-1125-405c-a1af-222fd1c35ddf\",\r\n                    \"path\": \"<Keyboard>/f\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=17)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"319b5ace-cc32-45e8-b939-4daee77f405a\",\r\n                    \"path\": \"<Keyboard>/g\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=18)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"08e84f47-6925-4518-8a95-45733e31105c\",\r\n                    \"path\": \"<Keyboard>/h\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=19)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f22996d1-830e-449a-9ebc-8876b22fd181\",\r\n                    \"path\": \"<Keyboard>/j\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=20)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"5075df44-70f7-4910-bdf0-d6b53bba5552\",\r\n                    \"path\": \"<Keyboard>/k\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=21)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0c81127a-c8db-4d1a-8dc2-699530f7ef71\",\r\n                    \"path\": \"<Keyboard>/l\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=22)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"d5e15747-8d8d-4db0-ba80-a992fad6f0b3\",\r\n                    \"path\": \"<Keyboard>/z\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=23)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"b6f5f0c4-9d05-445c-b026-5925b6a4b6f7\",\r\n                    \"path\": \"<Keyboard>/x\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=24)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"0d8ddc85-a79e-4de5-8a2e-49e88c6119c5\",\r\n                    \"path\": \"<Keyboard>/c\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=25)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f6b2fa34-505b-43b1-b842-27ba84c5a7d2\",\r\n                    \"path\": \"<Keyboard>/v\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=26)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e3470f39-f1c9-4163-a7b2-ac01e0855c27\",\r\n                    \"path\": \"<Keyboard>/b\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=27)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"08e64270-6f46-4385-98e4-e65a529337f1\",\r\n                    \"path\": \"<Keyboard>/n\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=28)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1fb951b3-ed29-4d6e-b73a-65a4ed64a7db\",\r\n                    \"path\": \"<Keyboard>/m\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=29)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"698dd54e-67ee-432d-839d-a97f2aeafafb\",\r\n                    \"path\": \"<Keyboard>/1\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=30)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"1c99d6df-f64f-485f-bc41-2f36b645d4a9\",\r\n                    \"path\": \"<Keyboard>/2\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=31)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"e2e46a09-0eea-43a6-b547-b4d541185a5e\",\r\n                    \"path\": \"<Keyboard>/3\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=32)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"16302e10-3939-4751-ac03-b64bcbcd4c9d\",\r\n                    \"path\": \"<Keyboard>/4\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=33)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fb98c9a4-4927-48c1-8bdf-b417f2aafc3e\",\r\n                    \"path\": \"<Keyboard>/5\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=34)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"231497fc-9fc5-4034-8c11-bcc6187572f7\",\r\n                    \"path\": \"<Keyboard>/6\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=35)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fcba799e-a3b7-428c-bf61-355d0a45fb49\",\r\n                    \"path\": \"<Keyboard>/7\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=36)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"bde80f8d-d3fb-457c-b25b-114d5fa8564f\",\r\n                    \"path\": \"<Keyboard>/8\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=37)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"020d95f8-7a92-496e-874d-8e0389c7cee2\",\r\n                    \"path\": \"<Keyboard>/9\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=38)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"f6a4efce-e4dc-4a94-9c07-2c2019c7a0d1\",\r\n                    \"path\": \"<Keyboard>/0\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=39)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"04bb9878-3e6a-45d0-84ea-f159621aa2f8\",\r\n                    \"path\": \"<Keyboard>/enter\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale(factor=40)\",\r\n                    \"groups\": \"Keyboard&Mouse\",\r\n                    \"action\": \"Keys\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Oculus\",\r\n            \"id\": \"0bf239de-7a6e-4788-90e9-3b3a6f3f8fec\",\r\n            \"actions\": [\r\n                {\r\n                    \"name\": \"Controller\",\r\n                    \"type\": \"Button\",\r\n                    \"id\": \"681e1637-3f07-4af7-ab1d-d01b6939f66a\",\r\n                    \"expectedControlType\": \"Button\",\r\n                    \"processors\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"initialStateCheck\": false\r\n                }\r\n            ],\r\n            \"bindings\": [\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"fffb7181-9a20-4b3b-9363-0b96b4ced336\",\r\n                    \"path\": \"<XRInputV1::Oculus::OculusTouchControllerLeft>{LeftHand}/primarybutton\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"Scale\",\r\n                    \"groups\": \"XR\",\r\n                    \"action\": \"Controller\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                },\r\n                {\r\n                    \"name\": \"\",\r\n                    \"id\": \"7371a8fe-0c34-43c8-9881-8b47b4daf4ce\",\r\n                    \"path\": \"\",\r\n                    \"interactions\": \"\",\r\n                    \"processors\": \"\",\r\n                    \"groups\": \"\",\r\n                    \"action\": \"Controller\",\r\n                    \"isComposite\": false,\r\n                    \"isPartOfComposite\": false\r\n                }\r\n            ]\r\n        }\r\n    ],\r\n    \"controlSchemes\": [\r\n        {\r\n            \"name\": \"Keyboard&Mouse\",\r\n            \"bindingGroup\": \"Keyboard&Mouse\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Keyboard>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                },\r\n                {\r\n                    \"devicePath\": \"<Mouse>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Gamepad\",\r\n            \"bindingGroup\": \"Gamepad\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Gamepad>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Touch\",\r\n            \"bindingGroup\": \"Touch\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Touchscreen>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"Joystick\",\r\n            \"bindingGroup\": \"Joystick\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<Joystick>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        },\r\n        {\r\n            \"name\": \"XR\",\r\n            \"bindingGroup\": \"XR\",\r\n            \"devices\": [\r\n                {\r\n                    \"devicePath\": \"<XRController>\",\r\n                    \"isOptional\": false,\r\n                    \"isOR\": false\r\n                }\r\n            ]\r\n        }\r\n    ]\r\n}");
		m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
		m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
		m_Player_Look = m_Player.FindAction("Look", throwIfNotFound: true);
		m_Player_Fire = m_Player.FindAction("Fire", throwIfNotFound: true);
		m_UI = asset.FindActionMap("UI", throwIfNotFound: true);
		m_UI_Navigate = m_UI.FindAction("Navigate", throwIfNotFound: true);
		m_UI_Submit = m_UI.FindAction("Submit", throwIfNotFound: true);
		m_UI_Cancel = m_UI.FindAction("Cancel", throwIfNotFound: true);
		m_UI_Point = m_UI.FindAction("Point", throwIfNotFound: true);
		m_UI_Click = m_UI.FindAction("Click", throwIfNotFound: true);
		m_UI_ScrollWheel = m_UI.FindAction("ScrollWheel", throwIfNotFound: true);
		m_UI_MiddleClick = m_UI.FindAction("MiddleClick", throwIfNotFound: true);
		m_UI_RightClick = m_UI.FindAction("RightClick", throwIfNotFound: true);
		m_UI_TrackedDevicePosition = m_UI.FindAction("TrackedDevicePosition", throwIfNotFound: true);
		m_UI_TrackedDeviceOrientation = m_UI.FindAction("TrackedDeviceOrientation", throwIfNotFound: true);
		m_Keyboard = asset.FindActionMap("Keyboard", throwIfNotFound: true);
		m_Keyboard_Keys = m_Keyboard.FindAction("Keys", throwIfNotFound: true);
		m_Oculus = asset.FindActionMap("Oculus", throwIfNotFound: true);
		m_Oculus_Controller = m_Oculus.FindAction("Controller", throwIfNotFound: true);
	}

	public void Dispose()
	{
		UnityEngine.Object.Destroy(asset);
	}

	public bool Contains(InputAction action)
	{
		return asset.Contains(action);
	}

	public IEnumerator<InputAction> GetEnumerator()
	{
		return asset.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Enable()
	{
		asset.Enable();
	}

	public void Disable()
	{
		asset.Disable();
	}

	public InputAction FindAction(string actionNameOrId, bool throwIfNotFound = false)
	{
		return asset.FindAction(actionNameOrId, throwIfNotFound);
	}

	public int FindBinding(InputBinding bindingMask, out InputAction action)
	{
		return asset.FindBinding(bindingMask, out action);
	}
}
