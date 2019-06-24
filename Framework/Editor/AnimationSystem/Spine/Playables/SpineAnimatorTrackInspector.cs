using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

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
				[CustomEditor(typeof(SpineAnimatorTrack))]
				[CanEditMultipleObjects]
				public class SpineAnimatorTrackInspector : UnityEditor.Editor
				{
					protected ReorderableList _channelTracks;

					public void OnEnable()
					{
						_channelTracks = new ReorderableList(new TrackAsset[0], typeof(TrackAsset), false, true, true, false)
						{
							drawElementCallback = new ReorderableList.ElementCallbackDelegate(OnDrawSubTrack),
							drawHeaderCallback = new ReorderableList.HeaderCallbackDelegate(OnDrawHeader),
							onAddCallback = new ReorderableList.AddCallbackDelegate(OnAddChannel),
							showDefaultBackground = true,
							index = 0,
							elementHeight = 20f
						};
					}

					public override void OnInspectorGUI()
					{
						foreach (Object target in base.targets)
						{
							SpineAnimatorTrack track = target as SpineAnimatorTrack;
							if (track == null)
								break;

							IEnumerable<TrackAsset> childTracks = track.GetChildTracks();
							
							GUILayout.Label(track.name, EditorStyles.boldLabel);
							track._resetPose = EditorGUILayout.Toggle("Reset Pose", track._resetPose);
							GUILayout.Space(3f);
							_channelTracks.list = new List<TrackAsset>(childTracks);
							_channelTracks.DoLayoutList();
							_channelTracks.index = -1;

							track.EnsureMasterClipExists();
						}
					}

					private void OnAddChannel(ReorderableList list)
					{
						foreach (Object target in base.targets)
						{
							AddChanelToTrack(target as SpineAnimatorTrack);
						}
					}

					protected virtual void AddChanelToTrack(SpineAnimatorTrack spineAnimatorTrack)
					{
						if (spineAnimatorTrack != null)
						{
							//Work out next free channel to add
							int channel = 0;

							foreach (SpineAnimatorChannelTrack track in spineAnimatorTrack.GetChildTracks())
							{
								if (track != null)
								{
									channel = Mathf.Max(channel, track._animationChannel + 1);
								}
							}

							SpineAnimatorChannelTrack newTrack = TimelineEditorUtils.CreateChildTrack<SpineAnimatorChannelTrack>(spineAnimatorTrack, "Channel " + channel);
							newTrack._animationChannel = channel;
						}
					}

					protected virtual void OnDrawHeader(Rect rect)
					{
						float columnWidth = rect.width /= 3f;
						GUI.Label(rect, "Channel", EditorStyles.label);
						rect.x += columnWidth;
						GUI.Label(rect, "Duration", EditorStyles.label);
						rect.x += columnWidth;
						GUI.Label(rect, "Clips", EditorStyles.label);
					}

					protected virtual void OnDrawSubTrack(Rect rect, int index, bool selected, bool focused)
					{ 
						float columnWidth = rect.width / 3f;
						SpineAnimatorChannelTrack track = _channelTracks.list[index] as SpineAnimatorChannelTrack;

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