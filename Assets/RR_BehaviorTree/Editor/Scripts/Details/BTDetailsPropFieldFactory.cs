using UnityEngine;
using UnityEngine.UIElements;

using System;
using System.Collections.Generic;
using UnityEditor.UIElements;

namespace RR.AI.BehaviorTree
{
    public class BTDetailsPropFieldFactory
    {
        Dictionary<Type, Func<System.Reflection.FieldInfo, object, GraphBlackboard, VisualElement>> _propFieldDict;

        public BTDetailsPropFieldFactory()
        {
            _propFieldDict = new Dictionary<Type, Func<System.Reflection.FieldInfo, object, GraphBlackboard, VisualElement>>
            {
                { typeof(string), StringPropField},
                { typeof(int), IntPropField},
                { typeof(float), FloatPropField},
                { typeof(bool), BoolPropField},
                { typeof(Vector2), Vector2PropField},
                { typeof(Vector3), Vector3PropField},
                { typeof(UnityEngine.Object), ObjectPropField }
            };
        }

        public VisualElement PropField(
            Type type, 
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            if (!_propFieldDict.ContainsKey(type))
            {
                return new Label($"Unsupported type\n {type}");
            }

            return _propFieldDict[type](fieldInfo, propFieldValue, blackboard);
        }

        private VisualElement StringPropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            var BBValueAttribs = fieldInfo.GetCustomAttributes(typeof(RR.AI.BlackboardValueAttribute), true);
                
            if (BBValueAttribs.Length > 0)
            {
                var valType = (BBValueAttribs[0] as BlackboardValueAttribute).ValueType;
                
                var BBKeys = blackboard.GetKeys(valType);

                if (BBKeys.Count == 0)
                {
                    var label = new Label($"No {valType} entry");
                    label.style.whiteSpace = WhiteSpace.Normal;
                    return label;
                }

                var fieldValue = (string) fieldInfo.GetValue(propFieldValue);
                var field = new PopupField<string>(BBKeys, string.IsNullOrEmpty(fieldValue) ? BBKeys[0] : fieldValue);
                return field;
            }

            var tagFieldAttribs = fieldInfo.GetCustomAttributes(typeof(RR.Serialization.TagFieldAttribute), true);
            
            if (tagFieldAttribs.Length > 0)
            {
                return CreatePropField(new TagField(string.Empty, "Untagged"), fieldInfo, propFieldValue);
            }

            return CreatePropField(new TextField(string.Empty, 200, true, false, '*'), fieldInfo, propFieldValue);
        }

        private VisualElement IntPropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            var layerMaskFieldAttribs = fieldInfo.GetCustomAttributes(typeof(RR.Serialization.LayerMaskFieldAttribute), true);
                
                if (layerMaskFieldAttribs.Length > 0)
                {
                    return CreatePropField(new LayerMaskField(), fieldInfo, propFieldValue);
                }

                return CreatePropField(new IntegerField(), fieldInfo, propFieldValue);
        }

        private VisualElement FloatPropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            return CreatePropField(new FloatField(), fieldInfo, propFieldValue);
        }

        private VisualElement BoolPropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            return CreatePropField(new Toggle(), fieldInfo, propFieldValue);
        }   

        private VisualElement Vector2PropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            return CreatePropField(new Vector2Field(), fieldInfo, propFieldValue);
        }

        private VisualElement Vector3PropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            return CreatePropField(new Vector3Field(), fieldInfo, propFieldValue);
        }

        private VisualElement ObjectPropField(
            System.Reflection.FieldInfo fieldInfo, 
            object propFieldValue, 
            GraphBlackboard blackboard)
        {
            var field = new ObjectField() { objectType = typeof(UnityEngine.Object) };
            return field;  
        }

        private VisualElement CreatePropField<TProp>(
            BaseField<TProp> baseField,
            System.Reflection.FieldInfo fieldInfo,
            object propFieldValue)
        {
            var field = baseField;
            field.value = (TProp) fieldInfo.GetValue(propFieldValue);
            field.RegisterValueChangedCallback(evt => fieldInfo.SetValue(propFieldValue, evt.newValue));
            return field;
        }
    }
}
