//using System;

//namespace Charon.StarValor.ModCore {
//    public partial class IndexSystem {
//        class RegisteredTemplate {
//            public Type InstanceType { get; }
//            public IndexableTemplate Template { get; }
//            public RegisteredTemplate(IndexableTemplate template, Type instanceType) => (this.Template, this.InstanceType) = (template, instanceType);

//            public static RegisteredTemplate GetRegisteredTemplate(Type type) {
//                if (IndexSystem.Instance.registeredInstanceContainers.TryGetValue(type, out var reg))
//                    return reg;
//                throw new ArgumentException("Not registered", "type");
//            }

//            public InstanceContainer AllocateStatic(int id, object data) {
//                var resources = IndexSystem.Instance.GetRegisterableContainer(InstanceType);
//                var iobj = new InstanceContainer(Template, InstanceType, data);
//                iobj.Id = resources.AllocateStatic(id, iobj);
//                iobj.Ref();
//                return iobj;
//            }
//            public InstanceContainer Allocate(object data) {
//                var resources = IndexSystem.Instance.GetRegisterableContainer(InstanceType);
//                var iobj = new InstanceContainer(Template, InstanceType, data);
//                iobj.Id = resources.Allocate(iobj);
//                iobj.Ref();
//                return iobj;
//            }
//        }
//    }
//}
