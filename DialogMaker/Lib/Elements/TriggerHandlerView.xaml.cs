using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows;
using Trigger = DialogMaker.Core.Executioning.Trigger;
using Acly.Tokens;
using System.Text;

namespace DialogMaker.Lib.Elements
{
    public partial class TriggerHandlerView : UserControl
    {
        public TriggerHandlerView()
        {
            InitializeComponent();
            _outputsList.ItemsSource = _outputs;
        }

        private readonly ObservableCollection<TriggerOutput> _outputs = [];
        private Token? _currentTriggerToken;

        #region Управление

        public async Task Handle(Trigger trigger, CancellationToken cancellationToken)
        {
            Token token = new();
            _currentTriggerToken = token;
            StringBuilder builder = new();
            builder.AppendLine(trigger.Id);

            foreach (var input in trigger.Parameters)
            {
                builder.AppendLine($"{input.Key}: {input.Value}");
            }

            _id.Text = builder.ToString().Trim();
            _outputs.Clear();

            foreach (var output in trigger.OutputKeys)
            {
                _outputs.Add(new(output));
            } 

            while (_currentTriggerToken == token)
            {
                if(cancellationToken.IsCancellationRequested)
                {
                    _currentTriggerToken = null;
                    return;
                }

                await Task.Delay(50);
            }

            try
            {
                foreach (var output in _outputs)
                {
                    trigger.SetOutput(output.Id, new(output.Value));
                }
            }
            catch (Exception error)
            {
                error.Alert();
            }
        }

        #endregion

        #region События

        private void OnConfirmButtonClicked(object sender, RoutedEventArgs e)
        {
            _currentTriggerToken = null;
        }

        #endregion

        #region Классы

        private class TriggerOutput(string id)
        {
            public string Id { get; } = id;
            public object? Value { get; set; }
        }

        #endregion
    }
}
