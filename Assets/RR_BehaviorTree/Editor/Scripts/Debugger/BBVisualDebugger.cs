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
            private Dictionary<Type, Delegate> _callbackMap;

            protected BBEventActionBase(BBEventBroker evtBroker, GraphBlackboard blackboard)
            {
                _evtBroker = evtBroker;
                _blackboard = blackboard;
                _callbackMap = new Dictionary<Type, Delegate>(N_PRIM_TYPES);     
            }

            public virtual void SubscribeAll()
            {
                AddCallbackToMap<int>(Subscribe<int>());
                AddCallbackToMap<float>(Subscribe<float>());
                AddCallbackToMap<bool>(Subscribe<bool>());
                AddCallbackToMap<string>(Subscribe<string>());
                AddCallbackToMap<Vector2>(Subscribe<Vector2>());
                AddCallbackToMap<Vector3>(Subscribe<Vector3>());
                AddCallbackToMap<UnityEngine.Object>(Subscribe<UnityEngine.Object>());
            }

            private void AddCallbackToMap<T>(Delegate callback) => _callbackMap.Add(typeof(T), callback);

            protected abstract Delegate Subscribe<TVal>();

            public virtual void UnsubscribeAll()
            {
                RemoveCallbackAndUnsubscribe<int>();
                RemoveCallbackAndUnsubscribe<float>();
                RemoveCallbackAndUnsubscribe<bool>();
                RemoveCallbackAndUnsubscribe<string>();
                RemoveCallbackAndUnsubscribe<Vector2>();
                RemoveCallbackAndUnsubscribe<Vector3>();
                RemoveCallbackAndUnsubscribe<UnityEngine.Object>();
            }

            protected void RemoveCallbackAndUnsubscribe<T>()
            {
                Delegate callback = RemoveCallbackFromMap<T>();
                Unsubscribe<T>(callback);
            }

            private Delegate RemoveCallbackFromMap<T>()
            {
                _callbackMap.Remove(typeof(T), out Delegate callback);
                return callback;
            }

            protected abstract void Unsubscribe<TVal>(Delegate callback);
        }

        private class AddEntryAction : BBEventActionBase
        {
            public AddEntryAction(BBEventBroker evtBroker, GraphBlackboard blackboard) : base(evtBroker, blackboard)
            {}

            protected override Delegate Subscribe<TVal>()
            {
                Action<BBAddEntryEvent<TVal>> callback = evt => _blackboard.AddEntry<TVal>(evt.key, evt.value);
                _evtBroker.Subscribe<BBAddEntryEvent<TVal>>(callback);
                return callback;
            }

            protected override void Unsubscribe<TVal>(Delegate callback)
            {
                _evtBroker.Unsubscribe((Action<BBAddEntryEvent<TVal>>)callback);
            }
        }

        private class UpdateEntryAction : BBEventActionBase
        {
            public UpdateEntryAction(BBEventBroker evtBroker, GraphBlackboard blackboard) : base(evtBroker, blackboard)
            {}

            protected override Delegate Subscribe<TVal>()
            {
                Action<BBUpdateEntryEvent<TVal>> callback = evt => _blackboard.UpdateEntry<TVal>(evt.key, evt.value);
                _evtBroker.Subscribe<BBUpdateEntryEvent<TVal>>(callback);
                return callback;
            }

            protected override void Unsubscribe<TVal>(Delegate callback)
            {
                _evtBroker.Unsubscribe((Action<BBUpdateEntryEvent<TVal>>)callback);
            }
        }

        private class DeleteEntryAction : BBEventActionBase
        {
            public DeleteEntryAction(BBEventBroker evtBroker, GraphBlackboard blackboard) : base(evtBroker, blackboard)
            {}

            public override void SubscribeAll()
            {
                Action<BBDeleteEntryEvent> callback = evt => _blackboard.DeleteEntry(evt.key);
                _evtBroker.Subscribe<BBDeleteEntryEvent>(callback);
            }

            public override void UnsubscribeAll()
            {
                RemoveCallbackAndUnsubscribe<int>(); // Any type will do
            }

            protected override Delegate Subscribe<TVal>() => null;

            protected override void Unsubscribe<TVal>(Delegate callback)
            {
                _evtBroker.Unsubscribe((Action<BBDeleteEntryEvent>)callback);
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
                new AddEntryAction(_evtBroker, _blackboard),
                new UpdateEntryAction(_evtBroker, _blackboard),
                new DeleteEntryAction(_evtBroker, _blackboard)
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
