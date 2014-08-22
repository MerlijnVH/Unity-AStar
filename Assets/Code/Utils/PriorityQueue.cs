using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PriorityQueue<P, V>
{
	private SortedDictionary<P, LinkedList<V>> list = new SortedDictionary<P, LinkedList<V>>();

	public void Enqueue(P priority, V value)
	{
		LinkedList<V> _list;

		if (!list.TryGetValue(priority, out _list))
		{
			_list = new LinkedList<V>();
			list.Add(priority, _list);
		}

		_list.AddLast(value);
	}

	public V Dequeue()
	{
		SortedDictionary<P, LinkedList<V>>.KeyCollection.Enumerator enumerate = list.Keys.GetEnumerator();
		enumerate.MoveNext();

		P key = enumerate.Current;

		LinkedList<V> _list = list[key];
		V value = _list.First.Value;
		_list.RemoveFirst();

		if (_list.Count == 0)
		{
			list.Remove(key);
		}

		return value;
	}

	public void Replace(P oldPriority, P newPriority, V value)
	{
		// Check if the list has the old value.

		if (!list.ContainsKey(oldPriority))
		{
			return;
		}

		// Remove the old value from the list.

		LinkedList<V> _list = list[oldPriority];
		_list.Remove(value);

		// If that was the last value with this key, remove the key.

		if (_list.Count == 0)
		{
			list.Remove(oldPriority);
		}

		// Push it back in its new spot.

		Enqueue(newPriority, value);
	}

	public bool IsEmpty
	{
		get { return list.Count == 0; }
	}
}
