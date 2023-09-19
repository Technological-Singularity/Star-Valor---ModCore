namespace Charon.StarValor.ModCore {
    public abstract class IndexableTemplate {
        public abstract IIndexableInstance GenerateInstance(int id, object data);

        public void Apply(IIndexableInstance instance, object data) {
            instance.Template?.OnRemoving(instance, data);
            instance.Data.Template = this;
            instance.Template.OnApplying(instance, data);
        }
        public virtual void OnRemoving(IIndexableInstance instance, object data) { }
        public virtual void OnApplying(IIndexableInstance instance, object data) { }

        public virtual void VerifyComponents(IIndexableInstance instance) { }
        public virtual int GetHashCode(HashContext context) => this.GetHashCode();

        public IIndexableInstance Allocate(object data) => IndexSystem.Instance.Allocate(this, data);
    }
}
