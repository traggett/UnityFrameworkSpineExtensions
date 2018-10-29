using System;

namespace Framework
{
	namespace AnimationSystem
	{
		namespace Spine
		{
			[Flags]
			public enum eSpine3DOrientation
			{
				Front = 1 << 0,
				Back = 1 << 1,
				Left = 1 << 2,
				Right = 1 << 3,

				FrontLeft = 1 << 4,
				FrontRight = 1 << 5,
				BackLeft = 1 << 6,
				BackRight = 1 << 7,
			}
		}
	}
}