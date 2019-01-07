using Spine;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public static class SpineUtils
			{
				public static void AddToSkin(Skeleton skeleton, Skin skin, Skin otherSkin)
				{
					ExposedList<Slot> slots = skeleton.Slots;
					for (int i = 0, n = slots.Count; i < n; i++)
					{
						Slot slot = slots.Items[i];
						string name = slot.Data.AttachmentName;
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

				public static bool FindSlot(Slot slot, string name)
				{
					if (slot.Data.Name == name)
						return true;
					else
						return false;
				}
			}
		}
	}
}
