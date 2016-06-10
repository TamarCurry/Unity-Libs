// Author : Tamar Curry

namespace Libs.Tweens
{
	using System;

	public class Easing
	{
		// -----------------------------------------------------------------------------------------------
		// Formulas taken from the feffects easing classes for HaXE: 
		// https://code.google.com/p/feffects/
		// -----------------------------------------------------------------------------------------------
		public static float Linear(float t, float b, float c, float d)
		{
			return c * t / d + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float BackEaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * ((1.70158f + 1) * t - 1.70158f) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float BackEaseOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1) * t * ((1.70158f + 1) * t + 1.70158f) + 1) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float BackEaseInOut(float t, float b, float c, float d)
		{
			float s = 1.70158f;
			if ((t /= d * 0.5f) < 1)
			{
				return c * 0.5f * (t * t * (((s *= (1.525f)) + 1) * t - s)) + b;
			}
			else
			{
				return c * 0.5f * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float BounceEaseOut(float t, float b, float c, float d)
		{
			if ((t /= d) < (1 / 2.75))
			{
				return c * (7.5625f * t * t) + b;
			}
			else if (t < (2f / 2.75f))
			{
				return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + b;
			}
			else if (t < (2.5f / 2.75f))
			{
				return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + b;
			}
			else
			{
				return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float BounceEaseIn(float t, float b, float c, float d)
		{
			return c - BounceEaseOut(d - t, 0, c, d) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float BounceEaseInOut(float t, float b, float c, float d)
		{
			if (t < d * 0.5f)
			{
				return BounceEaseIn(t * 2f, 0, c, d) * .5f + b;
			}
			else
			{
				return BounceEaseOut(t * 2f - d, 0, c, d) * .5f + c * .5f + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float CircEaseIn(float t, float b, float c, float d)
		{
			return -c * (float)(Math.Sqrt(1 - (t /= d) * t) - 1) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float CircEaseOut(float t, float b, float c, float d)
		{
			return c * (float)Math.Sqrt(1 - (t = t / d - 1) * t) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float CircEaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d * 0.5f) < 1)
			{
				return -c * 0.5f * (float)(Math.Sqrt(1 - t * t) - 1) + b;
			}
			else
			{
				return c * 0.5f * (float)(Math.Sqrt(1 - (t -= 2) * t) + 1) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float CubicEaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float CubicEaseOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1) * t * t + 1) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float CubicEaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d * 0.5f) < 1)
			{
				return c * 0.5f * t * t * t + b;
			}
			else
			{
				return c * 0.5f * ((t -= 2) * t * t + 2) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float ElasticEaseIn(float t, float b, float c, float d)
		{
			if (t == 0)
			{
				return b;
			}
			if ((t /= d) == 1)
			{
				return b + c;
			}
			else
			{
				float p = d * .3f;
				float s = p * 0.25f;
				return -(float)(c * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float ElasticEaseOut(float t, float b, float c, float d)
		{
			if (t == 0)
			{
				return b;
			}
			else if ((t /= d) == 1)
			{
				return b + c;
			}
			else
			{
				float p = d * .3f;
				float s = p * 0.25f;
				return (float)(c * Math.Pow(2, -10 * t) * Math.Sin((t * d - s) * (2 * Math.PI) / p) + c + b);
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float ElasticEaseInOut(float t, float b, float c, float d)
		{
			if (t == 0)
			{
				return b;
			}
			else if ((t /= d / 2) == 2)
			{
				return b + c;
			}
			else
			{
				float p = d * (.3f * 1.5f);
				float s = p * 0.25f;
				if (t < 1)
					return -.5f * (float)(c * Math.Pow(2, 10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) + b;
				else
					return c * (float)(Math.Pow(2, -10 * (t -= 1)) * Math.Sin((t * d - s) * (2 * Math.PI) / p)) * .5f + c + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float ExpoEaseIn(float t, float b, float c, float d)
		{
			return (t == 0) ? b : c * (float)Math.Pow(2, 10 * (t / d - 1)) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float ExpoEaseOut(float t, float b, float c, float d)
		{
			return (t == d) ? b + c : c * (float)(-Math.Pow(2, -10 * t / d) + 1) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float ExpoEaseInOut(float t, float b, float c, float d)
		{
			if (t == 0)
			{
				return b;
			}
			else if (t == d)
			{
				return b + c;
			}
			else if ((t /= d / 2) < 1)
			{
				return c * 0.5f * (float)Math.Pow(2, 10 * (t - 1)) + b;
			}
			else
			{
				return c * 0.5f * (float)(-Math.Pow(2, -10 * --t) + 2) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuadEaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuadEaseOut(float t, float b, float c, float d)
		{
			return -c * (t /= d) * (t - 2) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuadEaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d * 0.5f) < 1)
			{
				return c * 0.5f * t * t + b;
			}
			else
			{
				return -c * 0.5f * ((--t) * (t - 2) - 1) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuartEaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t * t + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuartEaseOut(float t, float b, float c, float d)
		{
			return -c * ((t = t / d - 1) * t * t * t - 1) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuartEaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d * 0.5f) < 1)
			{
				return c * 0.5f * t * t * t * t + b;
			}
			else
			{
				return -c * 0.5f * ((t -= 2) * t * t * t - 2) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuintEaseIn(float t, float b, float c, float d)
		{
			return c * (t /= d) * t * t * t * t + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuintEaseOut(float t, float b, float c, float d)
		{
			return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float QuintEaseInOut(float t, float b, float c, float d)
		{
			if ((t /= d * 0.5f) < 1)
			{
				return c * 0.5f * t * t * t * t * t + b;
			}
			else
			{
				return c * 0.5f * ((t -= 2) * t * t * t * t + 2) + b;
			}
		}

		// -----------------------------------------------------------------------------------------------
		public static float SineEaseIn(float t, float b, float c, float d)
		{
			return -c * (float)Math.Cos(t / d * (Math.PI * 0.5)) + c + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float SineEaseOut(float t, float b, float c, float d)
		{
			return c * (float)Math.Sin(t / d * (Math.PI * 0.5)) + b;
		}

		// -----------------------------------------------------------------------------------------------
		public static float SineEaseInOut(float t, float b, float c, float d)
		{
			return -c * 0.5f * (float)(Math.Cos(Math.PI * t / d) - 1) + b;
		}

		// -----------------------------------------------------------------------------------------------
		// End static functions
		// -----------------------------------------------------------------------------------------------
	}
}
