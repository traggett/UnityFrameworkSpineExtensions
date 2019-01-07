using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Spine;
using AnimationState = Spine.AnimationState;
using Animation = Spine.Animation;

namespace Framework
{
	using Playables;

	namespace AnimationSystem
	{
		namespace Spine
		{
			public class Spine3DAnimatorTrackMixer : PlayableBehaviour, ITrackMixer
			{
				private Spine3DAnimatorTrack _trackAsset;
				private PlayableDirector _director;
				private Spine3DAnimator _trackBinding;
				private AnimationState[] _animationStates;

				public struct ChannelAnimationData
				{
					public string _animationId;
					public float _animationTime;
					public float _animationWeight;
					public float _animationSpeed;
					public Animation _proxyAnimation;
					public eSpine3DOrientation _proxyAnimationOrientations;
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
					_trackAsset = trackAsset as Spine3DAnimatorTrack;
					_director = playableDirector;
					_trackBinding = _director.GetGenericBinding(trackAsset) as Spine3DAnimator;

					if (_trackBinding != null)
					{
						_animationStates = new AnimationState[_trackBinding._renderer._animationSets.Length];

						for (int i = 0; i < _animationStates.Length; i++)
						{
							SpineAnimator spineAnimator = _trackBinding._renderer._animationSets[i]._animatior;
							_animationStates[i] = new AnimationState(spineAnimator.GetSkeletonAnimation().SkeletonDataAsset.GetAnimationStateData());
						}
					}
				}

				public TrackAsset GetTrackAsset()
				{
					return _trackAsset;
				}
				#endregion

				public Spine3DAnimator GetTrackBinding()
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
					if (_trackBinding != null && _animationStates != null)
					{

						for(int i = 0; i < _animationStates.Length; i++)
						{
							Spine3DAnimationSet animationSet = _trackBinding._renderer._animationSets[i];
							ApplyChannelsToState(animationSet, _animationStates[i]);
						}
#if UNITY_EDITOR
						if (!Application.isPlaying)
						{
							ApplyInEditor();
						}
						else
#endif
						{
							for (int i = 0; i < _animationStates.Length; i++)
							{
								SpineAnimator spineAnimator = _trackBinding._renderer._animationSets[i]._animatior;
								_animationStates[i].Apply(spineAnimator.GetSkeletonAnimation().Skeleton);
							}	
						}
					}
				}

				public override void OnGraphStop(Playable playable)
				{
#if UNITY_EDITOR
					OnEditorUnBound();
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

				private void ApplyChannelsToState(Spine3DAnimationSet animationSet, AnimationState animationState)
				{
					//First work out how many track entries are needed
					int numTrackEntries = 0;

					foreach (ChannelData channelData in _channelData)
					{
						numTrackEntries++;
						numTrackEntries += channelData._backgroundAnimations.Length;
					}

					//Grow to new amount if needed
					animationState.Tracks.GrowIfNeeded(numTrackEntries);
					TrackEntry[] trackEntries = animationState.Tracks.Items;

					//Ensure animations are playing at correct times / weights
					int trackIndex = 0;

					foreach (ChannelData channelData in _channelData)
					{
						for (int i = 0; i < channelData._backgroundAnimations.Length; i++)
						{
							PlayAnimation(animationSet, animationState, trackEntries, trackIndex, channelData._backgroundAnimations[i]);
							trackIndex++;
						}

						PlayAnimation(animationSet, animationState, trackEntries, trackIndex, channelData._primaryAnimation);
						trackIndex++;
					}

					//Clear unused tracks
					for (; trackIndex < trackEntries.Length; trackIndex++)
					{
						animationState.ClearTrack(trackIndex);
					}
				}

				private void PlayAnimation(Spine3DAnimationSet animationSet, AnimationState animationState, TrackEntry[] trackEntries, int trackIndex, ChannelAnimationData animation)
				{
					//Proxy Animation
					if (animation._proxyAnimation != null)
					{
						//Valid for this animation set (matches orientations)
						if ((animationSet._orientation & animation._proxyAnimationOrientations) != 0)
						{
							TrackEntry trackEntry = trackEntries[trackIndex];

							if (trackEntry == null || trackEntry.Animation != animation._proxyAnimation)
							{
								animationState.ClearTrack(trackIndex);
								trackEntry = animationState.SetAnimation(trackIndex, animation._proxyAnimation, true);
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
							animationState.ClearTrack(trackIndex);
						}
					}
					//Normal animation
					else if (!string.IsNullOrEmpty(animation._animationId))
					{
						string animationId = _trackBinding.GetAnimNameForAnimationSet(animationSet, animation._animationId);
						TrackEntry trackEntry = trackEntries[trackIndex];

						if (trackEntry == null || trackEntry.Animation == null || trackEntry.Animation.Name != animationId)
						{
							animationState.ClearTrack(trackIndex);

							Animation anim = animationState.Data.SkeletonData.FindAnimation(animationId);
							if (anim != null)
								trackEntry = animationState.SetAnimation(trackIndex, anim, true);
						}

						if (trackEntry != null)
						{
							trackEntry.TrackTime = animation._animationTime * animation._animationSpeed;
							trackEntry.Alpha = animation._animationWeight;
							trackEntry.TimeScale = animation._animationSpeed;
						}
					}
					//Nothing playing
					else
					{
						animationState.ClearTrack(trackIndex);
					}
				}

#if UNITY_EDITOR
				private void ApplyInEditor()
				{
					for (int i = 0; i < _animationStates.Length; i++)
					{
						SpineAnimator spineAnimator = _trackBinding._renderer._animationSets[i]._animatior;
						spineAnimator.GetSkeletonAnimation().Skeleton.SetToSetupPose();
						_animationStates[i].Apply(spineAnimator.GetSkeletonAnimation().Skeleton);
					}
				}

				private void OnEditorUnBound()
				{
					if (_trackBinding != null)
					{
						for (int i = 0; i < _trackBinding._renderer._animationSets.Length; i++)
						{
							_trackBinding._renderer._animationSets[i]._animatior.GetSkeletonAnimation().Skeleton.SetToSetupPose();
						}
					}
				}
#endif
			}
		}
	}
}
