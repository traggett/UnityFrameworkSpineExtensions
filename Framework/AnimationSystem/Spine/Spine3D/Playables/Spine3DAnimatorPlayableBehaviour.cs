using UnityEngine.Playables;
using Spine;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public class Spine3DAnimatorPlayableBehaviour : PlayableBehaviour
			{
				public PlayableAsset _clipAsset;
				public string _animationId;
				public float _animationDuration;
				public float _animationSpeed;

				public Animation _proxyAnimation;
				public eSpine3DOrientation _proxyAnimationOrientations;
			}
		}
	}
}
