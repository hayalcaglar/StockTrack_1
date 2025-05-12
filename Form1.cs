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

namespace StockTrack_1
{
    public partial class Form1 : Form
    {
        // Ürün sınıfı
        public class Product
        {
            public string Name { get; set; }
            public int Stock { get; set; }
            public DateTime CreatedAt { get; set; }

        }

        private List<Product> products = new List<Product>();

        public Form1()
        {
            InitializeComponent();
            dataGridView1.AutoGenerateColumns = true;

            LoadData();

        }




        private void btnAdd_Click_1(object sender, EventArgs e)
        {
            string productName = txtProductName.Text;
            int productStock = (int)numProductStock.Value;

            if (string.IsNullOrWhiteSpace(productName))
            {
                MessageBox.Show("Ürün adı boş olamaz.");
                return;
            }

            Product newProduct = new Product { Name = productName, Stock = productStock, CreatedAt = DateTime.Now };
            products.Add(newProduct);

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = products;
            dataGridView1.Columns["Name"].HeaderText = "Ürün Adı";
            dataGridView1.Columns["Stock"].HeaderText = "Stok Miktarı";
            dataGridView1.Columns["CreatedAt"].HeaderText = "Eklenme Tarihi";


            txtProductName.Text = "";
            numProductStock.Value = 1;
            MessageBox.Show("Kaydedildi");
            SaveData();

        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int index = dataGridView1.SelectedRows[0].Index;
                products.RemoveAt(index);

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = products;
            }
            else
            {
                MessageBox.Show("Lütfen silmek için bir satır seçin.");
            }

            SaveData();

        }

        private void FilterProducts()
        {
            string keyword = txtSearch.Text.ToLower();

            var filtered = products.FindAll(p => p.Name.ToLower().Contains(keyword));

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = filtered;

            // Süton başlıklarını yeniden ayarla
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns["Name"].HeaderText = "Ürün Adı";
                dataGridView1.Columns["Stock"].HeaderText = "Stok Miktarı";
                dataGridView1.Columns["CreatedAt"].HeaderText = "Eklenme Tarihi";
            }
        }


        private void SaveData()
        {
            string json = JsonSerializer.Serialize(products);
            File.WriteAllText("products.json", json);
            string jsonPath = $"{LoginForm.LoggedInUser}_products.json";

        }

        private void LoadData()
        {
            string jsonPath = $"{LoginForm.LoggedInUser}_products.json";

            if (File.Exists("products.json"))
            {
                string json = File.ReadAllText("products.json");
                products = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = products;
                dataGridView1.Columns["Name"].HeaderText = "Ürün Adı";
                dataGridView1.Columns["Stock"].HeaderText = "Stok Miktarı";
                dataGridView1.Columns["CreatedAt"].HeaderText = "Eklenme Tarihi";

            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterProducts();

        }
    }
}
