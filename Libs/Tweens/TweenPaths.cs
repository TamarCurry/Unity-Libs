using UnityEngine;

namespace Libs.Tweens
{
	public class LinearPath : ITweenPath
	{
		private readonly Vector3 _start;
		private readonly Vector3 _end;

		public LinearPath(Vector3 start, Vector3 end)
		{
			_start = start;
			_end = end;
		}

		public LinearPath Reverse()
		{
			return new LinearPath(_end, _start);
		}

		public Vector3 Evaluate(float t)
		{
			return _start + ((_end - _start)*t);
		}
	}

	public class BezierPath3 : ITweenPath
	{
		/// <summary>
		///     Control points of the Bezier curve.
		/// </summary>
		private readonly Vector3[] _b;

		public BezierPath3(Vector3 b0, Vector3 b1, Vector3 b2)
		{
			_b = new Vector3[3];
			_b[0] = b0;
			_b[1] = b1;
			_b[2] = b2;
		}

		public Vector3 this[int index]
		{
			get { return _b[index]; }
			set { _b[index] = value; }
		}

		public Vector3 Evaluate(float t)
		{
			float u = 1.0f - t;
			float uu = u * u;
			float ut = u * t * 2f;
			float tt = t * t;
			return (_b[0] * uu) + (_b[1] * ut) + (_b[2] * tt);
		}
	}

	public class BezierPath4 : ITweenPath
	{
		/// <summary>
		///     Control points of the Bezier curve.
		/// </summary>
		private readonly Vector3[] _b;

		public BezierPath4(Vector3 b0, Vector3 b1, Vector3 b2, Vector3 b3)
		{
			_b = new Vector3[4];
			_b[0] = b0;
			_b[1] = b1;
			_b[2] = b2;
			_b[3] = b3;
		}

		public Vector3 this[int index]
		{
			get { return _b[index]; }
			set { _b[index] = value; }
		}

		public Vector3 Evaluate(float t)
		{
			float u = 1.0f - t;
			float uuu = u * u * u;
			float uut = u * u * t * 3f;
			float utt = u * t * t * 3f;
			float ttt = t * t * t;
			return (_b[0] * uuu) + (_b[1] * uut) + (_b[2] * utt) + (_b[3] * ttt);
		}
	}

}

