using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using Spine.Unity;
using Animation = Spine.Animation;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			namespace Editor
			{
				[CustomEditor(typeof(SpineAnimationClipAsset))]
				[CanEditMultipleObjects]
				public class SpineAnimationClipAssetInspector : UnityEditor.Editor
				{
					public override void OnInspectorGUI()
					{
						SerializedProperty nameProperty = serializedObject.FindProperty("m_Name");
						SerializedProperty animationIdProperty = serializedObject.FindProperty("_animationId");
						SerializedProperty animationDurationProperty = serializedObject.FindProperty("_animationDuration");
						SerializedProperty animationSpeedProperty = serializedObject.FindProperty("_animationSpeed");

						SkeletonAnimation animator = GetClipBoundAnimator();

						if (animator != null)
						{
							Animation[] animations = animator.skeletonDataAsset.GetAnimationStateData().SkeletonData.Animations.Items;

							string[] animationNames = new string[animations.Length];

							for (int i = 0; i < animations.Length; i++)
							{
								animationNames[i] = animations[i].Name;
							}

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
								nameProperty.stringValue = animationNames[index];
								animationIdProperty.stringValue = animationNames[index];
								animationDurationProperty.doubleValue = animations[index].Duration;
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
					
					private SkeletonAnimation GetClipBoundAnimator()
					{
						PlayableDirector selectedDirector = TimelineEditor.inspectedDirector;
						SpineAnimationClipAsset clip = base.target as SpineAnimationClipAsset;

						if (selectedDirector != null && clip != null)
						{
							return selectedDirector.GetGenericBinding(clip.GetParentTrack()) as SkeletonAnimation;
						}

						return null;
					}
				}
			}
		}
	}
}