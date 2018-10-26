using UnityEngine;

namespace Game
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public class SpineRootMotionPathMover : SpineRootMotion
			{
				public CharacterMover _mover;
				public bool _flipForwardDirection;

				protected override void OnApplyMotion(Vector2 localDelta)
				{
					//Move along path in using localDelta.x
					if (_mover != null)
					{
						_mover.Move(_flipForwardDirection ? -localDelta.x : localDelta.x);
					}
				}
			}
		}
	}
}
