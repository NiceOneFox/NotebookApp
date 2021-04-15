using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SQLite;

namespace NotebookApp
{
    public partial class CreateNoteForm : Form
    {
        public CreateNoteForm()
        {
            InitializeComponent();
        }

        private void CreateNoteForm_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Get text from richTextbox and value from checkBox and add data to database also refresh datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text.Equals("") || richTextBox1.Text == null)
            {
                MessageBox.Show("Please enter note.\nNote can not be empty");
            }
            else
            {
                using (var connection = new SQLiteConnection(Program.
                    GetConnectionStringByName("ConnectionStringToDatabase")))
                {
                    connection.Open();

                    int check = checkBox1.Checked ? 1 : 0;

                    using (SQLiteCommand command = new SQLiteCommand($"INSERT INTO Notes(text, important) VALUES('{richTextBox1.Text}', {check})", connection))
                    {
                        command.ExecuteNonQuery();
                    }
                    MessageBox.Show("Note has been added");
                }
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }
    }
}
