using MiniCapstone_TechStoreApp.DatabaseMongoDB;
using MiniCapstone_TechStoreApp.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiniCapstone_TechStoreApp.Services
{
    public class AuthenticatorService
    {
        private readonly MongoDBConnector db;
        private readonly EmailVerificationService verificationService;

        public AuthenticatorService(MongoDBConnector db, EmailVerificationService verificationService)
        {
            this.db = db;
            this.verificationService = verificationService;
        }

        private string NormalizeEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new Exception("Email is required.");
            }

            return email.Trim().ToLowerInvariant();
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            email = NormalizeEmail(email);

            long count = await db.Users.CountDocumentsAsync(x => x.Email == email);

            return count > 0;
        }

        public async Task SendRegistrationCodeAsync(string email)
        {
            email = NormalizeEmail(email);

            if (await EmailExistsAsync(email))
            {
                throw new Exception("This email address is already registered.");
            }

            await verificationService.SendCodeAsync(email);
        }

        public async Task RegisterVerifiedBuyerAsync(string username, string email, string password, string verificationCode)
        {
            username = username.Trim();
            email = NormalizeEmail(email);

            if (string.IsNullOrWhiteSpace(username))
            {
                throw new Exception("Missing username");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            {
                throw new Exception("Password must contain at least 8 characters.");
            }

            if (await EmailExistsAsync(email))
            {
                throw new Exception("This email address is already registered.");
            }

            bool verified = await verificationService.VerifyCodeAsync(email, verificationCode);

            if (!verified)
            {
                throw new Exception("Invalid, expired, or already used verification code.");
            }

            UserDB user = new UserDB();
            user.Username = username;
            user.Email = email;

            //Password hasher
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Auto set role to buyer.
            user.Role = "Buyer";

            try
            {
                await db.Users.InsertOneAsync(user);
            }
            catch (MongoWriteException ex)
            {
                if (ex.WriteError != null && ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
                {
                    throw new Exception("This email address is already registered.");
                }
                throw;
            }
        }

        public async Task<UserDB> LoginAsync(string email, string password)
        {
            email = NormalizeEmail(email);

            UserDB user = await db.Users.Find(x => x.Email == email).FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            bool validPassword = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);

            if (!validPassword)
            {
                return null;
            }

            return user;
        }
    }
}