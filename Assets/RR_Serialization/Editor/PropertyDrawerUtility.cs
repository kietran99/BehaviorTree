using UnityEditor;

namespace RR.Serialization.Editor
{
    public static class PropertyDrawerUtility
    {
        public static int GetChildrenSize(SerializedProperty property, string propName)
        {
            var child = property.FindPropertyRelative(propName);
            var childrenSize = child.isArray ? child.arraySize : CountChildren(property, propName);
            return childrenSize == 0 ? 1 : childrenSize;
        }

        private static int CountChildren(SerializedProperty property, string propName)
        {
            int cnt = 0;

            foreach (var _ in property.FindPropertyRelative(propName))
            {
                cnt++;
            }

            return cnt;
        }
    }
}