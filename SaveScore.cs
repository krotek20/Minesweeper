using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
namespace solb
{
    public partial class SaveScore : Form
    {
        int score;
        int contor = 0;
        public SaveScore(int score)
        {
            this.score = score;
            InitializeComponent();

            lbl_score.Text = "Felicitari! Ai terminat jocul in " + score + " secunde!";
            lbl_score.TextAlign = ContentAlignment.MiddleCenter;

            PopulateListView();
        }

        private void PopulateListView()
        {
            contor = 0;
            listView1.Items.Clear();

            string dbPath = Application.StartupPath + @"\Scoruri.mdf"; // Absolute path

            try
            {
                SqlConnection con = new SqlConnection();
                con.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=" + dbPath + ";Integrated Security=True;User Instance=True";
                con.Open();

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = con;
                cmd.CommandText = "select Nume_Utilizator, Scor from Punctaje order by Scor";
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read() && contor < 10)
                {
                    contor++;
                    ListViewItem item = new ListViewItem(contor.ToString());
                    item.SubItems.Add(reader[0].ToString());
                    item.SubItems.Add(reader[1].ToString());
                    switch (contor)
                    {
                        case 1:
                            item.BackColor = Color.Gold; // first score
                            break;
                        case 2:
                            item.BackColor = Color.Silver; // second score
                            break;
                        case 3:
                            item.BackColor = Color.Chocolate; // third score
                            break;
                    }
                    listView1.Items.Add(item);
                }
                reader.Dispose();
                cmd.Dispose();
                con.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Saving score
        private void button1_Click(object sender, EventArgs e)
        {
            if (tb_nume.Text != "")
            {
                string dbPath = Application.StartupPath + @"\Scoruri.mdf";

                try
                {
                    SqlConnection con = new SqlConnection();
                    con.ConnectionString = @"Data Source=.\SQLEXPRESS;AttachDbFilename=" + dbPath + ";Integrated Security=True;User Instance=True";
                    con.Open();

                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "insert into Punctaje(Nume_Utilizator, Scor) values('" + tb_nume.Text + "','" + score + "')";
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    con.Close();

                    MessageBox.Show("Ti-ai salvat scorul cu succes!");
                    PopulateListView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Introduceti un nume de utilizator!", "Error", MessageBoxButtons.OK ,MessageBoxIcon.Asterisk);
                tb_nume.Focus();
            }
        }
    }
}
