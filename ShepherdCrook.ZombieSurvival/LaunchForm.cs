using ShepherdCrook.Library.Experiment;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShepherdCrook.ZombieSurvival
{
    public partial class LaunchForm : Form
    {
        private Bitmap m_buffer;

        private BackgroundWorker m_backgroundWorker;
        private Agent m_demoAgent;
        private ZombieSurvivalExperiment m_experiment = new ZombieSurvivalExperiment();


        public LaunchForm()
        {
            InitializeComponent();

            m_backgroundWorker = new BackgroundWorker();
            m_backgroundWorker.DoWork += M_backgroundWorker_DoWork;
            m_backgroundWorker.ProgressChanged += M_backgroundWorker_ProgressChanged;
            m_backgroundWorker.RunWorkerCompleted += M_backgroundWorker_RunWorkerCompleted;
            m_backgroundWorker.WorkerReportsProgress = true;

            this.Paint += LaunchForm_Paint;

            // Prevents flickering during drawing.
            this.SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer, true);
        }

        private void LaunchForm_Paint(object sender, PaintEventArgs e)
        {
            if (m_buffer != null)
            {
                e.Graphics.DrawImageUnscaled(m_buffer, Point.Empty);
            }
        }

        private void M_backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            button1.Enabled = true;
            button2.Enabled = true;
        }

        private void M_backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            Size totalSize = GetTotalAvailableSize();
            Bitmap b = new Bitmap(totalSize.Width, totalSize.Height);
            Graphics g = Graphics.FromImage(b);

            g.FillRectangle(Brushes.Green, new RectangleF(0, 0, totalSize.Width, totalSize.Height));

            g.Dispose();
            AcceptLatestBitmapFrame(b);
        }

        private void M_backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            while(!m_experiment.GameWorld.SimulationOver)
            {
                List<double> input = m_experiment.BuildInputArrayForWorld();
                double walkRightActivation;
                double walkLeftActivation;
                double shootRightActivation;
                double shootLeftActivation;
                List<double> output = m_experiment.GetAgentOutput(m_demoAgent, input, out walkRightActivation,
                    out walkLeftActivation, out shootRightActivation, out shootLeftActivation);

                Direction toWalk = Direction.None;
                Direction toShoot = Direction.None;
                m_experiment.GetAgentDecisions(walkRightActivation, walkLeftActivation, shootRightActivation, shootLeftActivation,
                    out toWalk, out toShoot);

                m_experiment.GameWorld.Tick(toWalk, toShoot);

                m_backgroundWorker.ReportProgress(0);
                Thread.Sleep(1000);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            button2.Enabled = false;
            m_demoAgent = GetAgentForDemo();
            m_experiment.GameWorld.Reset();
            m_backgroundWorker.RunWorkerAsync();
        }

        public void AcceptLatestBitmapFrame(Bitmap buffer)
        {
            Bitmap previousBuffer = m_buffer;
            m_buffer = buffer;
            if (previousBuffer != null)
            {
                // Eliminate any previous bitmap.
                previousBuffer.Dispose();
            }
        }


        /// <summary>
        /// Returns how much total size is available for drawing.
        /// </summary>
        public Size GetTotalAvailableSize()
        {
            return new Size(ClientSize.Width, ClientSize.Height);
        }


        private Agent GetAgentForDemo()
        {
            Agent toUse;
            if(m_experiment.BestAgents.Count > 0)
            {
                toUse = m_experiment.BestAgents[0];
            }
            else
            {
                toUse = new Agent(new Library.ANN.Network(54, 4));
            }
            return toUse;
        }
    }
}
