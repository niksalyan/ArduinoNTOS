using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
