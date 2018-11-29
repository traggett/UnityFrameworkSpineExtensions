using UnityEngine;
using Spine;
using AnimationState = Spine.AnimationState;
using Spine.Unity;
using System.Collections.Generic;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			[RequireComponent(typeof(SpineAnimator))]
			public class SpineAnimatorRootMotion : MonoBehaviour
			{
				#region Inspector
				[SpineBone]
				[SerializeField]
				protected string _sourceBoneName = "root";
				public bool _useX = true;
				public bool _useY = false;
				public bool _applyToTransform = true;

				[SpineBone]
				[SerializeField]
				protected List<string> _siblingBoneNames = new List<string>();
				#endregion

				public delegate void OnMotion(SpineAnimatorRootMotion rootMotion, Vector2 motion);

				public event OnMotion _onMotion;

				protected Bone _bone;
				protected int _boneIndex;

				public readonly List<Bone> _siblingBones = new List<Bone>();

				private SpineAnimator _spineAnimator;
				private AnimationState _state;

				private void Start()
				{
					_spineAnimator = GetComponent<SpineAnimator>();
					
					ISkeletonAnimation s = _spineAnimator.GetSkeletonAnimation() as ISkeletonAnimation;
					if (s != null) s.UpdateLocal += HandleUpdateLocal;

					IAnimationStateComponent sa = _spineAnimator.GetSkeletonAnimation() as IAnimationStateComponent;
					if (sa != null) this._state = sa.AnimationState;

					SetSourceBone(_sourceBoneName);

					Skeleton skeleton = s.Skeleton;
					_siblingBones.Clear();
					foreach (string bn in _siblingBoneNames)
					{
						Bone b = skeleton.FindBone(bn);
						if (b != null) _siblingBones.Add(b);
					}
				}

				private void HandleUpdateLocal(ISkeletonAnimation animatedSkeletonComponent)
				{
					if (!this.isActiveAndEnabled) return; // Root motion is only applied when component is enabled.

					Vector2 localDelta = Vector2.zero;

					int boneIndex = this._boneIndex;

					TrackEntry[] tracks = _spineAnimator.GetChannelTracks(0);

					if (tracks != null)
					{
						for (int i = 0; i < tracks.Length; i++)
						{
							if (tracks[i] == null)
								continue;

							var a = tracks[i].Animation;
							var tt = a.FindTranslateTimelineForBone(boneIndex);

							if (tt != null)
							{
								// 1. Get the delta position from the root bone's timeline.
								float start = tracks[i].animationLast;
								float end = tracks[i].AnimationTime;
								Vector2 currentDelta;
								if (start > end)
									currentDelta = (tt.Evaluate(end) - tt.Evaluate(0)) + (tt.Evaluate(a.duration) - tt.Evaluate(start));  // Looped
								else if (start != end)
									currentDelta = tt.Evaluate(end) - tt.Evaluate(start);  // Non-looped
								else
									currentDelta = Vector2.zero;

								// 2. Apply alpha to the delta position (based on AnimationState.cs)
								currentDelta *= tracks[i].alpha;

								// 3. Add the delta from the track to the accumulated value.
								localDelta += currentDelta;
							}
						}

						// 4. Apply flip to the delta position.
						Skeleton skeleton = animatedSkeletonComponent.Skeleton;
						if (skeleton.flipX) localDelta.x = -localDelta.x;
						if (skeleton.flipY) localDelta.y = -localDelta.y;

						// 5. Apply root motion to Transform or RigidBody;
						if (!_useX) localDelta.x = 0f;
						if (!_useY) localDelta.y = 0f;

						if (_applyToTransform)
							this.transform.Translate(localDelta, Space.Self);

						if (_onMotion != null)
							_onMotion.Invoke(this, localDelta);

						if (localDelta != Vector2.zero)
						{
							// 6. Position bones to be base position
							// BasePosition = new Vector2(0, 0);
							foreach (Bone b in _siblingBones)
							{
								if (_useX) b.x -= _bone.x;
								if (_useY) b.y -= _bone.y;
							}

							if (_useX) _bone.x = 0;
							if (_useY) _bone.y = 0;
						}
					}
				}

				public void SetSourceBone(string name)
				{
					var skeleton = _spineAnimator.GetSkeletonAnimation().Skeleton;
					int bi = skeleton.FindBoneIndex(name);
					if (bi >= 0)
					{
						this._boneIndex = bi;
						this._bone = skeleton.bones.Items[bi];
					}
					else {
						Debug.Log("Bone named \"" + name + "\" could not be found.");
						this._boneIndex = 0;
						this._bone = skeleton.RootBone;
					}
				}
			}
		}
	}
}
