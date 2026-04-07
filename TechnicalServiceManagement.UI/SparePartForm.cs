using TechnicalServiceManagement.Business.Managers;
using TechnicalServiceManagement.UI.UiHelpers;

namespace TechnicalServiceManagement.UI;

public sealed class SparePartForm : Form
{
    private readonly SparePartManager _sparePartManager = new();
    private readonly TextBox _partNameTextBox = new();
    private readonly TextBox _stockCodeTextBox = new();
    private readonly NumericUpDown _unitPriceInput = new()
    {
        DecimalPlaces = 2,
        Maximum = 500000,
        Increment = 10
    };
    private readonly NumericUpDown _quantityInput = new()
    {
        Minimum = 1,
        Maximum = 10000,
        Value = 1
    };
    private readonly DataGridView _partsGrid = FormStyles.CreateReadOnlyGrid();

    public SparePartForm()
    {
        FormStyles.ApplyBaseForm(this, "Spare Parts Inventory");

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

        root.Controls.Add(FormStyles.CreateSection("Add or Refill Spare Part", CreateEntryLayout()), 0, 0);
        root.Controls.Add(FormStyles.CreateSection("Current Inventory", _partsGrid), 0, 1);

        Controls.Add(root);
        Load += (_, _) => RefreshParts();
    }

    private Control CreateEntryLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 5
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 140));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        AddLabeledControl(layout, 0, "Part Name", _partNameTextBox);
        AddLabeledControl(layout, 1, "Stock Code", _stockCodeTextBox);
        AddLabeledControl(layout, 2, "Unit Price", _unitPriceInput);
        AddLabeledControl(layout, 3, "Quantity", _quantityInput);

        var buttonRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 0)
        };

        buttonRow.Controls.Add(FormStyles.CreatePrimaryButton("Save Stock", (_, _) => SavePart()));
        buttonRow.Controls.Add(FormStyles.CreateSecondaryButton("Refresh List", (_, _) => RefreshParts()));

        layout.Controls.Add(new Label(), 0, 4);
        layout.Controls.Add(buttonRow, 1, 4);
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

    private void SavePart()
    {
        try
        {
            _sparePartManager.AddOrUpdateStock(
                _partNameTextBox.Text,
                _stockCodeTextBox.Text,
                _unitPriceInput.Value,
                (int)_quantityInput.Value);

            _partNameTextBox.Clear();
            _stockCodeTextBox.Clear();
            _unitPriceInput.Value = 0;
            _quantityInput.Value = 1;
            RefreshParts();
            MessageBox.Show("Spare part saved successfully.", "Inventory", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception exception)
        {
            MessageBox.Show(exception.Message, "Inventory Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RefreshParts()
    {
        _partsGrid.DataSource = _sparePartManager.GetSpareParts()
            .Select(part => new
            {
                part.Id,
                part.Name,
                part.StockCode,
                part.UnitPrice,
                part.StockQuantity,
                StockState = part.StockQuantity <= 3 ? "Low Stock" : "Available"
            })
            .ToList();
    }
}
