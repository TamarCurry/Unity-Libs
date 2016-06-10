using Libs.Audio;
using Libs.Tweens;
using Libs.Utils;
using UnityEngine;

namespace Libs
{
	public class Core {

		#region HELPER CLASSES
		/// <summary>
		/// Private helper class that invokes updates for Core.
		/// </summary>
		private class Dispatcher : MonoBehaviour
		{
			void Awake()
			{
				DontDestroyOnLoad(gameObject);
			}

			void Update()
			{
				_onUpdate();
			}
		}
		#endregion

		/// <summary>
		/// Delegate for custom update functions for non-MonoBehaviour classes.
		/// </summary>
		public delegate void UpdateDelegate();

		#region MEMBERS
		/// <summary>
		/// Our main update delegate.
		/// </summary>
		private static UpdateDelegate _onUpdate;
		#endregion

		#region GETTERS
		/// <summary>
		/// Static tween instance. Use this if you don't want to instantiate your own tween manager.
		/// </summary>
		public static TweenManager tweens { get; private set; }

		/// <summary>
		/// Static function queue instance. Use this if you don't want to instantiate your own function queue.
		/// </summary>
		public static FunctionQueue queue { get; private set; }

		/// <summary>
		/// Flag that indicates whether or not Core has been initialized.
		/// </summary>
		public static bool initialized { get; private set; }
		#endregion

		#region METHODS
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Static constructor.
		/// </summary>
		static Core() { init(); }

		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Initialization function.
		/// </summary>
		public static void init()
		{
			if(initialized) return;
			initialized = true;
			AudioManager.Init(Camera.main);
			GameObject go = new GameObject();
			go.name = "Core";
			go.AddComponent<Dispatcher>();

			tweens = new TweenManager();
			queue = new FunctionQueue();
			_onUpdate += () => { };
		}
		
		// -------------------------------------------------------------------------------------------
		/// <summary>
		/// Subscribes or unsubscribes a function to updates.
		/// </summary>
		/// <param name="onUpdate"></param>
		/// <param name="subscribe"></param>
		public static void ListenForUpdates(UpdateDelegate onUpdate, bool subscribe)
		{
			if(onUpdate == null) return;
			
			if (subscribe) _onUpdate += onUpdate;
			else if (_onUpdate != null) _onUpdate -= onUpdate;
		}
		#endregion

	}
}

