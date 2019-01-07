using UnityEngine;

using Spine;
using Animation = Spine.Animation;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			public static class TimelineTools
			{
				/// <summary>Gets the translate timeline for a given boneIndex. You can get the boneIndex using SkeletonData.FindBoneIndex. The root bone is always boneIndex 0.
				/// This will return null if a TranslateTimeline is not found.</summary>
				public static TranslateTimeline FindTranslateTimelineForBone(this Animation a, int boneIndex)
				{
					foreach (var t in a.Timelines)
					{
						var tt = t as TranslateTimeline;
						if (tt != null && tt.BoneIndex == boneIndex)
							return tt;
					}
					return null;
				}

				/// <summary>Evaluates the resulting value of a TranslateTimeline at a given time.
				/// SkeletonData can be accessed from Skeleton.Data or from SkeletonDataAsset.GetSkeletonData.
				/// If no SkeletonData is given, values are computed relative to setup pose instead of local-absolute.</summary>
				public static Vector2 Evaluate(this TranslateTimeline tt, float time, SkeletonData skeletonData = null)
				{
					const int PREV_TIME = -3, PREV_X = -2, PREV_Y = -1;
					const int X = 1, Y = 2;

					var frames = tt.Frames;
					if (time < frames[0]) return Vector2.zero;

					float x, y;
					if (time >= frames[frames.Length - TranslateTimeline.ENTRIES])
					{ // Time is after last frame.
						x = frames[frames.Length + PREV_X];
						y = frames[frames.Length + PREV_Y];
					}
					else {
						int frame = Animation.BinarySearch(frames, time, TranslateTimeline.ENTRIES);
						x = frames[frame + PREV_X];
						y = frames[frame + PREV_Y];
						float frameTime = frames[frame];
						float percent = tt.GetCurvePercent(frame / TranslateTimeline.ENTRIES - 1, 1 - (time - frameTime) / (frames[frame + PREV_TIME] - frameTime));

						x += (frames[frame + X] - x) * percent;
						y += (frames[frame + Y] - y) * percent;
					}

					Vector2 o = new Vector2(x, y);

					if (skeletonData == null)
					{
						return o;
					}
					else {
						var boneData = skeletonData.Bones.Items[tt.BoneIndex];
						return o + new Vector2(boneData.X, boneData.Y);
					}
				}
			}
		}
	}
}
