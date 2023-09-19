using System;
using System.Collections.Generic;

namespace Charon.StarValor.ModCore {
    public partial class IndexSystem {
        class RegisteredTemplate {
            Type baseType;
            public IndexableTemplate Template { get; }
            public QualifiedName QualifiedName { get; }

            public RegisteredTemplate(QualifiedName qualifiedName, Type baseType, IndexableTemplate template) => (this.QualifiedName, this.baseType, this.Template) = (qualifiedName, baseType, template);

            public InstanceContainer AllocateStatic(int id, object data) {
                if (!(IndexSystem.Instance.allocatedContainers.TryGetValue(baseType, out var resources))) {
                    resources = new ResourceAllocator<InstanceContainer>();
                    IndexSystem.Instance.allocatedContainers[baseType] = resources;
                }
                var iobj = new InstanceContainer(baseType, QualifiedName, Template, data);
                iobj.Id = resources.AllocateStatic(id, iobj);
                iobj.IgnoreRef = true;
                iobj.Ref();
                return iobj;
            }
            public InstanceContainer Allocate(object data) {
                if (!(IndexSystem.Instance.allocatedContainers.TryGetValue(baseType, out var resources))) {
                    resources = new ResourceAllocator<InstanceContainer>();
                    IndexSystem.Instance.allocatedContainers[baseType] = resources;
                }
                var iobj = new InstanceContainer(baseType, QualifiedName, Template, data);
                iobj.Id = resources.Allocate(iobj);
                iobj.Ref();
                return iobj;
            }

            public static bool TryGetRegistered<T>(QualifiedName qualifiedName, out T obj) where T : IndexableTemplate {
                var baseType = IndexSystem.Instance.GetBaseRegisterableType(typeof(T));
                if (IndexSystem.Instance.registeredNames.TryGetValue(baseType, out var regList) && regList.TryGetValue(qualifiedName, out var reg)) {
                    obj = (T)reg.Template;
                    return true;
                }
                obj = default;
                return false;
            }

            public static T Register<T>(QualifiedName qualifiedName, T obj) where T : IndexableTemplate {
                var baseType = IndexSystem.Instance.GetBaseRegisterableType(typeof(T));
                if (!IndexSystem.Instance.registeredNames.TryGetValue(baseType, out var regList)) {
                    regList = new Dictionary<QualifiedName, RegisteredTemplate>();
                    IndexSystem.Instance.registeredNames[baseType] = regList;
                }
                if (regList.ContainsKey(qualifiedName))
                    throw new ArgumentException("Already registered", "qualifiedName");

                var regObj = new RegisteredTemplate(qualifiedName, baseType, obj);
                regList[qualifiedName] = regObj;
                IndexSystem.Instance.registeredTemplates[obj] = regObj;
                return obj;
            }
            public static IndexableTemplate GetRegisteredTemplate(Type type, QualifiedName qualifiedName) {
                type = IndexSystem.Instance.GetBaseRegisterableType(type);
                if (!IndexSystem.Instance.registeredNames.TryGetValue(type, out var regList) || !regList.TryGetValue(qualifiedName, out var obj))
                    throw new ArgumentException("Not registered", "qualifiedName");
                return obj.Template;
            }
            public static T GetRegisteredTemplate<T>(QualifiedName qualifiedName) where T : IndexableTemplate => (T)GetRegisteredTemplate(IndexSystem.Instance.GetBaseInstanceType(typeof(T)), qualifiedName);
            public static RegisteredTemplate GetRegisteredObject<T>(T obj) where T : IndexableTemplate {
                if (!IndexSystem.Instance.registeredTemplates.TryGetValue(obj, out var registered))
                    throw new ArgumentException("Not registered", "qualifiedName");
                return registered;
            }
        }
    }
}
