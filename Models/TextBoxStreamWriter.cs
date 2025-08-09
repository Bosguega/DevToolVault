using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace DevToolVault.Views
{
    public class TextBoxStreamWriter : TextWriter
    {
        private readonly TextBox _textBox;
        private readonly StringBuilder _builder = new StringBuilder();

        public TextBoxStreamWriter(TextBox textBox)
        {
            _textBox = textBox;
        }

        public override void Write(char value)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value.ToString());
                _textBox.ScrollToEnd();
            });
        }

        public override void Write(string value)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value);
                _textBox.ScrollToEnd();
            });
        }

        public override void WriteLine(string value)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value + Environment.NewLine);
                _textBox.ScrollToEnd();
            });
        }

        public override Encoding Encoding => Encoding.UTF8;
    }
}