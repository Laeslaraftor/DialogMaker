namespace DialogMaker.Core.Editor
{
    public abstract class DialogProjectVariable<T> : DialogProjectVariable
    {
        protected DialogProjectVariable(DialogProjectResources resources) : base(resources)
        {
        }
        protected DialogProjectVariable(DialogProjectResources resources, DialogProjectVariableSavedState savedState) : base(resources, savedState)
        {
        }

        public new T? Value
        {
            get
            {
                if (base.Value == null)
                {
                    return default;
                }

                return (T)base.Value;
            }
            set => base.Value = value;
        }
    }
}
