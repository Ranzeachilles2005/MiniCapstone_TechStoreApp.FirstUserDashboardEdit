using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MiniCapstone_TechStoreApp.DatabaseMongoDB;

namespace MiniCapstone_TechStoreApp
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
            MongoDBConnector.TestDB(test);
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }
    }
}
