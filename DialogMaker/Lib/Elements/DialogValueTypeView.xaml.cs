using DialogMaker.Core;
using DialogMaker.Core.Editor.Nodes;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DialogMaker.Lib.Elements
{
    public partial class DialogValueTypeView : UserControl
    {
        public DialogValueTypeView()
        {
            InitializeComponent();
        }

        public DialogNodePortType Type
        {
            get => (DialogNodePortType)GetValue(TypeProperty);
            set => SetValue(TypeProperty, value);
        }

        #region События

        private void OnTypeChanged(DialogNodePortType oldValue, DialogNodePortType newValue)
        {
            _typeName.Text = newValue.GetEnumAttribute<NameAttribute>()?.Name ?? newValue.ToString();
        }

        private static void OnTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DialogValueTypeView view) 
            {
                view.OnTypeChanged((DialogNodePortType)e.OldValue, (DialogNodePortType)e.NewValue);
            }
        }

        #endregion

        #region Dependency

        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(DialogNodePortType),
            typeof(DialogValueTypeView), new(DialogNodePortType.Object, OnTypeChanged));

        #endregion
    }
}
