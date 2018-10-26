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
			public class SpineAnimationAttacher : MonoBehaviour
			{
				public SkeletonDataAsset _skeletonDataSource;

				[SpineSlot]
				public string _targetSlot;

				[SpineSlot(dataField: "_skeletonDataSource")]
				public string _sourceSlot;

				private SpineAnimator _attachedAnimation;
				private SkeletonAnimation _skeletonRenderer;
				private Slot _slot;
				private Slot _slotSource;

				void Awake()
				{
					_skeletonRenderer = GetComponent<SkeletonAnimation>();
					_skeletonRenderer.Initialize(false);
					_slot = _skeletonRenderer.skeleton.FindSlot(_targetSlot);

					if (_skeletonDataSource != null)
					{
						SetAttachedAnimation(_skeletonDataSource, _sourceSlot);
					}
				}

				void Update()
				{
					if (_attachedAnimation != null && _slot != null && _slotSource != null)
					{
						_slot.Attachment = _slotSource.Attachment;
					}
				}

				public SpineAnimator GetAttachedAnimation()
				{
					return _attachedAnimation;
				}

				public Material GetAttachedMeshMaterial()
				{
					if (_slotSource.Attachment != null)
					{
						MeshAttachment mesh = _slotSource.Attachment as MeshAttachment;
						AtlasRegion atlas = mesh.RendererObject as AtlasRegion;
						return atlas.page.rendererObject as Material;
					}

					return null;
				}

				public void SetAttachedAnimation(SkeletonDataAsset skeletonDataAsset, string slotSourceName)
				{
					SkeletonAnimation skeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(skeletonDataAsset);
					skeletonAnimation.transform.parent = this.transform;
					skeletonAnimation.gameObject.name = skeletonDataAsset.name + "(Attached Animation)";
					skeletonAnimation.GetComponent<Renderer>().enabled = false;
					_attachedAnimation = skeletonAnimation.gameObject.AddComponent<SpineAnimator>();
					_slotSource = _attachedAnimation.GetSkeletonAnimation().Skeleton.FindSlot(slotSourceName);
				}
			}
		}
	}
}