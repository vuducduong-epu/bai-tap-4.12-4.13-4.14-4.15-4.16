using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BookManager
{
    public class frmBookManager : Form
    {
        private readonly BindingList<BookModel> books = new();
        private readonly BindingSource bindingSource = new();
        private readonly ErrorProvider errorProvider = new();

        private TextBox txtBookId = new();
        private TextBox txtTitle = new();
        private TextBox txtAuthor = new();
        private TextBox txtQuantity = new();
        private TextBox txtUnitPrice = new();
        private TextBox txtSearch = new();
        private ComboBox cboCategory = new();
        private PictureBox picBookCover = new();
        private DataGridView dgvBooks = new();

        private ToolStripStatusLabel lblCount = new();
        private ToolStripStatusLabel lblTotalValue = new();
        private ToolStripStatusLabel lblTime = new();

        private readonly System.Windows.Forms.Timer clockTimer = new();
        private string selectedCoverPath = "";
        private bool addingNew = false;

        public frmBookManager()
        {
            Text = "QUAN LY TAI LIEU SACH - NHA SACH";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1200, 760);
            MinimumSize = new Size(950, 620);
            Font = new Font("Segoe UI", 9);

            errorProvider.ContainerControl = this;
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            BuildInterface();
            ConfigureGrid();

            bindingSource.DataSource = books;
            dgvBooks.DataSource = bindingSource;

            books.ListChanged += (s, e) => UpdateStatus();
            bindingSource.CurrentChanged += (s, e) => ShowSelectedBook();
            dgvBooks.SelectionChanged += (s, e) => ShowSelectedBook();

            clockTimer.Interval = 1000;
            clockTimer.Tick += (s, e) =>
                lblTime.Text = "Thoi gian: " +
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            clockTimer.Start();

            AddSampleBooks();
            UpdateStatus();
            ResetForm();
        }

        private void BuildInterface()
        {
            var menu = new MenuStrip();

            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Mo file CSV", null,
                (s, e) => ImportCsv());
            fileMenu.DropDownItems.Add("Luu file CSV", null,
                (s, e) => ExportCsv());
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Thoat", null,
                (s, e) => Close());

            var editMenu = new ToolStripMenuItem("Edit");
            editMenu.DropDownItems.Add("Them moi", null,
                (s, e) => ResetFormForNew());
            editMenu.DropDownItems.Add("Sua thong tin", null,
                (s, e) => EditBook());
            editMenu.DropDownItems.Add("Xoa sach", null,
                (s, e) => DeleteBook());
            editMenu.DropDownItems.Add("Tim kiem", null,
                (s, e) => txtSearch.Focus());

            menu.Items.AddRange(new ToolStripItem[]
            {
                fileMenu, editMenu
            });

            var toolStrip = new ToolStrip();

            toolStrip.Items.Add("📂 Mo file", null,
                (s, e) => ImportCsv());
            toolStrip.Items.Add("💾 Luu file", null,
                (s, e) => ExportCsv());
            toolStrip.Items.Add("➕ Them moi", null,
                (s, e) => ResetFormForNew());
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripLabel("Tim kiem:"));

            txtSearch = new TextBox
            {
                Width = 220,
                PlaceholderText = "Nhap ma sach, ten sach, tac gia..."
            };

            var searchHost = new ToolStripControlHost(txtSearch)
            {
                AutoSize = false,
                Width = 230,
                Height = 26
            };

            toolStrip.Items.Add(searchHost);
            txtSearch.TextChanged += (s, e) => ApplySearch();

            var status = new StatusStrip();
            lblCount = new ToolStripStatusLabel("Tong so dau sach: 0");
            lblTotalValue = new ToolStripStatusLabel("Tong gia tri kho: 0 VND");
            lblTime = new ToolStripStatusLabel();

            status.Items.Add(lblCount);
            status.Items.Add(new ToolStripStatusLabel("|"));
            status.Items.Add(lblTotalValue);
            status.Items.Add(new ToolStripStatusLabel { Spring = true });
            status.Items.Add(lblTime);

            Controls.Add(menu);
            Controls.Add(toolStrip);
            Controls.Add(status);

            MainMenuStrip = menu;
            menu.Dock = DockStyle.Top;
            toolStrip.Dock = DockStyle.Top;
            status.Dock = DockStyle.Bottom;

            var main = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(10)
            };

            main.RowStyles.Add(new RowStyle(SizeType.Absolute, 285));
            main.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            Controls.Add(main);

            var detailGroup = new GroupBox
            {
                Text = "THONG TIN CHI TIET SACH",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };
            main.Controls.Add(detailGroup, 0, 0);

            var detailLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1
            };

            detailLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 36));
            detailLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 36));
            detailLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 28));

            detailGroup.Controls.Add(detailLayout);

            var left = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(3)
            };
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            left.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var middle = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                Padding = new Padding(3)
            };
            middle.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            middle.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            AddField(left, "Ma sach:", txtBookId, 0);
            AddField(left, "Ten sach:", txtTitle, 1);
            AddField(left, "Tac gia:", txtAuthor, 2);
            AddField(left, "The loai:", cboCategory, 3);

            AddField(middle, "So luong:", txtQuantity, 0);
            AddField(middle, "Don gia:", txtUnitPrice, 1);

            txtBookId.ReadOnly = true;

            cboCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCategory.Items.AddRange(new object[]
            {
                "Giao trinh", "Van hoc", "Khoa hoc",
                "Cong nghe", "Kinh te", "Khac"
            });

            var imagePanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(5)
            };
            imagePanel.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100));
            imagePanel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 38));

            picBookCover = new PictureBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };

            var btnCover = new Button
            {
                Text = "Tai anh bia",
                Dock = DockStyle.Fill
            };
            btnCover.Click += (s, e) => LoadCover();

            imagePanel.Controls.Add(picBookCover, 0, 0);
            imagePanel.Controls.Add(btnCover, 0, 1);

            detailLayout.Controls.Add(left, 0, 0);
            detailLayout.Controls.Add(middle, 1, 0);
            detailLayout.Controls.Add(imagePanel, 2, 0);

            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 42,
                FlowDirection = FlowDirection.LeftToRight
            };

            var btnAdd = new Button
            {
                Text = "Them moi",
                Width = 110,
                Height = 32
            };
            var btnEdit = new Button
            {
                Text = "Sua thong tin",
                Width = 120,
                Height = 32
            };
            var btnDelete = new Button
            {
                Text = "Xoa sach",
                Width = 100,
                Height = 32
            };
            var btnReset = new Button
            {
                Text = "Lam moi",
                Width = 100,
                Height = 32
            };

            btnAdd.Click += (s, e) => AddBook();
            btnEdit.Click += (s, e) => EditBook();
            btnDelete.Click += (s, e) => DeleteBook();
            btnReset.Click += (s, e) => ResetForm();

            buttonPanel.Controls.AddRange(new Control[]
            {
                btnAdd, btnEdit, btnDelete, btnReset
            });

            detailGroup.Controls.Add(buttonPanel);
            buttonPanel.BringToFront();

            var gridGroup = new GroupBox
            {
                Text = "DANH SACH SACH TRONG KHO",
                Dock = DockStyle.Fill,
                Padding = new Padding(8)
            };
            main.Controls.Add(gridGroup, 0, 1);

            dgvBooks = new DataGridView
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
                BackgroundColor = Color.White
            };

            gridGroup.Controls.Add(dgvBooks);
        }

        private void AddField(
            TableLayoutPanel panel,
            string label,
            Control control,
            int row)
        {
            panel.RowStyles.Add(
                new RowStyle(SizeType.Absolute, 43));

            panel.Controls.Add(new Label
            {
                Text = label,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, row);

            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control, 1, row);
        }

        private void ConfigureGrid()
        {
            dgvBooks.AutoGenerateColumns = false;
            dgvBooks.Columns.Clear();

            AddGridColumn("Ma sach", nameof(BookModel.BookId), 75);
            AddGridColumn("Ten sach", nameof(BookModel.Title), 150);
            AddGridColumn("Tac gia", nameof(BookModel.Author), 110);
            AddGridColumn("The loai", nameof(BookModel.Category), 90);
            AddGridColumn("So luong", nameof(BookModel.Quantity), 65);

            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Don gia",
                DataPropertyName = nameof(BookModel.UnitPrice),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0"
                },
                FillWeight = 90
            });

            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Gia tri kho",
                DataPropertyName = nameof(BookModel.TotalValue),
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Format = "N0"
                },
                FillWeight = 100
            });

            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Anh bia",
                DataPropertyName = nameof(BookModel.CoverPath),
                FillWeight = 90
            });
        }

        private void AddGridColumn(
            string header,
            string property,
            float fillWeight)
        {
            dgvBooks.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = header,
                DataPropertyName = property,
                FillWeight = fillWeight
            });
        }

        private void AddSampleBooks()
        {
            books.Add(new BookModel
            {
                BookId = "S001",
                Title = "Lap trinh C#",
                Author = "Nguyen Van A",
                Category = "Cong nghe",
                Quantity = 10,
                UnitPrice = 85000
            });

            books.Add(new BookModel
            {
                BookId = "S002",
                Title = "Lap trinh Java",
                Author = "Tran Van B",
                Category = "Giao trinh",
                Quantity = 8,
                UnitPrice = 95000
            });
        }

        private string GenerateBookId()
        {
            int max = books
                .Select(b =>
                {
                    string number = b.BookId.StartsWith("S")
                        ? b.BookId.Substring(1)
                        : b.BookId;

                    return int.TryParse(number, out int n) ? n : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            return $"S{max + 1:000}";
        }

        private void LoadCover()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Chon anh bia sach",
                Filter = "Anh bia (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = false
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using var image = Image.FromFile(dialog.FileName);
                var copy = new Bitmap(image);

                var oldImage = picBookCover.Image;
                picBookCover.Image = copy;
                oldImage?.Dispose();

                selectedCoverPath = dialog.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Khong the mo anh: " + ex.Message,
                    "Loi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private bool ValidateInput()
        {
            errorProvider.Clear();
            bool valid = true;

            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                errorProvider.SetError(txtTitle, "Nhap ten sach.");
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(txtAuthor.Text))
            {
                errorProvider.SetError(txtAuthor, "Nhap ten tac gia.");
                valid = false;
            }

            if (cboCategory.SelectedIndex < 0)
            {
                errorProvider.SetError(cboCategory, "Chon the loai sach.");
                valid = false;
            }

            if (!int.TryParse(txtQuantity.Text.Trim(), out int quantity) ||
                quantity <= 0)
            {
                errorProvider.SetError(
                    txtQuantity, "So luong phai la so nguyen duong.");
                valid = false;
            }

            if (!decimal.TryParse(
                    txtUnitPrice.Text.Trim(),
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out decimal price) ||
                price <= 0)
            {
                errorProvider.SetError(
                    txtUnitPrice, "Don gia phai lon hon 0.");
                valid = false;
            }

            if (!valid)
            {
                MessageBox.Show(
                    "Vui long kiem tra lai thong tin sach.",
                    "Du lieu khong hop le",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            return valid;
        }

        private BookModel ReadForm()
        {
            return new BookModel
            {
                BookId = txtBookId.Text,
                Title = txtTitle.Text.Trim(),
                Author = txtAuthor.Text.Trim(),
                Category = cboCategory.Text,
                Quantity = int.Parse(txtQuantity.Text.Trim()),
                UnitPrice = decimal.Parse(
                    txtUnitPrice.Text.Trim(),
                    CultureInfo.CurrentCulture),
                CoverPath = selectedCoverPath
            };
        }

        private void AddBook()
        {
            if (!ValidateInput())
                return;

            if (addingNew)
            {
                books.Add(ReadForm());
                bindingSource.ResetBindings(false);
                ResetForm();

                MessageBox.Show("Them sach thanh cong.");
                return;
            }

            addingNew = true;
            txtBookId.Text = GenerateBookId();
            txtTitle.Clear();
            txtAuthor.Clear();
            txtQuantity.Clear();
            txtUnitPrice.Clear();
            cboCategory.SelectedIndex = -1;
            selectedCoverPath = "";

            var oldImage = picBookCover.Image;
            picBookCover.Image = null;
            oldImage?.Dispose();

            txtTitle.Focus();

            MessageBox.Show(
                "Nhap thong tin sach moi, sau do bam Them moi lan nua de luu.");
        }

        private void ResetFormForNew()
        {
            addingNew = true;
            ResetFieldsForNew();
        }

        private void ResetFieldsForNew()
        {
            errorProvider.Clear();
            txtBookId.Text = GenerateBookId();
            txtTitle.Clear();
            txtAuthor.Clear();
            txtQuantity.Clear();
            txtUnitPrice.Clear();
            cboCategory.SelectedIndex = -1;
            selectedCoverPath = "";

            var oldImage = picBookCover.Image;
            picBookCover.Image = null;
            oldImage?.Dispose();

            dgvBooks.ClearSelection();
            txtTitle.Focus();
        }

        private void EditBook()
        {
            if (dgvBooks.CurrentRow?.DataBoundItem is not BookModel selected)
            {
                MessageBox.Show("Hay chon sach can sua.");
                return;
            }

            if (!ValidateInput())
                return;

            selected.Title = txtTitle.Text.Trim();
            selected.Author = txtAuthor.Text.Trim();
            selected.Category = cboCategory.Text;
            selected.Quantity = int.Parse(txtQuantity.Text.Trim());
            selected.UnitPrice = decimal.Parse(
                txtUnitPrice.Text.Trim(),
                CultureInfo.CurrentCulture);
            selected.CoverPath = selectedCoverPath;

            bindingSource.ResetBindings(false);
            UpdateStatus();
            ResetForm();

            MessageBox.Show("Cap nhat sach thanh cong.");
        }

        private void DeleteBook()
        {
            if (dgvBooks.CurrentRow?.DataBoundItem is not BookModel selected)
            {
                MessageBox.Show("Hay chon sach can xoa.");
                return;
            }

            var result = MessageBox.Show(
                $"Ban co chac muon xoa sach '{selected.Title}'?",
                "Xac nhan xoa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                books.Remove(selected);
                ResetForm();
            }
        }

        private void ShowSelectedBook()
        {
            if (addingNew)
                return;

            if (dgvBooks.CurrentRow?.DataBoundItem is not BookModel book)
                return;

            txtBookId.Text = book.BookId;
            txtTitle.Text = book.Title;
            txtAuthor.Text = book.Author;
            cboCategory.SelectedItem = book.Category;
            txtQuantity.Text = book.Quantity.ToString();
            txtUnitPrice.Text = book.UnitPrice.ToString("0.##");
            selectedCoverPath = book.CoverPath;

            Image? newImage = null;

            try
            {
                if (!string.IsNullOrWhiteSpace(book.CoverPath) &&
                    File.Exists(book.CoverPath))
                {
                    using var image = Image.FromFile(book.CoverPath);
                    newImage = new Bitmap(image);
                }
            }
            catch
            {
                newImage = null;
            }

            var oldImage = picBookCover.Image;
            picBookCover.Image = newImage;
            oldImage?.Dispose();

            errorProvider.Clear();
        }

        private void ResetForm()
        {
            addingNew = false;
            errorProvider.Clear();

            txtBookId.Text = GenerateBookId();
            txtTitle.Clear();
            txtAuthor.Clear();
            txtQuantity.Clear();
            txtUnitPrice.Clear();
            cboCategory.SelectedIndex = -1;
            selectedCoverPath = "";

            var oldImage = picBookCover.Image;
            picBookCover.Image = null;
            oldImage?.Dispose();

            if (dgvBooks.Rows.Count > 0)
            {
                dgvBooks.ClearSelection();
                dgvBooks.CurrentCell = dgvBooks.Rows[0].Cells[0];
                dgvBooks.Rows[0].Selected = true;
            }
        }

        private void UpdateStatus()
        {
            lblCount.Text = $"Tong so dau sach: {books.Count}";

            decimal total = books.Sum(b => b.TotalValue);

            lblTotalValue.Text =
                $"Tong gia tri kho: {total:N0} VND";
        }

        private void ApplySearch()
        {
            string keyword = txtSearch.Text.Trim();

            foreach (DataGridViewRow row in dgvBooks.Rows)
            {
                if (row.DataBoundItem is not BookModel book)
                    continue;

                bool match =
                    book.BookId.Contains(
                        keyword, StringComparison.OrdinalIgnoreCase) ||
                    book.Title.Contains(
                        keyword, StringComparison.OrdinalIgnoreCase) ||
                    book.Author.Contains(
                        keyword, StringComparison.OrdinalIgnoreCase);

                row.Visible = match;
            }
        }

        private void ExportCsv()
        {
            using var dialog = new SaveFileDialog
            {
                Title = "Luu bao cao sach",
                Filter = "CSV (*.csv)|*.csv",
                FileName = "DanhSachSach.csv",
                DefaultExt = "csv",
                AddExtension = true
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                using var writer = new StreamWriter(
                    dialog.FileName, false, new UTF8Encoding(true));

                writer.WriteLine(
                    "BookId,Title,Author,Category,Quantity,UnitPrice,CoverPath");

                foreach (BookModel book in books)
                {
                    writer.WriteLine(string.Join(",",
                        CsvEscape(book.BookId),
                        CsvEscape(book.Title),
                        CsvEscape(book.Author),
                        CsvEscape(book.Category),
                        book.Quantity.ToString(CultureInfo.InvariantCulture),
                        book.UnitPrice.ToString(CultureInfo.InvariantCulture),
                        CsvEscape(book.CoverPath)));
                }

                MessageBox.Show("Xuat file CSV thanh cong.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Loi xuat file: " + ex.Message);
            }
        }

        private void ImportCsv()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Mo danh sach sach",
                Filter = "CSV (*.csv)|*.csv",
                Multiselect = false
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            try
            {
                var lines = File.ReadAllLines(
                    dialog.FileName, Encoding.UTF8);

                if (lines.Length <= 1)
                {
                    MessageBox.Show("File CSV khong co du lieu.");
                    return;
                }

                var imported = new BindingList<BookModel>();

                for (int i = 1; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i]))
                        continue;

                    var fields = ParseCsvLine(lines[i]);

                    if (fields.Count < 7)
                        throw new FormatException(
                            $"Dong {i + 1} khong du 7 cot.");

                    if (!int.TryParse(fields[4], out int quantity) ||
                        !decimal.TryParse(
                            fields[5],
                            NumberStyles.Number,
                            CultureInfo.InvariantCulture,
                            out decimal price))
                    {
                        throw new FormatException(
                            $"Du lieu so khong hop le tai dong {i + 1}.");
                    }

                    imported.Add(new BookModel
                    {
                        BookId = fields[0],
                        Title = fields[1],
                        Author = fields[2],
                        Category = fields[3],
                        Quantity = quantity,
                        UnitPrice = price,
                        CoverPath = fields[6]
                    });
                }

                books.RaiseListChangedEvents = false;
                books.Clear();

                foreach (BookModel book in imported)
                    books.Add(book);

                books.RaiseListChangedEvents = true;
                bindingSource.ResetBindings(false);
                UpdateStatus();
                ResetForm();

                MessageBox.Show("Nhap file CSV thanh cong.");
            }
            catch (Exception ex)
            {
                books.RaiseListChangedEvents = true;
                bindingSource.ResetBindings(false);

                MessageBox.Show(
                    "Khong the doc file CSV: " + ex.Message,
                    "Loi nhap file",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private string CsvEscape(string value)
        {
            value ??= "";

            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private System.Collections.Generic.List<string> ParseCsvLine(
            string line)
        {
            var result = new System.Collections.Generic.List<string>();
            var field = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length &&
                        line[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(field.ToString());
                    field.Clear();
                }
                else
                {
                    field.Append(c);
                }
            }

            result.Add(field.ToString());
            return result;
        }
    }
}
