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
					if (_skeletonAnimation != null && _skeletonAnimation.valid && (Mathf.Sign(_skeletonAnimation.Skeleton.ScaleX) != (_flipX ? -1f : 1f) || Mathf.Sign(_skeletonAnimation.Skeleton.ScaleY) != (_flipY ? -1f : 1f)))
					{
						_skeletonAnimation.Skeleton.ScaleX = _flipX ? - Mathf.Abs(_skeletonAnimation.Skeleton.ScaleX) : Mathf.Abs(_skeletonAnimation.Skeleton.ScaleX);
						_skeletonAnimation.Skeleton.ScaleY = _flipY ? -Mathf.Abs(_skeletonAnimation.Skeleton.ScaleY) : Mathf.Abs(_skeletonAnimation.Skeleton.ScaleY);

						if (Application.isEditor)
							_skeletonAnimation.Skeleton.UpdateWorldTransform();
					}
				}
				#endregion
			}
		}
	}
}
