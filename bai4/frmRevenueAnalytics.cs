
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RevenueAnalytics
{
    public class frmRevenueAnalytics : Form
    {
        private DateTimePicker dtpFromDate = new DateTimePicker();
        private DateTimePicker dtpToDate = new DateTimePicker();
        private ComboBox cboBranch = new ComboBox();
        private Button btnAnalyze = new Button();
        private Button btnCancel = new Button();
        private ProgressBar pgbLoading = new ProgressBar();
        private Label lblProcessingStatus = new Label();
        private DataGridView dgvRevenueData = new DataGridView();
        private CancellationTokenSource? cts;

        public frmRevenueAnalytics()
        {
            InitializeUI();
        }

        private void InitializeUI()
        {
            Text = "Bảng điều khiển thống kê doanh thu";
            Size = new Size(1100, 680);
            MinimumSize = new Size(850, 500);
            StartPosition = FormStartPosition.CenterScreen;

            Label lblTitle = new Label
            {
                Text = "THỐNG KÊ DOANH THU",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 55,
                TextAlign = ContentAlignment.MiddleCenter
            };

            GroupBox grpCriteria = new GroupBox
            {
                Text = "Tiêu chí thống kê",
                Dock = DockStyle.Top,
                Height = 125,
                Padding = new Padding(12)
            };

            Label lblFrom = new Label
            {
                Text = "Từ ngày:",
                Location = new Point(18, 30),
                AutoSize = true
            };

            dtpFromDate.Location = new Point(85, 25);
            dtpFromDate.Width = 150;
            dtpFromDate.Format = DateTimePickerFormat.Short;
            dtpFromDate.Value = DateTime.Today.AddDays(-7);

            Label lblTo = new Label
            {
                Text = "Đến ngày:",
                Location = new Point(255, 30),
                AutoSize = true
            };

            dtpToDate.Location = new Point(325, 25);
            dtpToDate.Width = 150;
            dtpToDate.Format = DateTimePickerFormat.Short;
            dtpToDate.Value = DateTime.Today;

            Label lblBranch = new Label
            {
                Text = "Chi nhánh:",
                Location = new Point(495, 30),
                AutoSize = true
            };

            cboBranch.Location = new Point(570, 25);
            cboBranch.Width = 160;
            cboBranch.DropDownStyle = ComboBoxStyle.DropDownList;
            cboBranch.Items.AddRange(new object[]
            {
                "Tất cả chi nhánh",
                "Chi nhánh Hà Nội",
                "Chi nhánh TP.HCM",
                "Chi nhánh Đà Nẵng"
            });
            cboBranch.SelectedIndex = 0;

            btnAnalyze.Text = "Thống kê doanh thu";
            btnAnalyze.Location = new Point(85, 70);
            btnAnalyze.Size = new Size(180, 35);
            btnAnalyze.Click += async (s, e) => await AnalyzeRevenueAsync();

            btnCancel.Text = "Hủy thao tác";
            btnCancel.Location = new Point(280, 70);
            btnCancel.Size = new Size(140, 35);
            btnCancel.Enabled = false;
            btnCancel.Click += (s, e) => CancelAnalysis();

            grpCriteria.Controls.AddRange(new Control[]
            {
                lblFrom, dtpFromDate,
                lblTo, dtpToDate,
                lblBranch, cboBranch,
                btnAnalyze, btnCancel
            });

            GroupBox grpStatus = new GroupBox
            {
                Text = "Trạng thái xử lý",
                Dock = DockStyle.Top,
                Height = 85,
                Padding = new Padding(12)
            };

            lblProcessingStatus.Text = "Sẵn sàng thống kê";
            lblProcessingStatus.Location = new Point(15, 20);
            lblProcessingStatus.Size = new Size(650, 22);

            pgbLoading.Location = new Point(15, 47);
            pgbLoading.Size = new Size(650, 20);
            pgbLoading.Minimum = 0;
            pgbLoading.Maximum = 100;
            pgbLoading.Value = 0;
            pgbLoading.Anchor = AnchorStyles.Top |
                                AnchorStyles.Left |
                                AnchorStyles.Right;

            grpStatus.Controls.Add(lblProcessingStatus);
            grpStatus.Controls.Add(pgbLoading);

            dgvRevenueData.Dock = DockStyle.Fill;
            dgvRevenueData.ReadOnly = true;
            dgvRevenueData.AllowUserToAddRows = false;
            dgvRevenueData.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
            dgvRevenueData.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            dgvRevenueData.MultiSelect = false;

            Controls.Add(dgvRevenueData);
            Controls.Add(grpStatus);
            Controls.Add(grpCriteria);
            Controls.Add(lblTitle);
        }

        private async Task AnalyzeRevenueAsync()
        {
            if (dtpFromDate.Value.Date > dtpToDate.Value.Date)
            {
                MessageBox.Show(
                    "Ngày bắt đầu không được lớn hơn ngày kết thúc.",
                    "Dữ liệu không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            cts?.Dispose();
            cts = new CancellationTokenSource();

            CancellationToken token = cts.Token;

            DateTime fromDate = dtpFromDate.Value.Date;
            DateTime toDate = dtpToDate.Value.Date;
            string branch = cboBranch.SelectedItem?.ToString()
                            ?? "Tất cả chi nhánh";

            btnAnalyze.Enabled = false;
            btnCancel.Enabled = true;
            dtpFromDate.Enabled = false;
            dtpToDate.Enabled = false;
            cboBranch.Enabled = false;

            pgbLoading.Value = 0;
            lblProcessingStatus.Text = "Đang truy xuất dữ liệu...";

            IProgress<int> progress = new Progress<int>(value =>
            {
                pgbLoading.Value = Math.Clamp(value, 0, 100);
                lblProcessingStatus.Text =
                    $"Đang thống kê doanh thu... {value}%";
            });

            try
            {
                List<RevenueRecord> result = await Task.Run(async () =>
                {
                    Random random = new Random();
                    List<RevenueRecord> records =
                        new List<RevenueRecord>();

                    for (int i = 1; i <= 100; i++)
                    {
                        token.ThrowIfCancellationRequested();

                        await Task.Delay(50, token);

                        if (i % 10 == 0)
                        {
                            records.Add(new RevenueRecord
                            {
                                TransactionId = $"GD{i:000}",
                                TransactionDate = RandomDate(
                                    fromDate, toDate, random),
                                Branch = branch == "Tất cả chi nhánh"
                                    ? GetRandomBranch(random)
                                    : branch,
                                Description = $"Giao dịch bán hàng {i}",
                                Amount = random.Next(100000, 5000000)
                            });
                        }

                        progress.Report(i);
                    }

                    token.ThrowIfCancellationRequested();

                    return records
                        .OrderByDescending(x => x.TransactionDate)
                        .ToList();
                }, token);

                dgvRevenueData.DataSource = null;
                dgvRevenueData.DataSource = result;

                if (dgvRevenueData.Columns["Amount"] != null)
                {
                    dgvRevenueData.Columns["Amount"]
                        .DefaultCellStyle.Format = "N0";

                    dgvRevenueData.Columns["Amount"]
                        .DefaultCellStyle.Alignment =
                        DataGridViewContentAlignment.MiddleRight;
                }

                pgbLoading.Value = 100;
                lblProcessingStatus.Text =
                    $"Hoàn tất! Đã tổng hợp {result.Count} giao dịch.";

                MessageBox.Show(
                    "Thống kê doanh thu hoàn tất!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                pgbLoading.Value = 0;
                lblProcessingStatus.Text = "Đã hủy thao tác!";

                MessageBox.Show(
                    "Đã hủy thao tác!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                pgbLoading.Value = 0;
                lblProcessingStatus.Text = "Có lỗi khi thống kê.";

                MessageBox.Show(
                    "Lỗi: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                btnAnalyze.Enabled = true;
                btnCancel.Enabled = false;
                dtpFromDate.Enabled = true;
                dtpToDate.Enabled = true;
                cboBranch.Enabled = true;

                cts?.Dispose();
                cts = null;
            }
        }

        private void CancelAnalysis()
        {
            if (cts != null && !cts.IsCancellationRequested)
            {
                cts.Cancel();
                btnCancel.Enabled = false;
                lblProcessingStatus.Text = "Đang hủy thao tác...";
            }
        }

        private static DateTime RandomDate(
            DateTime fromDate,
            DateTime toDate,
            Random random)
        {
            int days = (toDate - fromDate).Days;

            return fromDate
                .AddDays(random.Next(days + 1))
                .AddHours(random.Next(24))
                .AddMinutes(random.Next(60));
        }

        private static string GetRandomBranch(Random random)
        {
            string[] branches =
            {
                "Chi nhánh Hà Nội",
                "Chi nhánh TP.HCM",
                "Chi nhánh Đà Nẵng"
            };

            return branches[random.Next(branches.Length)];
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            cts?.Cancel();
            base.OnFormClosing(e);
        }
    }

    public class RevenueRecord
    {
        public string TransactionId { get; set; } = "";
        public DateTime TransactionDate { get; set; }
        public string Branch { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal Amount { get; set; }
    }
}
