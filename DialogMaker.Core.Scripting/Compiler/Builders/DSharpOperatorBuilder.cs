using DialogMaker.Core.Scripting.Compiler.Ast;
using DialogMaker.Core.Scripting.Compiler.Lexer;
using DialogMaker.Core.Scripting.Runtime;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    /// <summary>
    /// Type operator builder
    /// </summary>
    /// <param name="declaringType">Type that declaring this operator</param>
    /// <param name="name">Name of operator</param>
    /// <param name="metadataToken">Member token</param>
    public class DSharpOperatorBuilder(DSharpTypeBuilder declaringType, string name, DSharpTypeToken metadataToken)
        : DSharpMemberInfoBuilder(declaringType.Assembly, name, metadataToken), IDSharpOperatorInfo
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override DSharpTypeBuilder DeclaringType { get; } = declaringType;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override bool IsDeclaration => false;
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DSharpTypeToken? ReturnType
        {
            get
            {
                if (field == null && ReturnTypeResolver != null)
                {
                    field = ReturnTypeResolver();
                    ReturnTypeResolver = null;
                }
                if (field == null && DeclaringType != null && OriginalOperator != null)
                {
                    field = GetReplacedType(OriginalOperator.ReturnType);
                }

                return field;
            }
            set;
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DSharpMethodBuilder Method
        {
            get
            {
                if (field == null)
                {
                    field = DeclaringType.CreateMethod(t => DSharpMethodBuilder.CreateOperator(this, t));
                    OnMethodCreated(field);
                }

                return field;
            }
        }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DSharpOperatorType Type { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DSharpBinaryOperator? BinaryOperator { get; set; }
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public DSharpUnaryOperator? UnaryOperator { get; set; }
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
        internal IDSharpOperatorInfo? OriginalOperator { get; set; }
        internal Func<DSharpTypeToken>? ReturnTypeResolver { get; set; }

        IDSharpMethodInfo IDSharpOperatorInfo.Method => Method;
        IDSharpType IDSharpOperatorInfo.ReturnType
        {
            get
            {
                if (ReturnType == null)
                {
                    throw new InvalidOperationException($"Can not get return type because it was not specified: {this}");
                }

                return (IDSharpType)Assembly.GetType(ReturnType);
            }
        }

        private bool _skipMethodCreateEvent;

        #region Управление

        internal override void Update()
        {
            base.Update();

            foreach (var parameter in Parameters)
            {
                _ = parameter.Type;
            }

            _ = Method;
            _ = ReturnType;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public IDSharpParameterInfo[] GetParameters() => [.. Parameters];
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns><inheritdoc/></returns>
        public override string ToString()
        {
            string result = string.Empty;

            if (ReturnType != null)
            {
                result += Assembly.GetType(ReturnType) + " ";
            }

            result += Type;

            if (BinaryOperator != null)
            {
                result += " " + (DSharpTokenType)BinaryOperator.Value;
            }
            else if (UnaryOperator != null)
            {
                result += " " + (DSharpTokenType)UnaryOperator.Value;
            }

            result += '(';
            bool isFirst = true;

            foreach (var parameter in Parameters)
            {
                if (!isFirst)
                {
                    result += ", ";
                }

                result += parameter;
                isFirst = false;
            }

            result += ')';

            return result;
        }

        private void CopyTo(IList<DSharpMethodBuilderParameter> source, IList<DSharpMethodBuilderParameter> destination)
        {
            while (destination.Count > source.Count)
            {
                destination.RemoveAt(destination.Count - 1);
            }

            for (int i = 0; i < source.Count; i++)
            {
                if (i >= destination.Count)
                {
                    destination.Add(source[i]);
                    continue;
                }

                destination[i] = source[i];
            }
        }

        #endregion

        #region События

        private void OnMethodCreated(DSharpMethodBuilder method)
        {
            if (!_skipMethodCreateEvent)
            {
                CopyTo(Parameters, method.Parameters);
            }
        }

        private void OnParametersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _skipMethodCreateEvent = true;

            if (Method != null)
            {
                CopyTo(Parameters, Method.Parameters);
            }
        }

        #endregion
    }
}
