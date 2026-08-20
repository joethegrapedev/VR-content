using UnityEngine;
using UnityEngine.InputSystem;

public class InputActionEnabler : MonoBehaviour
{
	[SerializeField]
	private InputActionAsset m_ActionAsset;

	public InputActionAsset actionAsset
	{
		get
		{
			return m_ActionAsset;
		}
		set
		{
			m_ActionAsset = value;
		}
	}

	private void OnEnable()
	{
		if (m_ActionAsset != null)
		{
			m_ActionAsset.Enable();
		}
	}
}
