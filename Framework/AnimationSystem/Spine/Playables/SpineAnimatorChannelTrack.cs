using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			[TrackColor(255f / 255f, 64f / 255f, 0f / 255f)]
			[TrackClipType(typeof(SpineAnimationClipAsset))]
			public class SpineAnimatorChannelTrack : TrackAsset
			{
				public int _animationChannel;

				public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
				{
					ScriptPlayable<SpineAnimatorChannelTrackMixer> playable = ScriptPlayable<SpineAnimatorChannelTrackMixer>.Create(graph, inputCount);
					
					SpineAnimatorTrack parentTrack = this.parent as SpineAnimatorTrack;

					if (parentTrack != null)
					{
						SpineAnimatorTrackMixer parentMixer = SpineAnimatorTrack.GetTrackMixer(graph, parentTrack);

						if (parentMixer != null)
						{
							SpineAnimatorChannelTrackMixer mixer = playable.GetBehaviour();
							PlayableDirector playableDirector = go.GetComponent<PlayableDirector>();

							mixer.Init(this, playableDirector, parentMixer);
							
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


				public TimelineClip GetClip(UnityEngine.Object clipAsset)
				{
					IEnumerable<TimelineClip> clips = GetClips();

					foreach (TimelineClip clip in clips)
					{
						if (clip.asset == clipAsset)
							return clip;
					}

					return null;
				}
			}
		}
	}
}