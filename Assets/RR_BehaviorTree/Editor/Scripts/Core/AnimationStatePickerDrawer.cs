using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

using System.Linq;
using System;

namespace RR.AI.BehaviorTree
{
    [CustomPropertyDrawer(typeof(AnimationStatePicker))]
    public class AnimationStatePickerDrawer : PropertyDrawer
    {
        private class PickerCallbackPair
        {
            public Action<int, string> OkCallback { get; private set; }
            public Action CancelCallback { get; private set; }

            public PickerCallbackPair(Action<int, string> okCallback, Action cancelCallback = null)
            {
                OkCallback = okCallback;
                CancelCallback = cancelCallback;
            }
        }

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var containerView = new VisualElement();
            containerView.styleSheets.Add(StylesheetUtils.Load(nameof(AnimationStatePickerDrawer)));
            var mainView = new VisualElement();
            var pickerView = new VisualElement();
            PickerCallbackPair pickerCallbackPair = null;

            void onSwitchPickerRequest()
            {
                containerView.Remove(mainView);
                containerView.Add(pickerView);
            }

            void onSwitchMainRequest()
            {
                containerView.Remove(pickerView);
                containerView.Add(mainView);
            }

            (mainView, pickerCallbackPair) = MakeMainView(property, onSwitchPickerRequest);
            pickerView = MakePickerView(property, pickerCallbackPair, onSwitchMainRequest);
            containerView.Add(mainView);

            return containerView;
        }

        private (VisualElement, PickerCallbackPair) MakeMainView(SerializedProperty property, Action switchPickerRequestCallback)
        {
            var containerView = new VisualElement();
            var layerField = new IntegerField(property.FindPropertyRelative("_layer").displayName);
            layerField.BindProperty(property.FindPropertyRelative("_layer"));
            var animStateNameField = new TextField(property.FindPropertyRelative("_stateName").displayName);
            animStateNameField.BindProperty(property.FindPropertyRelative("_stateName"));
            var openPickerButton = new Button(switchPickerRequestCallback){ text = "Select" };

            containerView.Add(layerField);
            containerView.Add(animStateNameField);
            containerView.Add(openPickerButton);

            var pickerCallbackPair = new PickerCallbackPair(
                (layer, stateName) => 
                {
                    layerField.value = layer;
                    animStateNameField.value = stateName;
                }
            );
            
            return (containerView, pickerCallbackPair);
        }

        private VisualElement MakePickerView(
            SerializedProperty property
            , PickerCallbackPair pickerCallbackPair
            , Action switchMainRequestCallback
            )
        {
            var containerView = new VisualElement();
            containerView.AddToClassList("container-view-picker");

            var layerField = new PopupField<int>(property.FindPropertyRelative("_layer").displayName);
            layerField.AddToClassList("base-field");

            var animStateNameField = new PopupField<string>(property.FindPropertyRelative("_stateName").displayName);
            animStateNameField.BindProperty(property.FindPropertyRelative("_stateName"));
            animStateNameField.AddToClassList("base-field");

            var buttonContainerView = new VisualElement();
            buttonContainerView.AddToClassList("button-container-picker");

            var okButton = new Button(() => 
            {
                pickerCallbackPair.OkCallback?.Invoke(layerField.value, animStateNameField.value);
                switchMainRequestCallback?.Invoke();
            }){ text = "OK" };

            var cancelButton = new Button(() => 
            {
                pickerCallbackPair.CancelCallback?.Invoke();
                switchMainRequestCallback?.Invoke();
            }){ text = "Cancel" };
            buttonContainerView.Add(cancelButton);

            var animatorControllerField = new ObjectField("Controller"){ objectType = typeof(AnimatorController) };
            animatorControllerField.AddToClassList("base-field");
            animatorControllerField.RegisterValueChangedCallback(evt =>
            {
                RemoveIfChild(containerView, layerField);
                RemoveIfChild(containerView, animStateNameField);

                if (evt.newValue == null)
                {
                    RemoveIfChild(buttonContainerView, okButton);
                    return;
                }

                AnimatorController controller = evt.newValue as AnimatorController;
                int nLayers = controller.layers.Length;
                layerField.choices = Enumerable.Range(0, nLayers).ToList();
                int defaultLayer = 0;
                layerField.value = defaultLayer;
                containerView.Add(layerField);

                var animStateNames = controller.layers[defaultLayer].stateMachine.states.Select(state => state.state.name).ToList();
                animStateNameField.choices = animStateNames;
                animStateNameField.value = animStateNames[0];
                containerView.Add(animStateNameField);

                buttonContainerView.BringToFront();
                buttonContainerView.Add(okButton);
                cancelButton.BringToFront();

                static bool RemoveIfChild(VisualElement parent, VisualElement element)
                {
                    if (!parent.Contains(element))
                    {
                        return false;
                    }

                    parent.Remove(element);
                    return true;
                }
            });

            containerView.Add(animatorControllerField);
            containerView.Add(buttonContainerView);

            return containerView;
        }
    }
}
