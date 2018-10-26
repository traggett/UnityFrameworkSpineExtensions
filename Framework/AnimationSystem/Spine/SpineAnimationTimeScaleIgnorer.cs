using UnityEngine;
using Spine.Unity;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			[RequireComponent(typeof(SkeletonAnimation))]
			public class SpineAnimationTimeScaleIgnorer : MonoBehaviour
			{
				SkeletonAnimation _skeletonAnimation;

				void Awake()
				{
					_skeletonAnimation = GetComponent<SkeletonAnimation>();
				}

				void Update()
				{
					if (Mathf.Abs(Time.timeScale) > 0.0f)
						_skeletonAnimation.timeScale = 1.0f / Time.timeScale;
				}
			}
		}
	}
}