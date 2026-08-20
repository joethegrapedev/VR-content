using UnityEngine;

public class CollisionIgnores : MonoBehaviour
{
	public bool disableOnStart = true;

	public Collider[] cols1;

	public Collider[] cols2;

	private void Start()
	{
		DisableCollisions();
	}

	public void EnableCollisions()
	{
		for (int i = 0; i < cols1.Length; i++)
		{
			for (int j = 0; j < cols2.Length; j++)
			{
				Physics.IgnoreCollision(cols1[i], cols2[j], ignore: false);
			}
		}
	}

	public void DisableCollisions()
	{
		for (int i = 0; i < cols1.Length; i++)
		{
			for (int j = 0; j < cols2.Length; j++)
			{
				Physics.IgnoreCollision(cols1[i], cols2[j], ignore: true);
			}
		}
	}
}
