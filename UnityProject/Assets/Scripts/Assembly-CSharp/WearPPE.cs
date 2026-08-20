using UnityEngine;

public class WearPPE : MonoBehaviour
{
	public GameObject PPEGameobject;

	public GameObject correctPPEcanvas;

	public CanvasGroup correctPPECanvasGroup;

	public GameObject wrongPPEcanvas;

	public CanvasGroup wrongPPECanvasGroup;

	public Behaviour WearPPEScript;

	public Collider DoorScript;

	private void Start()
	{
		DoorScript.enabled = false;
	}

	public void CheckPPEType()
	{
		if (PPEGameobject.tag == "CorrectPPE")
		{
			FadeInCorrectCanvas();
			LTSeq lTSeq = LeanTween.sequence();
			lTSeq.append(3f);
			lTSeq.append(delegate
			{
				FadeOutCorrectCanvas();
			});
			correctPPEcanvas.SetActive(value: true);
			WearPPEScript.enabled = false;
			PPEGameobject.SetActive(value: false);
			DoorScript.enabled = true;
		}
		else if (PPEGameobject.tag == "WrongPPE")
		{
			FadeInWrongCanvas();
			LTSeq lTSeq2 = LeanTween.sequence();
			lTSeq2.append(3f);
			lTSeq2.append(delegate
			{
				FadeOutWrongCanvas();
			});
		}
	}

	private void FadeInCorrectCanvas()
	{
		LeanTween.alphaCanvas(correctPPECanvasGroup, 1f, 1f);
	}

	private void FadeOutCorrectCanvas()
	{
		LeanTween.alphaCanvas(correctPPECanvasGroup, 0f, 1f);
	}

	private void FadeInWrongCanvas()
	{
		LeanTween.alphaCanvas(wrongPPECanvasGroup, 1f, 1f);
	}

	private void FadeOutWrongCanvas()
	{
		LeanTween.alphaCanvas(wrongPPECanvasGroup, 0f, 1f);
	}
}
