using System;
using System.Windows.Forms;
using MiniCapstone_TechStoreApp.Services;

namespace MiniCapstone_TechStoreApp.FormsUI
{
    public partial class VerifyEmailForm : Form
    {
        private readonly AuthenticatorService authService;
        private readonly string username;
        private readonly string email;
        private readonly string password;

        public VerifyEmailForm(AuthenticatorService authService, string username, string email, string password)
        {
            InitializeComponent();
            this.authService = authService;
            this.username = username;
            this.email = email;
            this.password = password;

            CenterVerifyEmailPanel();
            this.Resize += VerifyEmailForm_Resize;
            this.Shown += VerifyEmailForm_Shown;
        }

        private void CenterVerifyEmailPanel()
        {
            int panelWidth = pnlVerifyEmail.Width;
            int panelHeight = pnlVerifyEmail.Height;
            FormLayoutHelper.CenterPanel(this, pnlVerifyEmail, panelWidth, panelHeight);
        }

        private void VerifyEmailForm_Resize(object sender, EventArgs e)
        {
            CenterVerifyEmailPanel();
        }

        private async void btnVerify_Click(object sender, EventArgs e)
        {
            string code = txtVerificationCode.Text.Trim();

            if (code.Length != 6)
            {
                MessageBox.Show("Please enter the six-digit verification code.");
                return;
            }

            try
            {
                btnVerify.Enabled = false;

                await authService.RegisterVerifiedBuyerAsync(username, email, password, code);

                MessageBox.Show("Your account has been created successfully!");

                this.DialogResult = DialogResult.OK;

                LoginForm loginForm = new LoginForm(authService);

                loginForm.FormClosed += (s, args) => this.Close();

                loginForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Verification Error");
            }
            finally
            {
                btnVerify.Enabled = true;
            }
        }

        private void VerifyEmailForm_Shown(object sender, EventArgs e)
        {
            txtVerificationCode.Focus();
        }
    }
}