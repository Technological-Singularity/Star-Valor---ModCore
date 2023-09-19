using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public sealed partial class IndexableInstanceData {
        GameObject _gameObj = null;
        GameObject GameObj {
            get {
                if (_gameObj == null) {
                    _gameObj = new GameObject();
                    _gameObj.AddComponent<DestructionMonitor>().InstanceData = this;
                }
                return _gameObj;
            }
        }
        
        public IIndexableInstance Instance { get; }
        public IndexableTemplate Template { get; set; }
        public object Data { get; set; } = null;

        public IndexableInstanceData(IIndexableInstance instance) {
            this.Instance = instance;
        }
        ~IndexableInstanceData() {
            if (_gameObj != null)
                UnityEngine.Object.Destroy(_gameObj);
            _gameObj = null;
        }
        public int GetHashCode(HashContext context) {
            var hashCodes = GameObj.GetComponents<ComponentEx>().Select(o => o.GetHashCode(context));
            return Utilities.GetHashCode(hashCodes, Template.GetHashCode(context));
        }
        public object GetSerialization() {
            List<object> serialized = new List<object>() { Data };
            if (_gameObj != null)
                serialized.Add(_gameObj.GetComponents<ComponentEx>().Select(o => (o.GetType(), o.GetSerialization())).ToList());
            return serialized;
        }
        public void Deserialize(object serialization) {
            if (_gameObj != null) {
                foreach (var o in _gameObj.GetComponents<ComponentEx>())
                    UnityEngine.Object.Destroy(o);
            }

            var objs = (List<object>)serialization;
            Data = objs[0];

            if (objs.Count > 1)
                foreach (var (type, data) in (List<(Type, object)>)objs[1]) {
                    var component = (ComponentEx)GameObj.AddComponent(type);
                    component.InstanceData = this;
                    component.Deserialize(data);
                }

            if (_gameObj != null && _gameObj.transform.childCount == 0) {
                UnityEngine.Object.Destroy(_gameObj);
                _gameObj = null;
            }
        }
        public T AddComponent<T>(bool exclusive) where T : ComponentEx {
            T component = null;
            if (exclusive)
                component = GameObj.GetComponent<T>();
            if (component == null)
                component = GameObj.AddComponent<T>();
            component.InstanceData = this;
            return component;
        }
        public T GetComponent<T>() where T : ComponentEx {
            if (_gameObj == null)
                return null;
            var component = GameObj.GetComponent<T>();
            if (component == null)
                Template.VerifyComponents(Instance);
            return GameObj.GetComponent<T>();
        }
        public IEnumerable<T> GetComponents<T>() where T : ComponentEx {
            Template.VerifyComponents(Instance);
            return GameObj.GetComponents<T>();
        }
        public bool TryGetComponent<T>(out T value) where T : ComponentEx {
            if (_gameObj == null) {
                value = null;
                return false;
            }
            return GameObj.TryGetComponent<T>(out value);
        }
        void OnMonitorDestroyed() { }
    }
}
