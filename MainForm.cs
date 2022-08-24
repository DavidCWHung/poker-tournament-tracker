using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PokerTournamentTracker
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();

            this.mnuGames.Click += MnuDataGames_Click;
            this.mnuFileExit.Click += MnuFileExit_Click;

            this.mnuFileCloseAll.Click += MnuFileCloseAll_Click;

            this.mnuViewTileHorizontal.Click += MnuViewTileHorizontal_Click;
            this.mnuViewTileVertical.Click += MnuViewTileVertical_Click;
            this.mnuViewCascade.Click += MnuViewCascade_Click;
        }

        /// <summary>
        /// Handles the click event of the cascade menu strip item.
        /// </summary>
        private void MnuViewCascade_Click(object sender, EventArgs e)
        {
            for (int i = this.MdiChildren.Length - 1; i >= 0; i--)
            {
                this.MdiChildren[i].Activate();
            }

            this.LayoutMdi(MdiLayout.Cascade);
        }

        /// <summary>
        /// Handles the Click event of the tile vertical menu strip item.
        /// </summary>
        private void MnuViewTileVertical_Click(object sender, EventArgs e)
        {
            for (int i = this.MdiChildren.Length - 1; i >= 0; i--)
            {
                this.MdiChildren[i].Activate();
            }

            this.LayoutMdi(MdiLayout.TileVertical);
        }

        /// <summary>
        /// Handles the Click event of the tile horizontal menu trip item.
        /// </summary>
        private void MnuViewTileHorizontal_Click(object sender, EventArgs e)
        {
            for (int i = this.MdiChildren.Length - 1; i >= 0; i--)
            {
                this.MdiChildren[i].Activate();
            }

            this.LayoutMdi(MdiLayout.TileHorizontal);
        }

        /// <summary>
        /// Handles the Click event of the close all menu strip item.
        /// </summary>
        private void MnuFileCloseAll_Click(object sender, EventArgs e)
        {
            foreach(Form childForm in this.MdiChildren)
            {
                childForm.Close();
            }
        }

        /// <summary>
        /// Handles the Exit event of the exit menu strip item.
        /// </summary>
        private void MnuFileExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Handles the Click event of the games menu strip item.
        /// </summary>
        private void MnuDataGames_Click(object sender, EventArgs e)
        {
            bool isShown = false;

            for (int i = 0; !isShown && i < this.MdiChildren.Length; i++)
            {
                if (this.MdiChildren[i] is GamesForm)
                {
                    isShown = true;
                    this.MdiChildren[i].Activate();
                }
            }

            if (!isShown)
            {
                try
                {
                    GamesForm gamesForm = new GamesForm();
                    gamesForm.MdiParent = this;
                    gamesForm.WindowState = FormWindowState.Maximized;

                    gamesForm.Show();
                }
                catch (Exception)
                {
                    MessageBox.Show("Oops, unable to load the games data. Try again!",
                                    "Data Load Error",
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Error);
                }
            }
        }
    }
}