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
        private List<BTGraphNodeAttacher> _attachers;
        protected List<BTGraphNodeAttacher> Attachers
        {
            get
            {
                if (_attachers == null)
                {
                    _attachers = new List<BTGraphNodeAttacher>();
                }

                return _attachers;
            }
        }
        private BTGraphNodeAttacher _hoveredAttacher;

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

        public static Action<string, string, Action> AttacherDeleted { get; set; }

        protected Action<string, Vector2, Action<BTGraphInitParamsAttacher>> OpenDecoSearchWnd;
        protected Action<string, Vector2, Action<BTGraphInitParamsAttacher>> OpenServiceSearchWnd;

        public abstract void OnConnect(BTGraphDesign graphDesign, string parentGuid);
        public abstract void OnCreate(BTGraphDesign graphDesign, Vector2 position);
        public abstract void OnDelete(BTGraphDesign graphDesign);
        public abstract void OnMove(BTGraphDesign graphDesign, Vector2 moveDelta);

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
                if (_attachers == null || _attachers.Count == 0)
                {
                    return Name.Length;
                }

                var longestDecorator = _attachers.Aggregate((longest, next) => next.Name.Length > longest.Name.Length ? next : longest);
                return Mathf.Max(Name.Length, longestDecorator.Name.Length);
            }
        }

        public void InitAttachers(List<BTSerializableAttacher> serializedAttachers)
        {
            _attachers = new List<BTGraphNodeAttacher>(serializedAttachers.Count);

            foreach (var serializedAttacher in serializedAttachers)
            {   
                bool isDecorator = typeof(BTDecoratorBase).IsAssignableFrom(serializedAttacher.task.GetType());
                BTGraphInitParamsAttacher initParams = ToGraphInitParams(serializedAttacher);
                var graphAttacher = isDecorator ? AttachNewDecorator(initParams) : AttachNewService(initParams);
            }
        }

        private BTGraphInitParamsAttacher ToGraphInitParams(BTSerializableAttacher serializedAttacher)
        {
            var icon = BTGlobalSettings.Instance.GetIcon(serializedAttacher.task.GetType());
            var initParams = new BTGraphInitParamsAttacher()
            {
                guid = serializedAttacher.guid,
                nodeID = _guid,
                name = serializedAttacher.name,
                icon = icon,
                task = serializedAttacher.task
            };

            return initParams;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_hoveredAttacher != null)
            {
                BuildContextualMenuAttacher(evt, _hoveredAttacher);
                return;
            }

            BuildContextualMenuNode(evt);
        }

        private void BuildContextualMenuNode(ContextualMenuPopulateEvent evt)
        {
            base.BuildContextualMenu(evt);

            if (!AreAttachersAllowed)
            {
                return;
            }

            evt.menu.InsertAction(1, "Add Decorator", action => 
            {
                var rect = GetPosition();
                var mousePos = rect.position + new Vector2(rect.width, 0f);
                OpenDecoSearchWnd(_guid, mousePos, initParams => AttachNewDecorator(initParams));
            });

            evt.menu.InsertAction(2, "Add Service", action => 
            {
                var rect = GetPosition();
                var mousePos = rect.position + new Vector2(rect.width, 0f);
                OpenServiceSearchWnd(_guid, mousePos, initParams => AttachNewService(initParams));
            });

            evt.menu.InsertSeparator("/", 1);
        }

        private void BuildContextualMenuAttacher(ContextualMenuPopulateEvent evt, BTGraphNodeAttacher hoveredAttacher)
        {
            evt.menu.InsertAction(0, "Delete Attacher", action =>
            {
                // Hovered attacher somehow becomes null here so it must be passed as arg
                string attacherToDeleteGuid = hoveredAttacher.Guid;
                AttacherDeleted?.Invoke(_guid, attacherToDeleteGuid, () =>
                {
                    Attachers.Remove(hoveredAttacher);
                    extensionContainer.Remove(hoveredAttacher);
                    hoveredAttacher.OnRemove();

                    if (Attachers.Count != 0)
                    {
                        return;
                    }

                    RefreshExpandedState();
                });
            });
        }

        protected abstract bool AreAttachersAllowed { get; }

        private BTGraphNodeAttacher AttachNewService(BTGraphInitParamsAttacher initParams) 
            => AddNewAttacher(initParams, BTGraphNodeAttacher.CreateService);

        private BTGraphNodeAttacher AttachNewDecorator(BTGraphInitParamsAttacher initParams)
            => AddNewAttacher(initParams, BTGraphNodeAttacher.CreateDecorator);

        private BTGraphNodeAttacher AddNewAttacher(BTGraphInitParamsAttacher initParams, Func<BTGraphInitParamsAttacher, BTGraphNodeAttacher> ctor)
        {
            BTGraphNodeAttacher attacher = ctor(initParams);
            extensionContainer.style.backgroundColor = Utils.ColorExtension.Create(62f);
            extensionContainer.style.paddingTop = 3f;
            extensionContainer.Add(attacher);
            Attachers.Add(attacher);
            RefreshExpandedState();

            if (OrderLabel != null)
            {
                OrderLabel.SetRealPosition(new Vector2(x + LabelPosX, y));
            }

            attacher.MouseEntered += OnAttacherMouseEnter;
            attacher.MouseExited += OnAttacherMouseExit;

            return attacher;
        }

        private void OnAttacherMouseEnter(BTGraphNodeAttacher attacher)
        {
            _hoveredAttacher = attacher;
            var evtPtrLeave = PointerLeaveEvent.GetPooled();
            evtPtrLeave.target = this;
            this.SendEvent(evtPtrLeave);
        }

        private void OnAttacherMouseExit(BTGraphNodeAttacher attacher)
        {
            _hoveredAttacher = null;
            var evtPtrEnter = PointerEnterEvent.GetPooled();
            evtPtrEnter.target = this;
            this.SendEvent(evtPtrEnter);
        }
    }
}