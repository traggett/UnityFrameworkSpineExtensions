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
			[RequireComponent(typeof(SkeletonAnimation))]
			public class SpineRootMotion : MonoBehaviour
			{
				#region Inspector
				[SpineBone]
				[SerializeField]
				protected string sourceBoneName = "root";
				public bool useX = true;
				public bool useY = false;

				[SpineBone]
				[SerializeField]
				protected List<string> siblingBoneNames = new List<string>();
				#endregion

				protected Bone bone;
				protected int boneIndex;
				public readonly List<Bone> siblingBones = new List<Bone>();

				ISkeletonComponent skeletonComponent;
				AnimationState state;

				void Start()
				{
					skeletonComponent = GetComponent<ISkeletonComponent>();

					var s = skeletonComponent as ISkeletonAnimation;
					if (s != null) s.UpdateLocal += HandleUpdateLocal;

					var sa = skeletonComponent as IAnimationStateComponent;
					if (sa != null) this.state = sa.AnimationState;

					SetSourceBone(sourceBoneName);

					var skeleton = s.Skeleton;
					siblingBones.Clear();
					foreach (var bn in siblingBoneNames)
					{
						var b = skeleton.FindBone(bn);
						if (b != null) siblingBones.Add(b);
					}
				}

				void HandleUpdateLocal(ISkeletonAnimation animatedSkeletonComponent)
				{
					if (!this.isActiveAndEnabled) return; // Root motion is only applied when component is enabled.

					Vector2 localDelta = Vector2.zero;
					TrackEntry current = state.GetCurrent(0); // Only apply root motion using AnimationState Track 0.

					TrackEntry track = current;
					TrackEntry next = null;
					int boneIndex = this.boneIndex;

					while (track != null)
					{
						var a = track.Animation;
						var tt = a.FindTranslateTimelineForBone(boneIndex);

						if (tt != null)
						{
							// 1. Get the delta position from the root bone's timeline.
							float start = track.animationLast;
							float end = track.AnimationTime;
							Vector2 currentDelta;
							if (start > end)
								currentDelta = (tt.Evaluate(end) - tt.Evaluate(0)) + (tt.Evaluate(a.duration) - tt.Evaluate(start));  // Looped
							else if (start != end)
								currentDelta = tt.Evaluate(end) - tt.Evaluate(start);  // Non-looped
							else
								currentDelta = Vector2.zero;

							// 2. Apply alpha to the delta position (based on AnimationState.cs)
							float mix;
							if (next != null)
							{
								if (next.mixDuration == 0)
								{ // Single frame mix to undo mixingFrom changes.
									mix = 1;
								}
								else
								{
									mix = next.mixTime / next.mixDuration;
									if (mix > 1) mix = 1;
								}
								float mixAndAlpha = track.alpha * next.interruptAlpha * (1 - mix);
								currentDelta *= mixAndAlpha;
							}
							else
							{
								if (track.mixDuration == 0)
								{
									mix = 1;
								}
								else
								{
									mix = track.alpha * (track.mixTime / track.mixDuration);
									if (mix > 1) mix = 1;
								}
								currentDelta *= mix;
							}

							// 3. Add the delta from the track to the accumulated value.
							localDelta += currentDelta;
						}

						// Traverse mixingFrom chain.
						next = track;
						track = track.mixingFrom;
					}

					// 4. Apply flip to the delta position.
					Skeleton skeleton = animatedSkeletonComponent.Skeleton;
					if (skeleton.flipX) localDelta.x = -localDelta.x;
					if (skeleton.flipY) localDelta.y = -localDelta.y;

					// 5. Apply root motion to Transform or RigidBody;
					if (!useX) localDelta.x = 0f;
					if (!useY) localDelta.y = 0f;

					OnApplyMotion(localDelta);

					if (localDelta != Vector2.zero)
					{
						// 6. Position bones to be base position
						// BasePosition = new Vector2(0, 0);
						foreach (Bone b in siblingBones)
						{
							if (useX) b.x -= bone.x;
							if (useY) b.y -= bone.y;
						}

						if (useX) bone.x = 0;
						if (useY) bone.y = 0;
					}			
				}

				public void SetSourceBone(string name)
				{
					var skeleton = skeletonComponent.Skeleton;
					int bi = skeleton.FindBoneIndex(name);
					if (bi >= 0)
					{
						this.boneIndex = bi;
						this.bone = skeleton.bones.Items[bi];
					}
					else {
						Debug.Log("Bone named \"" + name + "\" could not be found.");
						this.boneIndex = 0;
						this.bone = skeleton.RootBone;
					}
				}

				protected virtual void OnApplyMotion(Vector2 localDelta)
				{
					this.transform.Translate(localDelta, Space.Self);
				}
			}
		}
	}
}
