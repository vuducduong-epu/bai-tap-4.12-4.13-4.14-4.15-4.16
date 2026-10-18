
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptoStockTicker
{
    public class frmCryptoStockTicker : Form
    {
        private Button btnStart;
        private Button btnPause;
        private Label lblBTCPrice;
        private DataGridView dgvHistoryLog;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblLastUpdate;
        private ToolStripStatusLabel lblSyncCount;

        private CancellationTokenSource? cts;
        private Task? monitoringTask;
        private decimal currentPrice = 65000m;
        private int syncCount = 0;
        private readonly List<PriceRecord> history = new List<PriceRecord>();
        private bool isClosing = false;

        public frmCryptoStockTicker()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Giám sát tỷ giá tài chính thời gian thực";
            Size = new Size(1000, 650);
            MinimumSize = new Size(800, 450);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.WhiteSmoke;

            Label lblTitle = new Label
            {
                Text = "CRYPTO STOCK TICKER - BTC/USD",
                Dock = DockStyle.Top,
                Height = 55,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Panel pnlControl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85
            };

            btnStart = new Button
            {
                Text = "Bắt đầu Giám sát",
                Location = new Point(25, 20),
                Size = new Size(180, 40),
                BackColor = Color.LightGreen
            };

            btnPause = new Button
            {
                Text = "Tạm dừng",
                Location = new Point(220, 20),
                Size = new Size(150, 40),
                BackColor = Color.LightCoral,
                Enabled = false
            };

            btnStart.Click += async (s, e) => await StartMonitoringAsync();
            btnPause.Click += (s, e) => PauseMonitoring();

            pnlControl.Controls.Add(btnStart);
            pnlControl.Controls.Add(btnPause);

            GroupBox grpPrice = new GroupBox
            {
                Text = "Tỷ giá hiện tại",
                Dock = DockStyle.Top,
                Height = 125,
                Padding = new Padding(10)
            };

            lblBTCPrice = new Label
            {
                Text = "BTC/USD: $65,000.00",
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                ForeColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter
            };

            grpPrice.Controls.Add(lblBTCPrice);

            GroupBox grpHistory = new GroupBox
            {
                Text = "Lịch sử biến động tỷ giá",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            dgvHistoryLog = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false
            };

            dgvHistoryLog.Columns.Add("Time", "Thời gian");
            dgvHistoryLog.Columns.Add("Price", "Giá BTC/USD");
            dgvHistoryLog.Columns.Add("Change", "Biến động");
            dgvHistoryLog.Columns.Add("Percent", "Tỷ lệ (%)");

            dgvHistoryLog.Columns["Price"]!.DefaultCellStyle.Format = "N2";
            dgvHistoryLog.Columns["Change"]!.DefaultCellStyle.Format = "+#,##0.00;-#,##0.00;0.00";
            dgvHistoryLog.Columns["Percent"]!.DefaultCellStyle.Format = "+0.00;-0.00;0.00";

            grpHistory.Controls.Add(dgvHistoryLog);

            statusStrip = new StatusStrip();

            lblLastUpdate = new ToolStripStatusLabel("Cập nhật cuối: --:--:--");
            lblSyncCount = new ToolStripStatusLabel("Số lượt đồng bộ: 0");

            statusStrip.Items.Add(lblLastUpdate);
            statusStrip.Items.Add(new ToolStripStatusLabel(" | "));
            statusStrip.Items.Add(lblSyncCount);

            Controls.Add(grpHistory);
            Controls.Add(grpPrice);
            Controls.Add(pnlControl);
            Controls.Add(lblTitle);
            Controls.Add(statusStrip);
        }

        private async Task StartMonitoringAsync()
        {
            if (monitoringTask != null && !monitoringTask.IsCompleted)
                return;

            cts?.Dispose();
            cts = new CancellationTokenSource();

            CancellationToken token = cts.Token;

            btnStart.Enabled = false;
            btnPause.Enabled = true;

            lblLastUpdate.Text = "Đang khởi tạo...";

            monitoringTask = Task.Run(async () =>
            {
                Random random = new Random();

                try
                {
                    while (true)
                    {
                        token.ThrowIfCancellationRequested();

                        await Task.Delay(2000, token);

                        token.ThrowIfCancellationRequested();

                        decimal oldPrice = currentPrice;
                        decimal change = (decimal)(random.NextDouble() * 400 - 200);
                        decimal newPrice = Math.Max(1, oldPrice + change);
                        decimal percent = (newPrice - oldPrice) / oldPrice * 100;

                        currentPrice = newPrice;

                        PriceRecord record = new PriceRecord
                        {
                            Time = DateTime.Now.ToString("HH:mm:ss"),
                            Price = newPrice,
                            Change = newPrice - oldPrice,
                            Percent = percent
                        };

                        UpdateUI(record);
                    }
                }
                catch (OperationCanceledException)
                {
                }
            }, token);

            try
            {
                await monitoringTask;
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                if (!isClosing && !IsDisposed)
                {
                    btnStart.Enabled = true;
                    btnPause.Enabled = false;
                    lblLastUpdate.Text = "Đã tạm dừng";
                }

                cts?.Dispose();
                cts = null;
                monitoringTask = null;
            }
        }

        private void UpdateUI(PriceRecord record)
        {
            if (InvokeRequired)
            {
                try
                {
                    Invoke(new Action(() => UpdateUI(record)));
                }
                catch (ObjectDisposedException)
                {
                }
                catch (InvalidOperationException)
                {
                }

                return;
            }

            if (isClosing || IsDisposed)
                return;

            Color priceColor = record.Change > 0
                ? Color.Green
                : record.Change < 0
                    ? Color.Red
                    : Color.Black;

            lblBTCPrice.ForeColor = priceColor;
            lblBTCPrice.Text = $"BTC/USD: ${record.Price:N2}";

            dgvHistoryLog.Rows.Insert(
                0,
                record.Time,
                record.Price.ToString("N2"),
                record.Change.ToString("+#,##0.00;-#,##0.00;0.00"),
                record.Percent.ToString("+#,##0.00;-#,##0.00;0.00"));

            dgvHistoryLog.Rows[0].DefaultCellStyle.ForeColor = priceColor;

            if (dgvHistoryLog.Rows.Count > 100)
                dgvHistoryLog.Rows.RemoveAt(dgvHistoryLog.Rows.Count - 1);

            syncCount++;

            lblLastUpdate.Text = $"Cập nhật cuối: {record.Time}";
            lblSyncCount.Text = $"Số lượt đồng bộ: {syncCount}";
        }

        private void PauseMonitoring()
        {
            cts?.Cancel();
            btnPause.Enabled = false;
            lblLastUpdate.Text = "Đang dừng giám sát...";
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            isClosing = true;
            cts?.Cancel();
            base.OnFormClosing(e);
        }
    }

    public class PriceRecord
    {
        public string Time { get; set; } = "";
        public decimal Price { get; set; }
        public decimal Change { get; set; }
        public decimal Percent { get; set; }
    }
}
