using DialogMaker.Core;
using DialogMaker.Core.Common;
using DialogMaker.Core.Editor;
using DialogMaker.Core.Editor.Nodes;
using DialogMaker.Editor.Menus;
using DialogMaker.Lib.Controllers;
using DialogMaker.Lib.Elements;
using System.Collections.ObjectModel;
using System.Reflection;

namespace DialogMaker.Editor
{
    public class ProjectVariable : ProjectResourceItem<DialogProjectVariable>, IVariable
    {
        public ProjectVariable(ProjectController project, DialogProjectVariable original) : base(project, original)
        {
            ValueType = (DialogNodePortType)original.Type;
        }

        public DialogNodePortType ValueType { get; }
        public bool IsReadOnly => Original.IsReadOnly;
        public OperandValue Value
        {
            get => ((IVariable)Original).Value;
            set => ((IVariable)Original).Value = value;
        }

        private readonly ElementsPool<VariableView> _views = new();

        #region Управление

        public override bool ContainsValue(string value)
        {
            if (base.ContainsValue(value))
            {
                return true;
            }
            if (Original.Value is string textValue)
            {
                return textValue.Contains(value, StringComparison.InvariantCultureIgnoreCase);
            }

            return false;
        }

        public PropertyEditorController? CreateInputField()
        {
            PropertyEditorController.TryCreate(Original, ValueProperty, Original.ValueType, out var result);
            return result;
        }
        public override object? GetPreview()
        {
            var view = _views.GetElement();
            view.Variable = this;

            return view;
        }
        public override void FreePreview(object? preview)
        {
            if (preview is VariableView view && _views.Free(view))
            {
                view.Variable = null;
            }
        }

        public override ItemContextMenu CreateContextMenu()
        {
            return new VariableContextMenu(this);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _views.Dispose();
        }

        #endregion

        #region Статика

        public static ReadOnlyDictionary<DialogVariableType, string> TypeNames
        {
            get
            {
                if (field == null)
                {
                    var values = Enum.GetValues<DialogVariableType>();
                    Dictionary<DialogVariableType, string> names = new(values.Length);

                    foreach (var value in values)
                    {
                        var name = value.GetEnumAttribute<NameAttribute>()?.Name;
                        name ??= value.ToString();

                        names.Add(value, name);
                    }

                    field = new(names);
                }

                return field;
            }
        }
        public static PropertyInfo ValueProperty
        {
            get
            {
                field ??= typeof(DialogProjectVariable).GetProperty("Value");

                if (field == null)
                {
                    throw new InvalidProgramException("Не удалось получить поле значения");
                }

                return field;
            }
        }

        #endregion
    }
}
