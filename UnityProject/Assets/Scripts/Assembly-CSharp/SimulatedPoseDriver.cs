using System;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;

public class SimulatedPoseDriver : BasePoseProvider
{
	[Obsolete]
	public override bool TryGetPoseFromProvider(out Pose output)
	{
		output = new Pose(base.transform.position, base.transform.rotation);
		return true;
	}
}
