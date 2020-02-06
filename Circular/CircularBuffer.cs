using System;
using System.Collections.Generic;

namespace C
{
	/// <summary>
	/// Represents an implementation of <see cref="IList{T}" /> over a circular buffer
	/// </summary>
	/// <typeparam name="T">The type of item to hold</typeparam>
	#if CIRCLIB
	public
#endif
		partial class CircularBuffer<T>
	{
		const double _GrowthFactor = 1d;
		const int _DefaultCapacity = 16;
		T[] _items;
		int _start;
		int _count;
		/// <summary>
		/// Constructs a new instance filled with the items from the specified collection
		/// </summary>
		/// <param name="collection">The collection to copy into this container</param>
		public CircularBuffer(IEnumerable<T> collection) : this()
		{
			foreach (var item in collection)
				PushBack(item);
		}
		/// <summary>
		/// Constructs a new instance with the specified capacity
		/// </summary>
		/// <param name="capacity">The initial capacity of the container</param>
		public CircularBuffer(int capacity)
		{
			if (1 > capacity)
				throw new ArgumentOutOfRangeException(nameof(capacity));
			_items = new T[capacity];
			_start = 0;
			_count = 0;
		}
		/// <summary>
		/// Constructs a new instance
		/// </summary>
		public CircularBuffer() : this(_DefaultCapacity)
		{

		}
		/// <summary>
		/// Inserts an item into the front of the buffer
		/// </summary>
		/// <param name="item">The item to insert</param>
		public void PushFront(T item)
		{
			--_start;
			if (0 > _start)
				_start += _items.Length;
			_items[_start] = item;
			++_count;
			unchecked { ++_version; }
		}
		/// <summary>
		/// Inserts an item into the back of the buffer
		/// </summary>
		/// <param name="item">The item to add</param>
		public void PushBack(T item)
		{
			if (_items.Length == _count)
			{
				var arr = new T[unchecked((int)(_count * (1d + _GrowthFactor)))];
				if (0 == _start)
				{
					Array.Copy(_items, 0, arr, 0, _count);
					_items = arr;
				}
				else
				{
					Array.Copy(_items, _start, arr, 0, _items.Length - _start);
					Array.Copy(_items, 0, arr, _items.Length - _start, _start);
					_start = 0;
				}
			}
			_items[(_count + _start) % _items.Length] = item;
			++_count;
			unchecked { ++_version; };
		}
		/// <summary>
		/// Pops an item from the back of the buffer
		/// </summary>
		/// <returns>The item popped</returns>
		public T PopBack()
		{
			if (0 == _count)
				throw new InvalidOperationException("The list is empty");
			var result = _items[(_start) % _items.Length];
			_items[(_start) % _items.Length] = default(T);
			++_start;
			--_count;
			unchecked { ++_version; }
			return result;
		}
		/// <summary>
		/// Pops an item from the front of the buffer
		/// </summary>
		/// <returns>The item popped</returns>
		public T PopFront()
		{
			if (0 == _count)
				throw new InvalidOperationException("The list is empty");
			var result = _items[_start];
			_items[_start] = default(T);
			_start = (_start + 1) % _items.Length;
			--_count;
			unchecked { ++_version; }
			return result;
		}
		/// <summary>
		/// Returns an array with the items of this container
		/// </summary>
		/// <returns>An array with the items in this container</returns>
		public T[] ToArray()
		{
			var result = new T[_count];
			CopyTo(result, 0);
			return result;
		}
		/// <summary>
		/// Trims the container to the current count
		/// </summary>
		public void Trim()
		{
			if(0==_count && _DefaultCapacity<_items.Length)
			{
				_items = new T[_DefaultCapacity];
				unchecked { ++_version; }
				return;
			}
			if (_items.Length == _count)
				return;
			var arr = new T[_count];
			if (0 == _start)
			{
				Array.Copy(_items, 0, arr, 0, _count);
			} else
			{
				var len = Math.Min(_count, _items.Length - _start);
				Array.Copy(_items, _start, arr, 0, len);
				Array.Copy(_items, 0, arr, len, (_start + _count) % _items.Length);
				_start = 0;
			}
			_items = arr;
			unchecked { ++_version; }
		}
		/// <summary>
		/// Indicates the current capacity of the container
		/// </summary>
		public int Capacity {
			get {
				return _items.Length;
			}
		}
		void _RemoveAt(int index)
		{
			if (0 > index || _count <= index)
				throw new ArgumentOutOfRangeException(nameof(index));

			if (_count - 1 == index)
			{
				PopBack();
				return;
			}
			else if (0 == index)
			{
				PopFront();
				return;
			}
			else
			{
				if (0 == _start)
				{
					Array.Copy(_items, index + 1, _items, index, _count - index - 1);
					_items[_count - 1] = default(T);
				}
				else
				{
					if (index + _start >= _items.Length)
					{
						Array.Copy(_items, index + _start + 1, _items, index + _start, (_count + _start) - _items.Length);
						_items[(_count - 1 + _start) - _items.Length] = default(T);
					}
					else
					{
						if (index + _start + 1 < _items.Length)
						{
							Array.Copy(_items, _start + index + 1, _items, _start + index, _items.Length - (_start + index + 1));
							_items[(_count - 1 + _start) % _items.Length] = default(T);
						}
						if (_start + _count > _items.Length)
						{
							_items[_items.Length - 1] = _items[0];
							Array.Copy(_items, 1, _items, 0, (_start + _count) - _items.Length - 1);
							_items[(_count - 1 + _start) % _items.Length] = default(T);

						}

					}
				}
			}
			--_count;
			if (0 == _count)
			{
				_start = 0;
			}
			unchecked { ++_version; }
		}
	}
}
