using System.Text;
using System.Windows.Forms;

namespace HartUI.Controls.Internal
{
    internal partial class DigitsOnlyFormattedTextBox : TextBox
    {
        public DigitsOnlyFormattedTextBox()
        {
            InitializeComponent();
        }

        // Can be extended into a general formatted textbox later
        private const int MaxDigits = 19;
        private const int WM_PASTE = 0x0302;

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                base.OnKeyPress(e);
                return;
            }

            if (!char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
                return;
            }

            ReplaceSelection(e.KeyChar.ToString());
            e.Handled = true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_PASTE)
            {
                ReplaceSelection(Clipboard.GetText());
                return;
            }

            base.WndProc(ref m);
        }

        private void ReplaceSelection(string input)
        {
            string currentDigits = ExtractDigits(Text);

            int digitStart = CountDigits(Text, SelectionStart);
            int digitEnd = CountDigits(Text, SelectionStart + SelectionLength);

            string insertedDigits = ExtractDigits(input);

            string newDigits =
                currentDigits.Substring(0, digitStart) +
                insertedDigits +
                currentDigits.Substring(digitEnd);

            if (newDigits.Length > MaxDigits)
                newDigits = newDigits.Substring(0, MaxDigits);

            Text = FormatDigits(newDigits);

            SelectionStart = CaretFromDigitIndex(Text, digitStart + insertedDigits.Length);
            SelectionLength = 0;
        }

        private static string ExtractDigits(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var sb = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                if (char.IsDigit(c))
                {
                    sb.Append(c);

                    if (sb.Length == MaxDigits)
                        break;
                }
            }

            return sb.ToString();
        }

        private static string FormatDigits(string digits)
        {
            if (digits.Length <= 4)
                return digits;

            var sb = new StringBuilder(digits.Length + digits.Length / 4);

            for (int i = 0; i < digits.Length; i++)
            {
                if (i > 0 && i % 4 == 0)
                    sb.Append(' ');

                sb.Append(digits[i]);
            }

            return sb.ToString();
        }

        private static int CountDigits(string text, int charIndex)
        {
            int count = 0;

            for (int i = 0; i < charIndex && i < text.Length; i++)
            {
                if (char.IsDigit(text[i]))
                    count++;
            }

            return count;
        }

        private static int CaretFromDigitIndex(string formatted, int digitIndex)
        {
            int digitsSeen = 0;

            for (int i = 0; i < formatted.Length; i++)
            {
                if (!char.IsDigit(formatted[i]))
                    continue;

                if (digitsSeen == digitIndex)
                    return i;

                digitsSeen++;
            }

            return formatted.Length;
        }
    }
}
