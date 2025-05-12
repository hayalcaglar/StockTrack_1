# StockTrack - Desktop Inventory Management App

StockTrack is a modern and secure desktop application built with C# and WinForms, backed by SQLite. It enables users to manage product inventory efficiently, export data to CSV, and includes user authentication with hashed passwords.

---

## 🚀 Features

- ✅ **User Registration & Login**  
  - SHA256-encrypted password storage  
  - Unique login per user

- ✅ **SQLite Database Integration**  
  - Persistent local database  
  - Tables are created automatically if not present

- ✅ **Product Management**  
  - Add, delete products  
  - Define product stock quantity  
  - Assign categories to products  
  - Filter products by category or search by name

- ✅ **Admin Panel**  
  - View all registered users  
  - Delete users  
  - Reset user passwords (also hashed via SHA256)  
  - Access restricted to the admin user only

- ✅ **Data Export**  
  - Export full product list to `.csv`  
  - Compatible with Excel  
  - UTF-8 support for Turkish characters

- ✅ **Database Backup & Restore**  
  - One-click `.db` file backup  
  - Restore from saved versions

- ✅ **Visual Statistics**  
  - Total product count  
  - Top-stocked product(s)  
  - Bar chart by category

---

## 🛠 Technologies Used

- C# (.NET Framework)
- WinForms
- SQLite
- ADO.NET
- SHA256 encryption via System.Security.Cryptography
- System.IO, System.Data, System.Text, System.Drawing

---

## 📦 Setup Instructions

1. Clone or download this repository  
2. Open `StockTrack.sln` in Visual Studio  
3. Build and run the project  
4. Use **Register** to create a user  
5. Log in to access product dashboard  
6. Use `admin` as username to access **user management panel**

---

## 📸 Screenshots

> *(You can insert screenshots here after pushing images to GitHub or using relative links)*

- 🔒 Login Screen  
- 📦 Product Management Dashboard  
- 👤 User Management Panel  
- 📊 Statistics Graph

---

## 💡 Possible Extensions

- PDF export of reports  
- Role-based user access control  
- Multi-language interface (EN / TR)  
- Light/Dark theme support  
- Cloud-based database sync

---

## 👤 Author

Developed by [Your Name]  
For portfolio, educational and professional presentation purposes.

---

## 📝 License

This project is not under any commercial license. You may fork, use or contribute for learning and demonstration purposes.
