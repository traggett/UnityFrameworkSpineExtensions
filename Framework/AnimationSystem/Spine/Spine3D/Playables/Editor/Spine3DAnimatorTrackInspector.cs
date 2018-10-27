using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace Framework
{
	using Playables.Editor;

	namespace AnimationSystem
	{
		namespace Spine
		{
			namespace Editor
			{
				[CustomEditor(typeof(Spine3DAnimatorTrack))]
				[CanEditMultipleObjects]
				public class Spine3DAnimatorTrackInspector : SpineAnimatorTrackInspector
				{
					protected override void AddChanelToTrack(SpineAnimatorTrack spineAnimatorTrack)
					{
						if (spineAnimatorTrack != null)
						{
							//Work out next free channel to add
							int channel = 0;

							foreach (Spine3DAnimatorChannelTrack track in spineAnimatorTrack.GetChildTracks())
							{
								if (track != null)
								{
									channel = Mathf.Max(channel, track._animationChannel + 1);
								}
							}

							Spine3DAnimatorChannelTrack newTrack = TimelineEditorUtils.CreateChildTrack<Spine3DAnimatorChannelTrack>(spineAnimatorTrack, "Channel " + channel);
							newTrack._animationChannel = channel;
						}
					}
				}
			}
		}
	}
}