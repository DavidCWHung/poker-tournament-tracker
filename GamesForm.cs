using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokerTournamentTracker
{
    public partial class GamesForm : Form
    {
        private OleDbConnection connection;
        private OleDbDataAdapter dataAdapter;
        private DataSet dataSet; // TO-DO: Think if dataset should be re-used for Hands

        private BindingSource gamesBindingSource;
        
        public GamesForm()
        {
            InitializeComponent();
            this.InitializeForm();
            this.RetrieveDataFromTheDatabase();
            this.BindControls();
            this.dgvGames.DataBindingComplete += DgvGames_DataBindingComplete;

            this.mnuDataHands.Click += MnuDataHands_Click; 

            this.mnuFileClose.Click += MnuFileClose_Click;
            this.mnuFileSave.Click += MnuFileSave_Click;
            this.mnuEditDelete.Click += MnuEditDelete_Click;
            this.dgvGames.RowHeaderMouseDoubleClick += DgvGames_RowHeaderMouseDoubleClick;

            this.dgvGames.CellValueChanged += DgvGames_CellValueChanged;
            this.dataAdapter.RowUpdated += DataAdapter_RowUpdated;
            this.dgvGames.SelectionChanged += DgvGames_SelectionChanged;
            this.dgvGames.CellEndEdit += DgvGames_CellEndEdit;
            this.dgvGames.CellValidated += DgvGames_CellValidated;

            this.FormClosing += GamesForm_FormClosing;
            this.FormClosed += GamesForm_FormClosed;
        }

        /// <summary>
        /// Handles the CellValidated event of the games data grid view.
        /// </summary>
        private void DgvGames_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            if (this.dgvGames.Rows[e.RowIndex].Cells["Duration"].Value != DBNull.Value)
            {
                if (double.Parse(this.dgvGames.Rows[e.RowIndex].Cells["Duration"].Value.ToString()) <= 0)
                {
                    this.dgvGames.Rows[e.RowIndex].Cells["Duration"].ErrorText = "Duration should be greater than zero.";
                }
                else
                {
                    this.dgvGames.Rows[e.RowIndex].Cells["Duration"].ErrorText = null;

                }
            }
        }

        ///// <summary>
        ///// Handles the CellEndEdit event of the games data grid view.
        ///// </summary>
        private void DgvGames_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == this.dgvGames.Columns["StartingTime"].Index && this.dgvGames.CurrentCell.Value is DateTime ||
                e.ColumnIndex == this.dgvGames.Columns["EndingTime"].Index && this.dgvGames.CurrentCell.Value is DateTime)
            {
                DateTime time = DateTime.Parse(this.dgvGames.CurrentCell.Value.ToString());

                // Adjust the time to a set date for simplified calculation.
                DateTime adjustedTime = new DateTime(1, 1, 1, time.Hour,
                                                              time.Minute,
                                                              time.Second);

                this.dgvGames.CurrentCell.Value = adjustedTime;
            }
        }

        /// <summary>
        /// Handles the DataBindingComplete event of the game data grid view.
        /// </summary>
        private void DgvGames_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvGames.Columns["StartingTime"].DefaultCellStyle.Format = "t";
            this.dgvGames.Columns["EndingTime"].DefaultCellStyle.Format = "t";
            this.dgvGames.Columns["Duration"].DefaultCellStyle.Format = "N2";
            this.dgvGames.Columns["BuyIn"].DefaultCellStyle.Format = "C";
            this.dgvGames.Columns["CashOut"].DefaultCellStyle.Format = "C";
            this.dgvGames.Columns["HourlyRate"].DefaultCellStyle.Format = "C";

            this.dgvGames.Columns["ID"].Visible = false;

            this.dgvGames.Columns["Comments"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvGames.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
        }

        /// <summary>
        /// Handles the FormClosing event of this Form.
        /// </summary>
        private void GamesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.dataSet.HasChanges())
            {
                DialogResult result = MessageBox.Show("Do you want to save changes?",
                                                      "Save",
                                                      MessageBoxButtons.YesNoCancel,
                                                      MessageBoxIcon.Warning,
                                                      MessageBoxDefaultButton.Button3);

                switch (result)
                {
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        break;

                    case DialogResult.No:
                        break;

                    case DialogResult.Yes:
                        this.dgvGames.EndEdit();
                        this.gamesBindingSource.EndEdit();

                        this.dataAdapter.Update(this.dataSet, "Games");
                        break;
                }
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the games data grid view.
        /// </summary>
        private void DgvGames_SelectionChanged(object sender, EventArgs e)
        {
            this.mnuDataHands.Enabled = false;

            if (this.dgvGames.SelectedRows.Count > 0 && 
                !this.dgvGames.SelectedRows[0].IsNewRow && 
                !this.dgvGames.SelectedRows[0].Cells["ID"].Value.ToString().Equals(string.Empty))
            {
                this.mnuDataHands.Enabled = true;
            }
        }

        /// <summary>
        /// Handles the CellValueChanged event of the games data grid view.
        /// </summary>
        private void DgvGames_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            this.Text = "* Games";
            
            if (e.ColumnIndex == this.dgvGames.Columns["StartingTime"].Index || 
                e.ColumnIndex == this.dgvGames.Columns["EndingTime"].Index)
            {
                // Populate the duration in hours based on starting and ending times
                if (!this.dgvGames.CurrentRow.Cells["StartingTime"].Value.ToString().Equals(string.Empty) &&
                    !this.dgvGames.CurrentRow.Cells["EndingTime"].Value.ToString().Equals(string.Empty))
                {
                    DateTime startingTime = DateTime.Parse(this.dgvGames.CurrentRow.Cells["StartingTime"].Value.ToString());
                    DateTime endingTime = DateTime.Parse(this.dgvGames.CurrentRow.Cells["EndingTime"].Value.ToString());

                    double durationInHours = (endingTime - startingTime).TotalHours;

                    this.dgvGames.CurrentRow.Cells["Duration"].Value = durationInHours;
                }
            }

            if (e.ColumnIndex == this.dgvGames.Columns["StartingTime"].Index ||
                e.ColumnIndex == this.dgvGames.Columns["EndingTime"].Index ||
                e.ColumnIndex == this.dgvGames.Columns["Duration"].Index ||
                e.ColumnIndex == this.dgvGames.Columns["BuyIn"].Index || 
                e.ColumnIndex == this.dgvGames.Columns["CashOut"].Index)
            {
                // Populate the hourly rate based on the cashed out and buy-in amounts and duration.
                if (!this.dgvGames.CurrentRow.Cells["Duration"].Value.ToString().Equals(string.Empty) &&
                    !this.dgvGames.CurrentRow.Cells["CashOut"].Value.ToString().Equals(string.Empty) &&
                    !this.dgvGames.CurrentRow.Cells["BuyIn"].Value.ToString().Equals(string.Empty))
                {
                    double durationInHours = double.Parse(this.dgvGames.CurrentRow.Cells["Duration"].Value.ToString());

                    decimal cashOut = decimal.Parse(this.dgvGames.CurrentRow.Cells["CashOut"].Value.ToString());
                    decimal buyIn = decimal.Parse(this.dgvGames.CurrentRow.Cells["BuyIn"].Value.ToString());
                    decimal profit = cashOut - buyIn;


                    // TO-DO
                    if (profit > 0)
                    {
                        try
                        {
                            decimal hourlyRate = profit / (decimal)durationInHours;

                            this.dgvGames.CurrentRow.Cells["HourlyRate"].Value = hourlyRate;

                            //if (hourlyRate < 0)
                            //{
                            //    MessageBox.Show("Duration should be greater than zero! Try again.");
                            //}

                        }
                        catch (DivideByZeroException)
                        {
                            MessageBox.Show("Duration should be greater than zero! Try again.",
                                            "Error",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                        }
                    }
                    else
                    {
                        this.dgvGames.CurrentRow.Cells["HourlyRate"].Value = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the FormClosed event of this Form.
        /// </summary>
        private void GamesForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.connection.Close();
            this.connection.Dispose();
        }

        /// <summary>
        /// Handles the Click event of the delete menu strip item.
        /// </summary>
        private void MnuEditDelete_Click(object sender, EventArgs e)
        {
            if (this.dgvGames.SelectedRows.Count == 0)
            {
                foreach(DataGridViewCell cell in this.dgvGames.SelectedCells)
                {
                    cell.Value = DBNull.Value;
                }
            }
            else
            {
                foreach (DataGridViewRow row in this.dgvGames.SelectedRows)
                {
                    if (!row.Cells["ID"].Value.ToString().Equals(string.Empty))
                    {
                        string warning = String.Format("Are you sure you want to permanently delete \nGame \"{0}\"",
                                   row.Cells["Event"].Value);

                        string caption = "Delete Game";

                        DialogResult result = MessageBox.Show(warning,
                                                              caption,
                                                              MessageBoxButtons.YesNo,
                                                              MessageBoxIcon.Warning,
                                                              MessageBoxDefaultButton.Button2);

                        if (result == DialogResult.Yes)
                        {
                            this.dataSet.Tables["Games"].Rows[row.Index].Delete();

                            this.dataAdapter.Update(this.dataSet, "Games");
                        }
                    }
                    else
                    {
                        // Clear all cells in the selected row.
                        this.dgvGames.Rows.RemoveAt(row.Index);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the RowUpdated event of the data adapter.
        /// </summary>
        private void DataAdapter_RowUpdated(object sender, OleDbRowUpdatedEventArgs e)
        {
            if (e.StatementType == StatementType.Insert)
            {
                OleDbCommand command = new OleDbCommand("SELECT @@IDENTITY", this.connection);

                e.Row["ID"] = command.ExecuteScalar();
            }
        }

        /// <summary>
        /// Handles the Click event of the save menu strip item.
        /// </summary>
        private void MnuFileSave_Click(object sender, EventArgs e)
        {
            this.dgvGames.EndEdit();
            this.gamesBindingSource.EndEdit();

            this.dataAdapter.Update(this.dataSet, "Games");
            this.Text = "Games";
        }

        /// <summary>
        /// Handles the Click event of the close menu strip item.
        /// </summary>
        private void MnuFileClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the RowHeaderMouseDoubleClick event of the games data grid view.
        /// </summary>
        private void DgvGames_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (this.dgvGames.CurrentRow.Cells["ID"].Value != DBNull.Value)
            {
                int gameId = int.Parse(this.dgvGames.CurrentRow.Cells["ID"].Value.ToString());
                string eventName = this.dgvGames.CurrentRow.Cells["Event"].Value.ToString();

                this.CreateNewHandsMasterForm(gameId, eventName);
            }
        }

        /// <summary>
        /// Creates an instance of a hands master form and displays it as a MDI Child Form.
        /// </summary>
        /// <param name="gameId">The game ID of the related poker tournament game.</param>
        private void CreateNewHandsMasterForm(int gameId, string eventName)
        {
            HandsMasterForm handsMasterForm = new HandsMasterForm(gameId, eventName);
            handsMasterForm.MdiParent = this.MdiParent;
            handsMasterForm.Show();
        }

        /// <summary>
        /// Handles the Click event of the hands menu strip item.
        /// </summary>
        private void MnuDataHands_Click(object sender, EventArgs e)
        {
            for (int i = this.dgvGames.SelectedRows.Count - 1;
                 i >= 0 &&
                 !this.dgvGames.SelectedRows[i].IsNewRow &&
                 !this.dgvGames.SelectedRows[i].Cells["ID"].Value.ToString().Equals(string.Empty); 
                 i--)
            {
                int gameId = int.Parse(this.dgvGames.SelectedRows[i].Cells["ID"].Value.ToString());
                string eventName = this.dgvGames.SelectedRows[i].Cells["Event"].Value.ToString();

                this.CreateNewHandsMasterForm(gameId, eventName);
            }
        }

        /// <summary>
        /// Queries the database and populates a dataset.
        /// </summary>
        private void RetrieveDataFromTheDatabase()
        {
            this.connection = new OleDbConnection();
            this.connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=PokerTournamentDatabase.accdb";
            this.connection.Open();

            OleDbCommand selectCommand = new OleDbCommand("SELECT * FROM Games", this.connection);

            this.dataAdapter = new OleDbDataAdapter(selectCommand);

            this.dataSet = new DataSet();
            this.dataAdapter.Fill(this.dataSet, "Games");

            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(this.dataAdapter);
            //commandBuilder.ConflictOption = ConflictOption.OverwriteChanges;
        }

        /// <summary>
        /// Binds the controls with data for this Form.
        /// </summary>
        private void BindControls()
        {
            this.gamesBindingSource = new BindingSource();
            this.gamesBindingSource.DataSource = this.dataSet.Tables["Games"];

            this.dgvGames.DataSource = this.gamesBindingSource;
        }

        /// <summary>
        /// Initialize this form to its initial state.
        /// </summary>
        private void InitializeForm()
        {
            this.mnuDataHands.Enabled = false;
        }
    }
}