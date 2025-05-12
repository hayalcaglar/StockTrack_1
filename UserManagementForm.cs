using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockTrack_1
{
    public partial class UserManagementForm : Form

    {
        private string dbPath = "Data Source=products.db;";

        private void UserManagementForm_Load(object sender, EventArgs e)
        {
            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();

                string query = "SELECT Username FROM Users ORDER BY Username ASC";
                SQLiteDataAdapter adapter = new SQLiteDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dgvUsers.DataSource = dt;
                dgvUsers.AutoGenerateColumns = true;
                dgvUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

          

                //MessageBox.Show("Satır sayısı: " + dt.Rows.Count);
            }

            dgvUsers.ClearSelection();
        }


        public UserManagementForm()
        {
            if (LoginForm.LoggedInUser != "admin")
            {
                MessageBox.Show("Bu sayfaya yalnızca admin erişebilir.");
                Close();
                return;
            }

            InitializeComponent();
            this.Load += UserManagementForm_Load;
            
        }

        private void dgvUsers_Load(object sender, EventArgs e)
        {

        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silinecek kullanıcıyı seçin.");
                return;
            }

            string username = dgvUsers.SelectedRows[0].Cells["Username"].Value.ToString();

            DialogResult result = MessageBox.Show($"{username} adlı kullanıcıyı silmek istiyor musunuz?", "Onay", MessageBoxButtons.YesNo);
            if (result != DialogResult.Yes)
                return;

            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();

                string deleteQuery = "DELETE FROM Users WHERE Username = @Username";
                using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Kullanıcı silindi.");
            UserManagementForm_Load(null, null); // Listeyi yenile

        }

        private void btnUpdatePwd_Click(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen şifresi değiştirilecek kullanıcıyı seçin.");
                return;
            }

            string username = dgvUsers.SelectedRows[0].Cells["Username"].Value.ToString();
            string newPassword = txtNewPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Yeni şifre boş olamaz.");
                return;
            }

            string hashedPassword = ComputeSha256Hash(newPassword); // Aynı hash fonksiyonunu kullan

            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();

                string updateQuery = "UPDATE Users SET Password = @Password WHERE Username = @Username";
                using (SQLiteCommand command = new SQLiteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@Password", hashedPassword);
                    command.Parameters.AddWithValue("@Username", username);
                    command.ExecuteNonQuery();
                }
            }

            MessageBox.Show("Şifre güncellendi.");
            txtNewPassword.Clear();
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

    }
}
