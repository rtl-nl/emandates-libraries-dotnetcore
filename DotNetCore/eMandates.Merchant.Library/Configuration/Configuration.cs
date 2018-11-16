using System;

namespace eMandates.Merchant.Library.Configuration
{
    /// <summary>
    /// Configuration class
    /// </summary>
    public class Configuration : IConfiguration
    {
        public ContractConfiguration Contract { get; set; }
        public MerchantConfiguration Merchant { get; set; }

        /// <summary>
        /// The certificate to use to sign messages to the creditor bank.
        /// </summary>
        public CertificateConfiguration SigningCertificate { get; set; }

        public AcquirerConfiguration Acquirer { get; set; }
        public ServiceLogsConfiguration ServiceLogs { get; set; }

        /// <summary>
        /// Ensures that the configuration is valid.
        /// </summary>
        protected static void EnsureIsValid(IConfiguration configuration)
        {
            ValidateParameter(configuration?.Contract?.Id, "Contract.Id");
            ValidateParameter(configuration?.Merchant?.ReturnUrl, "Merchant.ReturnUrl");
            ValidateParameter(configuration?.SigningCertificate?.Thumbprint, "SigningCertificate.Thumbprint");
            ValidateParameter(configuration?.Acquirer?.Certificate?.Thumbprint, "Acquirer.Certificate.Thumbprint");
            ValidateParameter(configuration?.Acquirer?.DirectoryRequestUrl, "Acquirer.DirectoryRequestUrl");
            ValidateParameter(configuration?.Acquirer?.TransactionRequestUrl, "Acquirer.TransactionRequestUrl");
            ValidateParameter(configuration?.Acquirer?.StatusRequestUrl, "Acquirer.StatusRequestUrl");
        }

        private static void ValidateParameter(string value, string name)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("The configuration parameter is not configured.", name);
        }
    }
}