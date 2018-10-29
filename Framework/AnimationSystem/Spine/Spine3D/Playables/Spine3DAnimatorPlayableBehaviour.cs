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

				public Animation _proxyAnimation;
				public eSpine3DOrientation _proxyAnimationOrientations;
			}
		}
	}
}
