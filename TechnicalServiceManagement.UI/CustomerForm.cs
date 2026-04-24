using TechnicalServiceManagement.Business.Managers;
using TechnicalServiceManagement.UI.UiHelpers;

namespace TechnicalServiceManagement.UI;

public sealed class CustomerForm : Form
{
    private readonly CustomerManager _customerManager = new();
    private readonly TextBox _fullNameTextBox = new();
    private readonly TextBox _phoneTextBox = new();
    private readonly TextBox _emailTextBox = new();
    private readonly DataGridView _customerGrid = FormStyles.CreateReadOnlyGrid();
    private bool _isNormalizingPhoneText;

    public CustomerForm()
    {
        FormStyles.ApplyBaseForm(this, "Customer Management");
        _phoneTextBox.MaxLength = 15;
        _phoneTextBox.KeyPress += PhoneTextBox_KeyPress;
        _phoneTextBox.TextChanged += (_, _) => NormalizePhoneText();

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 2,
            AutoScroll = true
        };

        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(FormStyles.CreateSection("Create Customer", CreateEntryLayout()), 0, 0);
        root.Controls.Add(FormStyles.CreateSection("Registered Customers", _customerGrid), 0, 1);

        Controls.Add(root);
        Load += (_, _) => RefreshCustomers();
    }

    private Control CreateEntryLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 4
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddLabeledControl(layout, 0, "Full Name", _fullNameTextBox);
        AddLabeledControl(layout, 1, "Phone", _phoneTextBox);
        AddLabeledControl(layout, 2, "Email", _emailTextBox);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };

        buttonRow.Controls.Add(FormStyles.CreatePrimaryButton("Save Customer", (_, _) => SaveCustomer()));
        buttonRow.Controls.Add(FormStyles.CreateSecondaryButton("Refresh List", (_, _) => RefreshCustomers()));

        layout.Controls.Add(new Label(), 0, 3);
        layout.Controls.Add(buttonRow, 1, 3);
        return layout;
    }

    private static void AddLabeledControl(TableLayoutPanel layout, int rowIndex, string labelText, Control control)
    {
        var label = new Label
        {
            Text = labelText,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 6, 12, 6)
        };

        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 6, 0, 6);

        layout.Controls.Add(label, 0, rowIndex);
        layout.Controls.Add(control, 1, rowIndex);
    }

    private void SaveCustomer()
    {
        try
        {
            _customerManager.CreateCustomer(
                _fullNameTextBox.Text,
                _phoneTextBox.Text,
                _emailTextBox.Text);

            _fullNameTextBox.Clear();
            _phoneTextBox.Clear();
            _emailTextBox.Clear();
            RefreshCustomers();
        }
        catch (Exception exception)
        {
            ShowSafeError(exception, "Customer Save Error");
        }
    }

    private static void ShowSafeError(Exception exception, string title)
    {
        if (exception is InvalidOperationException)
        {
            MessageBox.Show(exception.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[{title}] {exception}");
            MessageBox.Show("An unexpected error occurred. Please try again.", title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RefreshCustomers()
    {
        _customerGrid.DataSource = _customerManager.GetCustomers()
            .Select(customer => new
            {
                customer.Id,
                customer.FullName,
                customer.Phone,
                customer.Email,
                RegisteredAt = customer.CreatedAt.ToString("g")
            })
            .ToList();
    }

    private void PhoneTextBox_KeyPress(object? sender, KeyPressEventArgs eventArgs)
    {
        if (!char.IsControl(eventArgs.KeyChar) && !char.IsDigit(eventArgs.KeyChar))
        {
            eventArgs.Handled = true;
        }
    }

    private void NormalizePhoneText()
    {
        if (_isNormalizingPhoneText)
        {
            return;
        }

        var digitsOnly = new string(_phoneTextBox.Text.Where(char.IsDigit).ToArray());
        if (digitsOnly == _phoneTextBox.Text)
        {
            return;
        }

        _isNormalizingPhoneText = true;
        var cursorPosition = Math.Min(digitsOnly.Length, _phoneTextBox.SelectionStart);
        _phoneTextBox.Text = digitsOnly;
        _phoneTextBox.SelectionStart = cursorPosition;
        _isNormalizingPhoneText = false;
    }
}
