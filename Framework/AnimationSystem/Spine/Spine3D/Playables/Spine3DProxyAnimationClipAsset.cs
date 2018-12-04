using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Framework.Playables;
	using Spine;

	namespace AnimationSystem
	{
		namespace Spine
		{
			[Serializable]
			[NotKeyable]
			public class Spine3DProxyAnimationClipAsset : Spine3DAnimationClipAsset
			{
				//The asset containing the animation to play
				public SkeletonDataAsset _animationSource;
				//The animation will only play on animation sets marked with these orientations
				public eSpine3DOrientation _validOrientations;

				public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
				{
					ScriptPlayable<Spine3DAnimatorPlayableBehaviour> playable = ScriptPlayable<Spine3DAnimatorPlayableBehaviour>.Create(graph, new Spine3DAnimatorPlayableBehaviour());
					Spine3DAnimatorPlayableBehaviour clone = playable.GetBehaviour();

					Spine3DAnimatorTrackMixer trackMixer = TimelineUtils.GetTrackMixer<Spine3DAnimatorTrackMixer>(graph, _parentAnimatorTrack);

					clone._clipAsset = this;

					if (_animationSource != null && !string.IsNullOrEmpty(_animationId))
					{
						SkeletonData skeletonData = _animationSource.GetSkeletonData(false);
						Animation animation = skeletonData.FindAnimation(_animationId);

						clone._animationDuration = animation != null ? animation.Duration : (float)PlayableBinding.DefaultDuration;
						clone._animationSpeed = _animationSpeed;

						clone._proxyAnimation = animation;
						clone._proxyAnimationOrientations = _validOrientations;
					}

					return playable;
				}
			}
		}
	}
}
