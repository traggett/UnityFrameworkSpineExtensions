using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Spine;

	namespace AnimationSystem
	{
		namespace Spine
		{
			[Serializable]
			[NotKeyable]
			public class SpineProxyAnimationClipAsset : SpineAnimationClipAsset
			{
				public SkeletonDataAsset _animations;

				public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
				{
					ScriptPlayable<SpineAnimatorPlayableBehaviour> playable = ScriptPlayable<SpineAnimatorPlayableBehaviour>.Create(graph, new SpineAnimatorPlayableBehaviour());
					SpineAnimatorPlayableBehaviour clone = playable.GetBehaviour();
					
					clone._clipAsset = this;
					
					if (!string.IsNullOrEmpty(_animationId))
					{
						SkeletonData skeletonData = _animations.GetSkeletonData(false);
						Animation animation = skeletonData.FindAnimation(_animationId);
						clone._animation = animation;
					}

					return playable;
				}
			}
		}
	}
}
