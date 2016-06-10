using UnityEngine;
using System.Collections;

namespace Libs.Interfaces
{
	public interface IDestroyable
	{
		void Dispose();
		bool isExpired { get; }
	}
}
