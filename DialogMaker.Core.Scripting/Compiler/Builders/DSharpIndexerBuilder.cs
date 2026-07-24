using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    public class DSharpIndexerBuilder(DSharpAssemblyBuilder assembly, DSharpTypeBuilder declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpPropertyBuilder(assembly, declaringType, name, metadataToken), IDSharpIndexerInfo
    {
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
