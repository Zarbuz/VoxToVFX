using System;
using Com.TheFallenGames.OSA.Core;

namespace Com.TheFallenGames.OSA.DataHelpers
{
	/// <summary>
	/// <para>Contains shortcuts for common operations on a list. Most notably, it adds/removes items for you and notifies the adapter after.</para>
	/// <para>If you need full control, consider using your own list and notifying the adapter after each modification. Inspect this class for how it's done</para>
	/// <para>This uses a <see cref="LazyList{T}"/> as storage. Models are created only when first accessed</para>
	/// </summary>
	public class LazyDataHelper<T>
	{
		public int Count { get { return _DataList.Count; } }

		/// <summary>See notes on <see cref="SimpleDataHelper{T}.List"/></summary>
		public LazyList<T> List { get { return _DataList; } }

		/// <summary>Will be set back to false after next event</summary>
		public bool SkipNotifyingAdapterForNextEvent { get; set; }

		protected IOSA _Adapter;
		protected LazyList<T> _DataList;
		bool _KeepVelocityOnCountChange;


		public LazyDataHelper(IOSA iAdapter, Func<int, T> newModelCreator, bool keepVelocityOnCountChange = true)
		{
			_Adapter = iAdapter;
			_DataList = new LazyList<T>(newModelCreator, 0);
			_KeepVelocityOnCountChange = keepVelocityOnCountChange;
		}

		public T GetOrCreate(int index)
		{
			return _DataList[index];
		}

		public LazyList<T>.EnumerableLazyList GetEnumerableForExistingItems() { return _DataList.AsEnumerableForExistingItems; }

		public void InsertItems(int index, int count, bool freezeEndEdge = false)
		{
			_DataList.Insert(index, count);
			if (SkipNotifyingAdapterForNextEvent)
				SkipNotifyingAdapterForNextEvent = false;
			else
				InsertItemsInternal(index, count, freezeEndEdge);
		}
		public void InsertItemsAtStart(int count, bool freezeEndEdge = false) { InsertItems(0, count, freezeEndEdge); }
		public void InsertItemsAtEnd(int count, bool freezeEndEdge = false) { InsertItems(_DataList.Count, count, freezeEndEdge); }

		/// <summary>NOTE: Use <see cref="InsertItems(int, int, bool)"/> for bulk inserts, as it's way faster</summary>
		public void InsertOneManuallyCreated(int index, T model, bool freezeEndEdge = false)
		{
			_DataList.Insert(index, 1);
			_DataList.SetOrUpdateManuallyCreatedValue(index, model);
			if (SkipNotifyingAdapterForNextEvent)
				SkipNotifyingAdapterForNextEvent = false;
			else
				InsertItemsInternal(index, 1, freezeEndEdge);
		}

		public void RemoveItems(int index, int count, bool freezeEndEdge = false)
		{
			_DataList.Remove(index, count);
			if (SkipNotifyingAdapterForNextEvent)
				SkipNotifyingAdapterForNextEvent = false;
			else
				RemoveItemsInternal(index, count, freezeEndEdge);
		}
		public void RemoveItemsFromStart(int count, bool freezeEndEdge = false) { RemoveItems(0, count, freezeEndEdge); }
		public void RemoveItemsFromEnd(int count, bool freezeEndEdge = false) { RemoveItems(_DataList.Count - count, count, freezeEndEdge); }

		public void RemoveOne(T model, bool freezeEndEdge = false)
		{
			int index = _DataList.Remove(model);
			if (index == -1)
				throw new OSAException("Not found: " + model);
			if (SkipNotifyingAdapterForNextEvent)
				SkipNotifyingAdapterForNextEvent = false;
			else
				RemoveItemsInternal(index, 1, freezeEndEdge);
		}

		public void ResetItems(int count, bool freezeEndEdge = false)
		{
			_DataList.InitWithNewCount(count);
			if (SkipNotifyingAdapterForNextEvent)
				SkipNotifyingAdapterForNextEvent = false;
			else
				_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}

		public void NotifyListChangedExternally(bool freezeEndEdge = false)
		{
			if (SkipNotifyingAdapterForNextEvent)
				throw new OSAException("Don't set SkipNotifyingAdapterForNextEvent=true before calling NotifyListChangedExternally");

			_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}

		void InsertItemsInternal(int index, int count, bool freezeEndEdge)
		{
			if (_Adapter.InsertAtIndexSupported)
				_Adapter.InsertItems(index, count, freezeEndEdge, _KeepVelocityOnCountChange);
			else
				_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}

		void RemoveItemsInternal(int index, int count, bool freezeEndEdge)
		{
			if (_Adapter.RemoveFromIndexSupported)
				_Adapter.RemoveItems(index, count, freezeEndEdge, _KeepVelocityOnCountChange);
			else
				_Adapter.ResetItems(_DataList.Count, freezeEndEdge, _KeepVelocityOnCountChange);
		}
	}
}