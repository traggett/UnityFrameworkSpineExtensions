using Spine;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			namespace Editor
			{
				[CustomEditor(typeof(SpineAnimatorStateBehaviour))]
				public class SpineAnimatorStateBehaviourInspector : UnityEditor.Editor
				{
					private static class EditorHelper
					{
						private static SpineAnimator _animator;

						static EditorHelper()
						{
							Selection.selectionChanged += OnSelectionChanged;
						}

						private static void OnSelectionChanged()
						{
							if (Selection.activeGameObject != null)
							{
								SpineAnimator animator = Selection.activeGameObject.GetComponent<SpineAnimator>();

								if (animator != null)
									_animator = animator;
							}
						}

						public static SpineAnimator GetLastInspectedSpineAnimator()
						{
							return _animator;
						}
					}

					public override void OnInspectorGUI()
					{
						SerializedProperty animationIdProperty = serializedObject.FindProperty("_animationName");
						SpineAnimator animator = EditorHelper.GetLastInspectedSpineAnimator();

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
								animationIdProperty.stringValue = animationNames[index];
								serializedObject.ApplyModifiedProperties();

								if (Application.isPlaying)
								{
									((SpineAnimatorStateBehaviour)target).ForceUpdate();
								}
							}
						}
						else
						{
							GUI.enabled = false;
							EditorGUILayout.PropertyField(animationIdProperty);
							GUI.enabled = true;
						}
					}
				}
			}
		}
	}
}