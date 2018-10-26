using UnityEngine;
using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using Animation = Spine.Animation;
using AnimationState = Spine.AnimationState;


namespace Framework
{
	using Maths;
	using Utils;
	using MathUtils = Maths.MathUtils;

	namespace AnimationSystem
	{
		namespace Spine
		{
			//Class that wraps up blending and layering of animations using a Spine Skeleton Animation
			[RequireComponent(typeof(SkeletonAnimation))]
			public class SpineAnimator : MonoBehaviour, IAnimator
			{
				#region Private Data
				private static readonly int kDefualtNumBackgroundTracks = 2;

				private struct ChannelTrack
				{
					//Index into AnimationState Tracks
					public int _trackIndex;
					//Used for blending when stopping
					public float _origWeight;
				}

				private class ChannelGroup
				{
					public int _channel;
					public ChannelTrack _primaryTrack;
					public ChannelTrack[] _backgroundTracks = new ChannelTrack[kDefualtNumBackgroundTracks];

					public enum eState
					{
						Stopped,
						Playing,
						BlendingIn,
						Stopping,
					}
					public eState _state = eState.Stopped;

					//Primary track blending data
					public float _lerpT;
					public float _lerpSpeed;
					public eInterpolation _lerpEase;
					public float _targetWeight;

					//Queued animation data
					public string _queuedAnimation;
					public float _queuedAnimationWeight;
					public float _queuedAnimationBlendTime;
					public eWrapMode _queuedAnimationWrapMode;
					public eInterpolation _queuedAnimationEase;
				}

				private SkeletonAnimation _skeletonAnimation;
				private AnimationState _animationState;
				private Dictionary<int, Animation> _proxyAnimations = new Dictionary<int, Animation>();

				private List<ChannelGroup> _channels = new List<ChannelGroup>();
				#endregion
				
				#region MonoBehaviour Calls
				void Awake()
				{
					_skeletonAnimation = GetComponent<SkeletonAnimation>();
					_skeletonAnimation.Initialize(false);
					_animationState = _skeletonAnimation.state;
				}

				void Update()
				{
					foreach (ChannelGroup channelGroup in _channels)
					{
						UpdateChannelBlends(channelGroup);
					}
				}
				#endregion

				#region IAnimator
				public void Play(int channel, string animName, eWrapMode wrapMode = eWrapMode.Default, float blendTime = 0.0f, eInterpolation easeType = eInterpolation.InOutSine, float weight = 1.0f, bool queued = false)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					//If queued then store the animation queued after, wait for it to be -blend time from end then play non queued with blend time.
					if (queued)
					{
						if (channelGroup != null && IsTrackPlaying(channelGroup._primaryTrack))
						{
							channelGroup._queuedAnimation = animName;
							channelGroup._queuedAnimationWeight = weight;
							channelGroup._queuedAnimationBlendTime = blendTime;
							channelGroup._queuedAnimationEase = easeType;
							channelGroup._queuedAnimationWrapMode = wrapMode;
							return;
						}
					}

					//If not blending stop relevant animations
					if (blendTime <= 0.0f)
					{
						//Stop all others from channel group if ePlayMode.Additive, stop all others if Singular
						StopChannel(channelGroup);
					}

					//If no group exists, add new one and return first track index
					if (channelGroup == null)
					{
						channelGroup = AddNewChannelGroup(channel);
					}
					//Otherwise check an animation is currently playing on this group
					else if (channelGroup._state != ChannelGroup.eState.Stopped)
					{
						MovePrimaryAnimationToBackgroundTrack(channelGroup);
					}

					//Start animation on primary track
					int trackIndex = channelGroup._primaryTrack._trackIndex;
					TrackEntry trackEntry = _animationState.AddAnimation(trackIndex, animName, wrapMode == eWrapMode.Loop, 0f);

					//if blending start with weight of zero
					if (blendTime > 0.0f)
					{
						channelGroup._state = ChannelGroup.eState.BlendingIn;
						trackEntry.alpha = 0.0f;
						channelGroup._lerpT = 0.0f;
						channelGroup._targetWeight = weight;
						channelGroup._lerpSpeed = 1.0f / blendTime;
						channelGroup._lerpEase = easeType;
					}
					else
					{
						channelGroup._state = ChannelGroup.eState.Playing;
						trackEntry.alpha = weight;
					}
				}
				
				public void Stop(int channel, float blendTime = 0.0f, eInterpolation easeType = eInterpolation.InOutSine)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						if (blendTime > 0.0f)
						{
							channelGroup._state = ChannelGroup.eState.Stopping;
							channelGroup._lerpSpeed = 1.0f / blendTime;
							channelGroup._lerpT = 0.0f;
							channelGroup._lerpEase = easeType;

							TrackEntry[] trackEntries = _animationState.Tracks.Items;

							if (IsTrackPlaying(trackEntries[channelGroup._primaryTrack._trackIndex]))
								channelGroup._primaryTrack._origWeight = trackEntries[channelGroup._primaryTrack._trackIndex].alpha;

							for (int i = 0; i < channelGroup._backgroundTracks.Length; i++)
							{
								if (IsTrackPlaying(trackEntries[channelGroup._primaryTrack._trackIndex]))
									channelGroup._backgroundTracks[i]._origWeight = trackEntries[channelGroup._primaryTrack._trackIndex].alpha;
							}
						}
						else
						{
							StopChannel(channelGroup);
						}
					}			
				}

				public void StopAll()
				{
					foreach (ChannelGroup channelGroup in _channels)
					{
						StopChannel(channelGroup);
					}
				}

				public void SetAnimationTime(int channel, string animName, float time)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						TrackEntry trackEntry;

						if (IsTrackPlaying(channelGroup._primaryTrack, animName, out trackEntry))
						{
							trackEntry.trackTime = time;
						}
					}
				}

				public void SetAnimationSpeed(int channel, string animName, float speed)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						TrackEntry trackEntry;

						if (IsTrackPlaying(channelGroup._primaryTrack, animName, out trackEntry))
						{
							trackEntry.timeScale = speed;
						}
					}
				}

				public void SetAnimationWeight(int channel, string animName, float weight)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						TrackEntry trackEntry;

						if (IsTrackPlaying(channelGroup._primaryTrack, animName, out trackEntry))
						{
							trackEntry.alpha = weight;
						}
					}
				}

				public bool IsPlaying(int channel, string animName)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						TrackEntry trackEntry;

						if (IsTrackPlaying(channelGroup._primaryTrack, animName, out trackEntry))
						{
							return true;
						}
					}

					return false;
				}

				public bool DoesAnimationExist(string animName)
				{
					if (_skeletonAnimation != null && _skeletonAnimation.Skeleton != null)
					{
						foreach (Animation animation in _skeletonAnimation.Skeleton.Data.Animations)
						{
							if (animation.Name == animName)
							{
								return true;
							}
						}
					}

					return false;
				}

				public float GetAnimationLength(string animName)
				{
					if (_skeletonAnimation != null && _skeletonAnimation.Skeleton != null)
					{
						foreach (Animation animation in _skeletonAnimation.Skeleton.Data.Animations)
						{
							if (animation.Name == animName)
							{
								return animation.Duration;
							}
						}
					}
					
					return 0.0f;
				}

				public float GetAnimationTime(int channel, string animName)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						TrackEntry trackEntry;

						if (IsTrackPlaying(channelGroup._primaryTrack, animName, out trackEntry))
						{
							return trackEntry.trackTime;
						}
					}

					return 0.0f;
				}

				public float GetAnimationSpeed(int channel, string animName)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						TrackEntry trackEntry;

						if (IsTrackPlaying(channelGroup._primaryTrack, animName, out trackEntry))
						{
							return trackEntry.timeScale;
						}
					}

					return 1.0f;
				}

				public float GetAnimationWeight(int channel, string animName)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						TrackEntry trackEntry;

						if (IsTrackPlaying(channelGroup._primaryTrack, animName, out trackEntry))
						{
							return trackEntry.alpha;
						}
					}

					return 0.0f;
				}

#if UNITY_EDITOR
				public string[] GetAnimationNames()
				{
					Animation[] animations = GetSkeletonAnimation().skeletonDataAsset.GetAnimationStateData().SkeletonData.Animations.Items;

					string[] animationNames = new string[animations.Length];

					for (int i = 0; i < animations.Length; i++)
					{
						animationNames[i] = animations[i].name;
					}

					return animationNames;
				}
#endif
				#endregion

				#region Public Interface
				public SkeletonAnimation GetSkeletonAnimation()
				{
					if (_skeletonAnimation == null)
					{
						_skeletonAnimation = GetComponent<SkeletonAnimation>();
					}
					return _skeletonAnimation;
				}

				public static void AddToSkin(Skeleton skeleton, Skin skin, Skin otherSkin)
				{
					ExposedList<Slot> slots = skeleton.slots;
					for (int i = 0, n = slots.Count; i < n; i++)
					{
						Slot slot = slots.Items[i];
						string name = slot.data.attachmentName;
						if (name != null)
						{
							Attachment attachment = otherSkin.GetAttachment(i, name);
							if (attachment != null)
							{
								skin.AddAttachment(i, name, attachment);
							}
						}
					}
				}

				public void SetProxyAnimation(Animation animation, int trackIndex)
				{
					//_animationState.ClearTrack(trackIndex);
					//_proxyAnimations[trackIndex] = animation;
				}

				public void SetPrimaryAnimation(int channel, string animName, float animTime, float animWeight)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					//If no group exists, add new one and return first track index
					if (channelGroup == null)
					{
						channelGroup = AddNewChannelGroup(channel);
					}

					TrackEntry[] tracks = _animationState.Tracks.Items;
					int trackIndex = channelGroup._primaryTrack._trackIndex;

					//If not playing this animation, start it on correct track
					if (tracks[trackIndex] == null || tracks[trackIndex].Animation == null || tracks[trackIndex].Animation.Name != animName)
					{
						tracks[trackIndex] = null;
						TrackEntry trackEntry = _animationState.AddAnimation(trackIndex, animName, true, 0f);
						trackEntry.trackTime = animTime;
						trackEntry.alpha = animWeight;
					}
					else
					{
						tracks[trackIndex].trackTime = animTime;
						tracks[trackIndex].alpha = animWeight;
					}
				}

				public void ClearPrimaryAnimation(int channel)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						_animationState.ClearTrack(channelGroup._primaryTrack._trackIndex);
					}
				}

				public void SetBackgroundAnimation(int channel, int index, string animName, float animTime)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					//If no group exists, add new one and return first track index
					if (channelGroup == null)
					{
						channelGroup = AddNewChannelGroup(channel);
					}

					TrackEntry[] tracks = _animationState.Tracks.Items;

					if (index >= channelGroup._backgroundTracks.Length)
					{
						//TO DO! Add more background tracks
						return;
					}

					int trackIndex = channelGroup._backgroundTracks[index]._trackIndex;

					//If not playing this animation, start it on correct track
					if (tracks[trackIndex] == null || tracks[trackIndex].Animation == null || tracks[trackIndex].Animation.Name != animName)
					{
						tracks[trackIndex] = null;
						TrackEntry trackEntry = _animationState.AddAnimation(trackIndex, animName, true, 0f);
						trackEntry.trackTime = animTime;
						trackEntry.alpha = 1.0f;
					}
					else
					{
						tracks[trackIndex].trackTime = animTime;
						tracks[trackIndex].alpha = 1.0f;
					}
				}

				public void ClearBackgroundAnimations(int channel, int fromIndex)
				{
					ChannelGroup channelGroup = GetChannelGroup(channel);

					if (channelGroup != null)
					{
						for (int i = fromIndex; i < channelGroup._backgroundTracks.Length; i++)
						{
							_animationState.ClearTrack(channelGroup._backgroundTracks[i]._trackIndex);
						}
					}
				}
				#endregion

				#region Private functions
				private ChannelGroup GetChannelGroup(int channel)
				{
					foreach (ChannelGroup channelGroup in _channels)
					{
						if (channelGroup._channel == channel)
							return channelGroup;
					}

					return null;
				}

				private void MovePrimaryAnimationToBackgroundTrack(ChannelGroup channelGroup)
				{
					int emptyChannelTrackIndex = GetFreeChannelBackgroundTrack(channelGroup);
					
					TrackEntry[] tracks = _animationState.Tracks.Items;

					//Move primary track to this background track index
					tracks[channelGroup._backgroundTracks[emptyChannelTrackIndex]._trackIndex] = tracks[channelGroup._primaryTrack._trackIndex];
					//Update its track index
					tracks[channelGroup._backgroundTracks[emptyChannelTrackIndex]._trackIndex].trackIndex = channelGroup._backgroundTracks[emptyChannelTrackIndex]._trackIndex;
					//Clear old track
					tracks[channelGroup._primaryTrack._trackIndex] = null;
				}

				private ChannelGroup AddNewChannelGroup(int channel)
				{
					//Create new group
					ChannelGroup channelGroup = new ChannelGroup();
					channelGroup._channel = channel;

					//Add to channels and sort by channel
					_channels.Add(channelGroup);
					_channels.Sort((x, y) => x._channel.CompareTo(y._channel));

					//Increase track size to include this channel
					_animationState.Tracks.GrowIfNeeded(_animationState.Tracks.Count + 1 + channelGroup._backgroundTracks.Length);
					TrackEntry[] tracks = _animationState.Tracks.Items;

					//Progress through sorted channels, updating track index
					int trackIndex = 0;

					for (int i = 0; i < _channels.Count; i++)
					{
						if (_channels[i]._channel == channel)
						{
							for (int j = 0; j < _channels[i]._backgroundTracks.Length; j++)
							{
								//Set track to new empty one
								tracks[trackIndex] = null;
								tracks[trackIndex] = _animationState.AddEmptyAnimation(trackIndex, 0f, 0f);
								//Update channel track entry's track index
								_channels[i]._backgroundTracks[j]._trackIndex = trackIndex;

								trackIndex++;
							}

							//Set track to new empty one
							tracks[trackIndex] = null;
							tracks[trackIndex] = _animationState.AddEmptyAnimation(trackIndex, 0f, 0f);
							//Update primary track entry's track index
							_channels[i]._primaryTrack._trackIndex = trackIndex;

							trackIndex++;
						}
						else
						{
							for (int j = 0; j < _channels[i]._backgroundTracks.Length; j++)
							{
								//Update channel track
								tracks[trackIndex] = tracks[_channels[i]._backgroundTracks[j]._trackIndex];
								//Update track entry index (if playing)
								if (tracks[trackIndex] != null)
									tracks[trackIndex].trackIndex = trackIndex;
								//Update channel track entry's track index
								_channels[i]._backgroundTracks[j]._trackIndex = trackIndex;

								trackIndex++;
							}

							//Set track to new empty one
							tracks[trackIndex] = tracks[_channels[i]._primaryTrack._trackIndex];
							//Update track entry index (if playing)
								if (tracks[trackIndex] != null)
								tracks[trackIndex].trackIndex = trackIndex;
							//Update channel track entry's track index
							_channels[i]._primaryTrack._trackIndex = trackIndex;

							trackIndex++;
						}
					}

					return channelGroup;
				}

				private int GetFreeChannelBackgroundTrack(ChannelGroup channelGroup)
				{
					//Find first empty channel thats not before any playing channels
					int emptyChannelTrackIndex = -1;

					TrackEntry[] tracks = _animationState.Tracks.Items;
					bool channelHasEmptyTracks = false;

					for (int i = 0; i < channelGroup._backgroundTracks.Length; i++)
					{
						if (IsTrackPlaying(tracks[channelGroup._backgroundTracks[i]._trackIndex]))
						{
							emptyChannelTrackIndex = -1;
						}
						else
						{
							channelHasEmptyTracks = true;

							if (emptyChannelTrackIndex == -1)
								emptyChannelTrackIndex = i;
							else
								emptyChannelTrackIndex = Math.Min(emptyChannelTrackIndex, i);
						}
					}

					//No valid empty channel found
					if (emptyChannelTrackIndex == -1)
					{
						//If there are empty tracks just need to reorder tracks
						if (channelHasEmptyTracks)
						{
							//Shift all tracks down into empty tracks
							for (int i = 0; i < channelGroup._backgroundTracks.Length; i++)
							{
								if (!IsTrackPlaying(tracks[channelGroup._backgroundTracks[i]._trackIndex]))
								{
									for (int j = i + 1; j < channelGroup._backgroundTracks.Length; j++)
									{
										int intoTrackIndex = channelGroup._backgroundTracks[j - 1]._trackIndex;
										//Update track entry track
										tracks[intoTrackIndex] = tracks[channelGroup._backgroundTracks[j]._trackIndex];
										//Update track entry index (if playing)
										if (tracks[intoTrackIndex] != null)
											tracks[intoTrackIndex].trackIndex = intoTrackIndex;
										//Update channel track entry's track index
										channelGroup._backgroundTracks[j]._trackIndex = intoTrackIndex;
									}
								}
							}

							//Then get first free valid track
							return GetFreeChannelBackgroundTrack(channelGroup);
						}
						//Otherwise need to grow the tracks (and shift all channels after this one)
						else
						{
							//Increase track size to include new channel track
							_animationState.Tracks.GrowIfNeeded(_animationState.Tracks.Count + 1);

							//Work out new index
							int trackIndex = channelGroup._backgroundTracks[channelGroup._backgroundTracks.Length - 1]._trackIndex + 1;

							//Add new track to channel group
							ArrayUtils.Add(ref channelGroup._backgroundTracks, new ChannelTrack());

							//Create empty track for it
							tracks[trackIndex] = null;
							tracks[trackIndex] = _animationState.AddEmptyAnimation(trackIndex, 0f, 0f);
							channelGroup._backgroundTracks[channelGroup._backgroundTracks.Length - 1]._trackIndex = trackIndex;
							trackIndex++;

							//Loop through all channel groups after this one and set new track indices
							for (int i = 0; i < _channels.Count; i++)
							{
								if (_channels[i]._channel > channelGroup._channel)
								{
									for (int j = 0; j < _channels[i]._backgroundTracks.Length; j++)
									{
										//Update track entry track
										tracks[trackIndex] = tracks[_channels[i]._backgroundTracks[j]._trackIndex];
										//Update track entry index
										tracks[trackIndex].trackIndex = trackIndex;
										//Update channel track entry's track index
										_channels[i]._backgroundTracks[j]._trackIndex = trackIndex;

										trackIndex++;
									}

									//Update track entry track
									tracks[trackIndex] = tracks[_channels[i]._primaryTrack._trackIndex];
									//Update track entry index (if playing)
									if (tracks[trackIndex] != null)
										tracks[trackIndex].trackIndex = trackIndex;
									//Update primary track entry's track index
									_channels[i]._primaryTrack._trackIndex = trackIndex;

									trackIndex++;
								}
							}

							//Index is the last added
							return channelGroup._backgroundTracks.Length - 1;
						}
					}
					//Otherwise return the empty track
					else
					{
						return emptyChannelTrackIndex;
					}
				}

				private bool IsTrackPlaying(ChannelTrack track)
				{
					TrackEntry trackEntry = _animationState.Tracks.Items[track._trackIndex];
					return IsTrackPlaying(trackEntry);
				}

				private bool IsTrackPlaying(TrackEntry trackEntry)
				{
					return trackEntry != null && trackEntry.Animation != null;
				}

				private bool IsTrackPlaying(ChannelTrack track, string animName, out TrackEntry trackEntry)
				{
					trackEntry = _animationState.Tracks.Items[track._trackIndex];
					return trackEntry != null && trackEntry.Animation != null && trackEntry.Animation.Name == animName;
				}
				
				private void UpdateChannelBlends(ChannelGroup channelGroup)
				{
					TrackEntry[] tracks = _animationState.Tracks.Items;

					switch (channelGroup._state)
					{
						//If fading in primary track...
						case ChannelGroup.eState.BlendingIn:
							{
								channelGroup._lerpT += channelGroup._lerpSpeed * Time.deltaTime;
								
								if (channelGroup._lerpT >= 1.0f)
								{
									tracks[channelGroup._primaryTrack._trackIndex].alpha = channelGroup._targetWeight;

									for (int i = 0; i < channelGroup._backgroundTracks.Length; i++)
									{
										_animationState.ClearTrack(channelGroup._backgroundTracks[i]._trackIndex);
									}

									channelGroup._state = ChannelGroup.eState.Playing;
								}
								else
								{
									tracks[channelGroup._primaryTrack._trackIndex].alpha = MathUtils.Interpolate(channelGroup._lerpEase, 0f, channelGroup._targetWeight, channelGroup._lerpT);
								}
							}
							break;


						case ChannelGroup.eState.Stopping:
							{
								channelGroup._lerpT += channelGroup._lerpSpeed * Time.deltaTime;
								
								if (channelGroup._lerpT >= 1.0f)
								{
									StopChannel(channelGroup);
									channelGroup._state = ChannelGroup.eState.Stopped;
								}
								else
								{
									tracks[channelGroup._primaryTrack._trackIndex].alpha = MathUtils.Interpolate(channelGroup._lerpEase, channelGroup._primaryTrack._origWeight, 0f, channelGroup._lerpT);

									for (int i = 0; i < channelGroup._backgroundTracks.Length; i++)
									{
										tracks[channelGroup._backgroundTracks[i]._trackIndex].alpha = MathUtils.Interpolate(channelGroup._lerpEase, channelGroup._backgroundTracks[i]._origWeight, 0f, channelGroup._lerpT);
									}
								}
							}
							break;
					}
					
					//If have a queued animation,
					if (!string.IsNullOrEmpty(channelGroup._queuedAnimation))
					{
						TrackEntry trackEntry = tracks[channelGroup._primaryTrack._trackIndex];

						if (trackEntry != null && trackEntry.Animation != null)
						{
							float timeRemaining = trackEntry.Animation.Duration - trackEntry.AnimationTime;
							
							if (timeRemaining <= channelGroup._queuedAnimationBlendTime)
							{
								Play(channelGroup._channel, channelGroup._queuedAnimation, channelGroup._queuedAnimationWrapMode, timeRemaining, channelGroup._queuedAnimationEase, channelGroup._queuedAnimationWeight);
							}
						}
					}
				}

				private void StopChannel(ChannelGroup channelGroup)
				{
					if (channelGroup != null)
					{
						for (int i = 0; i < channelGroup._backgroundTracks.Length; i++)
						{
							_animationState.ClearTrack(channelGroup._backgroundTracks[i]._trackIndex);
						}

						_animationState.ClearTrack(channelGroup._primaryTrack._trackIndex);
					}
				}
				#endregion
			}
		}
	}
}
