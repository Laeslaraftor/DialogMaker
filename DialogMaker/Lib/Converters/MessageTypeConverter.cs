using DialogMaker.Core.Editor.Messages;
using System.Globalization;
using System.Windows.Data;

namespace DialogMaker.Lib.Converters
{
    [ValueConversion(typeof(MessageImportance), typeof(MessageType))]
    public class MessageTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MessageImportance importance)
            {
                return MessageType.Normal;
            }

            return (MessageType)importance;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MessageType type)
            {
                return MessageImportance.Normal;
            }

            return (MessageImportance)type;
        }
    }
}
