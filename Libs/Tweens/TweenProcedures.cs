// Author : Tamar Curry

using UnityEngine;
using System;
using Libs.Interfaces;

namespace Libs.Tweens
{
	public interface ITweenPath
	{
		Vector3 Evaluate(float t);
	}

	internal interface ITweenProcedure : IDestroyable
	{
		void Update(float progress);
		void Reverse();
		void Invalidate();
		string property { get; }
	}

	internal class TweenTransform : ITweenProcedure
	{
		private bool _move, _rotate, _scale, _transformAroundPoint, _transformInParent, _invalidated;
		private float _startX, _startY, _startScaleX, _startScaleY, _startRotation;
		private float _endX, _endY, _endScaleX, _endScaleY, _endRotation;
		private Transform _target;
		private Vector3 _localPoint, _parentCoords, _worldCoords;

		// -------------------------------------------------------------------------------------------
		public string property { get { return "transform"; } }
		public bool isExpired { get; private set; }

		// -------------------------------------------------------------------------------------------
		public TweenTransform(Transform target)
		{
			_target = target;
		}

		// -------------------------------------------------------------------------------------------
		public void TransformAroundPoint(Vector3 localPoint)
		{
			_transformAroundPoint = true;
			_localPoint = localPoint;
			_worldCoords = _target.TransformPoint(localPoint);

			if (_target.parent != null)
			{
				_transformInParent = true;
				_parentCoords = _target.parent.InverseTransformPoint(_worldCoords);
			}
			Update(0f);
		}

		// -------------------------------------------------------------------------------------------
		public void Move(float startX, float startY, float endX, float endY)
		{
			_move = true;
			_startX = startX;
			_startY = startY;
			_endX = endX;
			_endY = endY;
			Update(0f);
		}

		// -------------------------------------------------------------------------------------------
		public void Rotate(float startRotation, float endRotation)
		{
			_rotate = true;
			_startRotation = startRotation;
			_endRotation = endRotation;
			Update(0f);
		}

		// -------------------------------------------------------------------------------------------
		public void Scale(float startScaleX, float startScaleY, float endScaleX, float endScaleY)
		{
			_scale = true;
			_startScaleX = startScaleX;
			_startScaleY = startScaleY;
			_endScaleX = endScaleX;
			_endScaleY = endScaleY;
			Update(0f);
		}

		// -------------------------------------------------------------------------------------------
		public void Update(float progress)
		{
			if (_invalidated || _target == null) { return; }
			Vector3 v;

			if (_move)
			{
				v = TweenFuncs.GetLocalPosition(_target);
				v.x = _startX + ((_endX - _startX) * progress);
				v.y = _startY + ((_endY - _startY) * progress);
                TweenFuncs.SetLocalPosition(_target, v);
			}

			if (_scale)
			{
				v = _target.localScale;
				v.x = _startScaleX + ((_endScaleX - _startScaleX) * progress);
				v.y = _startScaleY + ((_endScaleY - _startScaleY) * progress);
				_target.localScale = v;
			}

			if (_rotate)
			{
				v = _target.localEulerAngles;
				v.z = _startRotation + ((_endRotation - _startRotation) * progress);
				_target.localEulerAngles = v;
			}

			if (_transformAroundPoint)
			{
				if (_transformInParent && _target.parent != null)
				{
					Vector3 newWorldCoords = _target.TransformPoint(_localPoint);
					Vector3 newParentCoords = _target.parent.InverseTransformPoint(newWorldCoords);
					Vector3 position = TweenFuncs.GetLocalPosition(_target);
					position.x += _parentCoords.x - newParentCoords.x;
					position.y += _parentCoords.y - newParentCoords.y;
                    TweenFuncs.SetLocalPosition(_target, position);
				}
				else
				{
					Vector3 newWorldCoords = _target.TransformPoint(_localPoint);
					Vector3 position = _target.position;
					position.x += _worldCoords.x - newWorldCoords.x;
					position.x += _worldCoords.x - newWorldCoords.x;
					_target.position = position;
				}
			}
		}

		// -------------------------------------------------------------------------------------------
		public void Reverse()
		{
			float t;
			t = _startX;
			_startX = _endX;
			_endX = t;

			t = _startY;
			_startY = _endY;
			_endY = t;

			t = _startScaleX;
			_startScaleX = _endScaleX;
			_endScaleX = t;

			t = _startScaleY;
			_startScaleY = _endScaleY;
			_endScaleY = t;

			t = _startRotation;
			_startRotation = _endRotation;
			_endRotation = t;
		}

		// -------------------------------------------------------------------------------------------
		public void Invalidate()
		{
			_invalidated = true;
		}

		// -------------------------------------------------------------------------------------------
		public void Dispose()
		{
			isExpired = true;
			_invalidated = true;
			_target = null;
		}
	}

	internal class TweenProperty : ITweenProcedure
	{
		protected object _target;
		protected object _start;
		protected object _end;
		protected string _property;
		protected bool _invalidated;
		protected GetterFunc _getter;
		protected SetterFunc _setter;

		public bool isExpired { get; private set; }
		public string property { get { return _property; } }

		// -------------------------------------------------------------------------------------------
		public TweenProperty(object target, string property)
		{
			_target = target;
			_property = property;
			_invalidated = false;
			PropertyMapper map = PropertyMapper.GetMap(property);
			Type t = target.GetType();
			_getter = map.GetGetter(t);
			_setter = map.GetSetter(t);
		}

		// -------------------------------------------------------------------------------------------
		public void To(object end)
		{
			_end = end;
			_start = _getter(_target);
			Update(0.0f);
		}

		// -------------------------------------------------------------------------------------------
		public void From(object start)
		{
			_end = _getter(_target);
			_start = start;
			Update(0.0f);
		}

		// -------------------------------------------------------------------------------------------
		public void Invalidate()
		{
			_invalidated = true;
		}

		// -------------------------------------------------------------------------------------------
		public void Reverse()
		{
			object temp = _start;
			_start = _end;
			_end = temp;
		}

		// -------------------------------------------------------------------------------------------
		public void Update(float progress)
		{
			if (!_invalidated)
			{
				_setter(_target, _start, _end, progress);
			}
		}

		// -------------------------------------------------------------------------------------------
		public void Dispose()
		{
			isExpired = true;
			_invalidated = true;
			_target = null;
		}
	}

	internal class TweenColor : ITweenProcedure
	{
		private Color _color;
		private SpriteRenderer _target;

		private float _startR;
		private float _startG;
		private float _startB;
		private float _startA;

		private float _endR;
		private float _endG;
		private float _endB;
		private float _endA;

		protected bool _invalidated;

		// -------------------------------------------------------------------------------------------
		public bool isExpired { get; private set; }
		public string property { get { return "color"; } }

		// -------------------------------------------------------------------------------------------
		public TweenColor(SpriteRenderer target)
		{
			_target = target;
			_color = _target.color;
			_invalidated = false;
		}

		// -------------------------------------------------------------------------------------------
		public TweenColor(Color c)
		{
			_color = c;
			_invalidated = false;
		}

		// -------------------------------------------------------------------------------------------
		public void To(float r, float g, float b, float a)
		{
			_startR = _color.r;
			_startG = _color.g;
			_startB = _color.b;
			_startA = _color.a;

			_endR = r;
			_endG = g;
			_endB = b;
			_endA = a;
			Update(0.0f);
		}

		// -------------------------------------------------------------------------------------------
		public void From(float r, float g, float b, float a)
		{
			_startR = r;
			_startG = g;
			_startB = b;
			_startA = a;

			_endR = _color.r;
			_endG = _color.g;
			_endB = _color.b;
			_endA = _color.a;
			Update(0.0f);
		}

		// -------------------------------------------------------------------------------------------
		public void Invalidate()
		{
			_invalidated = true;
		}

		// -------------------------------------------------------------------------------------------
		public void Reverse()
		{
			float t;
			t = _startR;
			_startR = _endR;
			_endR = t;

			t = _startG;
			_startG = _endG;
			_endG = t;

			t = _startB;
			_startB = _endB;
			_endB = t;

			t = _startA;
			_startA = _endA;
			_endA = t;
		}

		// -------------------------------------------------------------------------------------------
		public void Update(float progress)
		{
			if (!_invalidated)
			{
				_color.r = _startR + ((_endR - _startR) * progress);
				_color.g = _startG + ((_endG - _startG) * progress);
				_color.b = _startB + ((_endB - _startB) * progress);
				_color.a = _startA + ((_endA - _startA) * progress);
				if (_target != null)
				{
					_target.color = _color;
				}
			}
		}

		// -------------------------------------------------------------------------------------------
		public void Dispose()
		{
			isExpired = true;
			_invalidated = true;
			_target = null;
		}
	}

	internal class TweenPath : ITweenProcedure
	{
		private bool _reverse;
		private bool _invalidated;

		private ITweenPath _path;
		private Transform _transform;

		// -------------------------------------------------------------------------------------------
		public bool isExpired { get; private set; }
		public string property { get { return "path"; } }

		// -------------------------------------------------------------------------------------------
		public TweenPath(ITweenPath path, Transform transform)
		{
			_path = path;
			_transform = transform;
		}

		// -------------------------------------------------------------------------------------------
		public void Reverse()
		{
			_reverse = true;
		}

		// -------------------------------------------------------------------------------------------
		public void Invalidate()
		{
			_invalidated = true;
		}

		// -------------------------------------------------------------------------------------------
		public void Update(float progress)
		{
			if (_invalidated) { return; }
			if (_reverse) { progress = 1 - progress; }
            TweenFuncs.SetLocalPosition(_transform, _path.Evaluate(progress));
		}

		// -------------------------------------------------------------------------------------------
		public void Dispose()
		{
			isExpired = true;
			_path = null;
			_transform = null;
		}
	}
}
