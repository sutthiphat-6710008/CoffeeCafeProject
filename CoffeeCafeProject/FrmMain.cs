using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace CoffeeCafeProject
{
    public partial class FrmMain : Form
    {
        float[] menuPrice = new float[10];
        int memberId = 0;
        public FrmMain()
        {
            InitializeComponent();
        }

        private void btMenu_Click(object sender, EventArgs e)
        {
            FrmMenu frmMenu = new FrmMenu();
            frmMenu.ShowDialog();
            resetForm(); //รีเซ็ตหน้าจอหลังจากเปิด FrmMenu
        }

        private void btMember_Click(object sender, EventArgs e)
        {
            FrmMember frmMember = new FrmMember();
            frmMember.ShowDialog();
            resetForm();
        }

        private void resetForm()
        {
            memberId = 0; //เคลียร์ memberId ให้เป็น 0
            //ให้ rdMemberNo, rdMemberYes ไม่ถูกเลือก
            rdMenberNo.Checked = false;
            rdMemberYes.Checked = false;
            //ให้ tbMemberPhone ว่าง และใช้งานไม่ได้
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = false;
            //ให้ tbMemberName เป็นข้อความ (ชื่อสมาชิก)
            tbMemberName.Text = "(ชื่อสมาชิก)";
            //ให้ lbMemberScore เป็น 0
            lbMemberScore.Text = "0";
            //ให้ lbOrderPay เป็น 0.00
            lbOrderPay.Text = "0.00";
            //เคลีย lvOrderMenu 
            lvOrderMenu.Items.Clear();
            lvOrderMenu.Columns.Clear();
            lvOrderMenu.FullRowSelect = true; //เลือกแถวเต็ม
            lvOrderMenu.View = View.Details;
            lvOrderMenu.Columns.Add("ชื่อเมนู", 150, HorizontalAlignment.Left);
            lvOrderMenu.Columns.Add("ราคา", 80, HorizontalAlignment.Left);
            //ดึงข้อมูลรายการเมนูจาก Database มาแสดงที่หน้าจอ และเก็บไว้ใช้กับตอนที่ผู้ใช้เลือกสั่งเมนู
            //สร้าง Connection ไปยังฐานข้อมูล
            using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
            {
                try
                {
                    sqlConnection.Open(); //เปิดการเชื่อมต่อไปยังฐานข้อมูล

                    //สร้างคำสั่ง SQL ในที่นี้คือ ดึงข้อมูลทั้งหมดจากตาราง menu_tb
                    string strSQL = "SELECT menuName, menuPrice, menuImage FROM menu_tb";

                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        //เอาข้อมูลที่ได้จาก strSQL ซึ่งเป็นก้อนใน dataAdapter มาทำให้เป็นตารางโดยใส่ไว้ใน dataTable
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        //สร้างตัวแปรอ้างถึง PictureBox และ Button ที่จะเอารูปและชื่อเมนูไปแสดง
                        PictureBox[] pbMenuImage = { pbMenu1, pbMenu2, pbMenu3, pbMenu4, pbMenu5, pbMenu6, pbMenu7, pbMenu8, pbMenu9, pbMenu10 };
                        Button[] btMenuName = { btMenu1, btMenu2, btMenu3, btMenu4, btMenu5, btMenu6, btMenu7, btMenu8, btMenu9, btMenu10 };

                        for (int i = 0; i < 10; i++)
                        {
                            pbMenuImage[i].Image = Properties.Resources.menu; //เคลียร์รูปภาพใน PictureBox
                            btMenuName[i].Text = "Menu"; //เคลียร์ชื่อเมนูใน Button
                        }

                        //วนลูปเอาข้อมูลที่อยู่ใน dataTable กำหนดให้กับ pbMenuImage, btMenuName, menPrice
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            btMenuName[i].Text = dataTable.Rows[i]["menuName"].ToString();
                            menuPrice[i] = float.Parse(dataTable.Rows[i]["menuPrice"].ToString());
                            //เอารูปไปกำหนดให้กับ pbMenuImage
                            if (dataTable.Rows[i]["menuImage"] != DBNull.Value)
                            {
                                //กรณีมีรูปต้องแปลงจาก Binary ให้เป็นรูป
                                byte[] imgByte = (byte[])dataTable.Rows[i]["menuImage"];
                                using (var ms = new System.IO.MemoryStream(imgByte))
                                {
                                    pbMenuImage[i].Image = System.Drawing.Image.FromStream(ms);
                                }
                            }
                            else
                            {
                                //กรณีไม่มีรูป
                                pbMenuImage[i].Image = Properties.Resources.menu;
                            }
                        }

                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                }
            }

        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            resetForm();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            resetForm();
        }

        private void rdMenberNo_CheckedChanged(object sender, EventArgs e)
        {
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = false; //ไม่สามารถกรอกเบอร์โทรได้
            tbMemberName.Text = "(ชื่อสมาชิก)"; //แสดงข้อความ (ชื่อสมาชิก)
            lbMemberScore.Text = "0"; //คะแนนสมาชิกเป็น 0
            memberId = 0; //เคลียร์ memberId ให้เป็น 0
        }

        private void rdMemberYes_CheckedChanged(object sender, EventArgs e)
        {
            tbMemberPhone.Clear();
            tbMemberPhone.Enabled = true; //ไม่สามารถกรอกเบอร์โทรได้
            tbMemberName.Text = "(ชื่อสมาชิก)"; //แสดงข้อความ (ชื่อสมาชิก)
            lbMemberScore.Text = "0"; //คะแนนสมาชิกเป็น 0
        }

        private void tbMemberPhone_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open(); // เปิดการเชื่อมต่อไปยังฐานข้อมูล
                        string strSQL = "SELECT memberId, memberName, memberScore FROM member_tb WHERE memberPhone=@memberPhone";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection))
                        {
                            sqlCommand.Parameters.Add("@memberPhone", SqlDbType.VarChar, 50).Value = tbMemberPhone.Text;
                            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(sqlCommand))
                            {
                                DataTable dataTable = new DataTable();
                                dataAdapter.Fill(dataTable);

                                if (dataTable.Rows.Count == 1)
                                {
                                    tbMemberName.Text = dataTable.Rows[0]["memberName"].ToString();

                                    // ดึงคะแนนเดิมของสมาชิก
                                    int currentScore = int.Parse(dataTable.Rows[0]["memberScore"].ToString());

                                    // นับจำนวนเมนูที่เลือกไว้ก่อนหน้า แล้วเพิ่มคะแนน
                                    int additionalScore = lvOrderMenu.Items.Count;
                                    lbMemberScore.Text = (currentScore + additionalScore).ToString();

                                    // เก็บ memberId สำหรับใช้ตอนบันทึกการสั่งซื้อ
                                    memberId = int.Parse(dataTable.Rows[0]["memberId"].ToString());
                                }
                                else
                                {
                                    MessageBox.Show("กรุณากรอกเบอร์โทรศัพท์สมาชิกให้ถูกต้อง");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด กรุณาลองใหม่หรือติดต่อ IT : " + ex.Message);
                    }
                }
            }
        }

        private void btMenu1_Click(object sender, EventArgs e)
        {
            if (btMenu1.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu1.Text);
                item.SubItems.Add(menuPrice[0].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[0]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu2_Click(object sender, EventArgs e)
        {
            if (btMenu2.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu2.Text);
                item.SubItems.Add(menuPrice[1].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[1]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu3_Click(object sender, EventArgs e)
        {
            if (btMenu3.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu3.Text);
                item.SubItems.Add(menuPrice[2].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[2]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu4_Click(object sender, EventArgs e)
        {
            if (btMenu4.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu4.Text);
                item.SubItems.Add(menuPrice[3].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[3]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu5_Click(object sender, EventArgs e)
        {
            if (btMenu5.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu5.Text);
                item.SubItems.Add(menuPrice[4].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[4]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu6_Click(object sender, EventArgs e)
        {
            if (btMenu6.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu6.Text);
                item.SubItems.Add(menuPrice[5].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[5]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu7_Click(object sender, EventArgs e)
        {
            if (btMenu7.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu7.Text);
                item.SubItems.Add(menuPrice[6].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[6]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu8_Click(object sender, EventArgs e)
        {
            if (btMenu8.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu8.Text);
                item.SubItems.Add(menuPrice[7].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[7]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu9_Click(object sender, EventArgs e)
        {
            if (btMenu9.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu9.Text);
                item.SubItems.Add(menuPrice[8].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[8]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btMenu10_Click(object sender, EventArgs e)
        {
            if (btMenu10.Text != "Menu")
            {
                ListViewItem item = new ListViewItem(btMenu10.Text);
                item.SubItems.Add(menuPrice[9].ToString("0.00")); //แสดงราคาเมนูที่เลือก
                lvOrderMenu.Items.Add(item); //เพิ่มรายการเมนูที่เลือกลงใน ListView

                if (tbMemberName.Text != "(ชื่อสมาชิก)")
                {
                    lbMemberScore.Text = (int.Parse(lbMemberScore.Text) + 1).ToString(); //เพิ่มคะแนนสมาชิก 1 คะแนน
                }


                lbOrderPay.Text = (float.Parse(lbOrderPay.Text) + menuPrice[9]).ToString("0.00"); //คำนวณยอดรวมที่ต้องจ่าย
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            if (lbOrderPay.Text == "0.00")
            {
                MessageBox.Show("กรุณาเลือกเมนูที่ต้องการสั่งซื้อก่อน", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
            else if (rdMemberYes.Checked != true && rdMenberNo.Checked != true)
            {
                MessageBox.Show(Text = "กรุณาเลือกประเภทสมาชิกก่อน", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (rdMemberYes.Checked == true && tbMemberName.Text == "(ชื่อสมาชิก)")
            {
                MessageBox.Show("กรุณากรอกเบอร์โทรศัพท์สมาชิกก่อน", "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                using (SqlConnection sqlConnection = new SqlConnection(ShareResource.connectionString))
                {
                    try
                    {
                        sqlConnection.Open();
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSQL1 = "INSERT INTO order_tb (memberId, orderPay, createAt, updateAT) " +
                                          "VALUES(@memberId, @orderPay, @createAt, @updateAT); " +
                                         "SELECT CAST(SCOPE_IDENTITY() AS INT)"; 
                        int orderId; //ตัวแปรสำหรับเก็บ Order ID ที่จะใช้ในการบันทึกรายการเมนูที่สั่งซื้อ

                        using (SqlCommand sqlcommand = new SqlCommand(strSQL1, sqlConnection, sqlTransaction))
                        {
                            sqlcommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberId;
                            sqlcommand.Parameters.Add("@orderPay", SqlDbType.Float).Value = float.Parse(lbOrderPay.Text);
                            sqlcommand.Parameters.Add("@createAt", SqlDbType.DateTime).Value = DateTime.Now;
                            sqlcommand.Parameters.Add("@updateAT", SqlDbType.DateTime).Value = DateTime.Now;

                            orderId = (int)sqlcommand.ExecuteScalar(); //บันทึกข้อมูลการสั่งซื้อและรับ Order ID ที่สร้างขึ้นใหม่
                        }
                        foreach (ListViewItem item in lvOrderMenu.Items)
                        {
                            string strSQL2 = "INSERT INTO order_detail_tb (orderId, menuName,menuPrice) "+
                                             "VALUES(@orderId, @menuName, @menuPrice)";


                            using (SqlCommand sqlcommand = new SqlCommand(strSQL2, sqlConnection, sqlTransaction))
                            {
                                sqlcommand.Parameters.Add("@orderId", SqlDbType.Int).Value = orderId;
                                sqlcommand.Parameters.Add("@menuName", SqlDbType.VarChar,100).Value = item.SubItems[0].Text ;
                                sqlcommand.Parameters.Add("@menuPrice", SqlDbType.Float).Value = float.Parse(item.SubItems[1].Text);

                                sqlcommand.ExecuteNonQuery(); //บันทึกรายการเมนูที่สั่งซื้อ
                            }
                        }

                        if (rdMemberYes.Checked == true)
                        {
                            string strSQL3 = "UPDATE member_tb SET memberScore = @memberScore WHERE memberId = @memberId";


                            using (SqlCommand sqlcommand = new SqlCommand(strSQL3, sqlConnection, sqlTransaction))
                            {
                                sqlcommand.Parameters.Add("@memberScore", SqlDbType.Int).Value = int.Parse(lbMemberScore.Text);
                                sqlcommand.Parameters.Add("@memberId", SqlDbType.Int).Value = memberId;

                                sqlcommand.ExecuteNonQuery(); //บันทึกรายการเมนูที่สั่งซื้อ
                            }

                        }

                        sqlTransaction.Commit(); //ยืนยันการทำธุรกรรม
                        MessageBox.Show("บันทึกข้อมูลการสั่งซื้อเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        resetForm(); //รีเซ็ตหน้าจอหลังจากบันทึกข้อมูล
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("พบข้อผิดพลาด :" + ex.Message);
                    }
                }
            }
        }

        private void lvOrderMenu_ItemActivate(object sender, EventArgs e)
        {
            if (lvOrderMenu.SelectedItems.Count > 0)
            {
                // ดึงรายการที่เลือก
                ListViewItem selectedItem = lvOrderMenu.SelectedItems[0];

                // แสดงกล่องยืนยันก่อนลบ
                DialogResult result = MessageBox.Show(
                    "คุณต้องการลบเมนู '" + selectedItem.SubItems[0].Text + "' ออกจากรายการใช่หรือไม่?",
                    "ยืนยันการลบ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    // ดึงราคาจาก SubItem ที่ 1
                    float price = float.Parse(selectedItem.SubItems[1].Text);

                    // ลบออกจากยอดรวมที่ต้องจ่าย
                    lbOrderPay.Text = (float.Parse(lbOrderPay.Text) - price).ToString("0.00");

                    // ลบคะแนนสมาชิกถ้ามี
                    if (tbMemberName.Text != "(ชื่อสมาชิก)")
                    {
                        int currentScore = int.Parse(lbMemberScore.Text);
                        if (currentScore > 0)
                        {
                            lbMemberScore.Text = (currentScore - 1).ToString();
                        }
                    }

                    // ลบรายการออกจาก ListView
                    lvOrderMenu.Items.Remove(selectedItem);
                }
            }
        }
    }
}
