namespace eMandates.Merchant.Library.Configuration
{
    public class AcquirerConfiguration
    {
        /// <summary>
        /// The certificate to use to validate messages from the creditor bank.
        /// </summary>
        public CertificateConfiguration Certificate { get; set; }

        /// <summary>
        /// The second certificate to use to validate messages from the creditor bank.
        /// </summary>
        public CertificateConfiguration AlternateCertificate { get; set; }

        /// <summary>
        /// The URL to which the library sends Directory request messages
        /// </summary>
        public string DirectoryRequestUrl { get; set; }

        /// <summary>
        /// The URL to which the library sends Transaction request messages (including eMandates messages).
        /// </summary>
        public string TransactionRequestUrl { get; set; }

        /// <summary>
        /// The URL to which the library sends Status request messages
        /// </summary>
        public string StatusRequestUrl { get; set; }
    }
}