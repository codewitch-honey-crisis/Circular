using System;
using System.Collections.Generic;

namespace C
{
	partial class CircularBuffer<T> : IList<T>
	{
		/// <summary>
		/// Gets or sets a value by index
		/// </summary>
		/// <param name="index">The index</param>
		/// <returns>The value retrieved</returns>
		public T this[int index] 
		{ 
			get {
				if (0 > index || _count <= index)
					throw new IndexOutOfRangeException();
				return _items[(_start + index) % _items.Length];
			}
			set {
				if (0 > index || _count <= index)
					throw new IndexOutOfRangeException();
				_items[(_start + index) % _items.Length] = value;
				++_version;
			}
		}
		/// <summary>
		/// Indicates the index of the specified item within the container
		/// </summary>
		/// <param name="item">The item to search for</param>
		/// <returns>The index of the item or a negative number if no item was found</returns>
		public int IndexOf(T item)
		{
			for(var i = 0;i<_count;++i)
				if (Equals(item, _items[(_start + i) % _items.Length]))
					return i;
			return -1;
		}

		void IList<T>.Insert(int index, T item)
		{
			if (0 > index || _count < index)
				throw new ArgumentOutOfRangeException(nameof(index));
			if(index==_count)
			{
				PushBack(item);
				return;
			}
			if(_count == _items.Length)
			{
				var arr = new T[unchecked((int)(_count * (1d + _GrowthFactor)))];
				if(0==index)
				{
					if (0 == _start)
					{
						Array.Copy(_items, 0, arr, 1, _count);
						_items = arr;
					}
					else
					{
						Array.Copy(_items, _start, arr, 1, _items.Length - _start);
						Array.Copy(_items, 0, arr, _items.Length - _start+1, (_start+_count)-_items.Length);
						_start = 0;
					}
					arr[0] = item;
					++_count;
					unchecked { ++_version; }
					_items = arr;
					return; 
				}
				// Insert(index,item) where index is not 0 or Count:
				if(0==_start)
				{
					Array.Copy(_items, 0, arr, 0, index);
					arr[index] = item;
					Array.Copy(_items, index, arr, index + 1, _items.Length - index);
				} else
				{
					Array.Copy(_items, _start, arr, 1, _items.Length - _start);
					Array.Copy(_items, 0, arr, _items.Length - _start + 1, (_start+_count)-_items.Length);
					_start = 0;
				}
				++_count;
				unchecked { ++_version; }
				_items = arr;
				return;
			} 
			if(0==index)
			{
				PushFront(item);
				return;
			}
			if(0==_start)
			{
				Array.Copy(_items, index, _items, index + 1, _count - index);
				_items[index] = item;
			} else
			{
				if(_count+_start<_items.Length)
				{
					Array.Copy(_items, index + _start, _items, index + _start+1, _count - index);
					_items[_start + index] = item;
				} else
				{
					if (index + _start < _items.Length)
					{
						Array.Copy(_items, 0, _items, 1, _count + _start - _items.Length);
						_items[0] = _items[_items.Length - 1];
						Array.Copy(_items, index + _start, _items, index + _start + 1, _items.Length - (index + _start + 1));
						_items[index + _start] = item;
						//Array.Copy(_items,index+_start-_items.Length, index + _start +1 - _items.Length,)
					} else
					{
						Array.Copy(_items, (index + _start)-_items.Length, _items, (index + _start + 1)-_items.Length, ((index + _count) % _items.Length));
						_items[(index + _start)-_items.Length] = item;

					}



				}
			}
			++_count;
			unchecked { ++_version; }
		}

		void IList<T>.RemoveAt(int index) => _RemoveAt(index);
	}
}

		
	
