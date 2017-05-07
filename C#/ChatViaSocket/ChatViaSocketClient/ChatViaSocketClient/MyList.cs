using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatViaSocketClient
{
    public class MyList<T> : IList<T>
    {
        public MyList()
        {
            
        }

        public int IndexOf(T item)
        {
            lock (_locker)
            {
                var r = _list.IndexOf(item);
                return r;
            }
        }

        public void Insert(int index, T item)
        {
            lock (_locker)
            {
                _list.Insert(index, item);
            }
        }

        public void RemoveAt(int index)
        {
            lock (_locker)
            {
                _list.RemoveAt(index);
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_locker)
                {
                    var r = _list[index];
                    return r;
                }
            }
            set
            {
                lock (_locker)
                {
                    _list[index] = value;
                }
            }
        }

        public void Add(T item)
        {
            lock (_locker)
            {
                _list.Add(item);
            }
        }

        public void Clear()
        {
            lock (_locker)
            {
                _list.Clear();
            }
        }

        public bool Contains(T item)
        {
            lock (_locker)
            {
                var r = _list.Contains(item);
                return r;
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            lock (_locker)
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                lock (_locker)
                {
                    var r = _list.Count;
                    return r;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                lock (_locker)
                {
                    var r = (_list as IList<T>).IsReadOnly;
                    return r;
                }
            }
        }

        public bool Remove(T item)
        {
            lock (_locker)
            {
                var r = _list.Remove(item);
                return r;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_locker)
            {
                var r = _list.GetEnumerator();
                return r;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            lock (_locker)
            {
                var r = (_list as IEnumerable).GetEnumerator();
                return r;
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            lock (_locker)
            {
                _list.AddRange(collection);
            }
        }

        public void RemoveRange(int index, int count)
        {
            lock (_locker)
            {
                _list.RemoveRange(index, count);
            }
        }

        public List<T> GetRange(int index, int count)
        {
            lock (_locker)
            {
               return _list.GetRange(index, count);
            }
        }

        private List<T> _list = new List<T>();
        private object _locker = new object();
    }
}
