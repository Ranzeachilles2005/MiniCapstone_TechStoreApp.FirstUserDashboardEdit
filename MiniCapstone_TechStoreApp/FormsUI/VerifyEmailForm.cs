using System;
using System.Windows.Forms;
using MiniCapstone_TechStoreApp.Services;

namespace MiniCapstone_TechStoreApp.FormsUI
{
    public partial class VerifyEmailForm : Form
    {
        private readonly AuthenticatorService authService;

        private readonly string _username;
        private readonly string _email;
        private readonly string _password;

        public VerifyEmailForm(
            AuthenticatorService authService,
            string username,
            string email,
            string password)
        {
            InitializeComponent();

            this.authService = authService;
            _username = username;
            _email = email;
            _password = password;
        }

        private async void btnVerify_Click(
            object sender,
            EventArgs e)
        {
            string code = txtVerificationCode.Text.Trim();

            if (code.Length != 6)
            {
                MessageBox.Show(
                    "Please enter the six-digit verification code.");
                return;
            }

            try
            {
                btnVerify.Enabled = false;

                await authService.RegisterVerifiedBuyerAsync(
                    _username,
                    _email,
                    _password,
                    code);

                MessageBox.Show(
                    "Your account has been created successfully!");

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Verification Error");
            }
            finally
            {
                btnVerify.Enabled = true;
            }
        }
    }
}