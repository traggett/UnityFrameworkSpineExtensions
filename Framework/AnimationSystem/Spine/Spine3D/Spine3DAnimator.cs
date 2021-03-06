using System.Collections.Generic;
using Framework.Maths;
using UnityEngine;
using Animation = Spine.Animation;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			//Class that animates all animation sets in sync on a Spine3DRenderer
			public class Spine3DAnimator : MonoBehaviour, IAnimator
			{
				#region Public Data
				public Spine3DRenderer _renderer;
				#endregion

				#region IAnimator
				public void Play(int channel, string animName, WrapMode wrapMode = WrapMode.Default, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine, float weight = 1.0f, bool queued = false)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						childAnimator.Play(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName), wrapMode, blendTime, easeType, weight, queued);
					}
				}

				public void Stop(int channel, float blendTime = 0.0f, InterpolationType easeType = InterpolationType.InOutSine)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						childAnimator.Stop(channel, blendTime, easeType);
					}
				}

				public void StopAll()
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						childAnimator.StopAll();
					}
				}

				public void SetAnimationSpeed(int channel, string animName, float speed)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						childAnimator.SetAnimationSpeed(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName), speed);
					}
				}

				public void SetAnimationTime(int channel, string animName, float time)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						childAnimator.SetAnimationTime(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName), time);
					}
				}

				public void SetAnimationWeight(int channel, string animName, float weight)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						childAnimator.SetAnimationWeight(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName), weight);
					}
				}

				public void SetAnimationNormalizedTime(int channel, string animName, float time)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						childAnimator.SetAnimationNormalizedTime(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName), time);
					}
				}

				public void SetAnimationNormalizedSpeed(int channel, string animName, float speed)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						childAnimator.SetAnimationNormalizedSpeed(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName), speed);
					}
				}

				public bool IsPlaying(int channel, string animName)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						if (childAnimator.IsPlaying(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName)))
						{
							return true;
						}
					}

					return false;
				}

				public float GetAnimationLength(string animName)
				{
					float length = 0.0f;

					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						length = Mathf.Max(childAnimator.GetAnimationLength(GetAnimNameForAnimationSet(_renderer._animationSets[i], animName)), length);
					}

					return length;
				}

				public float GetAnimationTime(int channel, string animName)
				{
					float time = 0.0f;

					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						time = Mathf.Max(childAnimator.GetAnimationTime(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName)), time);
					}

					return time;
				}

				public float GetAnimationSpeed(int channel, string animName)
				{
					float speed = 1.0f;

					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						speed = Mathf.Max(childAnimator.GetAnimationSpeed(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName)), speed);
					}

					return speed;
				}

				public float GetAnimationWeight(int channel, string animName)
				{
					float weight = 0.0f;

					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						weight = Mathf.Max(childAnimator.GetAnimationWeight(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName)), weight);
					}

					return weight;
				}

				public float GetAnimationNormalizedTime(int channel, string animName)
				{
					float time = 0.0f;

					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						time = Mathf.Max(childAnimator.GetAnimationNormalizedTime(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName)), time);
					}

					return time;
				}

				public float GetAnimationNormalizedSpeed(int channel, string animName)
				{
					float speed = 1.0f;

					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						speed = Mathf.Max(childAnimator.GetAnimationNormalizedSpeed(channel, GetAnimNameForAnimationSet(_renderer._animationSets[i], animName)), speed);
					}

					return speed;
				}

				public bool DoesAnimationExist(string animName)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						if (!childAnimator.DoesAnimationExist(GetAnimNameForAnimationSet(_renderer._animationSets[i], animName)))
							return false;
					}

					return true;
				}

#if UNITY_EDITOR
				public string[] GetAnimationNames()
				{
					HashSet<string> animationNames = new HashSet<string>();

					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						SpineAnimator childAnimator = _renderer._animationSets[i]._animatior;
						string[] childAnimationNames = childAnimator.GetAnimationNames();

						foreach (string childAnimationName in childAnimationNames)
						{
							animationNames.Add(childAnimationName);
						}
					}

					string[] stringArray = new string[animationNames.Count];
					animationNames.CopyTo(stringArray);

					return stringArray;
				}
#endif
				#endregion

				public string GetAnimNameForAnimationSet(Spine3DAnimationSet animationSet, string animName)
				{
					if (!string.IsNullOrEmpty(animationSet._animationPrefix))
					{
						SpineAnimator childAnimator = animationSet._animatior;
						string fullAnimName = animName + animationSet._animationPrefix;

						if (childAnimator.DoesAnimationExist(fullAnimName))
						{
							return fullAnimName;
						}
					}

					return animName;
				}

				public Animation GetChannelPrimaryAnimation(int channel)
				{
					for (int i = 0; i < _renderer._animationSets.Length; i++)
					{
						Animation animation = _renderer._animationSets[i]._animatior.GetChannelPrimaryAnimation(channel);

						if (animation != null)
							return animation;
					}

					return null;
				}
			}
		}
	}
}
