// Author : Tamar Curry

namespace Libs.Tweens
{
	using UnityEngine;
	using System;
	using System.Collections.Generic;

	public delegate float EasingFunc(float t, float b, float c, float d);

	internal delegate object GetterFunc(object target);
	internal delegate void SetterFunc(object target, object start, object end, double progress);

	public enum Overwrite
	{
		// Will not overwrite tweens.
		OVERWRITE_NONE,

		// Ignores any delay and immediately overwrites all existing tweens with the same target, 
		// regardless of the properties they are tweening or their status.
		OVERWRITE_ALL,

		// Once the tween starts (after any delay), it will overwrite any of the same properties
		// on active tweens of the same target. Tweens that aren't active are ignored.
		OVERWRITE_DEFAULT,

		// Once the tween starts (after any delay), it will overwrite any active tween with the same
		// target regardless of properties. Tweens that aren't active are ignored.
		OVERWRITE_CONCURRENT,

		// Once the tween starts (after any delay) it will overwrite any tween with the same target,
		// regardless of the properties they are tweening or their status.
		OVERWRITE_ALL_ON_START,

		// Once the tween starts (after any delay) it will overwrite any tween with the same target
		// that was created before it regardless of the properties they are tweening or their status.
		OVERWRITE_PREEXISTING
	}

	internal enum TweenState
	{
		DELAY, START, UPDATE, COMPLETE
	};

	public class TweenableValue<T>
	{
		private T _value;
		public Action<T> callback;

		public T value
		{
			get { return _value; }
			set {
				_value = value;
				if (callback != null) { callback(_value); }
			}
		}
	}

	public class Tween : IDisposable
	{
		// PRIMITIVES
		private readonly int _duration;
		private int _elapsed, _delay, _numRepeats;
		private readonly bool _useFrames;
		private bool _yoyo, _paused;

		// NULLABLES
		private object _target;
		private Action _onUpdate, _onStart, _onComplete, _onRepeat;
		private TweenManager _manager;
		private EasingFunc _easing;
		private List<ITweenProcedure> _procedures;

		// ENUMS
		private TweenState _state;
		private Overwrite _overwrite;

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// GETTERS & SETTERS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------

		public bool isExpired { get{ return _state == TweenState.COMPLETE; } }

		/// <summary>
		/// The object being manipulated by this tween.
		/// </summary>
		public object target
		{
			get { return _target; }
		}

		/// <summary>
		/// Indicates whether or not this tween is finished.
		/// </summary>
		public bool isFinished
		{
			get { return _state == TweenState.COMPLETE; }
		}

		/// <summary>
		/// Indicates whether or not this tween is paused.
		/// </summary>
		public bool isPaused
		{
			get { return _paused; }
		}

		/// <summary>
		/// Update state of this tween.
		/// </summary>
		internal TweenState state
		{
			get { return _state; }
		}

		/// <summary>
		/// Overwrite behavior of this tween.
		/// </summary>
		public Overwrite overwrite
		{
			get { return _overwrite; }
		}

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// METHODS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="time"></param>
		/// <param name="useFrames"></param>
		public Tween(object target, int time, bool useFrames)
		{
			_duration = time;
			_elapsed = 0;
			_delay = 0;
			_numRepeats = 0;
			_target = target;
			_useFrames = useFrames;
			_state = TweenState.START;
			_easing = Easing.QuadEaseOut;
			_overwrite = Overwrite.OVERWRITE_DEFAULT;
			_procedures = new List<ITweenProcedure>();
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the manager for this tween.
		/// </summary>
		/// <param name="manager"></param>
		internal void SetManager(TweenManager manager)
		{
			_manager = manager;
        }

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Retrieves a list of properties being tweened on the target.
		/// </summary>
		/// <returns></returns>
		public List<String> GetTweenedProperties()
		{
			List<String> list = new List<string>();

			foreach(ITweenProcedure p in _procedures)
			{
				list.Add(p.property);
			}

			return list;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Invalidates the specified properties if they are being tweened.
		/// </summary>
		/// <param name="properties"></param>
		internal void Invalidate(List<String> properties)
		{
			foreach(ITweenProcedure p in _procedures)
			{
				if ( properties.IndexOf(p.property) > -1 )
				{
					p.Invalidate();
				}
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Ends the tween.
		/// </summary>
		/// <param name="completeTweens"></param>
		/// <param name="executeCompletionCallbacks"></param>
		public void End(bool completeTweens = false, bool executeCompletionCallbacks = false)
		{
			if (_state != TweenState.COMPLETE)
			{
				if (completeTweens)
				{
					_elapsed = _duration;
					UpdateProgress();
				}

				_state = TweenState.COMPLETE;
				if (executeCompletionCallbacks && _onComplete != null) { _onComplete(); }
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses the tween.
		/// </summary>
		public void Pause() { _paused = true; }

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Resumes the tween.
		/// </summary>
		public void Unpause() { _paused = false; }

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Retrieves the reverse of the specified easing function, if one exists.
		/// Otherwise, returns the same function.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		private EasingFunc GetReverse(EasingFunc e)
		{
			EasingFunc result = e;

			if (e == Easing.BackEaseIn) { result = Easing.BackEaseOut; }
			else if (e == Easing.BackEaseOut) { result = Easing.BackEaseIn; }
			else if (e == Easing.BounceEaseIn) { result = Easing.BounceEaseOut; }
			else if (e == Easing.BounceEaseOut) { result = Easing.BounceEaseIn; }
			else if (e == Easing.CircEaseIn) { result = Easing.CircEaseOut; }
			else if (e == Easing.CircEaseOut) { result = Easing.CircEaseIn; }
			else if (e == Easing.CubicEaseIn) { result = Easing.CubicEaseOut; }
			else if (e == Easing.CubicEaseOut) { result = Easing.CubicEaseIn; }
			else if (e == Easing.ElasticEaseIn) { result = Easing.ElasticEaseOut; }
			else if (e == Easing.ElasticEaseOut) { result = Easing.ElasticEaseIn; }
			else if (e == Easing.ExpoEaseIn) { result = Easing.ExpoEaseOut; }
			else if (e == Easing.ExpoEaseOut) { result = Easing.ExpoEaseIn; }
			else if (e == Easing.QuadEaseIn) { result = Easing.QuadEaseOut; }
			else if (e == Easing.QuadEaseOut) { result = Easing.QuadEaseIn; }
			else if (e == Easing.QuartEaseIn) { result = Easing.QuartEaseOut; }
			else if (e == Easing.QuartEaseOut) { result = Easing.QuartEaseIn; }
			else if (e == Easing.QuintEaseIn) { result = Easing.QuintEaseOut; }
			else if (e == Easing.QuintEaseOut) { result = Easing.QuintEaseIn; }
			else if (e == Easing.SineEaseIn) { result = Easing.SineEaseOut; }
			else if (e == Easing.SineEaseOut) { result = Easing.SineEaseIn; }

			return result;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Throws a conversion error.
		/// </summary>
		/// <param name="t"></param>
		private void ThrowConversionError(Type t)
		{
			throw new ArgumentException("Cannot convert target " + _target.GetType().Name + " to " + t.Name);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Throws a generic error.
		/// </summary>
		/// <param name="message"></param>
		private void ThrowError(string message)
		{
			throw new ArgumentException(message);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Tweens the specified property from its current value to a new value.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="end"></param>
		/// <returns></returns>
		public Tween To(string property, object end)
		{
			TweenProperty proc = new TweenProperty(_target, property);
			_procedures.Add(proc);
			proc.To(end);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Tweens the specified property from the specified value to its current value.
		/// </summary>
		/// <param name="property"></param>
		/// <param name="start"></param>
		/// <returns></returns>
		public Tween From(string property, object start)
		{
			TweenProperty proc = new TweenProperty(_target, property);
			_procedures.Add(proc);
			proc.From(start);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Tints the target from its current color to the specified color.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public Tween TintTo(Color c)
		{
			return TintTo(c.r, c.b, c.g, c.a);
		}

		// -------------------------------------------------------------------------------------------
		private SpriteRenderer FetchSpriteRenderer()
		{
			if (_target is GameObject)
			{
				return ((GameObject) _target).GetComponent<SpriteRenderer>();
			}
			
			if (_target is Component)
			{
				return ((Component) _target).GetComponent<SpriteRenderer>();
			}

			throw new ArgumentException("Could not find SpriteRenderer for " + _target.GetType().Name);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Tints the target from its current color to the specified color.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public Tween TintTo(float r, float g, float b, float a = 1)
		{
			SpriteRenderer renderer = FetchSpriteRenderer();
			TweenColor proc = new TweenColor(renderer);
			proc.To(r, g, b, a);
			_procedures.Add(proc);

			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Tints the target from the specified color to its current.
		/// </summary>
		/// <param name="c"></param>
		/// <returns></returns>
		public Tween TintFrom(Color c)
		{
			return TintFrom(c.r, c.b, c.g, c.a);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Tints the target from the specified color to its current.
		/// </summary>
		/// <param name="r"></param>
		/// <param name="g"></param>
		/// <param name="b"></param>
		/// <param name="a"></param>
		/// <returns></returns>
		public Tween TintFrom(float r, float g, float b, float a = 1)
		{
			SpriteRenderer renderer = FetchSpriteRenderer();
			TweenColor proc = new TweenColor(renderer);
			proc.From(r, g, b, a);
			_procedures.Add(proc);

			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Moves the target from its current position to the specified position.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Tween MoveTo(float x, float y)
		{
			Transform t = GetTransform();
			if (t != null)
			{
				TweenTransform p = new TweenTransform(t);
			    Vector3 currentPos = TweenFuncs.GetLocalPosition(t);
				p.Move(currentPos.x, currentPos.y, x, y);
				_procedures.Add(p);
			}
			else
			{
				To("x", x);
				To("y", y);
			}
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Moves the target from the specified position to its current position.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Tween MoveFrom(float x, float y)
		{
			Transform t = GetTransform();
			if (t != null)
			{
				TweenTransform p = new TweenTransform(t);
                Vector3 currentPos = TweenFuncs.GetLocalPosition(t);
                p.Move(x, y, currentPos.x, currentPos.y);
				_procedures.Add(p);
			}
			else
			{
				From("x", x);
				From("y", y);
			}
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Tweens the target along a path.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public Tween PathTo(ITweenPath path)
		{
			Transform t = GetTransform();

			if ( t != null )
			{
				_procedures.Add(new TweenPath(path, t));
			}
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales the target from its current scale value to the specified value.
		/// </summary>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween ScaleTo(float scaleX, float scaleY)
		{
			Transform t = GetTransform();

			if (t != null)
			{
				TweenTransform p = new TweenTransform(t);
				p.Scale(t.localScale.x, t.localScale.y, scaleX, scaleY);
				_procedures.Add(p);
			}
			else
			{
				To("scaleX", scaleX);
				To("scaleY", scaleY);
			}
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales the target from the specified scale value to its current scale value.
		/// </summary>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween ScaleFrom(float scaleX, float scaleY)
		{
			Transform t = GetTransform();
			if (t != null) {
				TweenTransform p = new TweenTransform(t);
				p.Scale(scaleX, scaleY, t.localScale.x, t.localScale.y);
				_procedures.Add(p);
			}
			else
			{
				From("scaleX", scaleX);
				From("scaleY", scaleY);
			}
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Rotates the current target from its current rotation to the specified rotation.
		/// </summary>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public Tween RotateTo(float rotation)
		{
			Transform t = GetTransform();
			if (t != null)
			{
				TweenTransform p = new TweenTransform(t);
				p.Rotate(t.localEulerAngles.z, rotation);
				_procedures.Add(p);
			}
			else
			{
				To("rotation", rotation);
			}
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Rotates the specified target from the specified rotation to its current rotation.
		/// </summary>
		/// <param name="rotation"></param>
		/// <returns></returns>
		public Tween RotateFrom(float rotation)
		{
			Transform t = GetTransform();
			if (t != null)
			{
				TweenTransform p = new TweenTransform(t);
				p.Rotate(rotation, t.localEulerAngles.z);
				_procedures.Add(p);
			}
			else
			{
				From("rotation", rotation);
			}
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales and rotates the target around its center.
		/// </summary>
		/// <param name="angle"></param>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween TransformAroundCenter(float angle, float scaleX, float scaleY)
		{
			Vector3 v3 = GetCenter("transformAroundCenter");
			TransformAroundPoint(v3, angle, scaleX, scaleY);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Rotates the target around its center.
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public Tween TransformAroundCenter(float angle)
		{
			Vector3 v3 = GetCenter("transformAroundCenter");
			TransformAroundPoint(v3, angle);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales the target from its center.
		/// </summary>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween TransformAroundCenter(float scaleX, float scaleY)
		{
			Vector3 v3 = GetCenter("transformAroundCenter");
			TransformAroundPoint(v3, scaleX, scaleY);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales and rotates the target around its center from the specified angle and scales to its current values.
		/// </summary>
		/// <param name="angle"></param>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween TransformAroundCenterFrom(float angle, float scaleX, float scaleY)
		{
			Vector3 v3 = GetCenter("transformAroundCenter");
			TransformAroundPointFrom(v3, angle, scaleX, scaleY);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Rotates the target around its center from the specified angle to its current angle.
		/// </summary>
		/// <param name="angle"></param>
		/// <returns></returns>
		public Tween TransformAroundCenterFrom(float angle)
		{
			Vector3 v3 = GetCenter("transformAroundCenter");
			TransformAroundPointFrom(v3, angle);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales the target around its center from the specified scales to its current scale values.
		/// </summary>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween TransformAroundCenterFrom(float scaleX, float scaleY)
		{
			Vector3 v3 = GetCenter("transformAroundCenter");
			TransformAroundPointFrom(v3, scaleX, scaleY);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales and rotates the target around the specified local position.
		/// </summary>
		/// <param name="localPoint"></param>
		/// <param name="angle"></param>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween TransformAroundPoint(Vector3 localPoint, float angle, float scaleX, float scaleY)
		{
			Transform t = GetTransform("transformAroundPoint");
			TweenTransform p = new TweenTransform(t);
			p.Rotate(t.localRotation.z, angle);
			p.Scale(t.localScale.x, t.localScale.y, scaleX, scaleY);
			p.TransformAroundPoint(localPoint);
			_procedures.Add(p);

			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Rotates the target around the specified local position.
		/// </summary>
		/// <param name="localPoint"></param>
		/// <param name="angle"></param>
		/// <returns></returns>
		public Tween TransformAroundPoint(Vector3 localPoint, float angle)
		{
			Transform t = GetTransform("transformAroundPoint");
			TweenTransform p = new TweenTransform(t);
			p.Rotate(t.localRotation.z, angle);
			p.TransformAroundPoint(localPoint);
			_procedures.Add(p);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales the target from the specified local position.
		/// </summary>
		/// <param name="localPoint"></param>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween TransformAroundPoint(Vector3 localPoint, float scaleX, float scaleY)
		{
			Transform t = GetTransform("transformAroundPoint");
			TweenTransform p = new TweenTransform(t);
			p.Scale(t.localScale.x, t.localScale.y, scaleX, scaleY);
			p.TransformAroundPoint(localPoint);
			_procedures.Add(p);

			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales and rotates the target from the specified local position at the specified values to its current scale and rotation value.
		/// </summary>
		/// <param name="localPoint"></param>
		/// <param name="angle"></param>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween TransformAroundPointFrom(Vector3 localPoint, float angle, float scaleX, float scaleY)
		{
			Transform t = GetTransform("transformAroundPointFrom");
			TweenTransform p = new TweenTransform(t);
			p.Rotate(angle, t.localRotation.z);
			p.Scale(scaleX, scaleY, t.localScale.x, t.localScale.y);
			p.TransformAroundPoint(localPoint);
			_procedures.Add(p);

			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Rotates the target around the specified local position from the specified angle to its current angle.
		/// </summary>
		/// <param name="localPoint"></param>
		/// <param name="angle"></param>
		/// <returns></returns>
		public Tween TransformAroundPointFrom(Vector3 localPoint, float angle)
		{
			Transform t = GetTransform("transformAroundPointFrom");
			TweenTransform p = new TweenTransform(t);
			p.Rotate(t.localRotation.z, angle);
			p.TransformAroundPoint(localPoint);
			_procedures.Add(p);
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Scales the target around the specified local position from the specified scale values to its current scale values.
		/// </summary>
		/// <param name="localPoint"></param>
		/// <param name="scaleX"></param>
		/// <param name="scaleY"></param>
		/// <returns></returns>
		public Tween TransformAroundPointFrom(Vector3 localPoint, float scaleX, float scaleY)
		{
			Transform t = GetTransform("transformAroundPointFrom");
			TweenTransform p = new TweenTransform(t);
			p.Scale(t.localScale.x, t.localScale.y, scaleX, scaleY);
			p.TransformAroundPoint(localPoint);
			_procedures.Add(p);

			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the transform for the target.
		/// </summary>
		/// <returns></returns>
		private Transform GetTransform()
		{
			Transform t = _target as Transform;
			Component c = _target as Component;
			GameObject g = _target as GameObject;

			if (c != null) {
				t = c.transform;
			}
			else if (g != null) {
				t = g.transform;
			}

			return t;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the transform for the object, specifying which method called.
		/// If the transform comes back null, an exception is thrown.
		/// </summary>
		/// <param name="callingMethod"></param>
		/// <returns></returns>
		private Transform GetTransform(string callingMethod)
		{
			Transform t = GetTransform();

			if (t == null)
			{
				ThrowError("Cannot use " + callingMethod + " on " + _target.GetType().Name);
			}
			return t;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Returns the center position of the target.
		/// </summary>
		/// <param name="callingMethod"></param>
		/// <returns></returns>
		private Vector3 GetCenter(string callingMethod)
		{
			Bounds b = GetBounds(callingMethod);
			Vector3 v3 = b.center;

			Transform t = GetTransform();

			if(t != null)
			{ 
				v3 = t.InverseTransformPoint(v3);
			}

			return v3;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Gets the bounds of the object. If the object has no bounds, throws an error.
		/// </summary>
		/// <param name="callingMethod"></param>
		/// <returns></returns>
		private Bounds GetBounds(string callingMethod)
		{
			Bounds b = new Bounds();
			SpriteRenderer s = FetchSpriteRenderer();
			
			if (s != null)
			{
				b = s.bounds;
			}
			else
			{
				ThrowError("Cannot use " + callingMethod + " on " + _target.GetType().Name);
			}
			return b;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Delays the tween by the specified number of milliseconds.
		/// </summary>
		/// <param name="ms"></param>
		/// <returns></returns>
		public Tween Delay(int ms)
		{
			if ( ms > 0 && _state == TweenState.START)
			{
				_delay = ms;
				_state = TweenState.DELAY;
			}
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Sets a callback to be invoked whenever the tween updates.
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public Tween OnUpdate(Action action)
		{
			_onUpdate = action;
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Sets a callback to be invoked when the tween completes.
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public Tween OnComplete(Action action)
		{
			_onComplete = action;
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Sets a callback to be invoked when the tween first starts.
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public Tween OnStart(Action action)
		{
			_onStart = action;
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the easing function to use.
		/// </summary>
		/// <param name="e"></param>
		/// <returns></returns>
		public Tween SetEasing(EasingFunc e)
		{
			_easing = e;
			if (_easing == null) { _easing = Easing.QuadEaseIn; }
			return this;
		}
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Sets the number of times the tween should be repeated.
		/// </summary>
		/// <param name="numRepeats"></param>
		/// <param name="yoyo"></param>
		/// <returns></returns>
		public Tween Repeat(int numRepeats, bool yoyo=false)
		{
			_numRepeats = numRepeats;
			_yoyo = yoyo;
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Sets a callback to be invoked when the tween repeats itself.
		/// </summary>
		/// <param name="action"></param>
		/// <returns></returns>
		public Tween OnRepeat(Action action)
		{
			_onRepeat = action;
			return this;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the TweenProcedures.
		/// </summary>
		private void UpdateProgress()
		{
			if (_elapsed > _duration) { _elapsed = _duration; }

			float progress = _easing(_elapsed, 0, 1, _duration);

			foreach (ITweenProcedure proc in _procedures)
			{
				proc.Update(progress);
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Handles the overwrite behavior for the tween.
		/// </summary>
		/// <param name="behavior"></param>
		private void HandleOverwrite(Overwrite behavior)
		{
			if (_manager == null || _overwrite != behavior) { return; }

			switch(_overwrite)
			{
				case Overwrite.OVERWRITE_DEFAULT:
					_manager.OverwriteDefault(this);
					break;
				case Overwrite.OVERWRITE_ALL:
					_manager.OverwriteAllImmediately(this);
					break;
				case Overwrite.OVERWRITE_ALL_ON_START:
					_manager.OverwriteAllOnStart(this);
					break;
				case Overwrite.OVERWRITE_CONCURRENT:
					_manager.OverwriteConcurrent(this);
					break;
				case Overwrite.OVERWRITE_PREEXISTING:
					_manager.OverwritePreexisting(this);
					break;
			}
			_overwrite = Overwrite.OVERWRITE_NONE;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the tween.
		/// </summary>
		/// <param name="ms"></param>
		internal void Update(int ms)
		{
			if(_useFrames) { ms = 1; }

			if (_paused || _state == TweenState.COMPLETE) { return; }

			HandleOverwrite(Overwrite.OVERWRITE_ALL);

			_elapsed += ms;

			if (_state == TweenState.DELAY && _delay > 0)
			{
				if (_elapsed < _delay)
				{
					return;
				}
				_state = TweenState.START;
				_delay = 0;
				_elapsed = 0;
			}

			if (_state == TweenState.START)
			{
				HandleOverwrite(_overwrite);
				if (_onStart != null) { _onStart(); }
				_state = TweenState.UPDATE;
			}
			
			UpdateProgress();
			if (_onUpdate != null) { _onUpdate(); }

			if (_elapsed >= _duration && _state != TweenState.COMPLETE)
			{
				if (_numRepeats != 0)
				{
					_elapsed = 0;
					if (_numRepeats > 0) { --_numRepeats; }
					if (_onRepeat != null) { _onRepeat(); }

					if (_yoyo)
					{
						foreach (ITweenProcedure proc in _procedures)
						{
							proc.Reverse();
						}
						_easing = GetReverse(_easing);
					}
				}
				else
				{
					_state = TweenState.COMPLETE;
					if (_onComplete != null) { _onComplete(); }
				}
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Cleanup.
		/// </summary>
		public void Dispose()
		{
			if ( _state == TweenState.COMPLETE )
			{
				return;
			}

			_state = TweenState.COMPLETE;
			_onStart = null;
			_onUpdate = null;
			_onComplete = null;
			_onRepeat = null;
			_target = null;

			foreach(ITweenProcedure p in _procedures)
			{
				p.Dispose();
			}
			_procedures = null;
		}
	}
}
