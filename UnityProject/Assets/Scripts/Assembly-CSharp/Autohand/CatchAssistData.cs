namespace Autohand
{
	internal struct CatchAssistData
	{
		public Grabbable grab;

		public float estimatedRadius;

		public CatchAssistData(Grabbable grab, float estimatedRadius)
		{
			this.grab = grab;
			this.estimatedRadius = estimatedRadius;
		}
	}
}
