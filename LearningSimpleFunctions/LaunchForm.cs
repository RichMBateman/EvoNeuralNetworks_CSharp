using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShepherdCrook.LearningSimpleFunctions
{
    public partial class LaunchForm : Form
    {
        public LaunchForm()
        {
            InitializeComponent();
        }

        private void m_btnXOR_Click(object sender, EventArgs e)
        {
            XORExperiment experiment = new XORExperiment();
            experiment.Run();
        }
    }
}
