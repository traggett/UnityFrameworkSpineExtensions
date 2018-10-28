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

				public class ChannelBackroundAnimationData
				{
					public string _animation;
					public float _animationTime;
				}

				private class ChannelData
				{
					public int _channel;
					public string _primaryAnimation;
					public float _primaryAnimationTime;
					public float _primaryAnimationWeight;

					public ChannelBackroundAnimationData[] _backgroundAnimations;
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

				public void SetChannelData(int channel, string anim, float animTime, float animWeight, params ChannelBackroundAnimationData[] backroundAnimations)
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
							PlayAnimation(animationSet, animationState, trackEntries, trackIndex, channelData._backgroundAnimations[i]._animation, channelData._backgroundAnimations[i]._animationTime, 1.0f);
							trackIndex++;
						}

						PlayAnimation(animationSet, animationState, trackEntries, trackIndex, channelData._primaryAnimation, channelData._primaryAnimationTime, channelData._primaryAnimationWeight);
						trackIndex++;
					}

					//Clear unused tracks
					for (; trackIndex < trackEntries.Length; trackIndex++)
					{
						animationState.ClearTrack(trackIndex);
					}
				}

				private void PlayAnimation(Spine3DAnimationSet animationSet, AnimationState animationState, TrackEntry[] trackEntries, int trackIndex, string animation, float animationTime, float animationWeight)
				{
					if (animation != null)
					{
						TrackEntry trackEntry = trackEntries[trackIndex];

						animation = _trackBinding.GetAnimNameForAnimationSet(animationSet, animation);

						if (trackEntry == null || trackEntry.Animation == null || trackEntry.Animation.Name != animation)
						{
							animationState.ClearTrack(trackIndex);

							Animation anim = animationState.Data.skeletonData.FindAnimation(animation);
							if (anim != null)
								trackEntry = animationState.SetAnimation(trackIndex, animation, true);
						}

						if (trackEntry != null)
						{
							trackEntry.TrackTime = animationTime;
							trackEntry.Alpha = animationWeight;
						}
					}
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
