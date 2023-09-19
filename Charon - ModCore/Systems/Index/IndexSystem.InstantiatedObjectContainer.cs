using System;
using System.Runtime.Serialization;

namespace Charon.StarValor.ModCore {
    public partial class IndexSystem {
        class InstanceContainer {
            public int Id;
            object data;

            Type baseType;
            QualifiedName qualifiedName;
            int refCount = 0;
            public bool IgnoreRef { get; set; }

            [NonSerialized]
            public IndexableTemplate Template;

            [NonSerialized]
            public IIndexableInstance Instance;


            [OnSerializing]
            void OnSerializing() {
                if (Instance is null)
                    throw new NullReferenceException("Instance was null");
                data = Instance.GetSerialization();
            }

            [OnDeserialized]
            void OnDeserialized() {
                Template = RegisteredTemplate.GetRegisteredTemplate(baseType, qualifiedName);
                Instance = Template.GenerateInstance(Id, data);
                data = null;
            }

            public InstanceContainer(Type type, QualifiedName qualifiedName, IndexableTemplate template, object data) {
                this.qualifiedName = qualifiedName;
                this.Template = template;
                this.baseType = type;
                Instance = Template.GenerateInstance(Id, data);
            }

            public override int GetHashCode() => Instance.GetHashCode();
            public override bool Equals(object obj) => obj.GetType() == Instance.GetType() && obj.GetHashCode() == Instance.GetHashCode();

            public void Set(IndexableTemplate template, object data) {
                this.Template = template;
                if (Instance != null)
                    template.Apply(Instance, data);
            }
            internal void Ref() => ++refCount;
            internal bool Deref() {
                if (IgnoreRef)
                    return false;

                if (--refCount > 0)
                    return false;
                if (!(IndexSystem.Instance.allocatedContainers.TryGetValue(baseType, out var dict))) {
                    dict = new ResourceAllocator<InstanceContainer>();
                    IndexSystem.Instance.allocatedContainers[baseType] = dict;
                }
                dict.Deallocate(Id);
                return true;
            }
        }
    }
}
