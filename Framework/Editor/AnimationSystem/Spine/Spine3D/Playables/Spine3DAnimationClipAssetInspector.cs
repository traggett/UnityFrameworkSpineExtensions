using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			namespace Editor
			{
				[CustomEditor(typeof(Spine3DAnimationClipAsset))]
				public class Spine3DAnimationClipAssetInspector : UnityEditor.Editor
				{
					public override void OnInspectorGUI()
					{
						serializedObject.Update();

						SerializedProperty animationIdProperty = serializedObject.FindProperty("_animationId");
						SerializedProperty animationDurationProperty = serializedObject.FindProperty("_animationDuration");
						SerializedProperty animationSpeedProperty = serializedObject.FindProperty("_animationSpeed");

						Spine3DAnimationClipAsset clip = (Spine3DAnimationClipAsset)target;
						Spine3DAnimator animator = GetClipBoundAnimator(clip);

						if (animator != null)
						{
							string[] animationNames = animator.GetAnimationNames();

							int currentIndex = -1;

							for (int i = 0; i < animationNames.Length; i++)
							{
								if (animationNames[i] == animationIdProperty.stringValue)
								{
									currentIndex = i;
									break;
								}
							}

							int index = EditorGUILayout.Popup("Animation", currentIndex == -1 ? 0 : currentIndex, animationNames);

							if (currentIndex != index)
							{
								clip.name = animationNames[index];
								animationIdProperty.stringValue = animationNames[index];
								animationDurationProperty.doubleValue = animator.GetAnimationLength(animationNames[index]);
							}
						}
						else
						{
							GUI.enabled = false;
							EditorGUILayout.PropertyField(animationIdProperty);
							GUI.enabled = true;
						}

						EditorGUILayout.PropertyField(animationSpeedProperty);

						serializedObject.ApplyModifiedProperties();
					}
					
					private static Spine3DAnimator GetClipBoundAnimator(Spine3DAnimationClipAsset clip)
					{
						PlayableDirector selectedDirector = TimelineEditor.inspectedDirector;

						if (selectedDirector != null)
						{
							return selectedDirector.GetGenericBinding(clip.GetParentTrack()) as Spine3DAnimator;
						}

						return null;
					}
				}
			}
		}
	}
}