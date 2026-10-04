namespace NTOSEmulator.Libs
{
    public class Dialogs
    {
        public static void Alert(string line1, string line2)
        {
            TaskDialog.ShowDialog(new TaskDialogPage
            {
                Caption = "Emulator",
                Heading = line1,
                Text = line2,
                Icon = TaskDialogIcon.Information,
                Buttons =
                {
                    TaskDialogButton.OK
                }
            });
        }
        public static bool Confirm(string message)
        {
            var result = TaskDialog.ShowDialog(new TaskDialogPage
            {
                Caption = "Emulator",
                Text = message,
                Icon = TaskDialogIcon.Warning,
                Buttons =
                {
                    TaskDialogButton.Cancel,
                    TaskDialogButton.OK

                }
            });

            return result == TaskDialogButton.OK;
        }

        public static int ConfirmNumber(string line1, string line2, int from, int to)
        {
            var page = new TaskDialogPage
            {
                Caption = "Emulator",
                Heading = line1,
                Text = line2,
                Icon = TaskDialogIcon.Information
            };

            var buttons = new Dictionary<TaskDialogButton, int>();

            page.Buttons.Add(TaskDialogButton.Cancel);

            for (int i = from; i <= to; i++)
            {
                var button = new TaskDialogButton(i.ToString());
                page.Buttons.Add(button);
                buttons[button] = i;
            }


            var result = TaskDialog.ShowDialog(page);

            if (result == TaskDialogButton.Cancel)
                return -1;

            return buttons[result];
        }

        public static string EditText(
         string message,
         string defaultValue,
         int maxCharacters)
        {
            if (maxCharacters < 0)
                maxCharacters = 0;

            defaultValue ??= "";

            // Make sure the initial value already satisfies the rules.
            defaultValue = FilterAscii(defaultValue);

            if (defaultValue.Length > maxCharacters)
                defaultValue = defaultValue[..maxCharacters];

            using var form = new Form
            {
                Text = "Emulator",
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MinimizeBox = false,
                MaximizeBox = false,
                ShowInTaskbar = false,
                ClientSize = new Size(420, 135)
            };

            var label = new Label
            {
                AutoSize = false,
                Text = message,
                Location = new Point(12, 12),
                Size = new Size(396, 32)
            };

            var textBox = new TextBox
            {
                Location = new Point(12, 48),
                Width = 396,
                MaxLength = maxCharacters,
                Text = defaultValue,
                BorderStyle = BorderStyle.FixedSingle
            };

            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(252, 92),
                Width = 75
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(333, 92),
                Width = 75
            };

            textBox.KeyPress += (_, e) =>
            {
                // Allow control characters such as Backspace.
                if (char.IsControl(e.KeyChar))
                    return;

                // NTOS strings use printable ASCII only.
                if (e.KeyChar < 32 || e.KeyChar > 126)
                    e.Handled = true;
            };

            form.Controls.Add(label);
            form.Controls.Add(textBox);
            form.Controls.Add(okButton);
            form.Controls.Add(cancelButton);

            form.AcceptButton = okButton;
            form.CancelButton = cancelButton;

            form.Shown += (_, _) =>
            {
                textBox.Focus();
                textBox.SelectAll();
            };

            DialogResult result = form.ShowDialog();

            if (result != DialogResult.OK)
                return null;

            return textBox.Text;
        }

        private static string FilterAscii(string value)
        {
            var result = new System.Text.StringBuilder(value.Length);

            foreach (char c in value)
            {
                if (c >= 32 && c <= 126)
                    result.Append(c);
            }

            return result.ToString();
        }


    }
}
