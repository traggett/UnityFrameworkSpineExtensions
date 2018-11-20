using UnityEngine;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public class Spine3DAnimatorRootMotion : MonoBehaviour
			{
				#region Public Data
				public Spine3DRenderer _renderer;
				public event SpineRootMotion.OnMotion _onMotion = delegate { };
				#endregion

				#region Private Data
				private Spine3DAnimationSet _lastRenderedAnimationSet;
				#endregion

				#region MonoBehaviour
				private void Start()
				{
					if (_renderer!= null)
					{
						_renderer._onRenderAnimationSet += OnRenderAnimationSet;

						for (int i=0; i< _renderer._animationSets.Length; i++)
						{
							SpineRootMotion rootMotion = _renderer._animationSets[i]._animatior.GetComponent<SpineRootMotion>();

							if (rootMotion != null)
							{
								rootMotion._onMotion += OnApplyMotion;
							}
						}
					}
					
				}

				private void LateUpdate()
				{
					//Clear last rendered animator
					_lastRenderedAnimationSet = null;
				}
				#endregion

				#region Private Functions
				private void OnRenderAnimationSet(Spine3DRenderer renderer, Spine3DAnimationSet animationSet)
				{
					if (_lastRenderedAnimationSet == null)
						_lastRenderedAnimationSet = animationSet;
				}

				private void OnApplyMotion(SpineRootMotion rootMotion, Vector2 localDelta)
				{
					//Only apply root motion if this is the last rendered animator
					if (_lastRenderedAnimationSet != null && _lastRenderedAnimationSet._animatior.gameObject == rootMotion.gameObject)
					{
						if (_onMotion != null)
						{
							_onMotion.Invoke(rootMotion, localDelta);
						}
					}
				}
				#endregion
			}
		}
	}
}
