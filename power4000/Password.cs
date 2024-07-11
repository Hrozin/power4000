using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace power4000
{
    public partial class Password : Form
    {
        public Password()
        {
            InitializeComponent();
            txt_pw.Select(); // PasswordForm start Focus
        }

        private void btn_log_Click(object sender, EventArgs e)
        {
            Form1 frm = new Form1();

            if (txt_pw.Text == "2580")
            {
                frm.Show();
                Close();
            }
            else
            {
                MessageBox.Show("Wrong Password^^");
            }

        }
        
        private void txt_pw_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btn_log_Click(sender, e);              
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }            
        }

        private void btn_close_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
