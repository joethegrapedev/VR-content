using UnityEngine;

namespace Autohand.Demo
{
	public class HandSwapper : MonoBehaviour
	{
		public AutoHandPlayer player;

		public Hand fromHand;

		public Hand toHand;

		public GameObject fromModel;

		public GameObject toModel;

		private bool swapped;

		public void Swap()
		{
			if (!swapped)
			{
				if (toHand.left)
				{
					player.handLeft = toHand;
				}
				else
				{
					player.handRight = toHand;
				}
				fromHand.gameObject.SetActive(value: false);
				fromModel.gameObject.SetActive(value: true);
				toHand.gameObject.SetActive(value: true);
				toModel.gameObject.SetActive(value: false);
			}
			else
			{
				if (fromHand.left)
				{
					player.handLeft = fromHand;
				}
				else
				{
					player.handRight = fromHand;
				}
				fromHand.gameObject.SetActive(value: true);
				fromModel.gameObject.SetActive(value: false);
				toHand.gameObject.SetActive(value: false);
				toModel.gameObject.SetActive(value: true);
			}
			swapped = !swapped;
		}
	}
}
