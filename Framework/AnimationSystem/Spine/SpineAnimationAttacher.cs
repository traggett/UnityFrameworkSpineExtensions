using UnityEngine;

using Spine;
using Spine.Unity;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			// Attaches another Spine animation to a slot on the SkeletonAnimation this component is on.
			[RequireComponent(typeof(SkeletonAnimation))]
			[ExecuteInEditMode]
			public class SpineAnimationAttacher : MonoBehaviour
			{
				[SpineSlot]
				public string _targetSlot;

				public SpineAnimator _animation;

				[SpineSlot(dataField: "_animation")]
				public string _sourceSlot;

				private SkeletonAnimation _skeletonAnimation;
				private Slot _slot;
				private Slot _slotSource;

				private void Awake()
				{
					Initialize();
				}

				void Initialize()
				{
					if (_skeletonAnimation == null)
					{
						_skeletonAnimation = GetComponent<SkeletonAnimation>();
						_skeletonAnimation.Initialize(false);
						_skeletonAnimation.OnRebuild += OnRebuildAnimator;
						_skeletonAnimation.UpdateComplete += OnUpdateAnimator;
						_slot = _skeletonAnimation.Skeleton.FindSlot(_targetSlot);
						UpdateSlotAttachment();
					}

					if (_slotSource == null && _animation != null)
					{
						_slotSource = _animation.Skeleton.FindSlot(_sourceSlot);
					}
				}

				public Material GetAttachedMeshMaterial()
				{
					Initialize();

					if (_slotSource != null && _slotSource.Attachment != null)
					{
						MeshAttachment mesh = _slotSource.Attachment as MeshAttachment;
						AtlasRegion atlas = mesh.RendererObject as AtlasRegion;
						return atlas.page.rendererObject as Material;
					}

					return null;
				}

				private void OnRebuildAnimator(SkeletonRenderer skeletonRenderer)
				{
					UpdateSlotAttachment();
				}

				private void OnUpdateAnimator(ISkeletonAnimation animated)
				{
					UpdateSlotAttachment();
				}

				private void UpdateSlotAttachment()
				{
					if (_slot != null && _slotSource != null)
					{
						_slot.Attachment = _slotSource.Attachment;
					}
				}
			}
		}
	}
}