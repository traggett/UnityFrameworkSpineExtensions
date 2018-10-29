using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Spine;
using Spine.Unity;
using AnimationState = Spine.AnimationState;
using Animation = Spine.Animation;
using Framework.Playables;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public class SpineAnimatorTrackMixer : PlayableBehaviour, ITrackMixer
			{
				private SpineAnimatorTrack _trackAsset;
				private PlayableDirector _director;
				private SkeletonAnimation _trackBinding;
				private AnimationState _animationState;

				public class ChannelBackroundAnimationData
				{
					public Animation _animation;
					public float _animationTime;
				}

				private class ChannelData
				{
					public int _channel;
					public Animation _primaryAnimation;
					public float _primaryAnimationTime;
					public float _primaryAnimationWeight;

					public ChannelBackroundAnimationData[] _backgroundAnimations;
				}

				private List<ChannelData> _channelData = new List<ChannelData>();

				#region ITrackMixer
				public void SetTrackAsset(TrackAsset trackAsset, PlayableDirector playableDirector)
				{
					_trackAsset = trackAsset as SpineAnimatorTrack;
					_director = playableDirector;
					_trackBinding = _director.GetGenericBinding(GetTrackAsset()) as SkeletonAnimation;

					if (_trackBinding != null)
					{
						_animationState = new AnimationState(_trackBinding.SkeletonDataAsset.GetAnimationStateData());
					}
				}
				
				public TrackAsset GetTrackAsset()
				{
					return _trackAsset;
				}
				#endregion

				public SkeletonAnimation GetTrackBinding()
				{
					return _trackBinding;
				}

				public override void PrepareFrame(Playable playable, FrameData info)
				{
					_channelData.Clear();

#if UNITY_EDITOR
					if (_trackAsset != null)
						_trackAsset.EnsureMasterClipExists();
#endif
				}

				public override void ProcessFrame(Playable playable, FrameData info, object playerData)
				{
					if (_trackBinding != null && _animationState != null)
					{
						//Reset pose to default if set in track or playing preview in editor
						if (_trackAsset._resetPose
#if UNITY_EDITOR
							|| !Application.isPlaying
#endif
							)
						{
							_trackBinding.Skeleton.SetToSetupPose();
						}

						ApplyChannelsToState();

						_animationState.Apply(_trackBinding.Skeleton);
					}
				}

				public override void OnGraphStop(Playable playable)
				{
#if UNITY_EDITOR
					if (_trackBinding != null)
						_trackBinding.Skeleton.SetToSetupPose();
#endif
				}

				public void SetChannelData(int channel, Animation anim, float animTime, float animWeight, params ChannelBackroundAnimationData[] backroundAnimations)
				{
					ChannelData channelData = null;

					foreach (ChannelData c in _channelData)
					{
						if (c._channel == channel)
						{
							channelData = c;
							break;
						}
					}

					if (channelData == null)
					{
						channelData = new ChannelData();
						channelData._channel = channel;
						_channelData.Add(channelData);
						_channelData.Sort((x, y) => x._channel.CompareTo(y._channel));
					}

					channelData._primaryAnimation = anim;
					channelData._primaryAnimationTime = animTime;
					channelData._primaryAnimationWeight = animWeight;
					channelData._backgroundAnimations = backroundAnimations;
				}

				private void ApplyChannelsToState()
				{
					//First work out how many track entries are needed
					int numTrackEntries = 0;

					foreach (ChannelData channelData in _channelData)
					{
						numTrackEntries++;
						numTrackEntries += channelData._backgroundAnimations.Length;
					}

					//Grow to new amount if needed
					_animationState.Tracks.GrowIfNeeded(numTrackEntries);
					TrackEntry[] trackEntries = _animationState.Tracks.Items;

					//Ensure animations are playing at correct times / weights
					int trackIndex = 0;

					foreach (ChannelData channelData in _channelData)
					{
						for (int i = 0; i < channelData._backgroundAnimations.Length; i++)
						{
							PlayAnimation(trackEntries, trackIndex, channelData._backgroundAnimations[i]._animation, channelData._backgroundAnimations[i]._animationTime, 1.0f);
							trackIndex++;
						}

						PlayAnimation(trackEntries, trackIndex, channelData._primaryAnimation, channelData._primaryAnimationTime, channelData._primaryAnimationWeight);
						trackIndex++;
					}

					//Clear unused tracks
					for (; trackIndex < trackEntries.Length; trackIndex++)
					{
						_animationState.ClearTrack(trackIndex);
					}
				}

				private void PlayAnimation(TrackEntry[] trackEntries, int trackIndex, Animation animation, float animationTime, float animationWeight)
				{
					if (animation != null)
					{
						TrackEntry trackEntry = trackEntries[trackIndex];

						if (trackEntry == null || trackEntry.Animation == null || trackEntry.Animation.Name != animation.Name)
						{
							_animationState.ClearTrack(trackIndex);
							trackEntry = _animationState.SetAnimation(trackIndex, animation, true);
						}

						if (trackEntry != null)
						{
							trackEntry.TrackTime = animationTime;
							trackEntry.Alpha = animationWeight;
						}
					}
					else
					{
						_animationState.ClearTrack(trackIndex);
					}
				}
			}
		}
	}
}
