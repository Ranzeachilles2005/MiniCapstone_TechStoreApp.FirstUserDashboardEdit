using MongoDB.Driver;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MiniCapstone_TechStoreApp.DatabaseMongoDB;
using MiniCapstone_TechStoreApp.Models;

namespace MiniCapstone_TechStoreApp.Services
{
    public class EmailVerificationService
    {
        private readonly MongoDBConnector db;
        private readonly EmailService emailService;
        private readonly string hmacSecret;

        public EmailVerificationService(MongoDBConnector db, EmailService emailService)
        {
            this.db = db;
            this.emailService = emailService;
            hmacSecret = Environment.GetEnvironmentVariable("OTP_HMAC_SECRET");

            if (string.IsNullOrWhiteSpace(hmacSecret) || Encoding.UTF8.GetByteCount(hmacSecret) < 32)
            {
                throw new Exception("OTP_HMAC_SECRET must contain at least 32 bytes.");
            }
        }

        // Generates a cryptographically secure six-digit code.
        private string GenerateCode()
        {
            byte[] bytes = new byte[4];

            uint value;
            uint limit = 4294000000;

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                do
                {
                    rng.GetBytes(bytes);
                    value = BitConverter.ToUInt32(bytes, 0);
                }
                while (value >= limit);
            }

            int codeNumber = (int)(value % 1000000);
            return codeNumber.ToString("D6");
        }

        // Hashes the OTP using a secret key.
        private string HashCode(string code)
        {
            byte[] key = Encoding.UTF8.GetBytes(hmacSecret);
            byte[] data = Encoding.UTF8.GetBytes(code);

            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] hash = hmac.ComputeHash(data);
                return Convert.ToBase64String(hash);
            }
        }

        public async Task SendCodeAsync(string email)
        {
            email = email.Trim().ToLowerInvariant();

            DateTime now = DateTime.UtcNow;

            EmailVerification existing = await db.EmailVerifications.Find(x => x.Email == email).FirstOrDefaultAsync();

            // Prevent sending another code within 60 seconds.
            if (existing != null && existing.LastSentAt.AddSeconds(60) > now)
            {
                throw new Exception("Please wait 60 seconds before requesting another code.");
            }

            string code = GenerateCode();

            EmailVerification verification = new EmailVerification();

            verification.Email = email;
            verification.CodeHash = HashCode(code);
            verification.ExpiresAt = now.AddMinutes(5);
            verification.Attempts = 0;
            verification.IsUsed = false;
            verification.CreatedAt = now;
            verification.LastSentAt = now;

            // Save the new verification record.
            ReplaceOptions options = new ReplaceOptions
            {
                IsUpsert = true
            };

            await db.EmailVerifications.ReplaceOneAsync(x => x.Email == email, verification, options);

            try
            {
                await emailService.SendVerificationCodeAsync(email, code);
            }
            catch
            {
                // Remove only the record created for this code.
                await db.EmailVerifications.DeleteOneAsync(x => x.Email == email && x.CodeHash == verification.CodeHash);

                throw new Exception("Unable to send verification email. Please try again.");
            }
        }

        public async Task<bool> VerifyCodeAsync(string email, string enteredCode)
        {
            email = email.Trim().ToLowerInvariant();
            enteredCode = enteredCode.Trim();

            if (enteredCode.Length != 6)
            {
                return false;
            }

            DateTime now = DateTime.UtcNow;

            string enteredHash = HashCode(enteredCode);

            // Find and consume the code atomically.
            FilterDefinition<EmailVerification> correctFilter = 
                    Builders<EmailVerification>.Filter.And(
                    Builders<EmailVerification>.Filter.Eq(x => x.Email, email),
                    Builders<EmailVerification>.Filter.Eq(x => x.CodeHash, enteredHash),
                    Builders<EmailVerification>.Filter.Gt(x => x.ExpiresAt, now),
                    Builders<EmailVerification>.Filter.Eq(x => x.IsUsed, false),
                    Builders<EmailVerification>.Filter.Lt(x => x.Attempts, 5)
                );

            UpdateDefinition<EmailVerification> consumeUpdate = Builders<EmailVerification>.Update.Set(x => x.IsUsed, true);

            EmailVerification result = await db.EmailVerifications.FindOneAndUpdateAsync(correctFilter, consumeUpdate);

            if (result != null)
            {
                return true;
            }

            // Incorrect code: increase the attempt count.
            FilterDefinition<EmailVerification> wrongFilter = 
                    Builders<EmailVerification>.Filter.And(
                    Builders<EmailVerification>.Filter.Eq(x => x.Email, email),
                    Builders<EmailVerification>.Filter.Gt(x => x.ExpiresAt, now),
                    Builders<EmailVerification>.Filter.Eq(x => x.IsUsed, false),
                    Builders<EmailVerification>.Filter.Lt(x => x.Attempts, 5)
                );

            UpdateDefinition<EmailVerification> wrongUpdate = Builders<EmailVerification>.Update.Inc(x => x.Attempts, 1);

            await db.EmailVerifications.UpdateOneAsync(wrongFilter, wrongUpdate);

            return false;
        }
    }
}