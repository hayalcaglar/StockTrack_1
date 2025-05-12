using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.Json;
using System.Data.SQLite;


namespace StockTrack_1
{
    public partial class Form1 : Form
    {
        private string dbPath = "Data Source=products.db;";
        private SQLiteConnection connection;

        // Ürün sınıfı
        public class Product
        {
            public string Name { get; set; }
            public int Stock { get; set; }
            public DateTime CreatedAt { get; set; }
            public string Category { get; set; }
            

        }

        private List<Product> products = new List<Product>();

        public Form1()
        {
            InitializeComponent();
            InitializeDatabase();
            dataGridView1.AutoGenerateColumns = true;

            LoadProducts();

        }

        private void InitializeDatabase()
        {
            if (!File.Exists("products.db"))
            {
                SQLiteConnection.CreateFile("products.db");
            }

            connection = new SQLiteConnection(dbPath);
            connection.Open();

            string createTableQuery = @"
        CREATE TABLE IF NOT EXISTS Products (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Stock INTEGER NOT NULL,
            CreatedAt TEXT NOT NULL,
            Category TEXT,
            Username TEXT
        );";

            SQLiteCommand command = new SQLiteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }



        private void btnAdd_Click(object sender, EventArgs e)
        {
            string productName = txtProductName.Text.Trim();
            int productStock = (int)numProductStock.Value;
            string category = cmbCategory.SelectedItem?.ToString() ?? "";
            string username = LoginForm.LoggedInUser;
            string createdAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (string.IsNullOrWhiteSpace(productName))
            {
                MessageBox.Show("Ürün adı boş olamaz.");
                return;
            }
            if (cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Lütfen bir kategori seçin.");
                return;
            }


            string insertQuery = @"
        INSERT INTO Products (Name, Stock, CreatedAt, Category, Username)
        VALUES (@Name, @Stock, @CreatedAt, @Category, @Username);";

            using (SQLiteCommand command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", productName);
                command.Parameters.AddWithValue("@Stock", productStock);
                command.Parameters.AddWithValue("@CreatedAt", createdAt);
                command.Parameters.AddWithValue("@Category", category);
                command.Parameters.AddWithValue("@Username", username);

                command.ExecuteNonQuery();
            }

            txtProductName.Text = "";
            numProductStock.Value = 1;
            cmbCategory.SelectedIndex = -1;

            LoadProducts(); // 👈 Ekledikten sonra listeyi güncelle
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Lütfen silmek için bir ürün seçin.");
                return;
            }

            // Seçilen ürün bilgilerini al
            string name = dataGridView1.SelectedRows[0].Cells["Name"].Value.ToString();
            string createdAt = Convert.ToDateTime(dataGridView1.SelectedRows[0].Cells["CreatedAt"].Value).ToString("yyyy-MM-dd HH:mm:ss");
            string username = LoginForm.LoggedInUser;

            // SQL ile silme işlemi
            string deleteQuery = @"
        DELETE FROM Products
        WHERE Name = @Name AND CreatedAt = @CreatedAt AND Username = @Username";

            using (SQLiteCommand command = new SQLiteCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@CreatedAt", createdAt);
                command.Parameters.AddWithValue("@Username", username);

                command.ExecuteNonQuery();
            }

            // Listeyi güncelle
            LoadProducts();
        }


        private void FilterProducts()
        {
            string keyword = txtSearch.Text.ToLower();
            string selectedCategory = cmbFilter.SelectedItem?.ToString() ?? "Tümü";

            var filtered = products.FindAll(p =>
                (string.IsNullOrEmpty(keyword) || p.Name.ToLower().Contains(keyword)) &&
                (selectedCategory == "Tümü" || p.Category == selectedCategory)
            );

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = filtered;

            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns["Name"].HeaderText = "Ürün Adı";
                dataGridView1.Columns["Stock"].HeaderText = "Stok Miktarı";
                dataGridView1.Columns["CreatedAt"].HeaderText = "Eklenme Tarihi";
                dataGridView1.Columns["Category"].HeaderText = "Kategori";
            }
        }



       
        private void LoadProducts()
        {
            string username = LoginForm.LoggedInUser;
            products.Clear();

            string selectQuery = "SELECT * FROM Products WHERE Username = @Username";

            using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
            {
                command.Parameters.AddWithValue("@Username", username);
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product product = new Product
                        {
                            Name = reader["Name"].ToString(),
                            Stock = Convert.ToInt32(reader["Stock"]),
                            CreatedAt = DateTime.Parse(reader["CreatedAt"].ToString()),
                            Category = reader["Category"].ToString()
                        };

                        products.Add(product);
                    }
                }
            }

            FilterProducts(); // Arama + kategori filtre birlikte çalışmaya devam etsin
            UpdateStatistics();

        }

        private void UpdateStatistics()
        {
            string username = LoginForm.LoggedInUser;

            // 1. Toplam ürün sayısı

            string countQuery = "SELECT COUNT(*) FROM Products WHERE Username = @Username";
            SQLiteCommand countCmd = new SQLiteCommand(countQuery, connection);
            countCmd.Parameters.AddWithValue("@Username", username);
            long totalCount = (long)countCmd.ExecuteScalar();
            lTotalProducts.Text = $"Toplam Ürün: {totalCount}";

            // 2. En çok stoğa sahip tüm ürünleri bul

            string maxStockQuery = "SELECT MAX(Stock) FROM Products WHERE Username = @Username";
            SQLiteCommand maxStockCmd = new SQLiteCommand(maxStockQuery, connection);
            maxStockCmd.Parameters.AddWithValue("@Username", username);
            object maxObj = maxStockCmd.ExecuteScalar();

            if (maxObj != null && maxObj != DBNull.Value)
            {
                int maxStock = Convert.ToInt32(maxObj);

                string multiTopQuery = @"
                 SELECT Name FROM Products 
                 WHERE Username = @Username AND Stock = @MaxStock";

                SQLiteCommand multiCmd = new SQLiteCommand(multiTopQuery, connection);
                multiCmd.Parameters.AddWithValue("@Username", username);
                multiCmd.Parameters.AddWithValue("@MaxStock", maxStock);

                using (SQLiteDataReader reader = multiCmd.ExecuteReader())
                {
                    List<string> topProducts = new List<string>();
                    while (reader.Read())
                    {
                        topProducts.Add(reader["Name"].ToString());
                    }

                    lTopProduct.Text = $"En Çok Stok: {string.Join(", ", topProducts)} ({maxStock})";
                }
            }


            // 3. Kategori dağılımı

            string catQuery = @"
        SELECT Category, COUNT(*) as Count 
        FROM Products 
        WHERE Username = @Username
        GROUP BY Category";
            SQLiteCommand catCmd = new SQLiteCommand(catQuery, connection);
            catCmd.Parameters.AddWithValue("@Username", username);
            using (SQLiteDataReader reader = catCmd.ExecuteReader())
            {
                List<string> lines = new List<string>();
                while (reader.Read())
                {
                    string cat = reader["Category"].ToString();
                    string count = reader["Count"].ToString();
                    lines.Add($"{cat}: {count}");
                }
                lCategories.Text = "Kategoriler:\n" + string.Join("\n", lines);

                // 4. Grafik Güncelle

                chartCategories.Series.Clear();
                chartCategories.Titles.Clear();
                chartCategories.Titles.Add("Kategoriye Göre Ürün Dağılımı");

                var series = chartCategories.Series.Add("Ürün Sayısı");
                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Bar;

                foreach (var line in lines)
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        string category = parts[0].Trim();
                        int count = int.Parse(parts[1].Trim());
                        series.Points.AddXY(category, count);
                    }
                }

            }
        }



        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterProducts();

        }
        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterProducts();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (products.Count == 0)
            {
                MessageBox.Show("Aktarılacak ürün bulunamadı.");
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV Dosyası|*.csv";
            saveFileDialog.Title = "CSV Olarak Kaydet";
            saveFileDialog.FileName = "urunler.csv";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                StringBuilder csv = new StringBuilder();
                csv.AppendLine("Ürün Adı,Stok Miktarı,Kategori,Eklenme Tarihi");

                foreach (var p in products)
                {
                    csv.AppendLine($"{p.Name},{p.Stock},{p.Category},{p.CreatedAt:yyyy-MM-dd HH:mm}");
                }

                File.WriteAllText(saveFileDialog.FileName, csv.ToString(), Encoding.UTF8);
                MessageBox.Show("CSV dosyası başarıyla oluşturuldu.");
            }
        }

        private void btnBackup_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFile = new SaveFileDialog();
            saveFile.Filter = "SQLite Veritabanı|*.db";
            saveFile.Title = "Veritabanı Yedeğini Kaydet";
            saveFile.FileName = "products_backup.db";

            if (saveFile.ShowDialog() == DialogResult.OK)
            {
                File.Copy("products.db", saveFile.FileName, true);
                MessageBox.Show("Veritabanı başarıyla yedeklendi.");
            }
        }

        private void btnRestore_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Filter = "SQLite Veritabanı|*.db";
            openFile.Title = "Yedek Veritabanı Seç";

            if (openFile.ShowDialog() == DialogResult.OK)
            {
                connection.Close(); // Mevcut bağlantıyı kapat
                File.Copy(openFile.FileName, "products.db", true);
                connection = new SQLiteConnection(dbPath);
                connection.Open();

                MessageBox.Show("Veritabanı geri yüklendi.");
                LoadProducts(); // Yeniden listele
            }
        }
    }
}
