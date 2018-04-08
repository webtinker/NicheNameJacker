using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;

namespace NicheNameJacker.Controls
{
    public class NumericTextBox : TextBox
    {
        public NumericTextBox()
        {
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Space)
                {
                    e.Handled = true;
                }
            };

            PreviewTextInput += (s, e) =>
            {
                if (!e.Text.All(char.IsNumber))
                {
                    e.Handled = true;
                }
            };

            CommandManager.AddPreviewExecutedHandler(this, (s, e) =>
            {
                if (e.Command == ApplicationCommands.Copy ||
                    e.Command == ApplicationCommands.Cut ||
                    e.Command == ApplicationCommands.Paste)
                {
                    e.Handled = true;
                }
            });
        }
    }
}
