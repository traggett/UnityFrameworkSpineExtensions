using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Spine.Unity;

namespace Framework
{
	using Playables;

	namespace AnimationSystem
	{
		namespace Spine
		{
			[TrackColor(255f / 255f, 64f / 255f, 0f / 255f)]
			[TrackBindingType(typeof(SkeletonAnimation))]
			[TrackClipType(typeof(SpineMasterClipAsset), false)]
			//[SupportsChildTracks(typeof(SpineAnimatorChannelTrack), 1]		//Hopefully Unity will make this attribute public soon :/
			public class SpineAnimatorTrack : TrackAsset
			{
				//Reset pose before evaluating clips each frame
				public bool _resetPose = true;

				public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
				{
					EnsureMasterClipExists();
					return TimelineUtils.CreateTrackMixer<SpineAnimatorTrackMixer>(this, graph, go, inputCount);
				}

				public void EnsureMasterClipExists()
				{
					TimelineClip masterClip = null;

					foreach (TimelineClip clip in GetClips())
					{
						masterClip = clip;
						break;
					}

					if (masterClip == null)
					{
						masterClip = CreateDefaultClip();
					}

					//Set clips duration to match max duration of timeline
					masterClip.start = 0;
					masterClip.duration = 0;
					masterClip.duration = this.timelineAsset.duration;
					masterClip.displayName = GetMasterClipName();
				}

				protected virtual string GetMasterClipName()
				{
					return "(Spine Animation)";
				}
			}
		}
	}
}