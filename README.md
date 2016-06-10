# Unity-Libs
A set of useful libraries I put together for Unity. Much of this code is ported over from ActionScript3.

Quick overview
* Core.cs - initializes the other systems in the library
* Audio/ - houses helper classes to play AudioInstances
  * AudioManager.cs - use this to play sound effects, music, and voice queues.
  * AudioInstance.cs - handler for when audio is played.
  * AudioClipExtensions.cs - helper class with useful functions for quickly playing audio.
* Interfaces/ - houses generic interfaces
  * IDestroyable - interface for any class that can be disposed.
* Tweens/ - houses the tween library
  * TweenManager - responsible for creating, updating, and destroying tweens.
  * Tween - handles updating a list of properties on an object for a specified amount of time
  * TweenFuncs - helper functions for tweens
  * TweenPaths - a set of classes for tweening along a path.
  * TweenProcedures - a set of classes that handle tweening one specific property on an object
  * PropertyMapper - caches properties on classes as they are used to help alleviate overhead caused by using Reflection
  * Easing - a set of easing functions
* Utils/ - utility classes for making your life a smidge easier
  * FunctionQueue - handles a list of sequential operations. These can be simple function calls, queries, pauses, and repeats. Similar in functionality to Unity's Coroutines. They are slightly more rigid in terms of executing the sequence, but FunctionQueues can be stopped, resumed, cleared and then reused.
  
Overall, you'll mostly be dealing with AudioManager, TweenManager + Tween, and FunctionQueue while using this library.
