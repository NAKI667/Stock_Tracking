using TechnicalServiceManagement.Business.Managers;
using TechnicalServiceManagement.Business.Models;
using TechnicalServiceManagement.UI.UiHelpers;

namespace TechnicalServiceManagement.UI;

public sealed class ServiceRequestForm : Form
{
    private readonly CustomerManager _customerManager = new();
    private readonly ServiceRequestManager _serviceRequestManager = new();
    private readonly SparePartManager _sparePartManager = new();

    private readonly ComboBox _customerComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _brandTextBox = new();
    private readonly TextBox _modelTextBox = new();
    private readonly TextBox _serialTextBox = new();
    private readonly TextBox _problemTextBox = new() { Multiline = true, Height = 70, ScrollBars = ScrollBars.Vertical };

    private readonly DataGridView _requestGrid = FormStyles.CreateReadOnlyGrid();
    private readonly ComboBox _statusComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _selectedRequestLabel = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
        ForeColor = Color.FromArgb(15, 23, 42)
    };
    private readonly Label _problemLabel = new()
    {
        AutoSize = false,
        Height = 48,
        Dock = DockStyle.Top,
        ForeColor = Color.FromArgb(71, 85, 105)
    };
    private readonly Label _totalCostLabel = new()
    {
        AutoSize = true,
        Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
        ForeColor = FormStyles.Accent
    };
    private readonly ListBox _operationsListBox = new() { Dock = DockStyle.Fill };
    private readonly ListBox _partUsageListBox = new() { Dock = DockStyle.Fill };

    private readonly TextBox _operationDescriptionTextBox = new();
    private readonly NumericUpDown _operationCostInput = new()
    {
        DecimalPlaces = 2,
        Maximum = 500000,
        Increment = 10
    };
    private readonly ComboBox _partComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _partQuantityInput = new()
    {
        Minimum = 1,
        Maximum = 500,
        Value = 1
    };

    private int _selectedRequestId;

    public ServiceRequestForm()
    {
        FormStyles.ApplyBaseForm(this, "Service Request Management");
        MinimumSize = new Size(1340, 760);
        _serialTextBox.CharacterCasing = CharacterCasing.Upper;
        _serialTextBox.MaxLength = 50;
        _operationDescriptionTextBox.Multiline = true;
        _operationDescriptionTextBox.Height = 72;
        _operationDescriptionTextBox.ScrollBars = ScrollBars.Vertical;
        _statusComboBox.Width = 170;
        _statusComboBox.MinimumSize = new Size(170, 0);

        _statusComboBox.DataSource = _serviceRequestManager.GetAvailableStatuses().ToList();
        _requestGrid.SelectionChanged += (_, _) => UpdateSelectedRequestFromGrid();

        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            SplitterDistance = 700,
            Panel1MinSize = 580,
            Panel2MinSize = 500
        };

        splitContainer.Panel1.Controls.Add(CreateLeftPanel());
        splitContainer.Panel2.Controls.Add(CreateRightPanel());

        Controls.Add(splitContainer);
        Load += (_, _) => RefreshPage();
    }

    private Control CreateLeftPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 2,
            AutoScroll = true
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(FormStyles.CreateSection("Create Service Request", CreateRequestEntryLayout()), 0, 0);
        layout.Controls.Add(FormStyles.CreateSection("Service Request List", _requestGrid), 0, 1);

        return layout;
    }

    private Control CreateRightPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(0, 20, 20, 20),
            ColumnCount = 1,
            RowCount = 4,
            AutoScroll = true
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(FormStyles.CreateSection("Selected Request Details", CreateDetailsSection()), 0, 0);
        layout.Controls.Add(FormStyles.CreateSection("Add Service Operation", CreateOperationEntryLayout()), 0, 1);
        layout.Controls.Add(FormStyles.CreateSection("Assign Spare Part", CreatePartUsageEntryLayout()), 0, 2);
        layout.Controls.Add(FormStyles.CreateSection("Operations and Part Usage", CreateHistorySection()), 0, 3);

        return layout;
    }

    private Control CreateRequestEntryLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 6
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddLabeledControl(layout, 0, "Customer", _customerComboBox);
        AddLabeledControl(layout, 1, "Device Brand", _brandTextBox);
        AddLabeledControl(layout, 2, "Device Model", _modelTextBox);
        AddLabeledControl(layout, 3, "Serial Number", _serialTextBox);
        AddLabeledControl(layout, 4, "Problem Description", _problemTextBox);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };

        buttonRow.Controls.Add(FormStyles.CreatePrimaryButton("Create Request", (_, _) => CreateRequest()));
        buttonRow.Controls.Add(FormStyles.CreateSecondaryButton("Reload Data", (_, _) => RefreshPage(_selectedRequestId)));

        layout.Controls.Add(new Label(), 0, 5);
        layout.Controls.Add(buttonRow, 1, 5);
        return layout;
    }

    private Control CreateDetailsSection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 4
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

        var statusPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false
        };

        statusPanel.Controls.Add(_statusComboBox);
        statusPanel.Controls.Add(FormStyles.CreatePrimaryButton("Update Status", (_, _) => UpdateStatus()));

        layout.Controls.Add(_selectedRequestLabel, 0, 0);
        layout.SetColumnSpan(_selectedRequestLabel, 2);
        layout.Controls.Add(_problemLabel, 0, 1);
        layout.SetColumnSpan(_problemLabel, 2);
        layout.Controls.Add(_totalCostLabel, 0, 2);
        layout.Controls.Add(statusPanel, 1, 2);
        return layout;
    }

    private Control CreateHistorySection()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

        layout.Controls.Add(FormStyles.CreateSection("Operation Log", _operationsListBox), 0, 0);
        layout.Controls.Add(FormStyles.CreateSection("Used Parts", _partUsageListBox), 1, 0);
        return layout;
    }

    private Control CreateOperationEntryLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddLabeledControl(layout, 0, "Description", _operationDescriptionTextBox);
        AddLabeledControl(layout, 1, "Cost", _operationCostInput);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0),
            WrapContents = false
        };

        buttonRow.Controls.Add(FormStyles.CreatePrimaryButton("Add Operation", (_, _) => AddOperation()));
        layout.Controls.Add(new Label(), 0, 2);
        layout.Controls.Add(buttonRow, 1, 2);
        return layout;
    }

    private Control CreatePartUsageEntryLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddLabeledControl(layout, 0, "Spare Part", _partComboBox);
        AddLabeledControl(layout, 1, "Quantity", _partQuantityInput);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0),
            WrapContents = false
        };

        buttonRow.Controls.Add(FormStyles.CreatePrimaryButton("Assign Part", (_, _) => AddPartUsage()));
        layout.Controls.Add(new Label(), 0, 2);
        layout.Controls.Add(buttonRow, 1, 2);
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

    private void RefreshPage(int selectedRequestId = 0)
    {
        LoadCustomers();
        LoadParts();
        LoadRequests(selectedRequestId);
    }

    private void LoadCustomers()
    {
        var customers = _customerManager.GetCustomers()
            .Select(customer => new CustomerOption(
                customer.Id,
                $"{customer.FullName} | {customer.Phone}"))
            .ToList();

        _customerComboBox.DataSource = customers;
        _customerComboBox.DisplayMember = nameof(CustomerOption.DisplayText);
        _customerComboBox.ValueMember = nameof(CustomerOption.Id);
    }

    private void LoadParts()
    {
        var parts = _sparePartManager.GetAvailableParts()
            .Select(part => new SparePartOption(
                part.Id,
                $"{part.Name} | {part.StockCode} | Stock: {part.StockQuantity}"))
            .ToList();

        _partComboBox.DataSource = parts;
        _partComboBox.DisplayMember = nameof(SparePartOption.DisplayText);
        _partComboBox.ValueMember = nameof(SparePartOption.Id);
    }

    private void LoadRequests(int selectedRequestId)
    {
        var requests = _serviceRequestManager.GetServiceRequests().ToList();
        _requestGrid.DataSource = requests;

        if (requests.Count == 0)
        {
            ResetSelectedRequestPanel();
            return;
        }

        var requestIdToSelect = selectedRequestId != 0
            ? selectedRequestId
            : requests[0].Id;

        foreach (DataGridViewRow row in _requestGrid.Rows)
        {
            if (row.DataBoundItem is ServiceRequestListItem item && item.Id == requestIdToSelect)
            {
                row.Selected = true;
                _requestGrid.CurrentCell = row.Cells[0];
                break;
            }
        }

        UpdateSelectedRequestFromGrid();
    }

    private void UpdateSelectedRequestFromGrid()
    {
        if (_requestGrid.CurrentRow?.DataBoundItem is not ServiceRequestListItem request)
        {
            return;
        }

        try
        {
            _selectedRequestId = request.Id;
            var details = _serviceRequestManager.GetRequestDetails(_selectedRequestId);

            _selectedRequestLabel.Text = $"Request #{details.Id} | {details.CustomerName} | {details.DeviceName}";
            _problemLabel.Text = $"Problem: {details.ProblemDescription}";
            _totalCostLabel.Text = $"Current Total Cost: {details.TotalCost:C}";
            _statusComboBox.SelectedItem = details.Status;
            _operationsListBox.DataSource = details.Operations.ToList();
            _partUsageListBox.DataSource = details.PartUsages.ToList();
        }
        catch (Exception exception)
        {
            ResetSelectedRequestPanel();
            ShowSafeError(exception, "Detail Load Error");
        }
    }

    private void ResetSelectedRequestPanel()
    {
        _selectedRequestId = 0;
        _selectedRequestLabel.Text = "No service request selected.";
        _problemLabel.Text = "Problem: -";
        _totalCostLabel.Text = "Current Total Cost: -";
        _operationsListBox.DataSource = null;
        _partUsageListBox.DataSource = null;
    }

    private void CreateRequest()
    {
        try
        {
            if (_customerComboBox.SelectedValue is not int customerId)
            {
                throw new InvalidOperationException("Add a customer before creating a service request.");
            }

            var request = _serviceRequestManager.CreateServiceRequest(
                customerId,
                _brandTextBox.Text,
                _modelTextBox.Text,
                _serialTextBox.Text,
                _problemTextBox.Text);

            _brandTextBox.Clear();
            _modelTextBox.Clear();
            _serialTextBox.Clear();
            _problemTextBox.Clear();

            RefreshPage(request.Id);
        }
        catch (Exception exception)
        {
            ShowSafeError(exception, "Request Create Error");
        }
    }

    private void UpdateStatus()
    {
        try
        {
            if (_selectedRequestId == 0)
            {
                throw new InvalidOperationException("Select a request first.");
            }

            if (_statusComboBox.SelectedItem is not string statusText)
            {
                throw new InvalidOperationException("Select a status.");
            }

            _serviceRequestManager.UpdateStatus(_selectedRequestId, statusText);
            RefreshPage(_selectedRequestId);
        }
        catch (Exception exception)
        {
            ShowSafeError(exception, "Status Update Error");
        }
    }

    private void AddOperation()
    {
        try
        {
            _serviceRequestManager.AddOperation(
                _selectedRequestId,
                _operationDescriptionTextBox.Text,
                _operationCostInput.Value);

            _operationDescriptionTextBox.Clear();
            _operationCostInput.Value = 0;
            RefreshPage(_selectedRequestId);
        }
        catch (Exception exception)
        {
            ShowSafeError(exception, "Operation Error");
        }
    }

    private void AddPartUsage()
    {
        try
        {
            if (_partComboBox.SelectedValue is not int partId)
            {
                throw new InvalidOperationException("Add spare parts to inventory before using them.");
            }

            _serviceRequestManager.AddPartUsage(
                _selectedRequestId,
                partId,
                (int)_partQuantityInput.Value);

            _partQuantityInput.Value = 1;
            RefreshPage(_selectedRequestId);
        }
        catch (Exception exception)
        {
            ShowSafeError(exception, "Part Usage Error");
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

    private sealed record CustomerOption(int Id, string DisplayText);
    private sealed record SparePartOption(int Id, string DisplayText);
}
