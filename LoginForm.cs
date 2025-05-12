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
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            InitializeUsersTable();
        }

        public static string LoggedInUser = ""; // Global olarak erişilebilecek kullanıcı adı

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim().ToLower();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Kullanıcı adı ve şifre boş olamaz.");
                return;
            }

            string loginQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";

            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(loginQuery, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    string hashedPassword = ComputeSha256Hash(password);
                    command.Parameters.AddWithValue("@Password", hashedPassword);


                    long count = (long)command.ExecuteScalar();

                    if (count > 0)
                    {
                        LoggedInUser = username;
                        this.Hide();
                        Form1 mainForm = new Form1();
                        mainForm.ShowDialog();
                        this.Show();
                    }
                    else
                    {
                        MessageBox.Show("Hatalı kullanıcı adı veya şifre.");
                    }
                }
            }
        }

        private string dbPath = "Data Source=products.db;";

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim().ToLower();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Kullanıcı adı ve şifre boş olamaz.");
                return;
            }

            string insertUserQuery = "INSERT INTO Users (Username, Password) VALUES (@Username, @Password)";

            try
            {
                // ✅ Geçici SQLite bağlantısı aç
                using (SQLiteConnection connection = new SQLiteConnection(dbPath))
                {
                    connection.Open();

                    using (SQLiteCommand command = new SQLiteCommand(insertUserQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        string hashedPassword = ComputeSha256Hash(password);
                        command.Parameters.AddWithValue("@Password", hashedPassword);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Kayıt başarıyla oluşturuldu. Artık giriş yapabilirsiniz.");
                txtPassword.Text = "";
            }
            catch (SQLiteException ex)
            {
                if (ex.Message.Contains("UNIQUE constraint failed"))
                {
                    MessageBox.Show("Bu kullanıcı adı zaten kayıtlı.");
                }
                else
                {
                    MessageBox.Show("Kayıt sırasında bir hata oluştu:\n" + ex.Message);
                }
            }
        }

        private void InitializeUsersTable()
        {
            using (SQLiteConnection connection = new SQLiteConnection(dbPath))
            {
                connection.Open();

                string createUsersTable = @"
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                Password TEXT NOT NULL
            );";

                SQLiteCommand userCmd = new SQLiteCommand(createUsersTable, connection);
                userCmd.ExecuteNonQuery();
            }
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

