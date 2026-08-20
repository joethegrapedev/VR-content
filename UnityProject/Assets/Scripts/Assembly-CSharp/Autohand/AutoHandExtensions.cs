using System;
using System.Reflection;
using UnityEngine;

namespace Autohand
{
	public static class AutoHandExtensions
	{
		private static Transform _transformRuler;

		private static Transform _transformRulerChild;

		private static Transform _transformParent;

		public static Transform transformRuler
		{
			get
			{
				if (_transformRuler == null)
				{
					_transformRuler = new GameObject
					{
						name = "Ruler"
					}.transform;
				}
				if (_transformRuler.parent != transformParent)
				{
					_transformRuler.parent = transformParent;
				}
				if (_transformRuler.localScale != Vector3.one)
				{
					_transformRuler.localScale = Vector3.one;
				}
				if (IsPositionNan(_transformRuler.position))
				{
					_transformRuler.position = Vector3.zero;
				}
				if (IsRotationNan(_transformRuler.rotation))
				{
					_transformRuler.rotation = Quaternion.identity;
				}
				return _transformRuler;
			}
		}

		public static Transform transformRulerChild
		{
			get
			{
				if (_transformRulerChild == null)
				{
					_transformRulerChild = new GameObject
					{
						name = "RulerChild"
					}.transform;
					_transformRulerChild.parent = _transformRuler;
				}
				if (_transformRulerChild.parent != _transformRuler)
				{
					_transformRulerChild.parent = _transformRuler;
				}
				if (_transformRulerChild.localScale != Vector3.one)
				{
					_transformRulerChild.localScale = Vector3.one;
				}
				if (IsPositionNan(_transformRulerChild.position))
				{
					_transformRulerChild.position = Vector3.zero;
				}
				if (IsRotationNan(_transformRulerChild.rotation))
				{
					_transformRulerChild.rotation = Quaternion.identity;
				}
				return _transformRulerChild;
			}
		}

		public static Transform transformParent
		{
			get
			{
				if (Application.isEditor)
				{
					return null;
				}
				if (_transformParent == null)
				{
					_transformParent = new GameObject
					{
						name = "Auto Hand Generated"
					}.transform;
				}
				return _transformParent;
			}
		}

		private static bool IsPositionNan(Vector3 pos)
		{
			if (!float.IsNaN(pos.x) && !float.IsNaN(pos.y))
			{
				return float.IsNaN(pos.z);
			}
			return true;
		}

		private static bool IsRotationNan(Quaternion rot)
		{
			if (!float.IsNaN(rot.x) && !float.IsNaN(rot.y) && !float.IsNaN(rot.z))
			{
				return float.IsNaN(rot.w);
			}
			return true;
		}

		public static void RotateAround(this Transform target, Transform center, Quaternion deltaRotation)
		{
			transformRuler.SetPositionAndRotation(center.position, center.rotation);
			transformRulerChild.SetPositionAndRotation(target.position, target.rotation);
			transformRuler.rotation *= deltaRotation;
			target.SetPositionAndRotation(transformRulerChild.position, transformRulerChild.rotation);
		}

		public static float Round(this float value, int digits)
		{
			float num = Mathf.Pow(10f, digits);
			return Mathf.Round(value * num) / num;
		}

		public static bool HasGrabbable(this Hand hand, GameObject obj, out Grabbable grabbable)
		{
			return obj.HasGrabbable(out grabbable);
		}

		public static bool HasGrabbable(this GameObject obj, out Grabbable grabbable)
		{
			if (obj == null)
			{
				grabbable = null;
				return false;
			}
			if (obj.CanGetComponent<Grabbable>(out grabbable))
			{
				return true;
			}
			if (obj.CanGetComponent<GrabbableChild>(out var component))
			{
				grabbable = component.grabParent;
				return true;
			}
			grabbable = null;
			return false;
		}

		public static T GetCopyOf<T>(this Component comp, T other) where T : Component
		{
			Type type = comp.GetType();
			if (type != other.GetType())
			{
				return null;
			}
			BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
			PropertyInfo[] properties = type.GetProperties(bindingAttr);
			foreach (PropertyInfo propertyInfo in properties)
			{
				if (propertyInfo.CanWrite)
				{
					try
					{
						propertyInfo.SetValue(comp, propertyInfo.GetValue(other, null), null);
					}
					catch
					{
					}
				}
			}
			FieldInfo[] fields = type.GetFields(bindingAttr);
			foreach (FieldInfo fieldInfo in fields)
			{
				fieldInfo.SetValue(comp, fieldInfo.GetValue(other));
			}
			return comp as T;
		}

		public static bool CanGetComponent<T>(this Component componentClass, out T component)
		{
			return componentClass.TryGetComponent<T>(out component);
		}

		public static bool CanGetComponent<T>(this GameObject componentClass, out T component)
		{
			return componentClass.TryGetComponent<T>(out component);
		}

		public static LayerMask GetPhysicsLayerMask(int currentLayer)
		{
			int num = 0;
			for (int i = 0; i < 32; i++)
			{
				if (!Physics.GetIgnoreLayerCollision(currentLayer, i))
				{
					num |= 1 << i;
				}
			}
			return num;
		}
	}
}
