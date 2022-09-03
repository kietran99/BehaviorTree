using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

using System;
using System.Linq;
using System.Collections.Generic;

namespace RR.AI.BehaviorTree
{
    public abstract class BTGraphNodeBase : Node, IBTSerializableNode
    {
        protected string _guid;
        private List<BTGraphNodeDecorator> _decorators;
        protected List<BTGraphNodeDecorator> Decorators
        {
            get
            {
                if (_decorators == null)
                {
                    _decorators = new List<BTGraphNodeDecorator>();
                }

                return _decorators;
            }
        }

        public string Guid => _guid;
        public abstract string Name { get; }
        public BTGraphOrderLabel OrderLabel { get; set; }
        public int OrderValue
        {
            get => OrderLabel.Value;
            set => OrderLabel.Value = value;
        }

        public int x { get; protected set; }
        public int y { get; protected set; }

        protected Action<string, Vector2, Action<BTGraphInitParamsDeco>> OpenDecoSearchWnd;

        public abstract void OnConnect(BTDesignContainer designContainer, string parentGuid);
        public abstract void OnCreate(BTDesignContainer designContainer, Vector2 position);
        public abstract void OnDelete(BTDesignContainer designContainer);
        public abstract void OnMove(BTDesignContainer designContainer, Vector2 moveDelta);

        public int LabelPosX
        {
            get
            {
                const int maxCharForDefaultSize = 8;
                int titleLen = TextContentLength;
                var posX = 108 + (titleLen <= maxCharForDefaultSize ? 0 : (titleLen - maxCharForDefaultSize) * 17) - 14;
                return posX;
            }
        }

        private int TextContentLength
        {
            get
            {
                if (_decorators == null || _decorators.Count == 0)
                {
                    return Name.Length;
                }

                var longestDecorator = _decorators.Aggregate((longest, next) => next.Name.Length > longest.Name.Length ? next : longest);
                return Mathf.Max(Name.Length, longestDecorator.Name.Length);
            }
        }

        public void InitDecorators(List<BTSerializableDecoData> serializedDecorators)
        {
            _decorators = new List<BTGraphNodeDecorator>(serializedDecorators.Count);

            foreach (var serializedDeco in serializedDecorators)
            {
                var decoElement = CreateDecorator(serializedDeco);
                _decorators.Add(decoElement);
                AttachDecorator(decoElement);
            }
        }

        private BTGraphNodeDecorator CreateDecorator(BTSerializableDecoData serializedDecorator)
        {
            var decoIcon = BTGlobalSettings.Instance.GetIcon(serializedDecorator.decorator.GetType());
            var initParams = new BTGraphInitParamsDeco()
            {
                guid = serializedDecorator.guid,
                nodeID = _guid,
                decoName = serializedDecorator.name,
                icon = decoIcon,
                task = serializedDecorator.decorator
            };
            return CreateDecorator(initParams);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (!CanAttachDecorators)
            {
                return;
            }

            evt.menu.InsertAction(1, "Add Decorator", action => 
            {
                var rect = GetPosition();
                var mousePos = rect.position + new Vector2(rect.width, 0f);
                OpenDecoSearchWnd(_guid, mousePos, AttachNewDecorator);
            });

            evt.menu.InsertSeparator("/", 1);
        }

        protected abstract bool CanAttachDecorators { get; }

        private void AttachNewDecorator(BTGraphInitParamsDeco initParams)
        {
            var decorator = CreateDecorator(initParams);
            AttachDecorator(decorator);
            OrderLabel.SetRealPosition(new Vector2(x + LabelPosX, y));
        }

        private void AttachDecorator(BTGraphNodeDecorator decorator)
        {
            extensionContainer.style.backgroundColor = Utils.ColorExtension.Create(62f);
            extensionContainer.style.paddingTop = 3f;
            extensionContainer.Add(decorator);
            Decorators.Add(decorator);
            RefreshExpandedState();
        }

        private BTGraphNodeDecorator CreateDecorator(BTGraphInitParamsDeco initParams)
            => new BTGraphNodeDecorator(initParams);
    }
}