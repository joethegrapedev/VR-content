using System.Collections;
using UnityEngine;

public class Move : MonoBehaviour
{
	[SerializeField]
	private GameObject playerInitialPosition;

	[SerializeField]
	private GameObject WayPoint;

	public GameObject WayPoint2;

	public Vector3 initialRotation;

	public Vector3 turningRotation;

	public float speed;

	private void Start()
	{
		StartCoroutine(MoveAtoB(playerInitialPosition, WayPoint, speed));
	}

	private IEnumerator MoveAtoB(GameObject gameObjectA, GameObject gameObjectB, float speedTranslation)
	{
		while (gameObjectA.transform.position != gameObjectB.transform.position)
		{
			gameObjectA.transform.eulerAngles = initialRotation;
			gameObjectA.transform.position = Vector3.MoveTowards(gameObjectA.transform.position, gameObjectB.transform.position, speedTranslation * Time.deltaTime);
			yield return null;
		}
		StartCoroutine(MoveBtoA(playerInitialPosition, WayPoint2, speed));
	}

	private IEnumerator MoveBtoA(GameObject gameObjectA, GameObject gameObjectC, float speedTranslation)
	{
		while (gameObjectA.transform.position != gameObjectC.transform.position)
		{
			gameObjectA.transform.eulerAngles = turningRotation;
			gameObjectA.transform.position = Vector3.MoveTowards(gameObjectA.transform.position, gameObjectC.transform.position, speedTranslation * Time.deltaTime);
			yield return null;
		}
		StartCoroutine(MoveAtoB(playerInitialPosition, WayPoint, speed));
	}
}
