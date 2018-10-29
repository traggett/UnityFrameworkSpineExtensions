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
			[TrackClipType(typeof(Spine3DAnimationClipAsset))]
			public class Spine3DAnimatorChannelTrack : TrackAsset
			{
				public int _animationChannel;

				public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
				{
					ScriptPlayable<Spine3DAnimatorChannelTrackMixer> playable = TimelineUtils.CreateTrackMixer<Spine3DAnimatorChannelTrackMixer>(this, graph, go, inputCount);
					Spine3DAnimatorTrack parentTrack = this.parent as Spine3DAnimatorTrack;

					if (parentTrack != null)
					{
						Spine3DAnimatorTrackMixer parentMixer = TimelineUtils.GetTrackMixer< Spine3DAnimatorTrackMixer>(graph, parentTrack);

						if (parentMixer != null)
						{
							Spine3DAnimatorChannelTrackMixer mixer = playable.GetBehaviour();
							mixer.Init(parentMixer);
							
							IEnumerable<TimelineClip> clips = GetClips();

							foreach (TimelineClip clip in clips)
							{
								Spine3DAnimationClipAsset animationClip = clip.asset as Spine3DAnimationClipAsset;

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
					Spine3DAnimatorTrack parentTrack = this.parent as Spine3DAnimatorTrack;

					if (parentTrack != null)
					{
						Spine3DAnimationClipAsset animationClip = clip.asset as Spine3DAnimationClipAsset;

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