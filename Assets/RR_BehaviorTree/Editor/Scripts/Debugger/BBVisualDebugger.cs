using UnityEngine;

using System;
using System.Collections.Generic;

namespace RR.AI.Debugger
{
    public class BBVisualDebugger
    {
        private abstract class BBEventActionBase
        {
            private static int N_PRIM_TYPES = 7;

            protected BBEventBroker _evtBroker;
            protected GraphBlackboard _blackboard;
            protected Dictionary<Type, Delegate> _callbackMap;

            protected BBEventActionBase(BBEventBroker evtBroker, GraphBlackboard blackboard)
            {
                _evtBroker = evtBroker;
                _blackboard = blackboard;
                _callbackMap = new Dictionary<Type, Delegate>(N_PRIM_TYPES);     
            }

            public void SubscribeAll()
            {
                Subscribe<int>();
                Subscribe<float>();
                Subscribe<bool>();
                Subscribe<string>();
                Subscribe<Vector2>();
                Subscribe<Vector3>();
                Subscribe<UnityEngine.Object>();
            }

            protected abstract void Subscribe<TVal>();

            public void UnsubscribeAll()
            {
                Unsubscribe<int>();
                Unsubscribe<float>();
                Unsubscribe<bool>();
                Unsubscribe<string>();
                Unsubscribe<Vector2>();
                Unsubscribe<Vector3>();
                Unsubscribe<UnityEngine.Object>();
            }

            protected abstract void Unsubscribe<TVal>();
        }

        private class AddEntryAction : BBEventActionBase
        {
            public AddEntryAction(BBEventBroker evtBroker, GraphBlackboard blackboard) : base(evtBroker, blackboard)
            {}

            protected override void Subscribe<TVal>()
            {
                Action<BBAddEntryEvent<TVal>> callback = evt => _blackboard.AddEntry<TVal>(evt.key, evt.value);
                _evtBroker.Subscribe<BBAddEntryEvent<TVal>>(callback);
            }

            protected override void Unsubscribe<TVal>()
            {
                Delegate callback = _callbackMap[typeof(BBAddEntryEvent<TVal>)];
                _evtBroker.Unsubscribe((Action<BBAddEntryEvent<TVal>>)callback);
            }
        }

        private BBEventBroker _evtBroker;
        private GraphBlackboard _blackboard;

        private BBEventActionBase[] _BBEventActionList;
        private BBEventActionBase _addEntry;

        public BBVisualDebugger(GraphBlackboard graphBB)
        {
            // UnityEngine.Debug.Log("BB Debugger is running...");
            _blackboard = graphBB;
            _evtBroker = BBEventBroker.Instance;

            _BBEventActionList = new BBEventActionBase[]
            {
                new AddEntryAction(_evtBroker, _blackboard)
            };
            
            foreach (BBEventActionBase action in _BBEventActionList)
            {
                action.SubscribeAll();
            }
        }

        private void OnPlayModeStop()
        {
            foreach (BBEventActionBase action in _BBEventActionList)
            {
                action.UnsubscribeAll();
            }
        }
    }
}
