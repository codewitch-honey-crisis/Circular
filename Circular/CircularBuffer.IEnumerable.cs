using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace C
{
	partial class CircularBuffer<T> : IEnumerable<T>
	{
		int _version = 0;
		/// <summary>
		/// Retrieves an enumerator that can be used to enumerate this instance
		/// </summary>
		/// <returns>The enumerator</returns>
		public IEnumerator<T> GetEnumerator()
		{
			return new _Enumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private struct _Enumerator : IEnumerator<T>
		{
			CircularBuffer<T> _outer;
			T _current;
			int _version;
			int _index;
			public _Enumerator(CircularBuffer<T> outer)
			{
				_outer = outer;
				_version = outer._version;
				_current = default(T);
				_index = -2;
			}
			void _CheckVersion()
			{
				if (_version != _outer._version)
					throw new InvalidOperationException("The collection has changed");
			}
			public T Current { 
				get {
					if(0>_index)
					{
						switch(_index)
						{
							case -2:
								throw new InvalidOperationException("The cursor is before the beginning of the enumeration");
							case -1:
								throw new InvalidOperationException("The cursor is after the end of the enumeration");
							case -3:
								throw new ObjectDisposedException(GetType().Name);
						}
					}
					_CheckVersion();
					return _current;
					
				}
			}
			
			
			object IEnumerator.Current { get { return Current; } }

			public void Dispose()
			{
				_index = -3;
			}

			public bool MoveNext()
			{
				if(0>_index) 
				{ 
					switch(_index)
					{
						case -3: // disposed
							throw new ObjectDisposedException(GetType().Name);
						case -1: // after end
							return false;
						case -2: // before start
							++_index;
							break;
					}
				}
				_CheckVersion();
				++_index;
				if (_outer._count == _index)
					return false;
				_current = _outer._items[(_index+_outer._start) % _outer._items.Length];
				return true;
			}

			public void Reset()
			{
				if (-3 == _index)
					throw new ObjectDisposedException(GetType().Name);
				_CheckVersion();
				_index = -2;
			}
		}
	}
}
