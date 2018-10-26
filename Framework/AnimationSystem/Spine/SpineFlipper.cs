using UnityEngine;
using Spine.Unity;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			[ExecuteInEditMode]
			[RequireComponent(typeof(SkeletonAnimation))]
			public class SpineFlipper : MonoBehaviour
			{
				public bool _flipX;
				public bool _flipY;

				#region Private Data
				private SkeletonAnimation _skeletonAnimation;
				#endregion

				#region MonoBehaviour Calls
				void Awake()
				{
					_skeletonAnimation = GetComponent<SkeletonAnimation>();
				}

				void Update()
				{
					if (_skeletonAnimation != null && _skeletonAnimation.valid && (_skeletonAnimation.Skeleton.FlipX != _flipX || _skeletonAnimation.Skeleton.FlipY != _flipY))
					{
						_skeletonAnimation.Skeleton.FlipX = _flipX;
						_skeletonAnimation.Skeleton.FlipY = _flipY;

						if (Application.isEditor)
							_skeletonAnimation.Skeleton.UpdateWorldTransform();
					}
				}
				#endregion
			}
		}
	}
}
