using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DialogMaker.Lib
{
    public static class Icons
    {
        public const string Add = "\uE710";
        public const string Close = "\uE711";
        public const string Delete = "\uE74D";
        public const string More = "\uE712";
        public const string Settings = "\uE713";
        public const string Question = "\uE897";
        public const string Unknown = "\uE9CE";
        public const string Warning = "\uE7BA";
        public const string Message = "\uE8BD";
        public const string Update = "\uE72C";
        public const string Play = "\uE768";
        public const string Pause = "\uE769";
        public const string Stop = "\uE71A";
        public const string OpenFolder = "\uE8DA";
        public const string OpenFile = "\uE8E5";
        public const string Copy = "\uE8C8";
        public const string Cut = "\uE8C6";
        public const string Paste = "\uE77F";
        public const string Edit = "\uE70F";
        public const string Create = "\uECC8";
        public const string Language = "\uF2B7";
        public const string Node = "\uE964";
        public const string RightArrow = "\uEBE7";
        public static FontFamily? Font
        {
            get
            {
                if (_font == null && App.TryFindResource<FontFamily>("SymbolThemeFontFamily", out var font))
                {
                    _font = font;
                }

                return _font;
            }
        }

        private static FontFamily? _font;

        public static TextBlock? CreateIconBlock(string? icon, TextBlock? textBlock)
        {
            if (icon == null)
            {
                return null;
            }

            var font = Font;

            if (font != null)
            {
                textBlock ??= new();
                textBlock.Text = icon;
                textBlock.FontFamily = font;

                return textBlock;
            }

            return null;
        }
        public static TextBlock? CreateIconBlock(string? icon)
        {
            return CreateIconBlock(icon, null);
        }
        public static bool TryCreateIconBlock(string? icon, TextBlock? textBlock, [NotNullWhen(true)] out TextBlock? result)
        {
            result = CreateIconBlock(icon, textBlock);
            return result != null;
        }
        public static bool TryCreateIconBlock(string? icon, [NotNullWhen(true)] out TextBlock? result)
        {
            return TryCreateIconBlock(icon, null, out result);
        }
    }
}
