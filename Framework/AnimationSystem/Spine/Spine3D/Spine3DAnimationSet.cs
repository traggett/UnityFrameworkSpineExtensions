using UnityEngine;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			//Represents a set of animations relating to a 3d sprite from a set angle
			public class Spine3DAnimationSet : MonoBehaviour
			{
				public SpineAnimator _animatior;

				//The angle this sprite visually looks like its facing down (if a sprite graphics look like its facing into the screen a bit then set this angle accordingly).
				public float _faceAngle;
				//The max angle to the camera this sprite can be used (if its 90 then can always be used)
				public float _maxAngle;
				//The direction of the sprite (either forward (0) or backward (180)
				public float _fowardAngle;
				//The max angle this sprite can be facing away from the camera before rotating to face it. (If this is 0 then sprite will always face the camera).
				public float _maxViewAngle;
				//This is the prefix used by this set
				public string _animationPrefix;
			}
		}
	}
}