using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using System;

namespace RR.AI.BehaviorTree
{
    public class BTGraphNodeLeaf<T> : BTGraphNode<BTGraphLeaf<T>> where T : BTTaskBase
    {
        protected override BTTaskBase Task => _nodeAction.Task;

        private Func<object> TaskPropConstructFn;

        // Invoked by BTGraphNodeFactory using Reflection
        public BTGraphNodeLeaf(BTGraphInitParamsNodeLeaf initParams) : base(initParams)
        {
            _nodeAction.Task = initParams.task;
            _name = initParams.task.Name;
        }

        // public BTGraphNodeLeaf(Vector2 pos, GraphBlackboard blackboard, string name = "", string desc = "", string guid = "") 
        //     : base(pos, blackboard, name, desc, guid)
        // {
        //     mainContainer.style.backgroundColor = new StyleColor(new Color(50f / 255f, 50f / 255f, 50f / 255f));
        //     DrawTaskProperties(_nodeAction.TaskLegacy.PropertyType, blackboard);
        // }

        // private void DrawTaskProperties(Type propType, GraphBlackboard blackboard)
        // {
        //     var serializableAttribs = propType.GetCustomAttributes(typeof(SerializableAttribute), true);
            
        //     if (serializableAttribs.Length == 0)
        //     {
        //         Debug.LogError($"{propType} must be Serializable");
        //         return;
        //     }

        //     var propFieldData = _nodeAction.TaskLegacy.LoadPropValue(_guid);

        //     var container = new VisualElement();
        //     var fieldInfoList = propType.GetFields();

        //     Action<object> bindPropDataFn = null;

        //     foreach (var fieldInfo in fieldInfoList)
        //     {
        //         var childContainer = CreatePropContainer(20f, 190f);

        //         childContainer.Add(CreatePropLabel(fieldInfo.Name, 90f)); 

        //         var (field, bindPropFieldFn) = DrawPropField(fieldInfo, propFieldData, blackboard);
        //         childContainer.Add(field);
        //         field.style.maxWidth = 100f;
                
        //         bindPropDataFn += bindPropFieldFn;

        //         container.Add(childContainer);
        //     }
            
        //     TaskPropConstructFn = () => 
        //     {
        //         var prop = Activator.CreateInstance(propType);
        //         bindPropDataFn?.Invoke(prop);
        //         return prop;
        //     };

        //     mainContainer.Add(container);
        // }

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

        private (VisualElement field, Action<object> bindPropFieldFn) DrawPropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldData, 
            GraphBlackboard blackboard)
        {
            var type = fieldInfo.FieldType;

            if (type == typeof(string))
            {
                var BBValueAttribs = fieldInfo.GetCustomAttributes(typeof(BlackboardValueAttribute), true);
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

                var tagFieldAttribs = fieldInfo.GetCustomAttributes(typeof(Serialization.TagFieldAttribute), true);
                
                if (tagFieldAttribs.Length > 0)
                {
                    return CreatePropField(new TagField(string.Empty, "Untagged"), fieldInfo, propFieldData);
                }

                return CreatePropField(new TextField(string.Empty, 200, true, false, '*'), fieldInfo, propFieldData);
            }

            if (type == typeof(int))
            {
                var layerMaskFieldAttribs = fieldInfo.GetCustomAttributes(typeof(Serialization.LayerMaskFieldAttribute), true);
                
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

        private (VisualElement field, Action<object> bindPropFieldFn) CreatePropField<TProp>(
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

        public override void OnCreate(BTGraphDesign designContainer, Vector2 position)
        {   
            designContainer.TaskDataList.Add(
                new BTSerializableTaskData(position, 
                _name,
                _description,
                _guid, 
                GetParentGuid(inputContainer),
                _nodeAction.Task));
        }

        public override void OnDelete(BTGraphDesign designContainer)
        {
            BTSerializableTaskData nodeToDelete = designContainer.TaskDataList.Find(node => node.Guid == _guid);
            designContainer.TaskDataList.Remove(nodeToDelete);
            UnityEditor.AssetDatabase.RemoveObjectFromAsset(nodeToDelete.Task);
        }

        public override void OnMove(BTGraphDesign designContainer, Vector2 moveDelta)
        {
            designContainer.TaskDataList.Find(node => node.Guid == _guid).Position = GetPosition().position;
            SyncOrderLabelPosition(moveDelta);
        }

        public override void OnConnect(BTGraphDesign designContainer, string parentGuid)
        {
            designContainer.TaskDataList.Find(node => node.Guid == _guid).ParentGuid = parentGuid;
        }
    }
}
