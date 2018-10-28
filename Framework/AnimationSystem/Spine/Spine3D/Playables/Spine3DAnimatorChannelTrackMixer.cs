using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	using Playables;

	namespace AnimationSystem
	{
		namespace Spine
		{
			public class Spine3DAnimatorChannelTrackMixer : SpineAnimatorChannelTrackMixer
			{
				protected override void PrepareChannelFrame(Playable playable)
				{
					int numInputs = playable.GetInputCount();

					string primaryAnimation = null;
					float primaryAnimationTime = 0.0f;
					float primaryAnimationWeight = 0.0f;

					List<Spine3DAnimatorTrackMixer.ChannelBackroundAnimationData> backgroundAnimations = new List<Spine3DAnimatorTrackMixer.ChannelBackroundAnimationData>();
					
					for (int i = 0; i < numInputs; i++)
					{
						ScriptPlayable<Spine3DAnimatorPlayableBehaviour> scriptPlayable = (ScriptPlayable<Spine3DAnimatorPlayableBehaviour>)playable.GetInput(i);
						Spine3DAnimatorPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

						if (inputBehaviour != null && !string.IsNullOrEmpty(inputBehaviour._animation))
						{
							float inputWeight = playable.GetInputWeight(i);

							if (inputWeight > 0.0f)
							{
								TimelineClip clip = TimelineUtils.GetClip(_trackAsset, inputBehaviour._clipAsset);

								if (clip != null)
								{
									double clipStart = clip.hasPreExtrapolation ? clip.extrapolatedStart : clip.start;
									double clipDuration = clip.hasPreExtrapolation || clip.hasPostExtrapolation ? clip.extrapolatedDuration : clip.duration;

									if (_director.time >= clipStart && _director.time <= clipStart + clipDuration)
									{
										bool isPrimaryClip = IsPrimaryClip(clip);

										//Work out track time
										float animationDuration = inputBehaviour._animationDuration;
										float trackTime = GetExtrapolatedTrackTime(clip, _director.time, animationDuration);

										if (isPrimaryClip)
										{
											primaryAnimation = inputBehaviour._animation;
											primaryAnimationTime = trackTime;
											primaryAnimationWeight = inputWeight;
										}
										else
										{
											Spine3DAnimatorTrackMixer.ChannelBackroundAnimationData backroundAnimation = new Spine3DAnimatorTrackMixer.ChannelBackroundAnimationData
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

					Spine3DAnimatorTrackMixer parentMixer = (Spine3DAnimatorTrackMixer)_parentMixer;
					Spine3DAnimatorChannelTrack track = (Spine3DAnimatorChannelTrack)_trackAsset;
					parentMixer.SetChannelData(track._animationChannel, primaryAnimation, primaryAnimationTime, primaryAnimationWeight, backgroundAnimations.ToArray());
				}
			}
		}
	}
}