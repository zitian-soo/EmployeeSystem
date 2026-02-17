using System;
using System.Data;
using System.Data.SQLite; // ต้องมี NuGet Package
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace EmployeeSystem
{
    // Class Form1 และ Main รวมอยู่ที่เดียวกัน
    public class Form1 : Form
    {
        // ==========================================
        // 1. ประกาศตัวแปร Controls
        // ==========================================
        private TextBox txtID, txtName, txtPos, txtDept, txtEmail, txtPhone;
        private ComboBox cboStatus;
        private Button btnSave, btnUpdate, btnDelete;
        private DataGridView dgvEmployees;

        // ตัวแปรเชื่อมต่อ Database
        private string dbFile = "Employee.db";
        private string connString;

        public Form1()
        {
            // ตั้งค่าหน้าจอ (Part 1)
            this.Text = "โปรแกรมบันทึกข้อมูลพนักงาน";
            this.Size = new Size(900, 750); // เพิ่มความสูงฟอร์มหน่อยเผื่อตารางยาว
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 10F);

            // Connection String
            connString = $"Data Source={dbFile};Version=3;";

            InitializeCustomComponents();
            CreateDatabaseAndTable(); // สร้าง DB อัตโนมัติ
            LoadData(); // โหลดข้อมูลลงตาราง
        }

        // ==========================================
        // 2. สร้างหน้าจอ UI (Part 1 - แก้ตำแหน่งแล้ว)
        // ==========================================
        private void InitializeCustomComponents()
        {
            Label lblHeader = new Label { Text = "โปรแกรมบันทึกข้อมูลพนักงาน", Font = new Font("Segoe UI", 18, FontStyle.Bold), Location = new Point(280, 20), AutoSize = true, ForeColor = Color.DarkBlue };
            this.Controls.Add(lblHeader);

            int xLbl = 50, xTxt = 200, y = 80;

            // Helper สร้างช่องกรอก
            txtID = AddInput("Employee ID (รหัส):", xLbl, xTxt, ref y);
            txtName = AddInput("Name (ชื่อ-สกุล):", xLbl, xTxt, ref y);
            txtPos = AddInput("Position (ตำแหน่ง):", xLbl, xTxt, ref y);
            txtDept = AddInput("Department (แผนก):", xLbl, xTxt, ref y);
            txtEmail = AddInput("Email:", xLbl, xTxt, ref y);
            txtPhone = AddInput("Phone (เบอร์โทร):", xLbl, xTxt, ref y);

            // Status (ComboBox)
            this.Controls.Add(new Label { Text = "Status (สถานะ):", Location = new Point(xLbl, y), AutoSize = true });
            cboStatus = new ComboBox { Location = new Point(xTxt, y), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cboStatus.Items.AddRange(new string[] { "Active", "Resigned", "On Leave" });
            cboStatus.SelectedIndex = 0;
            this.Controls.Add(cboStatus);
            y += 60; // จบส่วน Input ที่ประมาณ y=380

            // ปุ่มกด (Buttons) - เริ่มที่ y=380 ความสูง 40 (จบที่ 420)
            btnSave = CreateBtn("Save (บันทึก)", Color.SeaGreen, xTxt, y);
            btnUpdate = CreateBtn("Update (แก้ไข)", Color.Orange, xTxt + 110, y);
            btnDelete = CreateBtn("Delete (ลบ)", Color.IndianRed, xTxt + 220, y);

            // ผูก Event ปุ่ม
            btnSave.Click += BtnSave_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;

            // ตารางข้อมูล (DataGridView) (Part 3)
            // ✅ แก้ไข: ขยับลงมาที่ 460 (ห่างจากปุ่ม 40px ไม่ทับแน่นอน)
            dgvEmployees = new DataGridView
            {
                Location = new Point(50, 460),
                Size = new Size(800, 220),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false
            };
            dgvEmployees.CellClick += DgvEmployees_CellClick; // คลิกแล้วโชว์ข้อมูล
            this.Controls.Add(dgvEmployees);
        }

        // ==========================================
        // 3. จัดการ Database SQLite (Part 2)
        // ==========================================
        private void CreateDatabaseAndTable()
        {
            if (!File.Exists(dbFile))
            {
                SQLiteConnection.CreateFile(dbFile);
                using (var conn = new SQLiteConnection(connString))
                {
                    conn.Open();
                    string sql = @"CREATE TABLE Employees (
                                    EmployeeID TEXT PRIMARY KEY,
                                    EmployeeName TEXT,
                                    Position TEXT,
                                    Department TEXT,
                                    Email TEXT,
                                    Phone TEXT,
                                    Status TEXT)";
                    using (var cmd = new SQLiteCommand(sql, conn)) cmd.ExecuteNonQuery();
                }
            }
        }

        private void ExecuteQuery(string sql, params SQLiteParameter[] parameters)
        {
            using (var conn = new SQLiteConnection(connString))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    if (parameters != null) cmd.Parameters.AddRange(parameters);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadData()
        {
            using (var conn = new SQLiteConnection(connString))
            {
                conn.Open();
                string sql = "SELECT * FROM Employees";
                using (var da = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvEmployees.DataSource = dt;
                }
            }
        }

        // ==========================================
        // 4. ฟังก์ชันการทำงาน (Events)
        // ==========================================

        // ปุ่มบันทึก (Save)
        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            try
            {
                string sql = "INSERT INTO Employees VALUES (@id, @name, @pos, @dept, @email, @phone, @status)";
                ExecuteQuery(sql,
                    new SQLiteParameter("@id", txtID.Text),
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@pos", txtPos.Text),
                    new SQLiteParameter("@dept", txtDept.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@status", cboStatus.Text)
                );
                MessageBox.Show("บันทึกข้อมูลเรียบร้อย!", "Success");
                LoadData();
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด (ID อาจซ้ำ): " + ex.Message);
            }
        }

        // ปุ่มแก้ไข (Update)
        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text)) return;

            try
            {
                string sql = "UPDATE Employees SET EmployeeName=@name, Position=@pos, Department=@dept, Email=@email, Phone=@phone, Status=@status WHERE EmployeeID=@id";
                ExecuteQuery(sql,
                    new SQLiteParameter("@name", txtName.Text),
                    new SQLiteParameter("@pos", txtPos.Text),
                    new SQLiteParameter("@dept", txtDept.Text),
                    new SQLiteParameter("@email", txtEmail.Text),
                    new SQLiteParameter("@phone", txtPhone.Text),
                    new SQLiteParameter("@status", cboStatus.Text),
                    new SQLiteParameter("@id", txtID.Text)
                );
                MessageBox.Show("แก้ไขข้อมูลเรียบร้อย!", "Success");
                LoadData();
                ClearForm();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        // ปุ่มลบ (Delete)
        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtID.Text)) return;

            if (MessageBox.Show("ยืนยันการลบ?", "Confirm", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    string sql = "DELETE FROM Employees WHERE EmployeeID=@id";
                    ExecuteQuery(sql, new SQLiteParameter("@id", txtID.Text));
                    MessageBox.Show("ลบข้อมูลเรียบร้อย!");
                    LoadData();
                    ClearForm();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        // คลิกตารางแล้วข้อมูลเด้งขึ้นมา
        private void DgvEmployees_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvEmployees.Rows[e.RowIndex];
                txtID.Text = row.Cells["EmployeeID"].Value.ToString();
                txtName.Text = row.Cells["EmployeeName"].Value.ToString();
                txtPos.Text = row.Cells["Position"].Value.ToString();
                txtDept.Text = row.Cells["Department"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                cboStatus.Text = row.Cells["Status"].Value.ToString();

                txtID.Enabled = false; // ล็อค ID ห้ามแก้
            }
        }

        // ==========================================
        // 5. Helper Methods (ตัวช่วย)
        // ==========================================
        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtID.Text) || string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("กรุณากรอกรหัสและชื่อพนักงาน", "Warning");
                return false;
            }
            return true;
        }

        private void ClearForm()
        {
            txtID.Clear(); txtName.Clear(); txtPos.Clear(); txtDept.Clear(); txtEmail.Clear(); txtPhone.Clear();
            cboStatus.SelectedIndex = 0;
            txtID.Enabled = true;
        }

        private TextBox AddInput(string label, int xLbl, int xTxt, ref int y)
        {
            this.Controls.Add(new Label { Text = label, Location = new Point(xLbl, y), AutoSize = true });
            TextBox txt = new TextBox { Location = new Point(xTxt, y), Width = 300 };
            this.Controls.Add(txt);
            y += 40;
            return txt;
        }

        private Button CreateBtn(string text, Color color, int x, int y)
        {
            Button btn = new Button { Text = text, Location = new Point(x, y), Size = new Size(100, 40), BackColor = color, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            this.Controls.Add(btn);
            return btn;
        }

        // จุดเริ่มโปรแกรม (Entry Point)
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}