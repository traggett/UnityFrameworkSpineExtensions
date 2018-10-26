using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Animation = Spine.Animation;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public class SpineAnimatorChannelTrackMixer : PlayableBehaviour
			{
				private SpineAnimatorChannelTrack _trackAsset;
				private PlayableDirector _director;
				private SpineAnimatorTrackMixer _parentMixer;

				public void Init(TrackAsset trackAsset, PlayableDirector director, SpineAnimatorTrackMixer parentMixer)
				{
					_trackAsset = (SpineAnimatorChannelTrack)trackAsset;
					_director = director;
					_parentMixer = parentMixer;
				}

				public override void PrepareFrame(Playable playable, FrameData info)
				{
					if (_parentMixer != null)
					{
						PrepareChannelFrame(playable);
					}
				}

				public override void ProcessFrame(Playable playable, FrameData info, object playerData)
				{
					
				}

				private void PrepareChannelFrame(Playable playable)
				{
					int numInputs = playable.GetInputCount();

					Animation primaryAnimation = null;
					float primaryAnimationTime = 0.0f;
					float primaryAnimationWeight = 0.0f;

					List<SpineAnimatorTrackMixer.ChannelBackroundAnimationData> backgroundAnimations = new List<SpineAnimatorTrackMixer.ChannelBackroundAnimationData>();
					
					for (int i = 0; i < numInputs; i++)
					{
						ScriptPlayable<SpineAnimatorPlayableBehaviour> scriptPlayable = (ScriptPlayable<SpineAnimatorPlayableBehaviour>)playable.GetInput(i);
						SpineAnimatorPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

						if (inputBehaviour != null && inputBehaviour._animation != null)
						{
							float inputWeight = playable.GetInputWeight(i);

							if (inputWeight > 0.0f)
							{
								TimelineClip clip = _trackAsset.GetClip(inputBehaviour._clipAsset);

								if (clip != null)
								{
									double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
									double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

									if (_director.time >= clipStart && _director.time <= clipStart + clipDuration)
									{
										//TO DO! Work out if this is the primary track or a secondary one.
										//If its fading in or in main body or in PreExtrapolation or fading out with not other track fading in or in PostExtrapolation then primary
										bool isPrimaryClip = true;

										//Work out track time
										float animationDuration = inputBehaviour._animation.Duration;
										float trackTime = GetExtrapolatedTrackTime(clip, _director.time, animationDuration);

										if (isPrimaryClip)
										{
											primaryAnimation = inputBehaviour._animation;
											primaryAnimationTime = trackTime;
											primaryAnimationWeight = inputWeight;
										}
										else
										{
											SpineAnimatorTrackMixer.ChannelBackroundAnimationData backroundAnimation = new SpineAnimatorTrackMixer.ChannelBackroundAnimationData
											{
												_animation = inputBehaviour._animation,
												_animationTime = trackTime
											};
											backgroundAnimations.Add(backroundAnimation);
										}
									}
								}
							}
						}
					}

					_parentMixer.SetChannelData(_trackAsset._animationChannel, primaryAnimation, primaryAnimationTime, primaryAnimationWeight, backgroundAnimations.ToArray());
				}

				private static float GetExtrapolatedTrackTime(TimelineClip clip, double directorTime, float animationLength)
				{
					TimelineClip.ClipExtrapolation extrapolation = directorTime < clip.start ? clip.preExtrapolationMode : clip.postExtrapolationMode;
					float time = (float)(directorTime - clip.start);

					if (clip.start <= directorTime && directorTime < clip.end)
						return time;

					if (animationLength <= 0.0f)
						return 0.0f;

					switch (extrapolation)
					{
						case TimelineClip.ClipExtrapolation.Continue:
						case TimelineClip.ClipExtrapolation.Hold:
							return time < 0.0f ? 0.0f : (float)clip.end;
						case TimelineClip.ClipExtrapolation.Loop:
							{
								if (time < 0.0f)
								{
									float t = -time / animationLength;
									int n = Mathf.FloorToInt(t);
									float fraction = animationLength - (t - n);

									time = (animationLength * n) + fraction;
								}

								return time;
							}
						case TimelineClip.ClipExtrapolation.PingPong:
							{
								float t = Mathf.Abs(time) / animationLength;
								int n = Mathf.FloorToInt(t);
								float fraction = t - n;

								if (n % 2 == 1)
									fraction = animationLength - fraction;

								return (animationLength * n) + fraction;
							}
						case TimelineClip.ClipExtrapolation.None:
						default:
							return 0.0f;
					}
				}
			}
		}
	}
}