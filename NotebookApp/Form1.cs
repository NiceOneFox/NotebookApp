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

namespace NotebookApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private SQLiteConnection SqlConnection = null;

        private SQLiteCommandBuilder SqlCommandBuilder = null;

        private SQLiteDataAdapter sqlDataAdapter = null;

        private DataSet dataSet = null;

        /// <summary>
        /// Curent table from database that shows in datagrid
        /// </summary>
        private string currentTable;

        private bool newRowAdding = false;

        /// <summary>
        /// last index of column in table
        /// </summary>
        private int lastIndexTable = 3;

        /// <summary>
        /// Connect to database and retrieve data from view when form load
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {

            GetNotesViewFromDB();
            //dataGridView1.Sort(dataGridView1.Columns["important"], ListSortDirection.Descending);
        }

        /// <summary>
        /// Get view from database and fill datagrid
        /// </summary>
        private void GetNotesViewFromDB()
        {
            DataTable dataTable = new DataTable();
            using (var connection = new SQLiteConnection(Program.
                    GetConnectionStringByName("ConnectionStringToDatabase")))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM Notes_view", connection))
                {
                    using (SQLiteDataReader dataReader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        dataTable.Load(dataReader);
                    }
                }
            }
            dataGridView1.DataSource = dataTable;

        }
        /// <summary>
        /// Load data from database to datagrid
        /// </summary>
        private void LoadData()
        {
            try
            {
                sqlDataAdapter = new SQLiteDataAdapter("Select *, 'Delete' AS [Delete] FROM " + currentTable, SqlConnection);

                SqlCommandBuilder = new SQLiteCommandBuilder(sqlDataAdapter);

                SqlCommandBuilder.GetInsertCommand();
                SqlCommandBuilder.GetUpdateCommand();
                SqlCommandBuilder.GetDeleteCommand();

                dataSet = new DataSet();

                sqlDataAdapter.Fill(dataSet, currentTable);

                dataGridView1.DataSource = null;
                dataGridView1.DataSource = dataSet.Tables[currentTable];

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {

                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridView1[lastIndexTable, i] = linkCell;
                }

                dataGridView1.Refresh();
                dataGridView1.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Reload data in datagrid
        /// </summary>
        private void ReloadData()
        {
            try
            {
                dataSet.Tables[currentTable].Clear();

                sqlDataAdapter.Fill(dataSet, currentTable);

                dataGridView1.DataSource = dataSet.Tables[currentTable];

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {

                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridView1[lastIndexTable, i] = linkCell;
                }

                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Show update text in cell when value has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (!newRowAdding)
                {
                    int rowIndex = dataGridView1.SelectedCells[0].RowIndex;

                    DataGridViewRow editingRow = dataGridView1.Rows[rowIndex];

                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();

                    dataGridView1[lastIndexTable, rowIndex] = linkCell;

                    editingRow.Cells["Delete"].Value = "Update";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        private void dataGridView1_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
        }

        /// <summary>
        /// Show insert text in cell when user added new data
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_UserAddedRow(object sender, DataGridViewRowEventArgs e)
        {
            try
            {
                if (!newRowAdding)
                {
                    newRowAdding = true;

                    int lastRow = dataGridView1.Rows.Count - 2;

                    DataGridViewRow row = dataGridView1.Rows[lastRow];

                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();

                    dataGridView1[lastIndexTable, lastRow] = linkCell;

                    row.Cells["Delete"].Value = "Insert";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Data handler. Delete, insert or update data depend on current command.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == lastIndexTable)
                {
                    string task = dataGridView1.Rows[e.RowIndex].Cells[lastIndexTable].Value.ToString();

                    if (task == "Delete")
                    {
                        if (MessageBox.Show("Are you sure?", "Delete row", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                            == DialogResult.Yes)
                        {
                            int rowIndex = e.RowIndex;

                            dataGridView1.Rows.RemoveAt(rowIndex);

                            dataSet.Tables[currentTable].Rows[rowIndex].Delete(); 

                            sqlDataAdapter.Update(dataSet, currentTable);
                        }

                    }
                    else if (task == "Insert")
                    {
                        int rowIndex = dataGridView1.Rows.Count - 2;
                        DataRow row = dataSet.Tables[currentTable].NewRow();

                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            string headerName = dataGridView1.Columns[i].HeaderText;
                            row[headerName] = dataGridView1.Rows[rowIndex].Cells[headerName].Value;
                        }

                        dataSet.Tables[currentTable].Rows.Add(row);

                        dataSet.Tables[currentTable].Rows.RemoveAt(dataSet.Tables[currentTable].Rows.Count - 1);

                        dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 2);

                        dataGridView1.Rows[e.RowIndex].Cells[lastIndexTable].Value = "Delete";

                        sqlDataAdapter.Update(dataSet, currentTable);

                        newRowAdding = false;
                    }
                    else if (task == "Update")
                    {

                        int r = e.RowIndex;

                        for (int i = 0; i < dataGridView1.Columns.Count; i++)
                        {
                            string headerName = dataGridView1.Columns[i].HeaderText;
                            dataSet.Tables[currentTable].Rows[r][headerName] = dataGridView1.Rows[r].Cells[headerName].Value;
                        }

                        sqlDataAdapter.Update(dataSet, currentTable);
                        dataGridView1.Rows[e.RowIndex].Cells[lastIndexTable].Value = "Delete";

                    }
                    ReloadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Reload data in datagrid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            GetNotesViewFromDB();
        }

        /// <summary>
        /// Open create note form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CreateNoteForm createNoteForm = new CreateNoteForm();
            createNoteForm.Show();
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {

        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        /// <summary>
        /// Get view from database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            GetNotesViewFromDB();
        }

        /// <summary>
        /// Get Notes table from database and allow to edit it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            SqlConnection = new SQLiteConnection(Program.
                GetConnectionStringByName("ConnectionStringToDatabase"));
            SqlConnection.Open();

            lastIndexTable = 3;
            currentTable = "Notes";
            LoadData();
        }
    }
}
