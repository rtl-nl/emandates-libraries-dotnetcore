namespace eMandates.Merchant.Library.Configuration
{
    /// <summary>
    /// Interface that describes the configuration settings for the library, which are tied with each ICommunicator instance:
    /// when you instantiate a Communicator object, it attempts to load its configuration using App.config or Web.config.
    /// </summary>
    public interface IConfiguration
    {
        ContractConfiguration Contract { get; set; }
        MerchantConfiguration Merchant { get; set; }

        /// <summary>
        /// The certificate to use to sign messages to the creditor bank.
        /// </summary>
        CertificateConfiguration SigningCertificate { get; set; }

        AcquirerConfiguration Acquirer { get; set; }
        ServiceLogsConfiguration ServiceLogs { get; set; }
    }
}
