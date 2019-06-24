using UnityEditor;
using UnityEngine;
using Spine.Unity;
using Animation = Spine.Animation;
using Spine;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			namespace Editor
			{
				[CustomEditor(typeof(SpineProxyAnimationClipAsset))]
				[CanEditMultipleObjects]
				public class SpineProxyAnimationClipAssetInspector : UnityEditor.Editor
				{
					private SkeletonData _skeletonData;

					public override void OnInspectorGUI()
					{
						SerializedProperty nameProperty = serializedObject.FindProperty("m_Name");
						SerializedProperty animationsProperty = serializedObject.FindProperty("_animationSource");
						SerializedProperty animationIdProperty = serializedObject.FindProperty("_animationId");
						SerializedProperty animationDurationProperty = serializedObject.FindProperty("_animationDuration");

						EditorGUI.BeginChangeCheck();
						EditorGUILayout.PropertyField(animationsProperty);
						if (EditorGUI.EndChangeCheck() || _skeletonData == null)
						{
							if (animationsProperty.objectReferenceValue != null)
							{
								_skeletonData = ((SkeletonDataAsset)(animationsProperty.objectReferenceValue)).GetSkeletonData(false);
							}
							else
							{
								animationIdProperty.stringValue = null;
							}
						}
						
						if (_skeletonData != null)
						{
							Animation[] animations = _skeletonData.Animations.Items;

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

						serializedObject.ApplyModifiedProperties();
					}
				}
			}
		}
	}
}