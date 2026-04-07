namespace TechnicalServiceManagement.UI.UiHelpers;

internal static class FormStyles
{
    public static readonly Color PageBackground = Color.FromArgb(242, 245, 247);
    public static readonly Color CardBackground = Color.White;
    public static readonly Color Accent = Color.FromArgb(0, 102, 204);
    public static readonly Color AccentSoft = Color.FromArgb(226, 239, 255);

    public static void ApplyBaseForm(Form form, string title)
    {
        form.Text = title;
        form.BackColor = PageBackground;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimumSize = new Size(980, 620);
        form.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
    }

    public static Button CreatePrimaryButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Accent,
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat,
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(0, 0, 12, 0)
        };

        button.FlatAppearance.BorderSize = 0;
        button.Click += onClick;
        return button;
    }

    public static Button CreateSecondaryButton(string text, EventHandler onClick)
    {
        var button = new Button
        {
            Text = text,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = AccentSoft,
            ForeColor = Accent,
            FlatStyle = FlatStyle.Flat,
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(0, 0, 12, 0)
        };

        button.FlatAppearance.BorderColor = Accent;
        button.Click += onClick;
        return button;
    }

    public static DataGridView CreateReadOnlyGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = CardBackground,
            BorderStyle = BorderStyle.None,
            RowHeadersVisible = false
        };

        grid.DefaultCellStyle.SelectionBackColor = AccentSoft;
        grid.DefaultCellStyle.SelectionForeColor = Color.Black;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(235, 240, 246);
        return grid;
    }

    public static Panel CreateSection(string title, Control content)
    {
        var autoSizeContent = content.AutoSize;
        var panel = new Panel
        {
            BackColor = CardBackground,
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 0, 16),
            AutoSize = autoSizeContent,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            AutoSize = autoSizeContent,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(autoSizeContent
            ? new RowStyle(SizeType.AutoSize)
            : new RowStyle(SizeType.Percent, 100));

        var header = new Label
        {
            Text = title,
            Dock = DockStyle.Fill,
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold),
            ForeColor = Color.FromArgb(30, 41, 59),
            Margin = new Padding(0, 0, 0, 12)
        };

        content.Dock = autoSizeContent ? DockStyle.Top : DockStyle.Fill;
        content.Margin = Padding.Empty;

        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(content, 0, 1);
        panel.Controls.Add(layout);
        return panel;
    }
}
