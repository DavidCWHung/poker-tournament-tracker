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
    public partial class HandsMasterForm : Form
    {
        private OleDbConnection connection;
        private OleDbDataAdapter dataAdapter;
        private DataSet dataSet;

        private BindingSource handsMasterBindingSource;

        private int gameId;
        private string eventName;

        public HandsMasterForm(int gameId, string eventName)
        {
            this.gameId = gameId;
            this.eventName = eventName;

            InitializeComponent();
            this.InitializeForm(eventName);
            
            this.RetrieveDataFromTheDatabase(gameId);
            this.BindControls();
            this.dgvHandsMaster.DataBindingComplete += DgvHandsMaster_DataBindingComplete;

            this.mnuFileClose.Click += MnuFileClose_Click;
            this.mnuFileSave.Click += MnuFileSave_Click;
            this.mnuEditDelete.Click += MnuEditDelete_Click;
            this.dgvHandsMaster.RowHeaderMouseDoubleClick += DgvHandsMaster_RowHeaderMouseDoubleClick;

            this.dgvHandsMaster.DefaultValuesNeeded += DgvHandsMaster_DefaultValuesNeeded;
            this.dataAdapter.RowUpdated += DataAdapter_RowUpdated;

            this.FormClosing += HandsMasterForm_FormClosing;
            this.FormClosed += HandsMasterForm_FormClosed;

            this.Shown += HandsMasterForm_Shown;
        }

        /// <summary>
        /// Handles the DataBindingComplete event of the hands master data grid view.
        /// </summary>
        private void DgvHandsMaster_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvHandsMaster.Columns["BlindLevel"].DefaultCellStyle.Format = "C0";

            this.dgvHandsMaster.Columns["ID"].Visible = false;
            this.dgvHandsMaster.Columns["GameID"].Visible = false;

            this.dgvHandsMaster.Columns["Comments"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvHandsMaster.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;
        }

        /// <summary>
        /// Handles the Shown event of this Form.
        /// </summary>
        private void HandsMasterForm_Shown(object sender, EventArgs e)
        {
            this.dgvHandsMaster.CellValueChanged += DgvHandsMaster_CellValueChanged;
        }

        /// <summary>
        /// Handles the FormClosing event of this Form.
        /// </summary>
        private void HandsMasterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.dgvHandsMaster.EndEdit();
            this.handsMasterBindingSource.EndEdit();

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
                        this.dgvHandsMaster.EndEdit();
                        this.handsMasterBindingSource.EndEdit();

                        this.dataAdapter.Update(this.dataSet, "Hands_Master");
                        break;
                }
            }
        }

        /// <summary>
        /// Handles the CellValueChanged event of the hands master data grid view.
        /// </summary>
        private void DgvHandsMaster_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != this.dgvHandsMaster.Columns["GameID"].Index)
            {
                this.Text = String.Format("* Hands Master - {0}", this.eventName);
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
        /// Handles the Click event of the delete menu strip item.
        /// </summary>
        private void MnuEditDelete_Click(object sender, EventArgs e)
        {
            if (this.dgvHandsMaster.SelectedRows.Count == 0)
            {
                // Clear the cell value when no rows are selected.
                foreach (DataGridViewCell cell in this.dgvHandsMaster.SelectedCells)
                {
                    cell.Value = DBNull.Value;
                }
            }
            else
            {
                // Delete the row(s) when they are selected.
                foreach (DataGridViewRow row in this.dgvHandsMaster.SelectedRows)
                {
                    if (!row.Cells["ID"].Value.ToString().Equals(string.Empty))
                    {
                        string warning = String.Format("Are you sure you want to permanently delete \nHand \"{0}\"?",
                                   row.Cells["MyHand"].Value);

                        string caption = "Delete Hand";

                        DialogResult result = MessageBox.Show(warning,
                                                              caption,
                                                              MessageBoxButtons.YesNo,
                                                              MessageBoxIcon.Warning,
                                                              MessageBoxDefaultButton.Button2);

                        if (result == DialogResult.Yes)
                        {
                            this.dataSet.Tables["Hands_Master"].Rows[row.Index].Delete();

                            this.dataAdapter.Update(this.dataSet, "Hands_Master");
                        }
                    }
                    else
                    {
                        // Clear all cells in the selected row.
                        this.dgvHandsMaster.Rows.RemoveAt(row.Index);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the FormClosed event of this Form.
        /// </summary>
        private void HandsMasterForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.connection.Close();
            this.connection.Dispose();
        }

        /// <summary>
        /// Handles the Click event of the close menu strip item.
        /// </summary>
        private void MnuFileClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the save menu strip item.
        /// </summary>
        private void MnuFileSave_Click(object sender, EventArgs e)
        {
            this.dgvHandsMaster.EndEdit();
            this.handsMasterBindingSource.EndEdit();

            this.dataAdapter.Update(this.dataSet, "Hands_Master");
            this.Text = String.Format("Hands Master - {0}", this.eventName);
        }

        /// <summary>
        /// Handles the DefaultValuesNeeded event of the hands master data grid view.
        /// </summary>
        private void DgvHandsMaster_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["GameID"].Value = this.gameId;
        }

        /// <summary>
        /// Handles the RowHeaderMouseDoubleClick event of the hands master data grid view.
        /// </summary>
        private void DgvHandsMaster_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (this.dgvHandsMaster.CurrentRow.Cells["ID"].Value != DBNull.Value)
            {
                int handsMasterId = int.Parse(this.dgvHandsMaster.CurrentRow.Cells["ID"].Value.ToString());
                string myHand = this.dgvHandsMaster.CurrentRow.Cells["MyHand"].Value.ToString();
                string position = this.dgvHandsMaster.CurrentRow.Cells["Position"].Value.ToString();

                HandDetailsForm handDetailsForm = new HandDetailsForm(handsMasterId, myHand, position);
                handDetailsForm.MdiParent = this.MdiParent;
                handDetailsForm.Show();
            }
        }

        /// <summary>
        /// Queries the database and populates a dataset.
        /// </summary>
        private void RetrieveDataFromTheDatabase(int gameId)
        {
            this.connection = new OleDbConnection();
            this.connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=PokerTournamentDatabase.accdb";
            this.connection.Open();

            string command = String.Format("SELECT * FROM Hands_Master WHERE GameID = {0}", gameId);
            OleDbCommand selectCommand = new OleDbCommand(command, this.connection);

            this.dataAdapter = new OleDbDataAdapter(selectCommand);

            this.dataSet = new DataSet();
            this.dataAdapter.Fill(this.dataSet, "Hands_Master");

            OleDbCommandBuilder commandBuilder = new OleDbCommandBuilder(this.dataAdapter);
            commandBuilder.QuotePrefix = "[";
            commandBuilder.QuoteSuffix = "]";
            commandBuilder.ConflictOption = ConflictOption.OverwriteChanges;
        }

        /// <summary>
        /// Binds the controls with data for this Form.
        /// </summary>
        private void BindControls()
        {
            this.handsMasterBindingSource = new BindingSource();
            this.handsMasterBindingSource.DataSource = this.dataSet.Tables["Hands_Master"];

            this.dgvHandsMaster.DataSource = this.handsMasterBindingSource;
        }

        /// <summary>
        /// Initialize this Form to its initial state.
        /// </summary>
        private void InitializeForm(string eventName)
        {
            this.Text = String.Format("Hands Master - {0}", eventName);
        }
    }
}