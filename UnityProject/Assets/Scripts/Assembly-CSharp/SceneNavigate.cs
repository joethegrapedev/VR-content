using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigate : MonoBehaviour
{
	public void MoveToScene(string scenename)
	{
		SceneManager.LoadScene(scenename);
	}
}
