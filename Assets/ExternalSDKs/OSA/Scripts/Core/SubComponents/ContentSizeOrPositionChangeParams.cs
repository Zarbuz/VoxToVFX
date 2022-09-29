//#define DEBUG_COMPUTE_VISIBILITY


namespace Com.TheFallenGames.OSA.Core.SubComponents
{
	internal struct ContentSizeOrPositionChangeParams
	{
		public bool cancelSnappingIfAny,
		fireScrollPositionChangedEvent,
		keepVelocity,
		allowOutsideBounds, // bounds are VPS/VPE when virtualizing and "ContentInferredStart_AccordingToPivot"/"ContentInferredEnd_AccordingToPivot" when not
		contentEndEdgeStationary;
		internal double? contentInsetOverride;

		public ComputeVisibilityParams computeVisibilityParams;
	}
}
