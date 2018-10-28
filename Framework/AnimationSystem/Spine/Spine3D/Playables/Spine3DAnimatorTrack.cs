using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Playables;

	namespace AnimationSystem
	{
		namespace Spine
		{
			[TrackColor(255f / 255f, 64f / 255f, 0f / 255f)]
			[TrackBindingType(typeof(Spine3DAnimator))]
			//[SupportsChildTracks(typeof(Spine3DAnimatorChannelTrack), 1]		//Hopefully Unity will make this attribute public soon :/
			public class Spine3DAnimatorTrack : SpineAnimatorTrack
			{
				public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
				{
					EnsureMasterClipExists();
					return TimelineUtils.CreateTrackMixer<Spine3DAnimatorTrackMixer>(this, graph, go, inputCount);
				}

				protected override string GetMasterClipName()
				{
					return "(Spine 3D Animation)";
				}
			}
		}
	}
}