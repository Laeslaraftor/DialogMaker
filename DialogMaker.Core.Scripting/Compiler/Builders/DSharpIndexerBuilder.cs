using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public class DSharpIndexerBuilder : DSharpPropertyBuilder, IDSharpIndexerInfo
    {
        public DSharpIndexerBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder declaringType, string name, DSharpTypeToken metadataToken)
            : base(assembly, declaringType, name, metadataToken)
        {
            _parametersValidator = DSharpParametersValidator.CreateWithSimpleModes();
            _parametersValidator.Parameters = Parameters;
        }
        ~DSharpIndexerBuilder()
        {
            _parametersValidator.Dispose();
        }

        public IList<DSharpMethodBuilderParameter> Parameters
        {
            get
            {
                if (field == null)
                {
                    ObservableCollection<DSharpMethodBuilderParameter> parameters = [];
                    parameters.CollectionChanged += OnParametersCollectionChanged;
                    field = parameters;
                }

                return field;
            }
        }

        int IDSharpCallableMember.ParametersCount => Parameters.Count;
        int IDSharpCallableMember.GenericParametersCount => 0;
        IDSharpType? IDSharpCallableMember.ReturnType => ((IDSharpPropertyInfo)this).PropertyType;
        bool IDSharpCallableMember.CanBeCalledDirectly => false;
        IDSharpCallableMember? IDSharpCallableMember.OverridenCallable => OverrideProperty as IDSharpCallableMember;

        private readonly DSharpParametersValidator _parametersValidator;

        #region Управление

        internal override void Update()
        {
            base.Update();

            foreach (var parameter in Parameters)
            {
                _ = parameter.Type;
            }
        }

        public IDSharpParameterInfo[] GetParameters() => [.. Parameters];
        IDSharpType[] IDSharpCallableMember.GetGenericParameters() => [];
        IEnumerable<IDSharpMethodInfo> IDSharpCallableMember.GetMethods()
        {
            if (Getter != null)
            {
                yield return Getter;
            }
            if (Setter != null)
            {
                yield return Setter;
            }
        }

        public override string ToString()
        {
            var result = base.ToString() + "[";
            bool isFirstParameter = true;

            foreach (var parameter in Parameters)
            {
                if (!isFirstParameter)
                {
                    result += ", ";
                }

                result += parameter;
                isFirstParameter = false;
            }

            result += "]";

            return result;
        }

        private void CopyTo(IList<DSharpMethodBuilderParameter> source, IList<DSharpMethodBuilderParameter> destination, bool saveFirst)
        {
            int startDestinationIndex = saveFirst ? 1 : 0;

            while (destination.Count - startDestinationIndex > source.Count)
            {
                destination.RemoveAt(destination.Count - 1);
            }

            for (int i = 0; i < source.Count; i++)
            {
                int destinationIndex = i + startDestinationIndex;

                if (destinationIndex >= destination.Count)
                {
                    destination.Add(source[i]);
                    continue;
                }

                destination[destinationIndex] = source[i];
            }
        }

        #endregion

        #region События

        protected override void OnGetterCreated(DSharpMethodBuilder method)
        {
            base.OnGetterCreated(method);
            CopyTo(Parameters, method.Parameters, false);
        }
        protected override void OnSetterCreated(DSharpMethodBuilder method)
        {
            base.OnSetterCreated(method);
            CopyTo(Parameters, method.Parameters, true);
        }

        private void OnParametersCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (Getter != null)
            {
                CopyTo(Parameters, Getter.Parameters, false);
            }
            if (Setter != null)
            {
                CopyTo(Parameters, Setter.Parameters, true);
            }
        }

        #endregion
    }
}
