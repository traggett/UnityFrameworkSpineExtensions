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

				[SpineSkin(dataField: "_skeletonDataSource")]
				public string _skinName;

				private SpineAnimator _attachedAnimation;
				private SkeletonAnimation _skeletonRenderer;
				private Slot _slot;
				private Slot _slotSource;

				void Awake()
				{
					_skeletonRenderer = GetComponent<SkeletonAnimation>();
					_skeletonRenderer.Initialize(false);
					_skeletonRenderer.OnRebuild += OnRendererRebuild;
					_slot = _skeletonRenderer.skeleton.FindSlot(_targetSlot);

					if (_attachedAnimation == null)
					{
						InitAttachedAnimation();
					}
				}

				void Update()
				{
					OnRendererRebuild(null);
				}

				public SpineAnimator GetAnimator()
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

				public void SetAttachedAnimation(SkeletonDataAsset skeletonDataAsset, string slotSourceName, string skinName = "default")
				{
					_skeletonDataSource = skeletonDataAsset;
					_sourceSlot = slotSourceName;
					_skinName = skinName;
					InitAttachedAnimation();
				}

				private void OnRendererRebuild(SkeletonRenderer skeletonRenderer)
				{
					if (_attachedAnimation != null && _slot != null && _slotSource != null)
					{
						_slot.Attachment = _slotSource.Attachment;
					}
				}

				private void InitAttachedAnimation()
				{
					if (_attachedAnimation != null)
					{
						Destroy(_attachedAnimation.gameObject);
					}

					if (_skeletonDataSource != null)
					{
						SkeletonAnimation skeletonAnimation = SkeletonAnimation.NewSkeletonAnimationGameObject(_skeletonDataSource);
						skeletonAnimation.transform.parent = this.transform;
						skeletonAnimation.gameObject.name = _skeletonDataSource.name + "(Attached Animation)";
						skeletonAnimation.initialSkinName = _skinName;
						skeletonAnimation.GetComponent<Renderer>().enabled = false;
						skeletonAnimation.Initialize(true);

						_attachedAnimation = skeletonAnimation.gameObject.AddComponent<SpineAnimator>();
						_slotSource = _attachedAnimation.GetSkeletonAnimation().Skeleton.FindSlot(_sourceSlot);
					}
				}
			}
		}
	}
}