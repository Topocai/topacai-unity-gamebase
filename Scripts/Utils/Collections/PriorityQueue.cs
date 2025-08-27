using System.Collections.Generic;
using System;

namespace Topacai.Utils.Collections
{
    public class PriorityQueue<T>
    {
        private List<Tuple<T, int>> _queue = new List<Tuple<T, int>>();

        public int Count => _queue.Count;

        private void SortByPriority()
        {
            _queue.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        }

        public void Enqueue(T item, int priority)
        {
            _queue.Add(Tuple.Create(item, priority));
            SortByPriority();
        }

        public T Dequeue()
        {
            if (_queue.Count == 0)
                return default;

            SortByPriority();
            T element = _queue[0].Item1;
            _queue.RemoveAt(0);
            return element;
        }
    }
}