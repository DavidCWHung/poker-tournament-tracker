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
    public partial class HandDetailsForm : Form
    {
        private OleDbConnection connection;
        private OleDbDataAdapter dataAdapter;
        private DataSet dataSet;

        private BindingSource handDetailsBindingSource;

        private int handsMasterId;
        private string myHand;
        private string position;

        public HandDetailsForm(int handsMasterId, string myHand, string position)
        {
            this.handsMasterId = handsMasterId;
            this.myHand = myHand;
            this.position = position;

            InitializeComponent();
            this.InitializeForm(myHand, position);

            this.RetrieveDataFromTheDatabase(handsMasterId);
            this.BindControls();
            this.dgvHandDetails.DataBindingComplete += DgvHandDetails_DataBindingComplete;

            this.mnuFileClose.Click += MnuFileClose_Click;
            this.mnuFileSave.Click += MnuFileSave_Click;
            this.mnuEditDelete.Click += MnuEditDelete_Click;

            this.dgvHandDetails.DefaultValuesNeeded += DgvHandDetails_DefaultValuesNeeded;
            this.dataAdapter.RowUpdated += DataAdapter_RowUpdated;

            this.FormClosing += HandDetailsForm_FormClosing;
            this.FormClosed += HandDetailsForm_FormClosed;

            this.dgvHandDetails.CellValueChanged += DgvHandDetails_CellValueChanged;
            this.Shown += HandDetailsForm_Shown;
        }

        /// <summary>
        /// Handles the DataBindingComplete event of the hand details data grid view.
        /// </summary>
        private void DgvHandDetails_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            this.dgvHandDetails.Columns["MyStack"].DefaultCellStyle.Format = "C";
            this.dgvHandDetails.Columns["PotSize"].DefaultCellStyle.Format = "C";

            this.dgvHandDetails.Columns["ID"].Visible = false;
            this.dgvHandDetails.Columns["HandsMasterID"].Visible = false;

            this.dgvHandDetails.Columns["Comments"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            this.dgvHandDetails.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders;

            // Populate the rounds for each hand when there is no existing record.



            //if (this.dgvHandDetails.Rows[0].Cells["ID"].Value != DBNull.Value)
            //{
            //    string[] round = { "Pre-Flop", "Flop", "Turn", "River" };

            //    MessageBox.Show(this.dgvHandDetails.Rows[0].Cells["ID"].Value.ToString());

            //    //for (int i = 0; i < round.Length; i++)
            //    //{

            //    //    MessageBox.Show(round[i]);

            //    //    this.dgvHandDetails.Rows[i].Cells["Round"].Value = round[i];

            //    //}
            //}
        }

        /// <summary>
        /// Handles the FormClosing event of this Form.
        /// </summary>
        private void HandDetailsForm_FormClosing(object sender, FormClosingEventArgs e)
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
                        this.dgvHandDetails.EndEdit();
                        this.handDetailsBindingSource.EndEdit();

                        this.dataAdapter.Update(this.dataSet, "Hand_Details");
                        break;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the delete menu strip item.
        /// </summary>
        private void MnuEditDelete_Click(object sender, EventArgs e)
        {
            if (this.dgvHandDetails.SelectedRows.Count == 0)
            {
                // Clear all the cell value when no rows are selected.
                foreach(DataGridViewCell cell in this.dgvHandDetails.SelectedCells)
                {
                    cell.Value = DBNull.Value;
                }
            }
            else
            {
                // Delete the row(s) when they are selected.
                foreach (DataGridViewRow row in this.dgvHandDetails.SelectedRows)
                {
                    if (!row.Cells["ID"].Value.ToString().Equals(string.Empty))
                    {
                        string warning = String.Format("Are you sure you want to permanently delete \nRound \"{0}\"?",
                                                       row.Cells["Round"].Value);

                        string caption = "Delete Round";

                        DialogResult result = MessageBox.Show(warning,
                                                              caption,
                                                              MessageBoxButtons.YesNo,
                                                              MessageBoxIcon.Warning,
                                                              MessageBoxDefaultButton.Button2);

                        if (result == DialogResult.Yes)
                        {
                            this.dataSet.Tables["Hand_Details"].Rows[row.Index].Delete();

                            this.dataAdapter.Update(this.dataSet, "Hand_Details");
                        }
                    }
                    else
                    {
                        // Clear all cells in the selected row.
                        this.dgvHandDetails.Rows.RemoveAt(row.Index);
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
        /// Handles the save menu strip item.
        /// </summary>
        private void MnuFileSave_Click(object sender, EventArgs e)
        {
            this.dgvHandDetails.EndEdit();
            this.handDetailsBindingSource.EndEdit();

            this.dataAdapter.Update(this.dataSet, "Hand_Details");

            this.Text = String.Format("Hand Details - {0} ({1})",
                          this.myHand,
                          this.position);
        }

        /// <summary>
        /// Handles the Shown event of this Form.
        /// </summary>
        private void HandDetailsForm_Shown(object sender, EventArgs e)
        {

            if (this.dataSet.Tables["Hand_Details"].Rows.Count == 0)
            {
                //MessageBox.Show(this.dgvHandDetails.Rows[0].Cells["ID"].Value.ToString());

                // Populate the rounds for each hand when there is no existing record.

                string[] round = { "Pre-Flop", "Flop", "Turn", "River" };

                for (int i = 0; i < round.Length; i++)
                {
                    MessageBox.Show(round[i]);

                    DataRow dataRow = this.dataSet.Tables["Hand_Details"].NewRow();

                    dataRow["HandsMasterID"] = this.handsMasterId;
                    dataRow["Round"] = round[i];

                    this.dataSet.Tables["Hand_Details"].Rows.Add(dataRow);
                }

                this.Text = String.Format("* Hand Details - {0} ({1})",
                          myHand,
                          position);
            }



            //if (this.dgvHandDetails.Rows[0].Cells["ID"].Value == DBNull.Value)
            //{
            //    MessageBox.Show(this.dgvHandDetails.Rows[0].Cells["ID"].Value.ToString());

            //    // Populate the rounds for each hand when there is no existing record.

            //    string[] round = { "Pre-Flop", "Flop", "Turn", "River" };

            //    for (int i = 0; i < round.Length; i++)
            //    {
            //        //MessageBox.Show(round[i]);

            //        DataRow dataRow = this.dataSet.Tables["Hand_Details"].NewRow();

            //        dataRow["HandsMasterID"] = this.handsMasterId;
            //        dataRow["Round"] = round[i];

            //        this.dataSet.Tables["Hand_Details"].Rows.Add(dataRow);
            //    }

            //        //MessageBox.Show(this.dgvHandDetails.Rows[0].Cells["ID"].Value.ToString());

            //        ////for (int i = 0; i < round.Length; i++)
            //        ////{

            //        ////    MessageBox.Show(round[i]);

            //        ////    this.dgvHandDetails.Rows[i].Cells["Round"].Value = round[i];

            //        ////}


            //}

        }

        /// <summary>
        /// Handles the CellValueChanged event of the hand details data grid view.
        /// </summary>
        private void DgvHandDetails_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != this.dgvHandDetails.Columns["HandsMasterID"].Index)
            {
                this.Text = String.Format("* Hand Details - {0} ({1})", 
                                          this.myHand,
                                          this.position);
            }
        }

        /// <summary>
        /// Handles the Click event of the close menu strip item.
        /// </summary>
        private void MnuFileClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the DefaultValuesNeeded event of the hand details data grid view.
        /// </summary>
        private void DgvHandDetails_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["HandsMasterID"].Value = this.handsMasterId;
        }

        /// <summary>
        /// Handles the FormClosed event of this Form.
        /// </summary>
        private void HandDetailsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            this.connection.Close();
            this.connection.Dispose();
        }

        /// <summary>
        /// Queries the database and populates a dataset.
        /// </summary>
        private void RetrieveDataFromTheDatabase(int handsMasterId)
        {
            this.connection = new OleDbConnection();
            this.connection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=PokerTournamentDatabase.accdb";
            this.connection.Open();

            string command = String.Format("SELECT * FROM Hand_Details WHERE HandsMasterID = {0}", handsMasterId);
            OleDbCommand selectCommand = new OleDbCommand(command, this.connection);

            this.dataAdapter = new OleDbDataAdapter(selectCommand);

            this.dataSet = new DataSet();
            this.dataAdapter.Fill(this.dataSet, "Hand_Details");

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
            this.handDetailsBindingSource = new BindingSource();
            this.handDetailsBindingSource.DataSource = this.dataSet.Tables["Hand_Details"];

            this.dgvHandDetails.DataSource = this.handDetailsBindingSource;
        }

        /// <summary>
        /// Initialize this Form to its initial state.
        /// </summary>
        private void InitializeForm(string myHand, string position)
        {
            this.Text = String.Format("Hand Details - {0} ({1})", 
                                      myHand, 
                                      position);
        }
    }
}