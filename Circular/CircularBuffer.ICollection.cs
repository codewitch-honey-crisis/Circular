using System;
using System.Collections.Generic;

namespace C
{
	partial class CircularBuffer<T> : ICollection<T>
	{
		/// <summary>
		/// Indicates the count of items in the container
		/// </summary>
		public int Count { get => _count; }
		bool ICollection<T>.IsReadOnly => false;

		void ICollection<T>.Add(T item)
		{
			PushBack(item);
		}
		/// <summary>
		/// Clears the container
		/// </summary>
		public void Clear()
		{
			if (0 == _start) {
				Array.Clear(_items, 0, _count);
			} else
			{
				Array.Clear(_items, _start, _items.Length - _start);
				Array.Clear(_items, 0, (_count + _start)-_items.Length);
			}
			++_version;
			_count = 0;
			_start = 0;
		}
		/// <summary>
		/// Indicates whether the container contains a particular item
		/// </summary>
		/// <param name="item">The item to search for</param>
		/// <returns>True if the item was found, otherwise false</returns>
		public bool Contains(T item)
		{
			for(var i =0;i<_count;++i)
			{
				if (Equals(item, _items[(_start + i) % _items.Length]))
					return true;
			}
			return false;
		}
		/// <summary>
		/// Copies the container to the specified array
		/// </summary>
		/// <param name="array">The destination array</param>
		/// <param name="arrayIndex">The index in the destination array at which to begin copying</param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (null == array)
				throw new ArgumentNullException(nameof(array));
			if (0 > arrayIndex || array.Length<=arrayIndex)
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			if (_count + arrayIndex > array.Length)
				throw new ArgumentOutOfRangeException();
			if (0 == _start)
				Array.Copy(_items, 0, array, arrayIndex, _count);
			else
			{
				Array.Copy(_items, _start, array, arrayIndex, _items.Length - _start);
				Array.Copy(_items, 0, array, arrayIndex+_items.Length - _start, _start);
			}
		}

		bool ICollection<T>.Remove(T item)
		{
			var i = IndexOf(item);
			if (0 > i) return false;
			_RemoveAt(i);
			return true;
		}
		
	}
}
