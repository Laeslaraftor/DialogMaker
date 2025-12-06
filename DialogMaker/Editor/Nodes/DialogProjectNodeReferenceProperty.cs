using Acly;
using DialogMaker.Core;
using DialogMaker.Core.Editor;
using DialogMaker.Lib;
using DialogMaker.Lib.Elements;
using System.Reflection;
using System.Windows;

namespace DialogMaker.Editor
{
    public class DialogProjectNodeReferenceProperty : DialogProjectNodeProperty
    {
        public DialogProjectNodeReferenceProperty(DialogProjectNode node, PropertyInfo property) : base(node, property)
        {
            ResourceType = property.GetReferenceType();
        }

        public DialogResourceType ResourceType { get; }

        private readonly ElementsPool<ReferenceView> _pool = new();
        private readonly List<ReferenceView> _views = [];

        #region Управление

        public override FrameworkElement GetView()
        {
            var view = _pool.GetElement();
            view.Placeholder = Name;
            view.ToolTip = Description;
            view.RequestedResourceType = ResourceType;

            if (_views.Count > 0)
            {
                view.Item = _views[0].Item;
            }
            else
            {
                try
                {
                    var obj = Resolve(Value);

                    if (obj != null)
                    {
                        view.Item = ProjectResourceItem.Create(Node.Project, obj);
                    }
                }
                catch (Exception error)
                {
                    error.Alert();
                }
            }
            _views.Add(view);
            view.ItemChanged += OnViewItemChanged;

            return view;
        }
        public override void FreeView(FrameworkElement element)
        {
            if (element is not ReferenceView view ||
                !_views.Remove(view))
            {
                return;
            }

            view.ItemChanged -= OnViewItemChanged;
            _pool.Free(view);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            _pool.Dispose();
        }

        #endregion

        #region События

        protected override void OnValueChanged()
        {
            base.OnValueChanged();

            var obj = Resolve(Value);
            ProjectResourceItem item;
            bool itemSetted = false;

            if (obj == null)
            {
                foreach (var view in _views)
                {
                    view.Item = null;
                }

                return;
            }

            try
            {
                item = ProjectResourceItem.Create(Node.Project, obj);
            }
            catch (Exception error)
            {
                error.Alert();
                return;
            }

            foreach (var view in _views)
            {
                if (view.Item?.Model != obj)
                {
                    view.Item = item;
                    itemSetted = true;
                }
            }

            if (!itemSetted)
            {
                item.Dispose();
            }
        }
        private void OnViewItemChanged(object? sender, ValueChangedEventArgs<ProjectResourceItem?> e)
        {
            var currentResource = Resolve(Value);

            if (currentResource != e.NewValue?.Model)
            {
                Value = e.NewValue?.Model;
            }
        }

        #endregion

        #region Статика

        private static DialogProjectResourceObject? Resolve(object? reference)
        {
            if (reference == null)
            {
                return null;
            }

            if (reference.GetType().TryFindMethod(nameof(Resolve), out var method))
            {
                var obj = method.Invoke(reference, []);

                if (obj is DialogProjectResourceObject resourceObject)
                {
                    return resourceObject;
                }
            }

            return null;
        }

        #endregion
    }
}
