using System.Collections.Generic;
using UnityEngine;

public class randomSpawn : MonoBehaviour
{
	public GameObject[] locations;

	public List<GameObject> objects = new List<GameObject>();

	private void Start()
	{
		for (int i = 0; i < locations.Length; i++)
		{
			int index = Random.Range(0, objects.Count);
			while (objects[index].gameObject == null)
			{
				index = Random.Range(0, 2);
			}
			objects[index].gameObject.transform.position = locations[i].transform.position;
			objects[index].gameObject.transform.rotation = locations[i].transform.rotation;
			objects.Remove(objects[index]);
		}
	}
}
