using Spine.Unity;
using System;
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
			[Serializable]
			[NotKeyable]
			public class SpineAnimationClipAsset : PlayableAsset, ITimelineClipAsset
			{
				public string _animationId;
				public double _animationDuration = PlayableBinding.DefaultDuration;

				private SpineAnimatorTrack _parentAnimatorTrack;

				public ClipCaps clipCaps
				{
					get { return ClipCaps.Blending | ClipCaps.Extrapolation | ClipCaps.Looping; }
				}

				public override double duration
				{
					get
					{
						if (_animationDuration <= 0.0f)
							return PlayableBinding.DefaultDuration;

						return _animationDuration;
					}
				}

				public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
				{
					ScriptPlayable<SpineAnimatorPlayableBehaviour> playable = ScriptPlayable<SpineAnimatorPlayableBehaviour>.Create(graph, new SpineAnimatorPlayableBehaviour());
					SpineAnimatorPlayableBehaviour clone = playable.GetBehaviour();
					
					SpineAnimatorTrackMixer trackMixer = TimelineUtils.GetTrackMixer<SpineAnimatorTrackMixer>(graph, _parentAnimatorTrack);

					clone._clipAsset = this;
					
					if (trackMixer != null && trackMixer.GetTrackBinding() != null && !string.IsNullOrEmpty(_animationId))
					{
						SkeletonAnimation skeletonAnimation = trackMixer.GetTrackBinding();
						clone._animation = skeletonAnimation.skeletonDataAsset.GetAnimationStateData().SkeletonData.FindAnimation(_animationId);
					}

					return playable;
				}

				public void SetParentTrack(SpineAnimatorTrack track)
				{
					_parentAnimatorTrack = track;
				}

				public SpineAnimatorTrack GetParentTrack()
				{
					return _parentAnimatorTrack;
				}
			}
		}
	}
}
