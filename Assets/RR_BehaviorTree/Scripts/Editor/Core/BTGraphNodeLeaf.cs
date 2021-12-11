using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeLeaf<T> : BTGraphNode<BTGraphLeaf<T>> where T : BTBaseTask
    {
        protected override BTBaseTask Task => _nodeAction.Task;

        private System.Func<object> TaskPropConstructFn;

        // Invoked by BTGraphNodeFactory using Reflection
        public BTGraphNodeLeaf(
            BTBaseTask task, Vector2 pos, GraphBlackboard blackboard, string name = "", string desc = "", string guid = "") 
            : base(pos, blackboard, task.Name, desc, guid, task.Icon)
        {
            _nodeAction.Task = task;
            _name = task.Name;
        }

        public BTGraphNodeLeaf(Vector2 pos, GraphBlackboard blackboard, string name = "", string desc = "", string guid = "") 
            : base(pos, blackboard, name, desc, guid)
        {
            mainContainer.style.backgroundColor = new StyleColor(new Color(50f / 255f, 50f / 255f, 50f / 255f));
            DrawTaskProperties(_nodeAction.Task.PropertyType, blackboard);
        }

        private void DrawTaskProperties(System.Type propType, GraphBlackboard blackboard)
        {
            var serializableAttribs = propType.GetCustomAttributes(typeof(System.SerializableAttribute), true);
            
            if (serializableAttribs.Length == 0)
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
                var childContainer = CreatePropContainer(20f, 190f);

                childContainer.Add(CreatePropLabel(fieldInfo.Name, 90f)); 

                var (field, bindPropFieldFn) = DrawPropField(fieldInfo, propFieldData, blackboard);
                childContainer.Add(field);
                field.style.maxWidth = 100f;
                
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

        private VisualElement CreatePropContainer(float height, float width)
        {
            var container = new VisualElement();
            container.style.height = height;
            container.style.width = width;
            container.style.flexDirection = FlexDirection.Row;
            container.style.marginTop = 5;
            container.style.marginBottom = 5;
            container.style.marginRight = 5;
            container.style.marginLeft = 5;

            return container;
        }

        private VisualElement CreatePropLabel(string text, float width)
        {
            var label = new Label(RR.Utils.StringUtility.InsertWhiteSpaces(text));
            label.style.width = width;
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = 12;
            label.style.whiteSpace = WhiteSpace.Normal;
            return label;
        }

        private (VisualElement field, System.Action<object> bindPropFieldFn) DrawPropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldData, 
            GraphBlackboard blackboard)
        {
            var type = fieldInfo.FieldType;

            if (type == typeof(string))
            {
                var BBValueAttribs = fieldInfo.GetCustomAttributes(typeof(RR.AI.BlackboardValueAttribute), true);
                if (BBValueAttribs.Length > 0)
                {
                    var valType = (BBValueAttribs[0] as BlackboardValueAttribute).ValueType;
                    
                    var BBKeys = blackboard.GetKeys(valType);

                    if (BBKeys.Count == 0)
                    {
                        var label = new Label($"No value of type {valType}");
                        label.style.whiteSpace = WhiteSpace.Normal;
                        return (label, _ => {});
                    }

                    var fieldValue = (string) fieldInfo.GetValue(propFieldData);
                    var field = new PopupField<string>(BBKeys, string.IsNullOrEmpty(fieldValue) ? BBKeys[0] : fieldValue);
                    return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
                }

                var tagFieldAttribs = fieldInfo.GetCustomAttributes(typeof(RR.Serialization.TagFieldAttribute), true);
                
                if (tagFieldAttribs.Length > 0)
                {
                    return CreatePropField(new TagField(string.Empty, "Untagged"), fieldInfo, propFieldData);
                }

                return CreatePropField(new TextField(string.Empty, 200, true, false, '*'), fieldInfo, propFieldData);
            }

            if (type == typeof(int))
            {
                var layerMaskFieldAttribs = fieldInfo.GetCustomAttributes(typeof(RR.Serialization.LayerMaskFieldAttribute), true);
                
                if (layerMaskFieldAttribs.Length > 0)
                {
                    return CreatePropField(new LayerMaskField(), fieldInfo, propFieldData);
                }

                return CreatePropField(new IntegerField(), fieldInfo, propFieldData);
            }

            if (type == typeof(float))
            {
                return CreatePropField(new FloatField(), fieldInfo, propFieldData);
            }

            if (type == typeof(bool))
            {
                return CreatePropField(new Toggle(), fieldInfo, propFieldData);
            }

            if (type == typeof(Vector2))
            {
                return CreatePropField(new Vector2Field(), fieldInfo, propFieldData);
            }

            if (type == typeof(Vector3))
            {
                return CreatePropField(new Vector3Field(), fieldInfo, propFieldData);
            }

            if (typeof(ScriptableObject).IsAssignableFrom(type) || type.IsInterface)
            {
                var field = new ObjectField() { objectType = type };
                return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
            }

            return (new Label($"Unsupported type\n {type}"), _ => {});
        }

        private (VisualElement field, System.Action<object> bindPropFieldFn) CreatePropField<TProp>(
            BaseField<TProp> baseField, 
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldData)
        {
            var field = baseField;
            field.value = (TProp) fieldInfo.GetValue(propFieldData);
            return (StylizePropField(field), prop => fieldInfo.SetValue(prop, field.value));
        }

        private VisualElement StylizePropField(VisualElement field)
        {
            field.style.width = 100;
            return field;
        }

        protected override Texture2D GetIcon(BTNodeType _) => _nodeAction.Task == null ? null : _nodeAction.Task.Icon;

        public override void OnCreate(BTDesignContainer designContainer, UnityEngine.Vector2 position)
        {   
            designContainer.TaskDataList.Add(
                new BTSerializableTaskData(position, 
                _name,
                _description,
                _guid, 
                GetParentGuid(inputContainer), 
                _nodeAction.Task));
                
            _nodeAction.Task.SavePropData(_guid, System.Activator.CreateInstance(_nodeAction.Task.PropertyType));        
        }

        public override void OnDelete(BTDesignContainer designContainer)
        {
            _nodeAction.Task.RemoveProp(_guid);
            BTSerializableTaskData nodeToDelete = designContainer.TaskDataList.Find(node => node.Guid == _guid);
            designContainer.TaskDataList.Remove(nodeToDelete);
        }

        public override void OnMove(BTDesignContainer designContainer, Vector2 position)
        {
            designContainer.TaskDataList.Find(node => node.Guid == _guid).Position = position;
        }

        public override void OnConnect(BTDesignContainer designContainer, string parentGuid)
        {
            designContainer.TaskDataList.Find(node => node.Guid == _guid).ParentGuid = parentGuid;
        }
    }
}
