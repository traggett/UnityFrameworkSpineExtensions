using UnityEngine;
using Animation = Spine.Animation;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public class SpineAnimatorStateBehaviour : StateMachineBehaviour
			{
				public string _animationName;

				private SpineAnimator _animator;
#if UNITY_EDITOR
				private bool _editorForceUpdate;
#endif

				#region StateMachineBehaviour
				public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
				{
					CacheAnimator(animator);

					float blendTime = 0.0f;

					if (animator.IsInTransition(layerIndex))
					{
						AnimatorTransitionInfo trans = animator.GetAnimatorTransitionInfo(layerIndex);				

						switch (trans.durationUnit)
						{
							case DurationUnit.Fixed:
								{
									blendTime = trans.duration;
								}
								break;
							case DurationUnit.Normalized:
								{
									Animation animation = _animator.GetChannelPrimaryAnimation(layerIndex);
									blendTime = trans.duration * (animation != null ? animation.Duration : 0.0f);
								}
								break;
						}
					}

					StartAnimation(stateInfo, layerIndex, blendTime);
				}

				public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
				{
#if UNITY_EDITOR
					if (_editorForceUpdate)
					{
						StartAnimation(stateInfo, layerIndex, 0.0f);
						_editorForceUpdate = false;
					}
#endif
				}
				#endregion

#if UNITY_EDITOR
				public void ForceUpdate()
				{
					_editorForceUpdate = true;
				}
#endif

				private void StartAnimation(AnimatorStateInfo stateInfo, int layerIndex, float blendTime)
				{
					_animator.Play(layerIndex, _animationName, stateInfo.loop ? WrapMode.Loop : WrapMode.Once, blendTime);
					_animator.SetAnimationSpeed(layerIndex, _animationName, stateInfo.speed * stateInfo.speedMultiplier);
				}

				private void CacheAnimator(Animator animator)
				{
					if (_animator == null)
					{
						_animator = animator.GetComponent<SpineAnimator>();
					}
				}

			}
		}
	}
}
