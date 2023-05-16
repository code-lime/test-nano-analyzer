using System.Windows.Forms;

namespace NanoAnalyzer.UI;

public static class InputBox
{
    public static DialogResult Show(string title, ref string input, IWin32Window? owner = null)
    {
        System.Drawing.Size size = new System.Drawing.Size(200, 70);
        TextBox textBox = new TextBox
        {
            Size = new System.Drawing.Size(size.Width - 10, 23),
            Location = new System.Drawing.Point(5, 5),
            Text = input
        };
        Button okButton = new Button
        {
            DialogResult = DialogResult.OK,
            Name = "okButton",
            Size = new System.Drawing.Size(75, 23),
            Text = "&OK",
            Location = new System.Drawing.Point(size.Width - 80 - 80, 39)
        };
        Button cancelButton = new Button
        {
            DialogResult = DialogResult.Cancel,
            Name = "cancelButton",
            Size = new System.Drawing.Size(75, 23),
            Text = "&Cancel",
            Location = new System.Drawing.Point(size.Width - 80, 39)
        };
        Form inputBox = new Form
        {
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = size,
            Text = title,
            Controls =
            {
                textBox,
                okButton,
                cancelButton
            },
            AcceptButton = okButton,
            CancelButton = cancelButton
        };

        DialogResult result = inputBox.ShowDialog(owner);
        input = textBox.Text;
        return result;
    }
    public static DialogResult Show(string title, string input, out string output, IWin32Window? owner = null)
    {
        System.Drawing.Size size = new System.Drawing.Size(200, 70);
        TextBox textBox = new TextBox
        {
            Size = new System.Drawing.Size(size.Width - 10, 23),
            Location = new System.Drawing.Point(5, 5),
            Text = input
        };
        Button okButton = new Button
        {
            DialogResult = DialogResult.OK,
            Name = "okButton",
            Size = new System.Drawing.Size(75, 23),
            Text = "&OK",
            Location = new System.Drawing.Point(size.Width - 80 - 80, 39)
        };
        Button cancelButton = new Button
        {
            DialogResult = DialogResult.Cancel,
            Name = "cancelButton",
            Size = new System.Drawing.Size(75, 23),
            Text = "&Cancel",
            Location = new System.Drawing.Point(size.Width - 80, 39)
        };
        Form inputBox = new Form
        {
            FormBorderStyle = FormBorderStyle.FixedDialog,
            ClientSize = size,
            Text = title,
            Controls =
            {
                textBox,
                okButton,
                cancelButton
            },
            AcceptButton = okButton,
            CancelButton = cancelButton
        };

        DialogResult result = inputBox.ShowDialog(owner);
        output = textBox.Text;
        return result;
    }
}
