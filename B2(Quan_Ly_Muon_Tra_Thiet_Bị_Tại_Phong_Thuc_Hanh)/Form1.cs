using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace B2_Quan_Ly_Muon_Tra_Thiet_Bị_Tại_Phong_Thuc_Hanh_
{
    public partial class Form1 : Form
    {
        private BindingList<LoanRecord> masterList = new BindingList<LoanRecord>();
        private BindingSource bs = new BindingSource();

        public Form1()
        {
            InitializeComponent();
            InitializeApp();
        }

        private void InitializeApp()
        {
            // sample devices
            cmbDevice.Items.AddRange(new string[] { "Oscilloscope", "Multimeter", "Power Supply", "Function Generator" });
            if (cmbDevice.Items.Count > 0) cmbDevice.SelectedIndex = 0;

            // wire events
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnMarkReturned.Click += BtnMarkReturned_Click;
            dgv.SelectionChanged += Dgv_SelectionChanged;
            btnSearch.Click += BtnSearch_Click;
            cmbFilter.SelectedIndexChanged += CmbFilter_SelectedIndexChanged;
            btnExport.Click += BtnExport_Click;

            cmbFilter.Items.AddRange(new string[] { "All", "Đang mượn", "Đã trả", "Quá hạn" });
            cmbFilter.SelectedIndex = 0;

            // setup datagridview columns
            dgv.AutoGenerateColumns = false;
            dgv.Columns.Clear();
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id", HeaderText = "Mã phiếu" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StudentId", HeaderText = "Mã SV" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StudentName", HeaderText = "Họ tên" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Device", HeaderText = "Thiết bị" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Số lượng" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "BorrowDate", HeaderText = "Ngày mượn" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "Hạn trả" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ReturnDate", HeaderText = "Ngày trả" });
            dgv.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Status", HeaderText = "Tình trạng" });

            bs.DataSource = masterList;
            dgv.DataSource = bs;

            // default dates
            dtpBorrow.Value = DateTime.Today;
            dtpDue.Value = DateTime.Today.AddDays(7);
            dtpReturn.Checked = false;

            RefreshStatus();
        }

        private void BtnAdd_Click(object? sender, EventArgs e)
        {
            if (!ValidateInputs(out string id)) return;

            if (masterList.Any(x => x.Id == id))
            {
                MessageBox.Show("Mã phiếu đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var record = new LoanRecord
            {
                Id = id,
                StudentId = txtStudentId.Text.Trim(),
                StudentName = txtStudentName.Text.Trim(),
                Device = cmbDevice.Text,
                Quantity = (int)numQuantity.Value,
                BorrowDate = dtpBorrow.Value.Date,
                DueDate = dtpDue.Value.Date,
                ReturnDate = dtpReturn.Checked ? dtpReturn.Value.Date : (DateTime?)null
            };

            masterList.Add(record);
            RefreshFilter();
            RefreshStatus();
        }

        private void BtnUpdate_Click(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            if (!ValidateInputs(out string id)) return;

            var current = dgv.CurrentRow.DataBoundItem as LoanRecord;
            if (current == null) return;

            // check id uniqueness
            if (id != current.Id && masterList.Any(x => x.Id == id))
            {
                MessageBox.Show("Mã phiếu đã tồn tại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            current.Id = id;
            current.StudentId = txtStudentId.Text.Trim();
            current.StudentName = txtStudentName.Text.Trim();
            current.Device = cmbDevice.Text;
            current.Quantity = (int)numQuantity.Value;
            current.BorrowDate = dtpBorrow.Value.Date;
            current.DueDate = dtpDue.Value.Date;
            current.ReturnDate = dtpReturn.Checked ? dtpReturn.Value.Date : (DateTime?)null;

            // notify update
            bs.ResetBindings(false);
            RefreshFilter();
            RefreshStatus();
        }

        private void BtnDelete_Click(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var current = dgv.CurrentRow.DataBoundItem as LoanRecord;
            if (current == null) return;
            var res = MessageBox.Show("Xác nhận xóa phiếu?") ;
            if (res != DialogResult.OK && res != DialogResult.Yes) return;
            masterList.Remove(current);
            RefreshFilter();
            RefreshStatus();
        }

        private void BtnMarkReturned_Click(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var current = dgv.CurrentRow.DataBoundItem as LoanRecord;
            if (current == null) return;
            current.ReturnDate = DateTime.Today;
            bs.ResetBindings(false);
            RefreshFilter();
            RefreshStatus();
        }

        private void Dgv_SelectionChanged(object? sender, EventArgs e)
        {
            if (dgv.CurrentRow == null) return;
            var current = dgv.CurrentRow.DataBoundItem as LoanRecord;
            if (current == null) return;
            txtId.Text = current.Id;
            txtStudentId.Text = current.StudentId;
            txtStudentName.Text = current.StudentName;
            cmbDevice.Text = current.Device;
            numQuantity.Value = current.Quantity > 0 ? current.Quantity : 1;
            dtpBorrow.Value = current.BorrowDate;
            dtpDue.Value = current.DueDate;
            if (current.ReturnDate.HasValue)
            {
                dtpReturn.Value = current.ReturnDate.Value;
                dtpReturn.Checked = true;
            }
            else
            {
                dtpReturn.Checked = false;
            }
        }

        private void BtnSearch_Click(object? sender, EventArgs e)
        {
            RefreshFilter();
        }

        private void CmbFilter_SelectedIndexChanged(object? sender, EventArgs e)
        {
            RefreshFilter();
        }

        private void RefreshFilter()
        {
            string q = txtSearch.Text.Trim().ToLower();
            string filter = cmbFilter.SelectedItem?.ToString() ?? "All";

            IEnumerable<LoanRecord> filtered = masterList;
            if (!string.IsNullOrEmpty(q))
            {
                filtered = filtered.Where(x => (x.StudentId ?? "").ToLower().Contains(q) || (x.Device ?? "").ToLower().Contains(q));
            }

            if (filter == "Đang mượn") filtered = filtered.Where(x => x.Status == "Đang mượn");
            else if (filter == "Đã trả") filtered = filtered.Where(x => x.Status == "Đã trả");
            else if (filter == "Quá hạn") filtered = filtered.Where(x => x.Status == "Quá hạn");

            bs.DataSource = new BindingList<LoanRecord>(filtered.ToList());
            dgv.DataSource = bs;
            RefreshStatus();
        }

        private void RefreshStatus()
        {
            int total = masterList.Count;
            int overdue = masterList.Count(x => x.Status == "Quá hạn");
            toolStripStatusLabel.Text = $"Tổng số phiếu: {total}";
            toolStripStatusOverdue.Text = $"Phiếu quá hạn: {overdue}";
        }

        private void BtnExport_Click(object? sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var listToExport = (bs.DataSource as BindingList<LoanRecord>)?.ToList() ?? masterList.ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Id,StudentId,StudentName,Device,Quantity,BorrowDate,DueDate,ReturnDate,Status");
            foreach (var r in listToExport)
            {
                sb.AppendLine($"{Escape(r.Id)},{Escape(r.StudentId)},{Escape(r.StudentName)},{Escape(r.Device)},{r.Quantity},{r.BorrowDate:yyyy-MM-dd},{r.DueDate:yyyy-MM-dd},{(r.ReturnDate.HasValue ? r.ReturnDate.Value.ToString("yyyy-MM-dd") : "")},{r.Status}");
            }
            File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
            MessageBox.Show("Export thành công.");
        }

        private static string Escape(string? s)
        {
            if (s == null) return "";
            if (s.Contains(",") || s.Contains("\"") || s.Contains("\n"))
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }

        private bool ValidateInputs(out string id)
        {
            id = txtId.Text.Trim();
            errorProvider.Clear();
            bool ok = true;
            if (string.IsNullOrEmpty(id))
            {
                errorProvider.SetError(txtId, "Mã phiếu không được để trống");
                ok = false;
            }
            if (string.IsNullOrEmpty(txtStudentId.Text.Trim()))
            {
                errorProvider.SetError(txtStudentId, "Mã sinh viên không được để trống");
                ok = false;
            }
            if (string.IsNullOrEmpty(cmbDevice.Text.Trim()))
            {
                errorProvider.SetError(cmbDevice, "Thiết bị không được để trống");
                ok = false;
            }
            if (numQuantity.Value <= 0)
            {
                errorProvider.SetError(numQuantity, "Số lượng phải lớn hơn 0");
                ok = false;
            }
            if (dtpDue.Value.Date < dtpBorrow.Value.Date)
            {
                errorProvider.SetError(dtpDue, "Hạn trả không được trước ngày mượn");
                ok = false;
            }
            if (dtpReturn.Checked && dtpReturn.Value.Date < dtpBorrow.Value.Date)
            {
                errorProvider.SetError(dtpReturn, "Ngày trả thực tế không được trước ngày mượn");
                ok = false;
            }
            return ok;
        }
    }
}

