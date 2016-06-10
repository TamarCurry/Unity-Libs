// Author : Tamar Curry

namespace Libs.Tweens
{ 
	using System;
	using System.Collections.Generic;
	using System.Reflection;

	public class PropertyMapper
	{
		private delegate object ConverterFunc(object start, object end, double progress);
		private static Dictionary<Type, ConverterFunc> _converters;
		private static Dictionary<string, PropertyMapper> _map;
		private static bool _initialized = false;

		private string _property;
		private Dictionary<Type, GetterFunc> _getters;
		private Dictionary<Type, SetterFunc> _setters;

		// -------------------------------------------------------------------------------------------
		public static void Init()
		{
			if (_initialized) { return; }
			_initialized = true;
			_converters = new Dictionary<Type, ConverterFunc>();
			_converters.Add(typeof(double), CalcDouble);
			_converters.Add(typeof(float), CalcFloat);
			_converters.Add(typeof(short), CalcShort);
			_converters.Add(typeof(int), CalcInt);
			_converters.Add(typeof(long), CalcLong);
			_converters.Add(typeof(ushort), CalcUShort);
			_converters.Add(typeof(uint), CalcUInt);
			_converters.Add(typeof(ulong), calcULong);

			_map = new Dictionary<string, PropertyMapper>();
		}

		// -------------------------------------------------------------------------------------------
		public static PropertyMapper GetMap(string property)
		{
			if ( !_initialized ) { Init(); }
			if (!_map.ContainsKey(property))
			{
				PropertyMapper m = new PropertyMapper(property);
				_map.Add(property, m);
			}
			return _map[property];
		}
		
		// -------------------------------------------------------------------------------------------
		public PropertyMapper(string property)
		{
			if (!_initialized)
			{
				Init();
			}
			_property = property;
			_getters = new Dictionary<Type, GetterFunc>();
			_setters = new Dictionary<Type, SetterFunc>();
		}

		// -------------------------------------------------------------------------------------------
		public void AddType(Type t)
		{
			GetGetter(t);
			GetSetter(t);
		}

		// -------------------------------------------------------------------------------------------
		public MemberInfo GetMemberInfo(Type t)
		{
			FieldInfo f = t.GetField(_property);
			PropertyInfo p = t.GetProperty(_property);

			MemberInfo m = null;
			if (f != null)
			{
				m = f;
			}
			else if (p != null)
			{
				m = p;
			}

			return m;
		}

		// -------------------------------------------------------------------------------------------
		private object EmptyGetter(object target)
		{
			return 0;
		}

		// -------------------------------------------------------------------------------------------
		private void EmptySetter(object target, object start, object end, double progress)
		{
			return;
		}

		// -------------------------------------------------------------------------------------------
		internal GetterFunc GetGetter(Type t)
		{
			if (!_getters.ContainsKey(t))
			{
				GetterFunc g = EmptyGetter;
				MemberInfo m = GetMemberInfo(t);
				if (m == null)
				{
					throw new System.InvalidOperationException("Getter " + _property + " does not exist on " + t.Name);
				}
				else if (m is FieldInfo)
				{
					FieldInfo f = (FieldInfo)m;
					g = (target) => f.GetValue(target);
				}
				else if (m is PropertyInfo)
				{
					PropertyInfo p = (PropertyInfo)m;
					g = (target) => p.GetValue(target, null);
				}
				_getters.Add(t, g);
			}
			return _getters[t];
		}

		// -------------------------------------------------------------------------------------------
		internal SetterFunc GetSetter(Type t)
		{
			if (!_setters.ContainsKey(t))
			{
				SetterFunc s = EmptySetter;
				MemberInfo m = GetMemberInfo(t);

				if (m == null)
				{
					throw new System.InvalidOperationException("Setter " + _property + " does not exist on " + t.Name);
				}
				else if (m is FieldInfo)
				{
					FieldInfo f = (FieldInfo)m;
					if (_converters.ContainsKey(f.FieldType))
					{
						s = (target, start, end, progress) => f.SetValue(target, _converters[f.FieldType](start, end, progress));
					}
					else
					{
						throw new System.InvalidOperationException("Cannot tween " + _property + " (type " + f.FieldType.Name + ") on " + t.Name);
					}
				}
				else if (m is PropertyInfo)
				{
					PropertyInfo p = (PropertyInfo)m;
					if (_converters.ContainsKey(p.PropertyType))
					{
						s = (target, start, end, progress) => p.SetValue(target, _converters[p.PropertyType](start, end, progress), null);
					}
					else
					{
						throw new System.InvalidOperationException("Cannot tween " + _property + " (type " + p.PropertyType.Name + ") on " + t.Name);
					}
				}
				_setters.Add(t, s);
			}
			return _setters[t];
		}

		// -------------------------------------------------------------------------------------------
		private static object CalcDouble(object start, object end, double progress)
		{
			double s = Convert.ToDouble(start);
			double e = Convert.ToDouble(end);
			double d = (e - s) * progress;
			return s + d;
		}

		// -------------------------------------------------------------------------------------------
		private static object CalcFloat(object start, object end, double progress)
		{
			float s = Convert.ToSingle(start);
			float e = Convert.ToSingle(end);
			float d = Convert.ToSingle((e - s) * progress);
			return s + d;
        }

		// -------------------------------------------------------------------------------------------
		private static object CalcShort(object start, object end, double progress)
		{
			short s = Convert.ToInt16(start);
			short e = Convert.ToInt16(end);
			short d = Convert.ToInt16((e - s) * progress);
			return (short)(s + d);
		}

		// -------------------------------------------------------------------------------------------
		private static object CalcInt(object start, object end, double progress)
		{
			int s = Convert.ToInt32(start);
			int e = Convert.ToInt32(end);
			int d = Convert.ToInt32((e - s) * progress);
			return s + d;
		}

		// -------------------------------------------------------------------------------------------
		private static object CalcLong(object start, object end, double progress)
		{
			long s = Convert.ToInt64(start);
			long e = Convert.ToInt64(end);
			long d = Convert.ToInt64((e - s) * progress);
			return s + d;
		}

		// -------------------------------------------------------------------------------------------
		private static object CalcUShort(object start, object end, double progress)
		{
			ushort s = Convert.ToUInt16(start);
			ushort e = Convert.ToUInt16(end);
			ushort d = Convert.ToUInt16((e - s) * progress);
			return (ushort)(s + d);
		}

		// -------------------------------------------------------------------------------------------
		private static object CalcUInt(object start, object end, double progress)
		{
			uint s = Convert.ToUInt32(start);
			uint e = Convert.ToUInt32(end);
			uint d = Convert.ToUInt32((e - s) * progress);
			return s + d;
		}

		// -------------------------------------------------------------------------------------------
		private static object calcULong(object start, object end, double progress)
		{
			ulong s = Convert.ToUInt64(start);
			ulong e = Convert.ToUInt64(end);
			ulong d = Convert.ToUInt64((e - s) * progress);
			return s + d;
		}
	}
}
