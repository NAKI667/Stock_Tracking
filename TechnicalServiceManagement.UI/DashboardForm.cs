using TechnicalServiceManagement.Business.Managers;
using TechnicalServiceManagement.Business.Models;
using TechnicalServiceManagement.Data.Entities;
using TechnicalServiceManagement.UI.UiHelpers;

namespace TechnicalServiceManagement.UI;

public sealed class DashboardForm : Form
{
    private readonly ServiceRequestManager _serviceRequestManager = new();
    private readonly AuditService _auditService = new();
    private readonly Label _totalRequestsValue = CreateSummaryValueLabel();
    private readonly Label _activeRequestsValue = CreateSummaryValueLabel();
    private readonly Label _finishedRequestsValue = CreateSummaryValueLabel();
    private readonly Label _lowStockValue = CreateSummaryValueLabel();
    private readonly DataGridView _requestGrid = FormStyles.CreateReadOnlyGrid();
    private readonly DataGridView _auditGrid = FormStyles.CreateReadOnlyGrid();

    public DashboardForm()
    {
        FormStyles.ApplyBaseForm(this, "Technical Service Management Dashboard");
        MinimumSize = new Size(1180, 720);

        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 5
        };

        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

        rootLayout.Controls.Add(CreateHeader(), 0, 0);
        rootLayout.Controls.Add(CreateButtonRow(), 0, 1);
        rootLayout.Controls.Add(CreateSummaryRow(), 0, 2);
        rootLayout.Controls.Add(FormStyles.CreateSection("Latest Service Requests", _requestGrid), 0, 3);
        rootLayout.Controls.Add(FormStyles.CreateSection("Recent Activity Log", _auditGrid), 0, 4);

        Controls.Add(rootLayout);
        Load += (_, _) => RefreshDashboard();
    }

    private Control CreateHeader()
    {
        var panel = new Panel
        {
            Height = 78,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 0, 0, 12)
        };

        var title = new Label
        {
            Text = "Technical Service Management with Spare Parts Tracking",
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 20F, FontStyle.Bold),
            ForeColor = Color.FromArgb(15, 23, 42),
            Location = new Point(0, 0)
        };

        var subtitle = new Label
        {
            Text = "Manage customers, service requests, spare parts, and delivery workflow from one desktop application.",
            AutoSize = true,
            Font = new Font("Segoe UI", 10.5F),
            ForeColor = Color.FromArgb(71, 85, 105),
            Location = new Point(3, 42)
        };

        panel.Controls.Add(title);
        panel.Controls.Add(subtitle);
        return panel;
    }

    private Control CreateButtonRow()
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 54,
            Margin = new Padding(0, 0, 0, 18)
        };

        flow.Controls.Add(FormStyles.CreatePrimaryButton("Customers", (_, _) => OpenChildForm(new CustomerForm())));
        flow.Controls.Add(FormStyles.CreatePrimaryButton("Service Requests", (_, _) => OpenChildForm(new ServiceRequestForm())));
        flow.Controls.Add(FormStyles.CreatePrimaryButton("Spare Parts", (_, _) => OpenChildForm(new SparePartForm())));
        flow.Controls.Add(FormStyles.CreateSecondaryButton("Refresh Dashboard", (_, _) => RefreshDashboard()));
        return flow;
    }

    private Control CreateSummaryRow()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 4,
            RowCount = 1,
            Height = 132,
            Margin = new Padding(0, 0, 0, 18)
        };

        for (var index = 0; index < 4; index++)
        {
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }

        layout.Controls.Add(CreateSummaryCard("Total Requests", _totalRequestsValue, "All registered technical service records"), 0, 0);
        layout.Controls.Add(CreateSummaryCard("Active Jobs", _activeRequestsValue, "Received, in progress, or waiting for part"), 1, 0);
        layout.Controls.Add(CreateSummaryCard("Finished Jobs", _finishedRequestsValue, "Completed or delivered requests"), 2, 0);
        layout.Controls.Add(CreateSummaryCard("Low Stock Parts", _lowStockValue, "Parts with stock quantity less than or equal to 3"), 3, 0);

        return layout;
    }

    private static Control CreateSummaryCard(string title, Label valueLabel, string description)
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = FormStyles.CardBackground,
            Margin = new Padding(0, 0, 16, 0),
            Padding = new Padding(18)
        };

        var titleLabel = new Label
        {
            Text = title,
            AutoSize = true,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold),
            ForeColor = Color.FromArgb(51, 65, 85),
            Location = new Point(18, 16)
        };

        valueLabel.Location = new Point(18, 44);

        var descriptionLabel = new Label
        {
            Text = description,
            AutoSize = false,
            Width = 230,
            Height = 36,
            Font = new Font("Segoe UI", 9F),
            ForeColor = Color.FromArgb(100, 116, 139),
            Location = new Point(18, 84)
        };

        card.Controls.Add(titleLabel);
        card.Controls.Add(valueLabel);
        card.Controls.Add(descriptionLabel);
        return card;
    }

    private static Label CreateSummaryValueLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold),
            ForeColor = FormStyles.Accent
        };
    }

    private void OpenChildForm(Form form)
    {
        using (form)
        {
            form.ShowDialog(this);
        }

        RefreshDashboard();
    }

    private void RefreshDashboard()
    {
        try
        {
            DashboardSummary summary = _serviceRequestManager.GetDashboardSummary();

            _totalRequestsValue.Text = summary.TotalRequests.ToString();
            _activeRequestsValue.Text = summary.ActiveRequests.ToString();
            _finishedRequestsValue.Text = summary.FinishedRequests.ToString();
            _lowStockValue.Text = summary.LowStockParts.ToString();
            _requestGrid.DataSource = _serviceRequestManager.GetServiceRequests();

            _auditGrid.DataSource = _auditService.GetRecentLogs()
                .Select(log => new
                {
                    log.Timestamp,
                    log.Action,
                    Entity = log.EntityName,
                    log.Details
                })
                .ToList();
        }
        catch (Exception exception)
        {
            ShowSafeError(exception, "Dashboard Refresh Error");
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
}
