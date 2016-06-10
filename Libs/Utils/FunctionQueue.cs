using System;
using UnityEngine;
using System.Collections.Generic;
using Libs.Interfaces;

namespace Libs.Utils
{
	/// <summary>
	/// The FunctionQueue class can execute a sequence of actions in the order they were received.
	/// Each frame cycle, the queue will execute an action and check if the action is finished.
	/// If the action is finished, it moves on to the next action.
	/// If the action is not finished, it'll exit the function call and wait until the next frame cycle.
	/// 
	/// A FunctionQueue is slightly more rigid than a Unity Coroutine in terms of how it handles executing commands,
	/// but you can stop/restart/clear/reuse the FunctionQueue instance as much as you'd like.
	/// 
	/// You can also adjust the speed of the FunctionQueue on the fly.
	/// </summary>
	public class FunctionQueue : IDestroyable
	{
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// CONSTANTS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		//public const string QUEUE_FINISHED = "FunctionQueue.queue_finished";


		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// PRIMITES
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Enables or disables auto updates.
		/// If you disable it, please use the Update function in order to update the Queue.
		/// </summary>
		public bool autoUpdatesEnabled;

		private float _speed;

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// NULLABLES
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// List of queued actions.
		/// </summary>
		private List<IQueuedAction> _actions;

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		public float speed
		{
			get { return _speed; }
			set {
				_speed = value;
				if (_speed <= 0) { _speed = 1; }
			}
		}

		public bool isExpired { get; private set; }
		public bool isStopped { get; private set; }

		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		// METHODS
		// -------------------------------------------------------------------------------------------
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="autoUpdate">If set to true, the Queue updates itself. If set to false, you must manuall update it using the Update function.</param>
		public FunctionQueue(Boolean autoUpdate = true)
		{
			autoUpdatesEnabled = autoUpdate;
			_speed = 1;
			Core.ListenForUpdates( AutoUpdate, true );
			_actions = new List<IQueuedAction>();
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Manual update function. If autoUpdatesEnabled is set to true, this function will not execute.
		/// </summary>
		public void Update()
		{
			if (autoUpdatesEnabled) { return; }
			UpdateQueue();
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Automatic update function. If autoUpdatesEnabled is set to false, this function will not execute.
		/// </summary>
		private void AutoUpdate()
		{
			if (!autoUpdatesEnabled) { return; }
			UpdateQueue();
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Clears the queue.
		/// </summary>
		public void Clear()
		{
			foreach (IQueuedAction queuedAction in _actions)
			{
				queuedAction.Dispose();
			}
			_actions.Clear();
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Stops the queue from executing.
		/// </summary>
		public void Stop()
		{
			isStopped = true;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Resumes execution.
		/// </summary>
		public void Resume()
		{
			isStopped = false;
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Updates the actions in the queue. If any action is not finished, it will exit the function.
		/// </summary>
		private void UpdateQueue()
		{
			if(isExpired || isStopped) { return; }

			// flag for if we can dispatch an event stating that the queue is empty.
			//bool canDispatch = false;
			IQueuedAction queuedAction = null;
			uint dt = Convert.ToUInt32(Time.deltaTime*1000*_speed);

			// continue the loop while there's still actions in the queue.
			while (_actions != null && _actions.Count > 0)
			{
				if(isExpired || isStopped) { return; }
				// mark the canDispatch flag.
				//canDispatch = true;
				queuedAction = _actions[0];
				queuedAction.Execute(dt);

				// if the action is finished, move on to the next action.
				if (queuedAction.isFinished)
				{
					queuedAction.Dispose();
					if ( _actions != null && _actions.Contains(queuedAction) )
					{ 
						_actions.RemoveAt(0);
					}
				}
				else // exit the loop.
				{
					break;
				}
			}

			// if we've executed at least one action and the queue is now empty, dispatch an event.
			//if (canDispatch && _actions != null && _actions.Count == 0)
			//{
				//Dispatch(QUEUE_FINISHED);
			//}
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Call a method. This action will only execute once.
		/// </summary>
		/// <param name="action"></param>
		public void Call(Action action)
		{
			_actions.Add(new CallMethod(action));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Call a method every frame cycle until it returns true.
		/// </summary>
		/// <param name="func"></param>
		public void Query(Func<bool> func)
		{
			_actions.Add(new QueryMethod(func));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Pause the queue for the specified amount of time. The time is measured in milliseconds.
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="useFrames"></param>
		public void Pause(uint duration, bool useFrames = false)
		{
			_actions.Add(new PauseExecution(duration, useFrames));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Call the method repeatedly for the specified amount of time. The time is measured in milliseconds.
		/// </summary>
		/// <param name="action"></param>
		/// <param name="duration"></param>
		/// <param name="useFrames"></param>
		public void Repeat(Action action, uint duration, bool useFrames = false)
		{
			_actions.Add(new RepeatMethod(action, duration, useFrames));
		}

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Cleanup.
		/// </summary>
		public void Dispose()
		{
			if(isExpired) return;
			isExpired = true;
			//DisplayController.onUpdateDelegate -= AutoUpdate;
			_actions.Clear();
		}
	}

	/// <summary>
	/// Interface for queued actions.
	/// </summary>
	internal interface IQueuedAction : IDestroyable
	{
		void Execute(uint deltaTime);
		bool isFinished { get; }
	}

	/// <summary>
	/// Queued action that calls a function once.
	/// </summary>
	internal class CallMethod : IQueuedAction
	{
		private Action _action;

		// -------------------------------------------------------------------------------------------
		public bool isFinished { get; private set; }
		public bool isExpired { get; private set; }

		// -------------------------------------------------------------------------------------------
		public CallMethod(Action action)
		{
			_action = action;
			isFinished = false;
		}

		// -------------------------------------------------------------------------------------------
		public void Execute(uint deltaTime)
		{
			if (isFinished) { return; }
			_action();
			isFinished = true;
		}

		// -------------------------------------------------------------------------------------------
		public void Dispose()
		{
			_action = null;
			isExpired = true;
		}
	}

	/// <summary>
	/// Queued action that calls a function that returns a boolean.
	/// The queue is finished once the function returns "true".
	/// </summary>
	internal class QueryMethod : IQueuedAction
	{
		private Func<bool> _func;

		// -------------------------------------------------------------------------------------------
		public bool isFinished { get; private set; }
		public bool isExpired { get; private set; }

		// -------------------------------------------------------------------------------------------
		public QueryMethod(Func<bool> func)
		{
			_func = func;
			isFinished = false;
		}

		// -------------------------------------------------------------------------------------------
		public void Execute(uint deltaTime)
		{
			if (isFinished) { return; }
			isFinished = _func();
		}

		// -------------------------------------------------------------------------------------------
		public void Dispose()
		{
			isExpired = true;
			_func = null;
		}

	}

	/// <summary>
	/// Queued action that waits a specified amount of time before letting the queue continue.
	/// </summary>
	internal class PauseExecution : IQueuedAction
	{
		private uint _duration;
		private uint _elapsed;
		private bool _useFrames;
		private bool _started;

		// -------------------------------------------------------------------------------------------
		public bool isExpired { get; private set; }
		public bool isFinished { get; protected set; }

		// -------------------------------------------------------------------------------------------
		public PauseExecution(uint duration, bool useFrames)
		{
			isFinished = false;
			_started = false;
			_duration = duration;
			_useFrames = useFrames;
			_elapsed = 0;
		}

		// -------------------------------------------------------------------------------------------
		public virtual void Execute(uint deltaTime)
		{
			if (isFinished) { return; }

			if (!_started) {
				_started = true;
			}
			else if (!_useFrames) {
				_elapsed += deltaTime;
			}
			else {
				++_elapsed;
			}
			isFinished = _elapsed >= _duration;
		}

		// -------------------------------------------------------------------------------------------
		public virtual void Dispose()
		{
			isExpired = true;
		}
	}

	/// <summary>
	/// Queued action that repeats a function call for a specified amount of time.
	/// </summary>
	internal class RepeatMethod : PauseExecution
	{
		private Action _action;

		// -------------------------------------------------------------------------------------------
		public RepeatMethod(Action action, uint duration, bool useFrames) : base(duration, useFrames)
		{
			_action = action;
		}

		// -------------------------------------------------------------------------------------------
		override public void Execute(uint deltaTime)
		{
			if (isFinished) { return; }
			_action();
			base.Execute(deltaTime);
		}

		// -------------------------------------------------------------------------------------------
		override public void Dispose()
		{
			_action = null;
		}
	}
}

