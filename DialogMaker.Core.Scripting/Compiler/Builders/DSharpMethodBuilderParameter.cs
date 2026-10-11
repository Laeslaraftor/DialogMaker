using DialogMaker.Core.Scripting.Runtime;
using System.ComponentModel;

namespace DialogMaker.Core.Scripting.Compiler.Builders
{
    /// <summary>
    /// Builder for calling parameter
    /// </summary>
    /// <param name="assembly">Assembly that contains calling member</param>
    public class DSharpMethodBuilderParameter(DSharpAssemblyBuilder assembly) : IDSharpParameterInfo, INotifyPropertyChanged
    {
        public DSharpMethodBuilderParameter(DSharpMethodBuilderParameter other)
            : this(other.Assembly)
        {
            Name = other.Name;
            Type = other.Type;
            Mode = other.Mode;
            TypeGetter = other.TypeGetter;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Assembly that contains calling member
        /// </summary>
        public DSharpAssemblyBuilder Assembly { get; } = assembly;
        public string? Name
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        public DSharpTypeToken? Type
        {
            get
            {
                if (field != null)
                {
                    return field;
                }

                return TypeGetter?.Invoke();
            }
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Type));
                }
            }
        }
        public DSharpMethodParameterMode Mode
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(Mode));
                }
            }
        }
        public Func<DSharpTypeToken?>? TypeGetter
        {
            get;
            set
            {
                if (field != value)
                {
                    field = value;
                    OnPropertyChanged(nameof(TypeGetter));
                }
            }
        }

        string IDSharpParameterInfo.Name => Name ?? string.Empty;
        IDSharpType IDSharpParameterInfo.Type
        {
            get
            {
                if (Type == null)
                {
                    throw new InvalidOperationException("Parameter type was not specified");
                }

                return (IDSharpType)Assembly.GetType(Type);
            }
        }

        #region Controls

        public override string ToString()
        {
            if (Type == null)
            {
                return Name ?? string.Empty;
            }

            return $"{Assembly.GetType(Type)} {Name}";
        }

        #endregion

        #region Events

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, ObservableObject.GetPropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
