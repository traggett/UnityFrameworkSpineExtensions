using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace Framework
{
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
					private ReorderableList _channelTracks;

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
						foreach (UnityEngine.Object target in base.targets)
						{
							SpineAnimatorTrack track = target as SpineAnimatorTrack;
							if (track == null)
								break;

							IEnumerable<TrackAsset> childTracks = track.GetChildTracks();
							
							GUILayout.Label(track.name, EditorStyles.boldLabel);
							GUILayout.Space(3f);
							_channelTracks.list = new List<TrackAsset>(childTracks);
							_channelTracks.DoLayoutList();
							_channelTracks.index = -1;
						}
					}

					//Until Unity makes the SupportsChildTracks public, have to hack our way around creating child tracks
					public static T CreateChildTrack<T>(TrackAsset parent, string name) where T : TrackAsset
					{
						T newTrack = null;

						//Add new track via reflection (puke)
						Type timelineWindowType = Type.GetType("UnityEditor.Timeline.TimelineWindow, UnityEditor.Timeline, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
						//UnityEditor.Timeline.TimelineWindow
						EditorWindow timelineWindow = EditorWindow.GetWindow(timelineWindowType);
						//AddTrack(Type type, TrackAsset parent = null, string name = null);
						MethodInfo methodInfo = timelineWindowType.GetMethod("AddTrack", new Type[] { typeof(Type), typeof(TrackAsset), typeof(string) });

						if (methodInfo != null)
						{
							newTrack = (T)methodInfo.Invoke(timelineWindow, new object[] { typeof(T), null, name });

							if (newTrack != null)
							{
								//Set as a child in parent track
								{
									SerializedObject parentSO = new SerializedObject(parent);

									SerializedProperty childrenProp = parentSO.FindProperty("m_Children");
									childrenProp.arraySize = childrenProp.arraySize + 1;
									SerializedProperty childProp = childrenProp.GetArrayElementAtIndex(childrenProp.arraySize - 1);
									childProp.objectReferenceValue = newTrack;

									parentSO.ApplyModifiedProperties();
								}

								//Mark parent on new track
								{
									SerializedObject childSO = new SerializedObject(newTrack);

									SerializedProperty parentProp = childSO.FindProperty("m_Parent");
									parentProp.objectReferenceValue = parent;

									childSO.ApplyModifiedProperties();
								}

								//Remove from timeline root tracks
								{
									SerializedObject timelineSO = new SerializedObject(parent.timelineAsset);
									SerializedProperty tracksProp = timelineSO.FindProperty("m_Tracks");

									List<UnityEngine.Object> tracks = new List<UnityEngine.Object>();
									
									for (int i = 0; i < tracksProp.arraySize; i++)
									{
										SerializedProperty trackProp = tracksProp.GetArrayElementAtIndex(i);

										if (trackProp.objectReferenceValue != newTrack)
										{
											tracks.Add(trackProp.objectReferenceValue);
										}
									}

									tracksProp.arraySize = tracks.Count;
									for (int i = 0; i < tracksProp.arraySize; i++)
									{
										SerializedProperty trackProp = tracksProp.GetArrayElementAtIndex(i);
										trackProp.objectReferenceValue = tracks[i];
									}

									timelineSO.ApplyModifiedProperties();
								}

								//Refresh the window to show new track as child
								{
									GameObject previousTimelineObject = null;
									if (TimelineEditor.inspectedDirector != null)
										previousTimelineObject = TimelineEditor.inspectedDirector.gameObject;

									//Have to set timeline to null and then back to this timeline to show changes grr...
									methodInfo = timelineWindowType.GetMethod("SetCurrentTimeline", new Type[] { typeof(TimelineAsset) });
									
									methodInfo.Invoke(timelineWindow, new object[] { null });
									methodInfo.Invoke(timelineWindow, new object[] { parent.timelineAsset });
									
									//Also need to reselect whatever timeline object was previously selected as above clears it
									Selection.activeGameObject = previousTimelineObject;
								}
							}
						}

						return newTrack;
					}

					private void OnAddChannel(ReorderableList list)
					{
						foreach (UnityEngine.Object target in base.targets)
						{
							AddChanelToTrack(target as SpineAnimatorTrack);
						}
					}

					private static void AddChanelToTrack(SpineAnimatorTrack spineAnimatorTrack)
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

							SpineAnimatorChannelTrack newTrack = CreateChildTrack<SpineAnimatorChannelTrack>(spineAnimatorTrack, "Channel " + channel);
							newTrack._animationChannel = channel;

							//The parent track needs at least one clip in order to create a mixer, ensure a dummy 'master clip' exists
							spineAnimatorTrack.EnsureMasterClipExists();
						}
					}
					
					private static void OnDrawHeader(Rect rect)
					{
						float columnWidth = rect.width /= 3f;
						GUI.Label(rect, "Channel", EditorStyles.label);
						rect.x += columnWidth;
						GUI.Label(rect, "Duration", EditorStyles.label);
						rect.x += columnWidth;
						GUI.Label(rect, "Clips", EditorStyles.label);
					}

					private void OnDrawSubTrack(Rect rect, int index, bool selected, bool focused)
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
							GUI.Label(rect, GetCount(track.GetClips()).ToString(), EditorStyles.label);
						}
					}

					private static int GetCount<T>(IEnumerable<T> enumerable)
					{
						int count = 0;

						foreach (T t in enumerable)
						{
							count++;
						}

						return count;
					}
				}
			}
		}
	}
}