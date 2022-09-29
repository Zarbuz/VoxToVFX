using frame8.Logic.Misc.Other;
using frame8.Logic.Misc.Other.Extensions;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Com.TheFallenGames.OSA.Core
{
	public class OSADebugger : MonoBehaviour, IDragHandler
	{
		public bool onlyAcceptedGameObjects;
		public string[] acceptedGameObjectsNames;

		IOSA _AdapterImpl;
		public Text debugText1, debugText2, debugText3, debugText4;
		public bool allowReinitializationWithOtherAdapter;
		Toggle _EndStationary;
		InputField _AmountInputField;

#if !UNITY_WSA && !UNITY_WSA_10_0 // UNITY_WSA uses .net core, which does not contain reflection code
		const BindingFlags BINDING_FLAGS = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

		void Update()
		{
			if (_AdapterImpl == null)
				return;

			IList vhs = GetFieldValue("_VisibleItems") as IList;
			int indexInViewOfFirst = -1, indexInViewOfLast = -1;
			if (vhs != null && vhs.Count > 0)
			{
				indexInViewOfFirst = (vhs[0] as BaseItemViewsHolder).itemIndexInView;
				indexInViewOfLast = (vhs[vhs.Count-1] as BaseItemViewsHolder).itemIndexInView;
			}

			IList recyclable = GetFieldValue("_RecyclableItems") as IList;
			string recyclableSiblingIndices = "";
			if (recyclable != null && recyclable.Count > 0)
			{
				for (int i = 0; i < recyclable.Count; i++)
				{
					recyclableSiblingIndices += (recyclable[i] as BaseItemViewsHolder).root.GetSiblingIndex() + ", ";
				}
				//indexInViewOfFirst = (vhs[0] as BaseItemViewsHolder).itemIndexInView;
				//indexInViewOfLast = (vhs[vhs.Count - 1] as BaseItemViewsHolder).itemIndexInView;
			}


			debugText1.text =
				"ctVrtIns: " + GetInternalStatePropertyValue("ctVirtualInsetFromVPS_Cached") + "\n" +
				"indexInViewOfFirst: " + indexInViewOfFirst + "\n" +
				"indexInViewOfLast: " + indexInViewOfLast + "\n" +
				"visCount: " + GetPropertyValue("VisibleItemsCount") + "\n" +
				"recyclableSiblingIndices: " + recyclableSiblingIndices + "\n" +
				//"ctRealSz: " + GetInternalStateFieldValue("contentPanelSize") + "\n" +
				"ctVrtSz: " + GetInternalStateFieldValue("ctVirtualSize") + "\n" +
				//"rec: " + GetPropertyValue("RecyclableItemsCount") + "\n" +
				"rec: " + _AdapterImpl.RecyclableItemsCount +
				"bufRec: " + _AdapterImpl.BufferedRecyclableItemsCount;
		}

		internal void InitWithAdapter(IOSA adapterImpl)
		{
			if (_AdapterImpl != null && !allowReinitializationWithOtherAdapter 
				|| onlyAcceptedGameObjects 
					&& acceptedGameObjectsNames != null 
					&& Array.IndexOf(acceptedGameObjectsNames, adapterImpl.gameObject.name) == -1
				)
				return;

			_AdapterImpl = adapterImpl;

			Button b;
			transform.GetComponentAtPath("ComputePanel/ComputeNowButton", out b);
			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(() => Call("ComputeVisibilityForCurrentPosition", true, false));

			transform.GetComponentAtPath("ComputePanel/ComputeNowButton_PlusDelta", out b);
			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(() => Call("ComputeVisibilityForCurrentPositionRawParams", true, false, .1f));

			transform.GetComponentAtPath("ComputePanel/ComputeNowButton_MinusDelta", out b);
			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(() => Call("ComputeVisibilityForCurrentPositionRawParams", true, false, -.1f));

			transform.GetComponentAtPath("ComputePanel/CorrectNowButton", out b);
			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(() => Call("CorrectPositionsOfVisibleItems", true, true));

			transform.GetComponentAtPath("DataManipPanel/EndStationaryToggle", out _EndStationary);
			transform.GetComponentAtPath("DataManipPanel/AmountInputField", out _AmountInputField);

			transform.GetComponentAtPath("DataManipPanel/head", out b);
			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(() => AddOrRemove(true));
			transform.GetComponentAtPath("DataManipPanel/tail", out b);
			b.onClick.AddListener(() => AddOrRemove(false));

			b.onClick.RemoveAllListeners();
			b.onClick.AddListener(() => Call("RemoveItems", adapterImpl.GetItemsCount() - 2, int.Parse(_AmountInputField.text), _EndStationary.isOn, false));
		}

		void AddOrRemove(bool atStart)
		{
			int endIdx = _AdapterImpl.GetItemsCount() - 1;
			int amount = int.Parse(_AmountInputField.text);

			if (amount < 0)
				Call("RemoveItems", atStart ? 0 : endIdx + amount, -amount, _EndStationary.isOn, false);
			else
				Call("InsertItems", atStart ? 0 : endIdx, amount, _EndStationary.isOn, false);
		}

		object GetFieldValue(string field)
		{
			var fi = GetBaseType().GetField(field, BINDING_FLAGS);
			return fi.GetValue(_AdapterImpl);
		}

		object GetPropertyValue(string prop)
		{
			var pi = GetBaseType().GetProperty(prop, BINDING_FLAGS);
			return pi.GetValue(_AdapterImpl, null);
		}

		object GetInternalStateFieldValue(string field)
		{
			var internalState = GetFieldValue("_InternalState");
			var internalStateBaseType = GetInternalStateBaseType(internalState);

			var fi = internalStateBaseType.GetField(field, BINDING_FLAGS);
			return fi.GetValue(internalState);
		}

		object GetInternalStatePropertyValue(string prop)
		{
			var internalState = GetFieldValue("_InternalState");
			var internalStateBaseType = GetInternalStateBaseType(internalState);

			var fi = internalStateBaseType.GetProperty(prop, BINDING_FLAGS);
			return fi.GetValue(internalState, null);
		}

		Type GetBaseType()
		{
			Type t = _AdapterImpl.GetType();
			while (!t.Name.Contains("OSA") || !t.IsGenericType)
			{
				t = t.BaseType;
				//if (t == typeof(object))
				//	return;
			}

			return t;
		}

		Type GetInternalStateBaseType(object internalState)
		{
			return internalState.GetType();

			//Type t = internalState.GetType();
			//while (!t.Name.ToLowerInvariant().Equals("internalstate"))
			//{
			//	t = t.BaseType;
			//	//if (t == typeof(object))
			//	//	return;
			//}

			//return t;
		}


		void Call(string methodName, params object[] parameters)
		{
			if (_AdapterImpl == null)
				return;

			Type t = GetBaseType();

			//foreach (var m in t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
			//	if (m.Name.ToLower().Contains("compute"))
			//	Debug.Log(m);

			var mi = t.GetMethod(
				methodName,
				BINDING_FLAGS,
				null,
				DotNETCoreCompat.ConvertAllToArray(parameters, p => p.GetType()),
				null
				);
			mi.Invoke(_AdapterImpl, parameters);
		}

		public void OnDrag(PointerEventData eventData)
		{
			transform.position += (Vector3)eventData.delta;
		}
#endif


	}
}
