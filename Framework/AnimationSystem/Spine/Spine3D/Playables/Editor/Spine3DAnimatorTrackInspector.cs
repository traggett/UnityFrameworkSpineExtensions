using UnityEditor;
using UnityEngine;

namespace Framework
{
	using Playables.Editor;
	using Utils;

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

					protected override void OnDrawSubTrack(Rect rect, int index, bool selected, bool focused)
					{
						float columnWidth = rect.width / 3f;
						Spine3DAnimatorChannelTrack track = _channelTracks.list[index] as Spine3DAnimatorChannelTrack;

						if (track != null)
						{
							rect.width = columnWidth;
							GUI.Label(rect, track._animationChannel.ToString(), EditorStyles.label);
							rect.x += columnWidth;
							GUI.Label(rect, track.duration.ToString(), EditorStyles.label);
							rect.x += columnWidth;
							GUI.Label(rect, ArrayUtils.GetCount(track.GetClips()).ToString(), EditorStyles.label);
						}
					}
				}
			}
		}
	}
}