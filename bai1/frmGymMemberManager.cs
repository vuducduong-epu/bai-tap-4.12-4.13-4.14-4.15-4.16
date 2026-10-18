
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace GymMemberManager
{
    public class frmGymMemberManager : Form
    {
        private readonly BindingList<MemberModel> members = new();
        private readonly BindingSource source = new();
        private readonly ErrorProvider errors = new();

        private TextBox txtMemberId = new();
        private TextBox txtFullName = new();
        private TextBox txtPhone = new();
        private TextBox txtFee = new();
        private ComboBox cboPackage = new();
        private DateTimePicker dtpStartDate = new();
        private DataGridView dgvMembers = new();

        private Button btnAdd = new();
        private Button btnEdit = new();
        private Button btnDelete = new();
        private Button btnReset = new();

        private bool isAdding = false;

        public frmGymMemberManager()
        {
            Text = "QUAN LY DICH VU PHONG GYM & HOI VIEN";
            Size = new Size(1050, 650);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(900, 550);
            Font = new Font("Segoe UI", 10);

            errors.ContainerControl = this;
            errors.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            BuildInterface();
            ConfigureGrid();

            source.DataSource = members;
            dgvMembers.DataSource = source;

            dgvMembers.SelectionChanged += DgvMembers_SelectionChanged;

            AddSampleData();
            ResetForm();
        }

        private void BuildInterface()
        {
            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(12)
            };

            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 245));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(main);

            var group = new GroupBox
            {
                Text = "THONG TIN HOI VIEN",
                Dock = DockStyle.Fill,
                Padding = new Padding(12)
            };

            main.Controls.Add(group, 0, 0);

            var fields = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 145,
                ColumnCount = 4,
                RowCount = 3
            };

            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115));
            fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            for (int i = 0; i < 3; i++)
                fields.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));

            group.Controls.Add(fields);

            txtMemberId.ReadOnly = true;

            cboPackage.DropDownStyle = ComboBoxStyle.DropDownList;
            cboPackage.Items.AddRange(new object[]
            {
                "Theo thang", "6 thang", "1 nam"
            });

            dtpStartDate.Format = DateTimePickerFormat.Custom;
            dtpStartDate.CustomFormat = "dd/MM/yyyy";

            AddField(fields, "Ma hoi vien:", txtMemberId, 0, 0);
            AddField(fields, "Ho va ten:", txtFullName, 1, 0);
            AddField(fields, "So dien thoai:", txtPhone, 0, 1);
            AddField(fields, "Goi tap:", cboPackage, 1, 1);
            AddField(fields, "Ngay dang ky:", dtpStartDate, 0, 2);
            AddField(fields, "Phi dich vu:", txtFee, 1, 2);

            var buttons = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 48,
                FlowDirection = FlowDirection.LeftToRight
            };

            group.Controls.Add(buttons);

            btnAdd = MakeButton("Them moi");
            btnEdit = MakeButton("Sua thong tin");
            btnDelete = MakeButton("Xoa hoi vien");
            btnReset = MakeButton("Lam moi");

            buttons.Controls.AddRange(new Control[]
            {
                btnAdd, btnEdit, btnDelete, btnReset
            });

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnReset.Click += (s, e) => ResetForm();

            var gridGroup = new GroupBox
            {
                Text = "DANH SACH HOI VIEN",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };

            main.Controls.Add(gridGroup, 0, 1);

            dgvMembers = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D
            };

            gridGroup.Controls.Add(dgvMembers);
        }

        private void AddField(
            TableLayoutPanel panel,
            string caption,
            Control control,
            int column,
            int row)
        {
            int labelColumn = column == 0 ? 0 : 2;
            int inputColumn = column == 0 ? 1 : 3;

            panel.Controls.Add(new Label
            {
                Text = caption,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, labelColumn, row);

            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control, inputColumn, row);
        }

        private Button MakeButton(string text)
        {
            return new Button
            {
                Text = text,
                Width = 130,
                Height = 36,
                Margin = new Padding(4)
            };
        }

        private void ConfigureGrid()
        {
            dgvMembers.AutoGenerateColumns = false;
            dgvMembers.Columns.Clear();

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colMemberId",
                HeaderText = "Ma HV",
                DataPropertyName = nameof(MemberModel.MemberId),
                FillWeight = 70
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFullName",
                HeaderText = "Ten hoi vien",
                DataPropertyName = nameof(MemberModel.FullName),
                FillWeight = 140
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPhone",
                HeaderText = "SDT",
                DataPropertyName = nameof(MemberModel.Phone),
                FillWeight = 90
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colPackage",
                HeaderText = "Goi tap",
                DataPropertyName = nameof(MemberModel.Package),
                FillWeight = 85
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colStartDate",
                HeaderText = "Ngay DK",
                DataPropertyName = nameof(MemberModel.StartDate),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "dd/MM/yyyy"
                },
                FillWeight = 90
            });

            dgvMembers.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFee",
                HeaderText = "Phi dich vu",
                DataPropertyName = nameof(MemberModel.Fee),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0"
                },
                FillWeight = 100
            });
        }

        private void AddSampleData()
        {
            members.Add(new MemberModel
            {
                MemberId = GenerateMemberId(),
                FullName = "Nguyen Van An",
                Phone = "0912345678",
                Package = "Theo thang",
                StartDate = DateTime.Today,
                Fee = 500000
            });

            members.Add(new MemberModel
            {
                MemberId = GenerateMemberId(),
                FullName = "Tran Thi Binh",
                Phone = "0987654321",
                Package = "6 thang",
                StartDate = DateTime.Today,
                Fee = 2700000
            });
        }

        private string GenerateMemberId()
        {
            int max = members
                .Select(m =>
                {
                    string number = m.MemberId.Replace("HV", "");
                    return int.TryParse(number, out int n) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"HV{max + 1:000}";
        }

        private bool ValidateInput()
        {
            errors.Clear();
            bool valid = true;

            string name = txtFullName.Text.Trim();

            if (string.IsNullOrWhiteSpace(name) ||
                !Regex.IsMatch(name, @"^[\p{L}\s]+$"))
            {
                errors.SetError(txtFullName,
                    "Ho ten khong duoc trong hoac chua ky tu dac biet.");
                valid = false;
            }

            if (!Regex.IsMatch(txtPhone.Text.Trim(), @"^\d{10}$"))
            {
                errors.SetError(txtPhone,
                    "So dien thoai phai gom dung 10 chu so.");
                valid = false;
            }

            if (!decimal.TryParse(txtFee.Text.Trim(), out decimal fee) ||
                fee <= 0 || fee != decimal.Truncate(fee))
            {
                errors.SetError(txtFee,
                    "Phi dich vu phai la so nguyen duong.");
                valid = false;
            }

            if (cboPackage.SelectedIndex < 0)
            {
                errors.SetError(cboPackage, "Vui long chon goi tap.");
                valid = false;
            }

            if (!valid)
                MessageBox.Show(
                    "Vui long kiem tra lai thong tin!",
                    "Du lieu khong hop le",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

            return valid;
        }

        private MemberModel ReadForm()
        {
            return new MemberModel
            {
                MemberId = txtMemberId.Text,
                FullName = txtFullName.Text.Trim(),
                Phone = txtPhone.Text.Trim(),
                Package = cboPackage.Text,
                StartDate = dtpStartDate.Value.Date,
                Fee = decimal.Parse(txtFee.Text.Trim())
            };
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!isAdding)
            {
                isAdding = true;
                txtMemberId.Text = GenerateMemberId();
                txtFullName.Clear();
                txtPhone.Clear();
                txtFee.Clear();
                cboPackage.SelectedIndex = -1;
                dtpStartDate.Value = DateTime.Today;
                dgvMembers.ClearSelection();
                txtFullName.Focus();
                btnAdd.Text = "Luu hoi vien";
                return;
            }

            if (!ValidateInput())
                return;

            members.Add(ReadForm());
            source.ResetBindings(false);
            ResetForm();

            MessageBox.Show("Them hoi vien thanh cong!");
        }

        private void BtnEdit_Click(object? sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow?.DataBoundItem is not MemberModel selected)
            {
                MessageBox.Show("Vui long chon hoi vien can sua.");
                return;
            }

            if (!ValidateInput())
                return;

            selected.FullName = txtFullName.Text.Trim();
            selected.Phone = txtPhone.Text.Trim();
            selected.Package = cboPackage.Text;
            selected.StartDate = dtpStartDate.Value.Date;
            selected.Fee = decimal.Parse(txtFee.Text.Trim());

            source.ResetBindings(false);
            ResetForm();

            MessageBox.Show("Cap nhat hoi vien thanh cong!");
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgvMembers.CurrentRow?.DataBoundItem is not MemberModel selected)
            {
                MessageBox.Show("Vui long chon hoi vien can xoa.");
                return;
            }

            DialogResult result = MessageBox.Show(
                $"Ban co chac muon xoa hoi vien {selected.FullName}?",
                "Xac nhan xoa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                members.Remove(selected);
                ResetForm();
            }
        }

        private void DgvMembers_SelectionChanged(object? sender, EventArgs e)
        {
            if (isAdding)
                return;

            if (dgvMembers.CurrentRow?.DataBoundItem is not MemberModel member)
                return;

            txtMemberId.Text = member.MemberId;
            txtFullName.Text = member.FullName;
            txtPhone.Text = member.Phone;
            cboPackage.SelectedItem = member.Package;
            dtpStartDate.Value = member.StartDate;
            txtFee.Text = member.Fee.ToString("0");
            errors.Clear();
        }

        private void ResetForm()
        {
            isAdding = false;
            errors.Clear();

            txtMemberId.Text = GenerateMemberId();
            txtFullName.Clear();
            txtPhone.Clear();
            txtFee.Clear();
            cboPackage.SelectedIndex = -1;
            dtpStartDate.Value = DateTime.Today;

            btnAdd.Text = "Them moi";

            if (dgvMembers.Rows.Count > 0)
            {
                dgvMembers.ClearSelection();
                dgvMembers.Rows[0].Selected = true;
                dgvMembers.CurrentCell = dgvMembers.Rows[0].Cells[0];
            }
        }
    }
}
