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

					Spine3DAnimatorTrackMixer.ChannelAnimationData primaryAnimation = new Spine3DAnimatorTrackMixer.ChannelAnimationData();
					List<Spine3DAnimatorTrackMixer.ChannelAnimationData> backgroundAnimations = new List<Spine3DAnimatorTrackMixer.ChannelAnimationData>();
					
					for (int i = 0; i < numInputs; i++)
					{
						ScriptPlayable<Spine3DAnimatorPlayableBehaviour> scriptPlayable = (ScriptPlayable<Spine3DAnimatorPlayableBehaviour>)playable.GetInput(i);
						Spine3DAnimatorPlayableBehaviour inputBehaviour = scriptPlayable.GetBehaviour();

						if (inputBehaviour != null && (!string.IsNullOrEmpty(inputBehaviour._animationId) || inputBehaviour._proxyAnimation != null))
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
											primaryAnimation._animationId = inputBehaviour._animationId;
											primaryAnimation._animationTime = trackTime;
											primaryAnimation._animationWeight = inputWeight;
											primaryAnimation._proxyAnimation = inputBehaviour._proxyAnimation;
											primaryAnimation._proxyAnimationOrientations = inputBehaviour._proxyAnimationOrientations;
										}
										else
										{
											Spine3DAnimatorTrackMixer.ChannelAnimationData backroundAnimation = new Spine3DAnimatorTrackMixer.ChannelAnimationData
											{
												_animationId = inputBehaviour._animationId,
												_animationTime = trackTime,
												_animationWeight = 1.0f,
												_proxyAnimation = inputBehaviour._proxyAnimation,
												_proxyAnimationOrientations = inputBehaviour._proxyAnimationOrientations,
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
					parentMixer.SetChannelData(track._animationChannel, primaryAnimation, backgroundAnimations.ToArray());
				}
			}
		}
	}
}