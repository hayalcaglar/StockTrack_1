using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StockTrack_1
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        public static string LoggedInUser = ""; // Global olarak erişilebilecek kullanıcı adı

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Lütfen kullanıcı adınızı girin.");
                return;
            }

            LoggedInUser = txtUsername.Text.Trim().ToLower();

            this.Hide(); // Login ekranını gizle
            Form1 mainForm = new Form1(); // Ana uygulamayı aç
            mainForm.Show();
        }
    }
}
