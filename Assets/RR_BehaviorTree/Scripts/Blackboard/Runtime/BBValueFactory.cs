using UnityEditor;
using UnityEngine;

namespace RR.AI
{
	public static class BBValueFactory
	{
		public static ScriptableObject New<T>(ScriptableObject assetObj, object value)
		{
			if (typeof(T) == typeof(int))
			{
				var res = CreateValueObject<BBInt>(assetObj);
				res.Value = (int)value;
				return res;
			}

			if (typeof(T) == typeof(bool))
			{
				var res = CreateValueObject<BBBool>(assetObj);
				res.Value = (bool)value;
				return res;
			}

			return null;
		}

		private static T CreateValueObject<T>(ScriptableObject assetObj) where T : ScriptableObject
		{
			var SO = ScriptableObject.CreateInstance<T>();
			AssetDatabase.AddObjectToAsset(SO, assetObj);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(SO));
			return SO;
		}
	}
}