using UnityEngine;
using UnityEngine.UI;

public class HoldClick : MonoBehaviour
{
	public float indicatorTimer = 1f;

	public float maxIndicatorTimer = 1f;

	public Image loadingUI;

	public GameObject interactionMenu;

	public GameObject interactionObject;

	public GameObject alertCanvas;

	private GameObject loadingVisual;

	private bool shouldUpdate;

	private bool detectPlayer;

	private bool ButtonDown;

	private bool ButtonUp;

	public void HoldButton()
	{
		ButtonUp = false;
		ButtonDown = true;
	}

	public void ReleaseButton()
	{
		ButtonDown = false;
		ButtonUp = true;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.tag == "Player")
		{
			Debug.Log("QWIUDHOIDHYWIUDWHUDH");
			detectPlayer = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.tag == "Player")
		{
			Debug.Log("fuck");
			detectPlayer = false;
			Reset();
		}
	}

	private void Update()
	{
		if (ButtonDown)
		{
			if (detectPlayer)
			{
				indicatorTimer -= Time.deltaTime;
				loadingUI.enabled = true;
				loadingUI.fillAmount = indicatorTimer;
				if (indicatorTimer <= 0f)
				{
					indicatorTimer = maxIndicatorTimer;
					loadingUI.fillAmount = maxIndicatorTimer;
					loadingUI.enabled = false;
					Object.Destroy(loadingVisual);
					ButtonDown = false;
					ButtonUp = true;
					interactionMenu.SetActive(value: true);
					interactionObject.GetComponent<HoldClick>().enabled = false;
				}
			}
			else
			{
				Invoke("ShowAlert", 0f);
			}
		}
		else if (shouldUpdate)
		{
			indicatorTimer += Time.deltaTime;
			loadingUI.fillAmount = indicatorTimer;
			if (indicatorTimer >= maxIndicatorTimer)
			{
				indicatorTimer = maxIndicatorTimer;
				loadingUI.fillAmount = maxIndicatorTimer;
				loadingUI.enabled = false;
				shouldUpdate = false;
			}
		}
		if (ButtonUp)
		{
			shouldUpdate = false;
			Reset();
		}
	}

	private void Reset()
	{
		loadingUI.fillAmount = 0f;
		indicatorTimer = 1f;
		maxIndicatorTimer = 1f;
	}

	private void ShowAlert()
	{
		alertCanvas.SetActive(value: true);
		Invoke("HideAlert", 1f);
	}

	private void HideAlert()
	{
		alertCanvas.SetActive(value: false);
	}
}
