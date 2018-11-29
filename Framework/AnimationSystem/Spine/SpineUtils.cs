using UnityEngine;
using System;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using Animation = Spine.Animation;
using AnimationState = Spine.AnimationState;

namespace Framework
{
	using Maths;
	using Utils;
	using MathUtils = Maths.MathUtils;

	namespace AnimationSystem
	{
		namespace Spine
		{
			public static class SpineUtils
			{
				public static void AddToSkin(Skeleton skeleton, Skin skin, Skin otherSkin)
				{
					ExposedList<Slot> slots = skeleton.slots;
					for (int i = 0, n = slots.Count; i < n; i++)
					{
						Slot slot = slots.Items[i];
						string name = slot.data.attachmentName;
						if (name != null)
						{
							Attachment attachment = otherSkin.GetAttachment(i, name);
							if (attachment != null)
							{
								skin.AddAttachment(i, name, attachment);
							}
						}
					}
				}
			}
		}
	}
}
