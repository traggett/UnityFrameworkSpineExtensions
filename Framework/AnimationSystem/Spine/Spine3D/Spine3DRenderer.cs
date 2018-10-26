using UnityEngine;

using Framework.Maths;
using Framework.Utils;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			[ExecuteInEditMode]

			//Class that renders a set of spine animators as a 3d object, switching  between them based on the angle to the camera currently rendering.
			public class Spine3DRenderer : MonoBehaviour
			{
				public Transform _graphicsOrigin;
				public Spine3DAnimationSet[] _animationSets;
				
				public float _minRollAngle;
				public float _maxRollAngle;

				public CameraEvents _camera;
				
				#region MonoBehaviour
				void Awake()
				{
					if (_graphicsOrigin == null)
					{
						_graphicsOrigin = this.transform;
					}
				}

				void OnEnable()
				{
					if (_camera != null)
						_camera._onPreCull += OnCameraPreRender;
				}

				void Update()
				{

				}

				void OnDestroy()
				{
					
				}
				#endregion

				private void OnCameraPreRender(object sender, CameraEvents.CameraEvent e)
				{
					SetAnimationSetForCamera(e.Camera);
				}

				private void SetAnimationSetActive(Spine3DAnimationSet animationSet, bool active)
				{
					animationSet.gameObject.SetActive(active);
				}

				public void SetAnimationSetForCamera(Camera camera)
				{
					//Work out angle between character face direction and camera forward.
					//Choose a animation set based on horizontal angle between camera forward and character forward

					//Convert camera pos and forward into character space
					Vector3 localspaceCameraPos = _graphicsOrigin.InverseTransformPoint(camera.transform.position);
					Vector3 localspaceCameraDir = _graphicsOrigin.InverseTransformDirection(-camera.transform.forward);

					//Get forward in XY space
					Vector2 localspaceCameraDirXY = new Vector2(localspaceCameraDir.x, localspaceCameraDir.z).normalized;
					Vector2 forwardXY = Vector2.up;

					//The angle between camera forward and character forward
					float horizAngle = MathUtils.AngleBetween(forwardXY, localspaceCameraDirXY);

					//Work out which animations to use
					int bestAnimationSet = -1;
					float nearestAngleDif = 0.0f;

					for (int i = 0; i < _animationSets.Length; i++)
					{
						//Disable the sets renderer
						SetAnimationSetActive(_animationSets[i], false);

						//Never use sprites that direction is more than 90 degrees to camera
						Vector2 spriteForwardXY = MathUtils.Rotate(forwardXY, _animationSets[i]._fowardAngle);
						float spriteAngle = MathUtils.AngleBetween(localspaceCameraDirXY, spriteForwardXY);

						if (Mathf.Abs(spriteAngle) < _animationSets[i]._maxAngle)
						{
							float angleDiff = MathUtils.AngleDiff(horizAngle, MathUtils.DegreesTo180Range(_animationSets[i]._faceAngle));

							if (bestAnimationSet == -1 || Mathf.Abs(angleDiff) < Mathf.Abs(nearestAngleDif))
							{
								bestAnimationSet = i;
								nearestAngleDif = angleDiff;
							}
						}
					}

					if (bestAnimationSet != -1)
					{
						Spine3DAnimationSet animationSet = _animationSets[bestAnimationSet];

						SetAnimationSetActive(animationSet, true);

						//Horizontal sprite rotation
						{
							//Rotate this forward to face sprite towards its face angle.
							Vector2 localSpaceSpriteForwardXY = MathUtils.Rotate(forwardXY, -animationSet._faceAngle);

							//When hit max view angle maintain that relative angle diff.
							float spriteAngle = MathUtils.AngleBetween(localspaceCameraDirXY, localSpaceSpriteForwardXY);
							float spriteMaxAngle =  animationSet._maxViewAngle;
							if (Mathf.Abs(spriteAngle) > spriteMaxAngle)
							{
								//Rotate camera forward by max angle in correct direction
								float clampedAngle = spriteMaxAngle;
								if (spriteAngle < 0) clampedAngle = -clampedAngle;

								localSpaceSpriteForwardXY = MathUtils.Rotate(localspaceCameraDirXY, -clampedAngle);
							}

							//Set rotation matrix based off adjusted up and the correct sprite forward vector
							{
								Vector3 spriteForward = new Vector3(localSpaceSpriteForwardXY.x, 0.0f, localSpaceSpriteForwardXY.y).normalized;
								animationSet.transform.localRotation = Quaternion.FromToRotation(Vector3.forward, -spriteForward);
							}
						}

						//Vertical sprite rotation
						{
							Vector3 localSpaceCamerDirFlat = new Vector3(localspaceCameraDir.x, 0.0f, localspaceCameraDir.z).normalized;

							//Work out roll angle between sprite and camera
							float rollAngle = Vector3.Angle(localSpaceCamerDirFlat, localspaceCameraDir);
							if (localspaceCameraDir.y < 0) rollAngle = -rollAngle;

							//If roll angle is too severe then rotate sprite to align better with the camera
							if (Mathf.Abs(rollAngle) > _minRollAngle)
							{
								//ifs over min angle then rotate by amount over
								float angle = Mathf.Abs(rollAngle) - _minRollAngle;

								if (angle > _maxRollAngle)
									angle = _maxRollAngle;

								if (rollAngle < 0.0f)
									angle = -angle;

								Vector3 spriteAxis = Vector3.Cross(Vector3.up, localspaceCameraDir).normalized;
								animationSet.transform.localRotation *= Quaternion.AngleAxis(angle, Vector3.right);
							}
						}
					}
				}

#if UNITY_EDITOR
				void OnDrawGizmosSelected()
				{
					foreach (Spine3DAnimationSet animation in _animationSets)
					{
						UnityEditor.Handles.ArrowHandleCap(0, this.transform.position, Quaternion.AngleAxis(animation._faceAngle, this.transform.up), 0.4f, EventType.Repaint);
					}
				}
#endif
			}
		}
	}
}