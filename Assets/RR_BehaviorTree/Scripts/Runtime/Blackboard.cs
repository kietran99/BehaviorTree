namespace RR.AI
{
	public class Blackboard : UnityEditor.Experimental.GraphView.Blackboard
	{
		private System.Collections.Generic.Dictionary<string, IBlackboardItem> _itemDict; 

		public Blackboard(UnityEditor.Experimental.GraphView.GraphView graphView = null) : base(graphView)
		{
			_itemDict = new System.Collections.Generic.Dictionary<string, IBlackboardItem>();
		}

		public bool Add<T>(string key, T value)
		{
			if (_itemDict.TryGetValue(key, out var _))
			{
				return false;
			}

			_itemDict.Add(key, new BlackboardItem<T>(value));
			return true;
		}

		public bool Update<T>(string key, T value)
		{
			if (!_itemDict.TryGetValue(key, out var _))
			{
				return false;
			}

			_itemDict[key] = value as IBlackboardItem;
			return true;
		}

		public bool Remove(string key)
		{
			if (!_itemDict.TryGetValue(key, out var _))
			{
				return false;
			}

			_itemDict.Remove(key);
			return true;
		}
	}
}