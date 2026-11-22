using MiniCapstone_TechStoreApp.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniCapstone_TechStoreApp.FormsUI
{
    public partial class RegistrationForm : Form
    {
        private readonly AuthenticatorService authService;

        public RegistrationForm(AuthenticatorService authService)
        {
            InitializeComponent();
            this.authService = authService;
            CenterRegisterPanel();
            this.Resize += RegistrationForm_Resize;
        }

        private void CenterRegisterPanel()
        {
            int panelWidth = pnlRegister.Width;
            int panelHeight = pnlRegister.Height;
            FormLayoutHelper.CenterPanel(this, pnlRegister, panelWidth, panelHeight);
        }

        private void RegistrationForm_Resize(object sender, EventArgs e)
        {
            CenterRegisterPanel();
        }

        private async void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please complete all fields.");
                return;
            }

            //potentially add requirement for small and big letter and number and special character
            if (password.Length < 8)
            {
                MessageBox.Show("Password must contain at least 8 characters.");
                return;
            }

            if (await authService.UsernameExistsAsync(username))
            {
                MessageBox.Show(
                    "Username is already taken. Please choose another.",
                    "Registration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                txtUsername.Focus();
                return;
            }

            try
            {
                btnRegister.Enabled = false;

                await authService.SendRegistrationCodeAsync(email);

                MessageBox.Show("Verification code sent to your email.");

                VerifyEmailForm verifyForm = new VerifyEmailForm(authService, username, email, password);

                verifyForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Registration Error");
            }
            finally
            {
                btnRegister.Enabled = true;
            }
        }

        private void linkLblLogin_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.Hide();
            LoginForm loginForm = new LoginForm(authService);
            loginForm.ShowDialog();
        }
    }
}
