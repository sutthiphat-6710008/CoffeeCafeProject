using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMenu : Form
    {
        byte[] menuImage;
       
        public FrmMenu()
        {
            InitializeComponent();
        }

        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }

        private byte[] convertImageToByteArray(Image image, ImageFormat imageFormat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, imageFormat);
                return ms.ToArray();
            }
        }

        private void getAllMenuToListView()
        {
            //กำหนด connection string เพื่อติดต่อไปยัง Database
            string connectionString = @"Server=MSI\SQLEXPRESS;Database=coffee_cafe_db;trusted_Connection=True;";

            //สร้าง connection
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    string strSQL = "select menuId, menuName, menuPrice, menuImage from menu_tb";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        //เอาข้อมูลที่ได้จาก strSQL ที่เป็นก้อน มาแปลงเป็น Table
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        //ตั้งค่า listViews
                        lvShowAllMenu.Items.Clear();
                        lvShowAllMenu.Columns.Clear();
                        lvShowAllMenu.FullRowSelect = true;
                        lvShowAllMenu.View = View.Details;

                        //ตั้งค่าการแสดง img ใน listView
                        if (lvShowAllMenu.SmallImageList == null)
                        {
                            lvShowAllMenu.SmallImageList = new ImageList();
                            lvShowAllMenu.SmallImageList.ImageSize = new Size(50, 50);
                            lvShowAllMenu.SmallImageList.ColorDepth = ColorDepth.Depth32Bit;
                        }
                        lvShowAllMenu.SmallImageList.Images.Clear();

                        //กำหนดรายละเอียดของ column ใน listView
                        lvShowAllMenu.Columns.Add("รูปภาพ", 130, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("รหัสเมนู", 70, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("ชื่อเมนู", 150, HorizontalAlignment.Left);
                        lvShowAllMenu.Columns.Add("ราคา", 80, HorizontalAlignment.Left);

                        //loop ข้อมูลใส่ใน Table
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(); //สร้าง item เพื่อเก็บแต่ละข้อมูลในแต่ละรายการ
                            Image menuImage = null;
                            if (dataRow["menuImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["menuImage"];

                                //แปลง byte เป็นภาพ
                                menuImage = convertByteArrayToImage(imgByte);
                            }

                            string imageKey = null;
                            if (menuImage != null)
                            {
                                imageKey = $"menu_{dataRow["menuId"]}";
                                lvShowAllMenu.SmallImageList.Images.Add(imageKey, menuImage);
                                item.ImageKey = imageKey;
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }
                            item.SubItems.Add(dataRow["menuId"].ToString());
                            item.SubItems.Add(dataRow["menuName"].ToString());
                            item.SubItems.Add(dataRow["menuPrice"].ToString());

                            //เอาข้อมูลใน item ใส่ใน listView
                            lvShowAllMenu.Items.Add(item);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด :" + ex.Message);
                }
            }
        }

        private void FrmMenu_Load(object sender, EventArgs e)
        {
            pbMenuImage.Image = null;
            tbMenuId.Clear();
            tbMenuName.Clear();
            tbMenuPrice.Clear();
            btSave.Enabled = true;
            btUpdate.Enabled = false;
            btDelete.Enabled = false;
            getAllMenuToListView();
        }

        private void btSelectMenuImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = @"C:\";
            ofd.Filter = "Image Files (*.jpg;*.png) | *.jpg;*.png";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                //เอารูปที่เลือกไปแสดงที่ pbMenuImage
                pbMenuImage.Image = Image.FromFile(ofd.FileName);
                //check format image และส่งรูปไปแปลง
                if (pbMenuImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    menuImage = convertImageToByteArray(pbMenuImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    menuImage = convertImageToByteArray(pbMenuImage.Image, ImageFormat.Png);
                }
            }
        }

        private void alertValidate(string msg)
        {
            MessageBox.Show(msg, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (menuImage == null)
            {
                alertValidate("กรุณาเลือกภาพเมนู");
            }
            else if (tbMenuName.Text.Length == 0)
            {
                alertValidate("กรุณาป้อนชื่อเมนู");
            }
            else if (tbMenuPrice.Text.Length == 0)
            {
                alertValidate("กรุณาป้อนราคาเมนู");
            }
            else
            {
                //บันทึกลง db

                string connectionString = @"Server=MSI\SQLEXPRESS;Database=coffee_cafe_db;trusted_Connection=True;";

                //สร้าง connection
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                    try
                    {
                        sqlConnection.Open();
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction(); //ใช้กับ CRUD

                        string countSQL = "SELECT COUNT(*) FROM menu_tb";
                        using (SqlCommand command = new SqlCommand(countSQL, sqlConnection, sqlTransaction))
                        {
                            int rowCount = (int)command.ExecuteScalar();

                            if (rowCount == 10)
                            {
                                alertValidate("ได้ไม่เกิน 10 เมนู");
                                return;
                            }
                        }

                        string strSQL = "INSERT INTO menu_tb (menuName, menuPrice, menuImage)" +
                                    "VALUES (@menuName,@menuPrice,@menuImage)";

                        using (SqlCommand command = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {

                            //กำหนด Parameter
                            command.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                            command.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(tbMenuPrice.Text);
                            command.Parameters.Add("@menuImage", SqlDbType.Image).Value = menuImage;


                            //excute sql
                            command.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("สำเร็จ");

                            getAllMenuToListView();

                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();

                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด :" + ex.Message);
                    }
            }
        }

        private void tbMenuPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            // อนุญาตเฉพาะตัวเลข, จุดทศนิยม (.) และ Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // ไม่ให้พิมพ์จุด (.) ซ้ำ
            if (e.KeyChar == '.' && (sender as TextBox).Text.Contains("."))
            {
                e.Handled = true;
            }
        }

        //private void btClose_Click(object sender, EventArgs e)
        //{
        //    this.Close();
        //}

        //private void btCancel_Click(object sender, EventArgs e)
        //{

        //}

       

        //private void btDelete_Click(object sender, EventArgs e)
        //{
        //    if (MessageBox.Show("ต้องการลบหรือไม่", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
        //    {
        //        string connectionString = @"Server=MSI\SQLEXPRESS;Database=coffee_cafe_db;trusted_Connection=True;";
        //        using (SqlConnection sqlConnection = new SqlConnection(connectionString))
        //        {
        //            try
        //            {
        //                sqlConnection.Open();
        //                SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

        //                string deleteSQL = "DELETE FROM menu_tb WHERE menuId = @menuId";

        //                using (SqlCommand command = new SqlCommand(deleteSQL, sqlConnection, sqlTransaction))
        //                {
        //                    command.Parameters.AddWithValue("@menuId", int.Parse(tbMenuId.Text));

        //                    int rowsAffected = command.ExecuteNonQuery();
        //                    sqlTransaction.Commit();

        //                    if (rowsAffected > 0)
        //                    {
        //                        MessageBox.Show("ลบข้อมูลสำเร็จ");
        //                        pbMenuImage.Image = null;
        //                        pbMenuImage.Image = null;
        //                        tbMenuId.Clear();
        //                        tbMenuName.Clear();
        //                        tbMenuPrice.Clear();
        //                        btSave.Enabled = true;
        //                        btUpdate.Enabled = false;
        //                        btDelete.Enabled = false;
        //                        getAllMenuToListView();
        //                    }
        //                    else
        //                    {
        //                        MessageBox.Show("ไม่พบข้อมูลที่จะลบ");
        //                    }
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show("พบข้อผิดพลาด: " + ex.Message);
        //            }
        //        }
        //    }

        //}


        //private void btUpdate_Click(object sender, EventArgs e)
        //{
        //    if (tbMenuName.Text.Length == 0)
        //    {
        //        alertValidate("กรุณาป้อนชื่อเมนู");
        //        return;
        //    }
        //    if (tbMenuPrice.Text.Length == 0)
        //    {
        //        alertValidate("กรุณาป้อนราคาเมนู");
        //        return;
        //    }
        //    if (!float.TryParse(tbMenuPrice.Text, out float price))
        //    {
        //        alertValidate("กรุณาป้อนราคาที่ถูกต้อง");
        //        return;
        //    }
        //    if (tbMenuId.Text.Length == 0)
        //    {
        //        alertValidate("กรุณาป้อนรหัสเมนู");
        //        return;
        //    }

        //    string connectionString = @"Server=MSI\SQLEXPRESS;Database=coffee_cafe_db;trusted_Connection=True;";

        //    using (SqlConnection sqlConnection = new SqlConnection(connectionString))
        //    {
        //        try
        //        {
        //            sqlConnection.Open();

        //            // เช็คว่าภาพเดิมมีอยู่หรือไม่
        //            string checkImageSql = "SELECT menuImage FROM menu_tb WHERE menuId = @menuId";
        //            byte[] existingImage = null;
        //            using (SqlCommand checkCmd = new SqlCommand(checkImageSql, sqlConnection))
        //            {
        //                checkCmd.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);
        //                object result = checkCmd.ExecuteScalar();
        //                if (result != DBNull.Value && result != null)
        //                {
        //                    existingImage = (byte[])result;
        //                }
        //            }

        //            // ถ้าไม่มีภาพใหม่และไม่มีภาพเก่า แจ้งเตือน
        //            if (menuImage == null && existingImage == null)
        //            {
        //                alertValidate("กรุณาเลือกภาพเมนู");
        //                return;
        //            }

        //            byte[] imageToSave = menuImage ?? existingImage;

        //            SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

        //            string strSQL = "UPDATE menu_tb " +
        //                            "SET menuName = @menuName, " +
        //                            "menuPrice = @menuPrice, " +
        //                            "menuImage = @menuImage " +
        //                            "WHERE menuId = @menuId";

        //            using (SqlCommand command = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
        //            {
        //                command.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
        //                command.Parameters.Add("@menuPrice", SqlDbType.Float).Value = price;
        //                command.Parameters.Add("@menuImage", SqlDbType.Image).Value = imageToSave;
        //                command.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);

        //                int rowsAffected = command.ExecuteNonQuery();
        //                sqlTransaction.Commit();

        //                if (rowsAffected > 0)
        //                {
        //                    MessageBox.Show("อัปเดตข้อมูลสำเร็จ");

        //                    pbMenuImage.Image = null;
        //                    tbMenuId.Clear();
        //                    tbMenuName.Clear();
        //                    tbMenuPrice.Clear();
        //                    btSave.Enabled = true;
        //                    btUpdate.Enabled = false;
        //                    btDelete.Enabled = false;
        //                    getAllMenuToListView();

        //                    // รีเซ็ตตัวแปรภาพใหม่
        //                    menuImage = null;
        //                }
        //                else
        //                {
        //                    MessageBox.Show("ไม่พบข้อมูลที่จะอัปเดต");
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show("พบข้อผิดพลาด :" + ex.Message);
        //        }
        //    }
        //}


        private void btClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lvShowAllMenu_ItemActivate(object sender, EventArgs e)
        {
            var item = lvShowAllMenu.SelectedItems[0];
            if (!string.IsNullOrEmpty(item.ImageKey) && lvShowAllMenu.SmallImageList.Images.ContainsKey(item.ImageKey))
            {
                pbMenuImage.Image = lvShowAllMenu.SmallImageList.Images[item.ImageKey];
            }
            else
            {
                pbMenuImage.Image = null;
            }
            tbMenuId.Text = lvShowAllMenu.SelectedItems[0].SubItems[1].Text;
            tbMenuName.Text = lvShowAllMenu.SelectedItems[0].SubItems[2].Text;
            tbMenuPrice.Text = lvShowAllMenu.SelectedItems[0].SubItems[3].Text;
            btSave.Enabled = false;
            btUpdate.Enabled = true;
            btDelete.Enabled = true;
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            pbMenuImage.Image = null;
            tbMenuId.Clear();
            tbMenuName.Clear();
            tbMenuPrice.Clear();
        }

        private void btDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("ต้องการลบหรือไม่", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                string connectionString = @"Server=MSI\SQLEXPRESS;Database=coffee_cafe_db;trusted_Connection=True;";
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string deleteSQL = "DELETE FROM menu_tb WHERE menuId = @menuId";

                        using (SqlCommand command = new SqlCommand(deleteSQL, sqlConnection, sqlTransaction))
                        {
                            command.Parameters.AddWithValue("@menuId", int.Parse(tbMenuId.Text));

                            int rowsAffected = command.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("ลบข้อมูลสำเร็จ");
                                pbMenuImage.Image = null;
                                pbMenuImage.Image = null;
                                tbMenuId.Clear();
                                tbMenuName.Clear();
                                tbMenuPrice.Clear();
                                btSave.Enabled = true;
                                btUpdate.Enabled = false;
                                btDelete.Enabled = false;
                                getAllMenuToListView();
                            }
                            else
                            {
                                MessageBox.Show("ไม่พบข้อมูลที่จะลบ");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด: " + ex.Message);
                    }
                }
            }
        }

        private void btUpdate_Click(object sender, EventArgs e)
        {
            if (tbMenuName.Text.Length == 0)
            {
                alertValidate("กรุณาป้อนชื่อเมนู");
                return;
            }
            if (tbMenuPrice.Text.Length == 0)
            {
                alertValidate("กรุณาป้อนราคาเมนู");
                return;
            }
            if (!float.TryParse(tbMenuPrice.Text, out float price))
            {
                alertValidate("กรุณาป้อนราคาที่ถูกต้อง");
                return;
            }
            if (tbMenuId.Text.Length == 0)
            {
                alertValidate("กรุณาป้อนรหัสเมนู");
                return;
            }

            string connectionString = @"Server=MSI\SQLEXPRESS;Database=coffee_cafe_db;trusted_Connection=True;";

            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open();

                    // เช็คว่าภาพเดิมมีอยู่หรือไม่
                    string checkImageSql = "SELECT menuImage FROM menu_tb WHERE menuId = @menuId";
                    byte[] existingImage = null;
                    using (SqlCommand checkCmd = new SqlCommand(checkImageSql, sqlConnection))
                    {
                        checkCmd.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);
                        object result = checkCmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            existingImage = (byte[])result;
                        }
                    }

                    // ถ้าไม่มีภาพใหม่และไม่มีภาพเก่า แจ้งเตือน
                    if (menuImage == null && existingImage == null)
                    {
                        alertValidate("กรุณาเลือกภาพเมนู");
                        return;
                    }

                    byte[] imageToSave = menuImage ?? existingImage;

                    SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                    string strSQL = "UPDATE menu_tb " +
                                    "SET menuName = @menuName, " +
                                    "menuPrice = @menuPrice, " +
                                    "menuImage = @menuImage " +
                                    "WHERE menuId = @menuId";

                    using (SqlCommand command = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                    {
                        command.Parameters.Add("@menuName", SqlDbType.NVarChar, 100).Value = tbMenuName.Text;
                        command.Parameters.Add("@menuPrice", SqlDbType.Float).Value = price;
                        command.Parameters.Add("@menuImage", SqlDbType.Image).Value = imageToSave;
                        command.Parameters.Add("@menuId", SqlDbType.Int).Value = int.Parse(tbMenuId.Text);

                        int rowsAffected = command.ExecuteNonQuery();
                        sqlTransaction.Commit();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("อัปเดตข้อมูลสำเร็จ");

                            pbMenuImage.Image = null;
                            tbMenuId.Clear();
                            tbMenuName.Clear();
                            tbMenuPrice.Clear();
                            btSave.Enabled = true;
                            btUpdate.Enabled = false;
                            btDelete.Enabled = false;
                            getAllMenuToListView();

                            // รีเซ็ตตัวแปรภาพใหม่
                            menuImage = null;
                        }
                        else
                        {
                            MessageBox.Show("ไม่พบข้อมูลที่จะอัปเดต");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด :" + ex.Message);
                }
            }
        }
    }
}