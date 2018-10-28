using Framework.Playables;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			[Serializable]
			[NotKeyable]
			public class Spine3DAnimatorClipAsset : PlayableAsset, ITimelineClipAsset
			{
				public string _animationId;
				public double _animationDuration = PlayableBinding.DefaultDuration;

				private Spine3DAnimatorTrack _parentAnimatorTrack;

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
					var playable = ScriptPlayable<Spine3DAnimatorPlayableBehaviour>.Create(graph, new Spine3DAnimatorPlayableBehaviour());
					Spine3DAnimatorPlayableBehaviour clone = playable.GetBehaviour();
					
					Spine3DAnimatorTrackMixer trackMixer = TimelineUtils.GetTrackMixer<Spine3DAnimatorTrackMixer>(graph, _parentAnimatorTrack);

					clone._clipAsset = this;
					
					if (trackMixer != null && trackMixer.GetTrackBinding() != null && !string.IsNullOrEmpty(_animationId))
					{
						clone._animation = _animationId;
						clone._animationDuration = trackMixer.GetTrackBinding().GetAnimationLength(_animationId);
					}

					return playable;
				}

				public void SetParentTrack(Spine3DAnimatorTrack track)
				{
					_parentAnimatorTrack = track;
				}

				public Spine3DAnimatorTrack GetParentTrack()
				{
					return _parentAnimatorTrack;
				}
			}
		}
	}
}
