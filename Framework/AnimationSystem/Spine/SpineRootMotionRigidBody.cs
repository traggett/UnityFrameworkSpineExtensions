using UnityEngine;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public class SpineRootMotionRigidBody : SpineRootMotion
			{
				#region Inspector
				public Rigidbody2D rb;
				#endregion
				
				bool useRigidBody;
				Vector2 accumulatedDisplacement;
				
				void FixedUpdate()
				{
					if (this.isActiveAndEnabled && this.useRigidBody)
					{ // Root motion is only applied when component is enabled.
						Vector2 v = rb.velocity;
						if (useX) v.x = accumulatedDisplacement.x / Time.fixedDeltaTime;
						if (useY) v.y = accumulatedDisplacement.y / Time.fixedDeltaTime;
						rb.velocity = v;
						accumulatedDisplacement = Vector2.zero;

						// When using Transform position. This causes the rigidbody to lose contact data.
						//				var p = transform.position;
						//				if (controlRigidbodyX) p.x += accumulatedDisplacement.x;
						//				if (controlRigidbodyY) p.y += accumulatedDisplacement.y;
						//				transform.position = p;
						//				accumulatedDisplacement = Vector2.zero;
					}
				}
				
				void OnDisable()
				{
					accumulatedDisplacement = Vector2.zero;
				}

				protected override void OnApplyMotion(Vector2 localDelta)
				{
					accumulatedDisplacement += (Vector2)transform.TransformVector(localDelta);
					// Accumulated displacement is applied on the next Physics update (FixedUpdate)
				}
			}
		}
	}
}
