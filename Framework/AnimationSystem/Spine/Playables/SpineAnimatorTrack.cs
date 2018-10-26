using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Spine.Unity;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			[TrackColor(255f / 255f, 64f / 255f, 0f / 255f)]
			[TrackBindingType(typeof(SkeletonAnimation))]
			[TrackClipType(typeof(SpineMasterClipAsset), false)]
			//[SupportsChildTracks(typeof(SpineAnimatorChannelTrack), 1]		//Hopefully Unity will make this attribute public soon :/
			public class SpineAnimatorTrack : TrackAsset
			{
				public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
				{
					EnsureMasterClipExists();

					ScriptPlayable<SpineAnimatorTrackMixer> playable = ScriptPlayable<SpineAnimatorTrackMixer>.Create(graph, inputCount);
					SpineAnimatorTrackMixer mixer = playable.GetBehaviour();
					PlayableDirector playableDirector = go.GetComponent<PlayableDirector>();

					mixer.SetTrackAsset(this, playableDirector);

					return playable;
				}
				
				//Find the track mixer matching this track asset inside a playable graph
				public static SpineAnimatorTrackMixer GetTrackMixer(PlayableGraph graph, SpineAnimatorTrack track)
				{
					int rootCount = graph.GetRootPlayableCount();

					for (int i = 0; i < rootCount; i++)
					{
						Playable root = graph.GetRootPlayable(i);

						SpineAnimatorTrackMixer trackMixer = GetTrackMixer(root, track);

						if (trackMixer != null)
						{
							return trackMixer;
						}
					}
					
					return null;
				}

				public void EnsureMasterClipExists()
				{
					TimelineClip masterClip = null;

					foreach (TimelineClip clip in GetClips())
					{
						masterClip = clip;
						break;
					}

					if (masterClip == null)
					{
						masterClip = CreateDefaultClip();
					}

					//Set clips duration to match max duration of timeline
					masterClip.start = 0;
					masterClip.duration = 0;
					masterClip.duration = this.timelineAsset.duration;
					masterClip.displayName = "(Spine Animation)";

					//TO DO! Match max length of all clips - not timeline duration which includes this
				}

				private static SpineAnimatorTrackMixer GetTrackMixer(Playable root, SpineAnimatorTrack track)
				{
					int inputCount = root.GetInputCount();;

					for (int i = 0; i < inputCount; i++)
					{
						Playable rootInput = root.GetInput(i);

						if (rootInput.IsValid())
						{
							//If this input is a SpineAnimatorTrackMixer, check it matches our track
							if (rootInput.GetPlayableType() == typeof(SpineAnimatorTrackMixer))
							{
								ScriptPlayable<SpineAnimatorTrackMixer> scriptPlayable = (ScriptPlayable<SpineAnimatorTrackMixer>)rootInput;
								SpineAnimatorTrackMixer trackMixer = scriptPlayable.GetBehaviour();

								if (trackMixer.GetTrackAsset() == track)
								{
									return trackMixer;
								}
							}

							//Otherwise search this playable's inputs
							{
								SpineAnimatorTrackMixer trackMixer = GetTrackMixer(rootInput, track);

								if (trackMixer != null)
								{
									return trackMixer;
								}
							}
						}
					}
					
					return null;
				}
			}
		}
	}
}