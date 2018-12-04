using UnityEngine.Playables;
using Spine;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public class SpineAnimatorPlayableBehaviour : PlayableBehaviour
			{
				public PlayableAsset _clipAsset;
				public Animation _animation;
				public float _animationSpeed;
			}
		}
	}
}
