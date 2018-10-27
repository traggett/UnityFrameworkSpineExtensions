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
				public string _animation;
				public float _animationDuration;
			}
		}
	}
}
