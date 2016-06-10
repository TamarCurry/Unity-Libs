// Author : Tamar Curry

using Libs.Interfaces;

namespace Libs.Tweens
{ 
	using System;
    using System.Collections.Generic;

	/// <summary>
	/// The tween manager class handles a collection of tweens.
	/// Ideally, you should never make a tween directly. Instead, you should use the TweenManager::AddTween function.
	/// </summary>
    public class TweenManager: IDestroyable {

		// PRIMITIVES
		private int _elapsed;
		private bool _paused;
		private readonly bool _autoUpdate;
		private float _speed;

		// NULLABLES
		private List<Tween> _tweens;

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// GETTERS & SETTERS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// The multiplicative speed at which tweens are executed. Default is 1.
		/// </summary>
		public float speed
		{
			get { return _speed; }
			set
			{
				_speed = value;
				if ( _speed <= 0 ) { _speed = 1; }
			}
		}

		/// <summary>
		/// Number of tweens still running.
		/// </summary>
		public int numTweens
		{
			get {
				int num = 0;
				foreach (Tween t in _tweens) {
					if (!t.isFinished) { ++num; }
				}
				return num;
			}
		}

        /// <summary>
        /// Whether the tween manager has been destroyed.
        /// </summary>
        public bool isExpired {
            get { return _tweens == null; }
        }

        // -------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------
        // METHODS
        // -------------------------------------------------------------------------------------------
        // -------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="autoUpdate">If set to true, the TweenManager updates itself. If set to false, you have to call "update" manually.</param>
        public TweenManager(bool autoUpdate=true)
        {
			_speed = 1;
			_elapsed = 0;
			_paused = false;
			_tweens = new List<Tween>();
			_autoUpdate = autoUpdate;
            if (_autoUpdate)
			{
				Core.ListenForUpdates( UpdateTweens, true );
			}
        }

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Ignores any delay and immediately overwrites all existing tweens with the same target, 
		/// regardless of the properties they are tweening or their status.
		/// </summary>
		/// <param name="t"></param>
		internal void OverwriteAllImmediately(Tween t)
		{
			OverwriteAllOnStart(t);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Once the tween starts (after any delay), it will overwrite any of the same properties
		/// on active tweens of the same target. Tweens that aren't active are ignored.
		/// </summary>
		/// <param name="t"></param>
		internal void OverwriteDefault(Tween t)
		{
			List<string> properties = t.GetTweenedProperties();
			
			foreach(Tween t2 in _tweens)
			{
				if (t2 == t || t2.state != TweenState.UPDATE || t2.target != t.target ) { continue; }
				t2.Invalidate(properties);
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Once the tween starts (after any delay), it will overwrite any active tween with the same
		/// target regardless of properties. Tweens that aren't active are ignored.
		/// </summary>
		/// <param name="t"></param>
		internal void OverwriteConcurrent(Tween t)
		{
			foreach (Tween t2 in _tweens)
			{
				if (t2 == t || t2.state != TweenState.UPDATE || t2.target != t.target) { continue; }
				t2.End();
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Once the tween starts (after any delay) it will overwrite any tween with the same target,
		/// regardless of the properties they are tweening or their status.
		/// </summary>
		/// <param name="t"></param>
		internal void OverwriteAllOnStart(Tween t)
		{
			foreach (Tween t2 in _tweens)
			{
				if (t2 == t || t2.target != t.target) { continue; }
				t2.End();
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Once the tween starts (after any delay) it will overwrite any tween with the same target
		/// that was created before it regardless of the properties they are tweening or their status.
		/// </summary>
		/// <param name="t"></param>
		internal void OverwritePreexisting(Tween t)
		{
			
			int len = _tweens.Count;
			for ( int i = 0; i < len; ++i )
			{
				Tween t2 = _tweens[i];
				if (t2 == t ) { break; } // only handle pre-existing tweens
				if (t2.target == t.target) { t2.End(); }
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pauses all tweens.
		/// </summary>
		public void Pause()
		{
			_paused = true;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Resumes all tweens.
		/// </summary>
		public void Unpause()
		{
			_paused = false;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Adds a tween for the specified target.
		/// </summary>
		/// <param name="target">The object to be tweened.</param>
		/// <param name="time">The time the tween lasts in milliseconds or number of update cycles.</param>
		/// <param name="useFrames">Indicates whether the time should be based on milliseconds (false) or number of update cycles (true).</param>
		/// <returns></returns>
		public Tween Add(object target, int time, bool useFrames=false)
		{
			Tween t = new Tween(target, time, useFrames);
			_tweens.Add(t);
			t.SetManager(this);
            return t;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Allows you to tween a primitive from one value to another and execute the provided callback when the value changes.
		/// </summary>
		/// <typeparam name="T">Primitive to be tweened. Should be an int, uint, float or double</typeparam>
		/// <param name="obj">Tweenable value.</param>
		/// <param name="start">Starting value.</param>
		/// <param name="end">Final value.</param>
		/// <param name="callback">Callback to be invoked whenever the value is updatedc.</param>
		/// <param name="time">The time the tween lasts in milliseconds or number of update cycles.</param>
		/// <param name="useFrames">Indicates whether the time should be based on milliseconds (false) or number of update cycles (true).</param>
		/// <returns></returns>
		public Tween TweenValue<T>(TweenableValue<T> obj, T start, T end, Action<T> callback, int time, bool useFrames=false)
		{
			obj.callback = null;
			obj.value = start;
			obj.callback = callback;
			return Add(obj, time, useFrames).To("value", end);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Allows you to tween a primitive from one value to another and execute the provided callback when the value changes.
		/// </summary>
		/// <typeparam name="T">Primitive to be tweened. Should be an int, uint, float or double</typeparam>
		/// <param name="start">Starting value.</param>
		/// <param name="end">Final value.</param>
		/// <param name="callback">Callback to be invoked whenever the value is updatedc.</param>
		/// <param name="time">The time the tween lasts in milliseconds or number of update cycles.</param>
		/// <param name="useFrames">Indicates whether the time should be based on milliseconds (false) or number of update cycles (true).</param>
		/// <returns></returns>
		public Tween TweenValue<T>(T start, T end, Action<T> callback, int time, bool useFrames = false)
		{
			return TweenValue(new TweenableValue<T>(), start, end, callback, time, useFrames);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Executes a callback after the specified time.
		/// </summary>
		/// <param name="callback">Callback to be invoked.</param>
		/// <param name="time">The time the tween lasts in milliseconds or number of update cycles.</param>
		/// <param name="useFrames">Indicates whether the time should be based on milliseconds (false) or number of update cycles (true).</param>
		public void DelayedCall(Action callback, int time, bool useFrames=false)
		{
			Add(this, time, useFrames).OnComplete(callback);
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Removes a tween from the queue.
		/// </summary>
		/// <param name="t">Tween to be removed.</param>
		/// <param name="completeTweens">Indicates whether or not the tween should set the properties to their final values.</param>
		/// <param name="executeCompletionCallbacks">Indicates whether or not the callback for the tween should be invoked.</param>
		public void Remove(Tween t, bool completeTweens=false, bool executeCompletionCallbacks=false)
		{
			int i = _tweens.IndexOf(t);
			if (i > -1)
			{
				t.End(completeTweens, executeCompletionCallbacks);
			}
		}


		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Kills all tweens.
		/// </summary>
		/// <param name="completeTweens">Indicates whether or not the tween should set the properties to their final values.</param>
		/// <param name="executeCompletionCallbacks">Indicates whether or not the callback for the tween should be invoked.</param>
		public void KillAllTweens(bool completeTweens = false, bool executeCompletionCallbacks = false)
		{
			foreach (Tween t in _tweens)
			{
				t.End(completeTweens, executeCompletionCallbacks);
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Kills tweens of the specified target.
		/// </summary>
		/// <param name="target">Object that is being tweened.</param>
		/// <param name="completeTweens">Indicates whether or not the tween should set the properties to their final values.</param>
		/// <param name="executeCompletionCallbacks">Indicates whether or not the callback for the tween should be invoked.</param>
		public void KillTweensOf(object target, bool completeTweens=false, bool executeCompletionCallbacks = false)
		{
			foreach(Tween t in _tweens)
			{
				if (t.target == target)
				{
					t.End(completeTweens, executeCompletionCallbacks);
				}
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Manually updates all the registered tweens.
		/// Please note: if autoUpdate has been specified, this function will not execute as intended.
		/// </summary>
		public void Update()
		{
			if (_autoUpdate) { return; }
			UpdateTweens();
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Updates all the registered tweens.
		/// </summary>
		internal void UpdateTweens()
		{
			if (_paused || isExpired) { return; }

			int ms = Convert.ToInt32( UnityEngine.Time.deltaTime * 1000 * _speed );
			_elapsed += ms;
		    int len = _tweens.Count;
            
			for(int i = 0; i < len; ++i) {
			    Tween t = _tweens[i];
				if (t.isFinished) { continue; }
				t.Update(ms);

                // Check if the manager was destroyed as a result of a tween callback.
			    if (isExpired) { return; }
			}

			if ( _elapsed >= 3000 )
			{
				_elapsed = 0;
				for (int i = _tweens.Count - 1; i >= 0; --i)
				{
					if (_tweens[i].isFinished)
					{
						_tweens.RemoveAt(i);
					}
				}
			}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Cleanup.
		/// </summary>
		public void Dispose()
		{
			// cannot destroy global TweenManager.
			if(this == Core.tweens || _tweens == null) { return; }

			foreach(Tween t in _tweens)
			{
				t.Dispose();
			}
			_tweens = null;

			if (_autoUpdate) { 
				Core.ListenForUpdates( UpdateTweens, false );
			}
		}
    }
}
