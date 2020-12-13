using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StorageProgram
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Подключение к БД
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            con = new SQLiteConnection("Data Source=" + Application.StartupPath + "\\Storages.db");
            con.Open();
            sda = new SQLiteDataAdapter("SELECT * FROM [Пользователи] Where [Логин]='" + login.Text + "' AND [Пароль]='" + password.Text + "';", con);
            dt = new DataTable();

            if (sda.Fill(dt) == 0)
            {
                button2.Visible = false;
                dgv.ReadOnly = true;
                MessageBox.Show("Неверный логин или пароль!", "Произошла ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if (dt.Rows[0][1].ToString()=="admin")
                {
                    button2.Visible = true;
                    dgv.ReadOnly = false;
                    button3.Visible = true;
                }
                else
                {
                    button2.Visible = false;
                    dgv.ReadOnly = true;
                    button3.Visible = false;
                }

                UpadteCB();
                tabControl1.SelectedIndex = 1;
            }
        }

        /// <summary>
        /// Вывод складов в список
        /// </summary>
        void UpadteCB()
        {
            sda = new SQLiteDataAdapter("SELECT * FROM Склады", con);
            dt = new DataTable();
            sda.Fill(dt);

            storages.Items.Clear();
            countStorahes = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                storages.Items.Add(dt.Rows[i][1].ToString());
                countStorahes++;
            }
            storages.SelectedIndex = storages.Items.Count - 1;
            storages_SelectedIndexChanged(storages,null);
        }

        static int countStorahes = 0;

        SQLiteDataAdapter mainsda;
        DataTable maindt;

        SQLiteDataAdapter sda;
        SQLiteConnection con;
        DataTable dt;
        SQLiteCommand com;

        //Событие закрытия формы
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (con != null)
                con.Close();
        }

        /// <summary>
        /// Номер склада
        /// </summary>
        static int numberStorage = 0;

        /// <summary>
        /// Обновление таблицы при изменении склада
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void storages_SelectedIndexChanged(object sender, EventArgs e)
        {
            sda = new SQLiteDataAdapter("SELECT * FROM Склады WHERE [Склады].[Название]=\""+storages.Text+"\"", con);
            dt = new DataTable();
            sda.Fill(dt);

            numberStorage = int.Parse(dt.Rows[0][0].ToString());

            mainsda = new SQLiteDataAdapter("SELECT * FROM  [Товары] " +
              "WHERE  [Товары].[Код склада]=" + numberStorage, con);


            maindt = new DataTable();

            mainsda.Fill(maindt);
            dgv.DataSource = maindt;
            dgv.Columns[0].Visible = false;
            dgv.Columns[1].Visible = false;
            dgv.ClearSelection();
        }

        /// <summary>
        /// Создание нового склада
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Trim(' ') != "" && con!=null)
            {
                countStorahes++;
                com = new SQLiteCommand("INSERT INTO [Склады] VALUES("+countStorahes+",\""+ textBox1.Text.Trim(' ') + "\");", con);
                com.ExecuteNonQuery();
                UpadteCB();
            }
        }

        /// <summary>
        /// Обновление таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            SQLiteCommandBuilder scb = new SQLiteCommandBuilder(mainsda);
            mainsda.Update(maindt);
            index = 0;
        }

        /// <summary>
        /// Максимальный элемент
        /// </summary>
        static int index = 0;

        /// <summary>
        /// Индексирует строки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgv_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            DataGridView buffer = (DataGridView)sender;

            if (buffer.SelectedCells.Count != 0)
            {
                if (buffer.SelectedCells[0].RowIndex == dgv.RowCount-2)
                {
                    sda = new SQLiteDataAdapter("SELECT Max(Код) FROM Товары", con);
                    dt = new DataTable();
                    sda.Fill(dt);

                    index++;
                    dgv.Rows[buffer.SelectedCells[0].RowIndex].Cells[1].Value = numberStorage.ToString();
                    dgv.Rows[buffer.SelectedCells[0].RowIndex].Cells[0].Value = int.Parse((dt.Rows[0][0]).ToString()) + index;
                }
            }

        }
    }
}
