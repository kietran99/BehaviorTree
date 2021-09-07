using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeLeaf<T> : BTGraphNode<BTGraphLeaf<T>> where T : BTBaseTask
    {
        private System.Func<object> TaskPropConstructFn;

        public BTGraphNodeLeaf(Vector2 pos, string guid = "") : base(pos, guid)
        {
            mainContainer.style.backgroundColor = new StyleColor(new Color(50f / 255f, 50f / 255f, 50f / 255f));
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
                childContainer.style.width = 125;
                childContainer.style.flexDirection = FlexDirection.Row;
                childContainer.style.marginTop = 5;
                childContainer.style.marginBottom = 5;
                childContainer.style.marginRight = 5;
                childContainer.style.marginLeft = 5;

                var label = new Label(RR.Utils.StringUtility.InsertWhiteSpaces(fieldInfo.Name));
                label.style.width = 60;
                label.style.unityTextAlign = TextAnchor.MiddleLeft;
                label.style.fontSize = 12;
                label.style.whiteSpace = WhiteSpace.Normal;
                childContainer.Add(label); 

                var (field, bindPropFieldFn) = DrawPropField(fieldInfo, propFieldData);
                childContainer.Add(field);
                field.style.maxWidth = 65;
                
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
                var attribs = fieldInfo.GetCustomAttributes(typeof(RR.Serialization.TagFieldAttribute), true);
                
                if (attribs.Length > 0)
                {
                    var tagField = new TagField(string.Empty, "Untagged");
                    tagField.value = (string) fieldInfo.GetValue(propFieldData);
                    return (StylizePropField(tagField), prop => fieldInfo.SetValue(prop, tagField.value));
                }

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

            if (type == typeof(Vector3))
            {
                var field = new Vector3Field();
                field.value = (Vector3) fieldInfo.GetValue(propFieldData);
                return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
            }

            if (typeof(ScriptableObject).IsAssignableFrom(type) || type.IsInterface)
            {
                var field = new ObjectField() { objectType = type };
                return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
            }

            return (new Label($"Unsupported type {type}"), _ => {});
        }

        private VisualElement StylizePropField(VisualElement field)
        {
            field.style.width = 100;
            return field;
        }

        protected override Texture2D GetIcon(BTNodeType _)
        {
            return _nodeAction.Task.Icon;
        }

        public override void Save(BTDesignContainer designContainer)
        {    
            _nodeAction.Task.SavePropData(_guid, 
                TaskPropConstructFn != null 
                    ? TaskPropConstructFn() 
                    : System.Activator.CreateInstance(_nodeAction.Task.PropertyType));            

            designContainer.taskDataList.Add(
                new BTSerializableTaskData(GetPosition().position, 
                _guid, 
                GetParentGuid(inputContainer), 
                _nodeAction.Task));
        }

        public override System.Action DeleteCallback => () => _nodeAction.Task.RemoveProp(_guid);
    }
}
