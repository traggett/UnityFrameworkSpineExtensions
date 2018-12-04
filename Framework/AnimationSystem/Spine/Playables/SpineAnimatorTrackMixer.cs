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

				public struct ChannelAnimationData
				{
					public Animation _animation;
					public float _animationTime;
					public float _animationWeight;
					public float _animationSpeed;
				}

				private class ChannelData
				{
					public int _channel;
					public ChannelAnimationData _primaryAnimation;
					public ChannelAnimationData[] _backgroundAnimations;
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

				public void SetChannelData(int channel, ChannelAnimationData primaryAnimation, params ChannelAnimationData[] backroundAnimations)
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

					channelData._primaryAnimation = primaryAnimation;
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
							PlayAnimation(trackEntries, trackIndex, channelData._backgroundAnimations[i]);
							trackIndex++;
						}

						PlayAnimation(trackEntries, trackIndex, channelData._primaryAnimation);
						trackIndex++;
					}

					//Clear unused tracks
					for (; trackIndex < trackEntries.Length; trackIndex++)
					{
						_animationState.ClearTrack(trackIndex);
					}
				}

				private void PlayAnimation(TrackEntry[] trackEntries, int trackIndex, ChannelAnimationData animation)
				{
					if (animation._animation != null)
					{
						TrackEntry trackEntry = trackEntries[trackIndex];

						if (trackEntry == null || trackEntry.Animation != animation._animation)
						{
							_animationState.ClearTrack(trackIndex);
							trackEntry = _animationState.SetAnimation(trackIndex, animation._animation, true);
						}

						if (trackEntry != null)
						{
							trackEntry.TrackTime = animation._animationTime * animation._animationSpeed;
							trackEntry.Alpha = animation._animationWeight;
							trackEntry.TimeScale = animation._animationSpeed;
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
