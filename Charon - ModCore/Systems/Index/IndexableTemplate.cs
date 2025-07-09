using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Charon.StarValor.ModCore {
    public abstract class IndexableTemplate : IIndexable, ISerializable {
        public Guid Guid { get; set; }
        public QualifiedName QualifiedName { get; set; }
        public abstract bool UseQualifiedName { get; }
        public abstract bool UniqueType { get; }

        public virtual QualifiedName GetQualifiedName() => new QualifiedName(GetType());

        public virtual object OnSerialize() => null;
        public virtual void OnDeserialize(object data) { }
        [Serialize]
        protected abstract Type InstanceType { get; }
        public int RefCount { get; set; } = 0;

        public virtual void Initialize() { }
        public virtual bool CanRegister() => true;
        public virtual void OnRegister() { }

        protected virtual QualifiedName GetInstanceQualifiedName(IIndexableInstance instance) => new QualifiedName(instance.GetType(), $"{GetType()}++{instance.GetType()}.{instance.Guid}");
        public IIndexableInstance CreateInstance(QualifiedName? qualifiedName, Guid? staticGuid) {
            var instance = typeof(ScriptableObject).IsAssignableFrom(InstanceType) ? (IIndexableInstance)ScriptableObject.CreateInstance(InstanceType) : (IIndexableInstance)Activator.CreateInstance(InstanceType);
            instance.Ref(staticGuid);
            Apply(instance, qualifiedName ?? GetInstanceQualifiedName(instance));
            return instance;
        }
        public void Apply(IIndexableInstance instance, QualifiedName qualifiedName) {
            if (!(instance.TemplateData.Template is null)) {
                IndexSystem.Instance.UnregisterTypeInstance(instance);
                instance.TemplateData.Template.Instances.Remove(instance.QualifiedName);
                instance.TemplateData.Template.OnRemoving(instance);
            }
            instance.QualifiedName = qualifiedName;
            instance.TemplateData.Template = this;

            IndexSystem.Instance.RegisterTypeInstance(instance);
            Instances.Add(instance.QualifiedName, instance);
            //ModCore.Instance.Log.LogMessage($"    Applying {instance.QualifiedName}");
            instance.TemplateData.Template.OnApplying(instance);
        }
        public virtual void OnApplying(IIndexableInstance instance) { }
        public virtual void OnRemoving(IIndexableInstance instance) { }

        public virtual void VerifyComponents(IIndexableInstance instance) { }
        public virtual int GetHashCode(HashContext context) => this.GetHashCode();

        public Dictionary<QualifiedName, IIndexableInstance> Instances { get; } = new Dictionary<QualifiedName, IIndexableInstance>();
        public IIndexableInstance FirstInstance() => Instances.Count == 0 ? null : Instances.FirstOrDefault().Value;
    }
}
