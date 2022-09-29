using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Com.TheFallenGames.OSA;
using System;
using UnityEngine.Events;
using frame8.Logic.Misc.Other.Extensions;
using frame8.Logic.Misc.Visual.UI;
using Com.TheFallenGames.OSA.Core;
using System.Collections.Generic;

namespace Com.TheFallenGames.OSA.DataHelpers
{
	/// <summary>
	/// <para>Contains shortcuts for common operations on a list. Most notably, it adds/removes items for you and notifies the adapter after.</para>
	/// <para>If you need full control, consider using your own list and notifying the adapter after each modification. Inspect this class to see how it's done</para>
	/// </summary>
	public class SimpleDataHelper<T> : IEnumerable<T>
	{
		public int Count { get { return _DataList.Count; } }
		public T this[int index]
		{
			get
			{
				return _DataList[index];
			}
		}

		/// <summary>
		/// <para>NOTE: If you modify the list directly, the changes won't be reflected in the adapter unless you call <see cref="NotifyListChangedExternally(bool)"/></para>
		/// <para>
		/// This is not encouraged for partial inserts/removes (i.e. when some of the items should be kept), because it updates all items' views. 
		/// Use only if necessary
		/// </para>
		/// </summary>
		public List<T> List { get { return _DataList; } }

		protected IOSA _Adapter;
		protected List<T> _DataList;
		bool _KeepVelocityOnCountChange;


		public SimpleDataHelper(IOSA iAdapter, bool keepVelocityOnCountChange = true)
		{
			_Adapter = iAdapter;
			_DataList = new List<T>();
			_KeepVelocityOnCountChange = keepVelocityOnCountChange;
		}


		#region IEnumerator<T> implementation
		IEnumerator<T> IEnumerable<T>.GetEnumerator() { return _DataList.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return _DataList.GetEnumerator(); }
		#endregion

		public void InsertItems(int index, IList<T> models, bool freezeEndEdge = false)
		{
			_DataList.InsertRange(index, models);

			if (_Adapter.InsertAtIndexSupported)
				_Adapter.InsertItems(index, models.Count, freezeEndEdge, _KeepVelocityOnCountChange);
			else
				_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}
		public void InsertItemsAtStart(IList<T> models, bool freezeEndEdge = false) { InsertItems(0, models, freezeEndEdge); }
		public void InsertItemsAtEnd(IList<T> models, bool freezeEndEdge = false) { InsertItems(_DataList.Count, models, freezeEndEdge); }

		/// <summary>NOTE: Use <see cref="InsertItems(int, IList{T}, bool)"/> for bulk inserts, as it's way faster</summary>
		public void InsertOne(int index, T model, bool freezeEndEdge = false)
		{
			_DataList.Insert(index, model);
			if (_Adapter.InsertAtIndexSupported)
				_Adapter.InsertItems(index, 1, freezeEndEdge, _KeepVelocityOnCountChange);
			else
				_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}
		public void InsertOneAtStart(T model, bool freezeEndEdge = false) { InsertOne(0, model, freezeEndEdge); }
		public void InsertOneAtEnd(T model, bool freezeEndEdge = false) { InsertOne(_DataList.Count, model, freezeEndEdge); }

		public void RemoveItems(int index, int count, bool freezeEndEdge = false)
		{
			_DataList.RemoveRange(index, count);
			if (_Adapter.RemoveFromIndexSupported)
				_Adapter.RemoveItems(index, count, freezeEndEdge, _KeepVelocityOnCountChange);
			else
				_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}
		public void RemoveItemsFromStart(int count, bool freezeEndEdge = false) { RemoveItems(0, count, freezeEndEdge); }
		public void RemoveItemsFromEnd(int count, bool freezeEndEdge = false) { RemoveItems(_DataList.Count - count, count, freezeEndEdge); }

		/// <summary>NOTE: Use <see cref="RemoveItems(int, int, bool)"/> for bulk removes, as it's way faster</summary>
		public void RemoveOne(int index, bool freezeEndEdge = false)
		{
			_DataList.RemoveAt(index);
			if (_Adapter.RemoveFromIndexSupported)
				_Adapter.RemoveItems(index, 1, freezeEndEdge, _KeepVelocityOnCountChange);
			else
				_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}
		public void RemoveOneFromStart(bool freezeEndEdge = false) { RemoveOne(0, freezeEndEdge); }
		public void RemoveOneFromEnd(bool freezeEndEdge = false) { RemoveOne(_DataList.Count - 1, freezeEndEdge); }

		/// <summary>
		/// NOTE: In case of resets, the preferred way is to clear the <see cref="List"/> yourself, add the models through it, and then call <see cref="NotifyListChangedExternally(bool)"/>.
		/// This saves memory by avoiding creating an intermediary array/list
		/// </summary>
		public void ResetItems(IList<T> models, bool freezeEndEdge = false)
		{
			_DataList.Clear();
			_DataList.AddRange(models);
			_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}

		public void NotifyListChangedExternally(bool freezeEndEdge = false)
		{
			_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}
	}
}