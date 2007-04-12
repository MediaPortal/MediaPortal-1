using System.Windows.Controls;

namespace MCEControls
{
    internal static class TextBoxHelper
    {
        // This method is shared by SoftKeyboardTextBox and SoftKeyboard
        internal static bool RemoveOneChar(TextBox textbox)
        {
            if (textbox != null && !textbox.IsReadOnly)
            {
                int index = textbox.CaretIndex;

                if (textbox.SelectionLength > 0)
                {
                    textbox.Text = textbox.Text.Remove(textbox.SelectionStart, textbox.SelectionLength);
                    textbox.CaretIndex = index;
                    
                    return true;
                }
                else
                {
                    --index;

                    if (index >= 0)
                    {
                        textbox.Text = textbox.Text.Remove(index, 1);
                        textbox.CaretIndex = index;

                        return true;
                    }
                }
            }

            return false;
        }
    }
}
