using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeLeaf<T> : BTGraphNode<BTGraphLeaf<T>>, IBTSavable where T : BTBaseTask
    {
        private System.Func<object> TaskPropConstructFn;

        public BTGraphNodeLeaf(Vector2 pos, string guid = "") : base(pos, guid)
        {
            mainContainer.style.backgroundColor = new StyleColor(new Color(16f / 255f, 16f / 255f, 16f / 255f));
            DrawTaskProperties(_nodeAction.Task.PropertyType);
        }

        private void DrawTaskProperties(System.Type propType)
        {
            var attribs = propType.GetCustomAttributes(typeof(System.SerializableAttribute), true);
            
            if (attribs.Length == 0)
            {
                Debug.LogError($"{propType} must be Serializable");
                return;
            }

            var propFieldData = _nodeAction.Task.LoadPropData(_guid);

            var container = new VisualElement();
            var fieldInfoList = propType.GetFields();

            System.Action<object> bindPropDataFn = null;

            foreach (var fieldInfo in fieldInfoList)
            {
                var childContainer = new VisualElement();
                childContainer.style.height = 20;
                childContainer.style.flexDirection = FlexDirection.Row;
                childContainer.style.marginTop = 5;
                childContainer.style.marginBottom = 5;
                childContainer.style.marginRight = 5;
                childContainer.style.marginLeft = 5;

                var label = new Label(fieldInfo.Name);
                label.style.width = 70;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                
                childContainer.Add(label); 
                var (field, bindPropFieldFn) = DrawPropField(fieldInfo, propFieldData);
                childContainer.Add(field);
                
                bindPropDataFn += bindPropFieldFn;

                container.Add(childContainer);
            }
            
            TaskPropConstructFn = () => 
            {
                var prop = System.Activator.CreateInstance(propType);
                bindPropDataFn?.Invoke(prop);
                return prop;
            };

            mainContainer.Add(container);
        }

        private (VisualElement field, System.Action<object> bindPropFieldFn) DrawPropField
            (System.Reflection.FieldInfo fieldInfo, object propFieldData)
        {
            var type = fieldInfo.FieldType;

            if (type == typeof(string))
            {
                var field = new TextField(string.Empty, 200, true, false, '*');
                field.value = (string) fieldInfo.GetValue(propFieldData);
                return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
            }

            if (type == typeof(int))
            {
                var field = new IntegerField();
                field.value = (int) fieldInfo.GetValue(propFieldData);
                return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
            }

            if (type == typeof(float))
            {
                var field = new FloatField();
                field.value = (float) fieldInfo.GetValue(propFieldData);
                return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
            }

            if (type == typeof(bool))
            {
                var field = new Toggle();
                field.value = (bool) fieldInfo.GetValue(propFieldData);
                return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
            }

            if (typeof(Component).IsAssignableFrom(type) 
                || typeof(ScriptableObject).IsAssignableFrom(type) 
                || type.IsInterface)
            {
                var field = new ObjectField() { objectType = type };
                field.value = (Object) fieldInfo.GetValue(propFieldData);
                return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
            }

            return (new Label($"Unsupported type {type}"), _ => {});
        }

        private VisualElement StylizePropField(VisualElement field)
        {
            field.style.width = 100;
            return field;
        }

        public override void Save(BTDesignContainer designContainer)
        {    
            _nodeAction.Task.SavePropData(_guid, 
                TaskPropConstructFn != null 
                    ? TaskPropConstructFn() 
                    : System.Activator.CreateInstance(_nodeAction.Task.PropertyType));            

            designContainer.taskDataList.Add(
                new BTTaskData(GetPosition().position, 
                _guid, 
                GetParentGuid(inputContainer), 
                _nodeAction.Task));
        }
    }
}
