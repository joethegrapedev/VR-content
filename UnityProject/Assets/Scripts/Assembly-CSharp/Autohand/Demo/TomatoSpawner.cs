using System.Collections.Generic;
using UnityEngine;

namespace Autohand.Demo
{
	public class TomatoSpawner : MonoBehaviour
	{
		public GameObject[] tomatoes;

		private List<GameObject> copies;

		private void Start()
		{
			copies = new List<GameObject>();
			GameObject[] array = tomatoes;
			foreach (GameObject gameObject in array)
			{
				GameObject gameObject2 = Object.Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation);
				gameObject2.transform.position += new Vector3(0f, 0.2f, 0f);
				gameObject2.SetActive(value: false);
				copies.Add(gameObject2);
			}
		}

		public void SpawnTomato()
		{
			int index = Random.Range(0, copies.Count - 1);
			Object.Instantiate(copies[index], copies[index].transform.position, copies[index].transform.rotation).SetActive(value: true);
		}
	}
}
