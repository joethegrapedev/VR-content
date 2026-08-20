using UnityEngine;

namespace Autohand.Demo
{
	public class XRHandPlayerControllerLink : MonoBehaviour
	{
		public XRHandControllerLink moveController;

		public XRHandControllerLink turnController;

		public AutoHandPlayer player;

		[Header("Input")]
		public Common2DAxis moveAxis;

		public Common2DAxis turnAxis;

		private void Update()
		{
			player.Move(moveController.GetAxis2D(moveAxis));
			player.Turn(turnController.GetAxis2D(turnAxis).x);
		}

		private void FixedUpdate()
		{
			player.Move(moveController.GetAxis2D(moveAxis));
		}
	}
}
