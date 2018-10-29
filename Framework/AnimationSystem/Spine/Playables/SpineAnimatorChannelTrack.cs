using System.Collections.Generic;
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
			[TrackClipType(typeof(SpineAnimationClipAsset))]
			[TrackClipType(typeof(SpineProxyAnimationClipAsset))]
			public class SpineAnimatorChannelTrack : TrackAsset
			{
				public int _animationChannel;

				public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
				{
					ScriptPlayable<SpineAnimatorChannelTrackMixer> playable = TimelineUtils.CreateTrackMixer<SpineAnimatorChannelTrackMixer>(this, graph, go, inputCount);
					SpineAnimatorTrack parentTrack = this.parent as SpineAnimatorTrack;

					if (parentTrack != null)
					{
						SpineAnimatorTrackMixer parentMixer = TimelineUtils.GetTrackMixer< SpineAnimatorTrackMixer>(graph, parentTrack);

						if (parentMixer != null)
						{
							SpineAnimatorChannelTrackMixer mixer = playable.GetBehaviour();
							mixer.Init(parentMixer);
							
							IEnumerable<TimelineClip> clips = GetClips();

							foreach (TimelineClip clip in clips)
							{
								SpineAnimationClipAsset animationClip = clip.asset as SpineAnimationClipAsset;

								if (animationClip != null)
								{
									clip.displayName = animationClip._animationId;
									animationClip.SetParentTrack(parentTrack);
								}
							}
						}
					}

					return playable;
				}

				protected override void OnCreateClip(TimelineClip clip)
				{
					SpineAnimatorTrack parentTrack = this.parent as SpineAnimatorTrack;

					if (parentTrack != null)
					{
						SpineAnimationClipAsset animationClip = clip.asset as SpineAnimationClipAsset;

						if (animationClip != null)
						{
							animationClip.SetParentTrack(parentTrack);
						}
					}
				}
			}
		}
	}
}