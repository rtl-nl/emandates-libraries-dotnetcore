using System;
using System.Net.Http;
using eMandates.Merchant.Library.Configuration;
using eMandates.Merchant.Library.Logging;
using eMandates.Merchant.Library.MessageBuilders;
using eMandates.Merchant.Library.XML;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace eMandates.Merchant.Library
{
    /// <summary>
    /// Communicator class, to be used for sending messages where LocalInstrumentationCode = CORE
    /// </summary>
    public class CoreCommunicator : ICoreCommunicator
    {
        /// <summary>
        /// Configuration instance used with this CoreCommunicator
        /// </summary>
        protected internal IConfiguration LocalConfiguration { get; private set; }

        /// <summary>
        /// Logger instance, to be used for logging library messages
        /// </summary>
        protected internal ILogger Logger { get; set; }

        /// <summary>
        /// Logger instance, to be used for logging iso pain raw messages
        /// </summary>
        protected internal IXmlLogger XmlLogger;

        /// <summary>
        /// XmlProcessor instance, used to process XMLs (signing, verifying, validating signature)
        /// </summary>
        protected internal IXmlProcessor XmlProcessor { get; set; }

        /// <summary>
        /// LocalInstrumentCode used by the current instance (can be CORE or B2B)
        /// </summary>
        protected internal Instrumentation LocalInstrumentCode { get; set; }

        /// <summary>
        /// Constructs a new Communication by specifying the Configuration
        /// </summary>
        public CoreCommunicator(
            IConfiguration configuration,
            IXmlLogger xmlLogger,
            IXmlProcessor xmlProcessor,
            ILogger<CoreCommunicator> logger)
        {
            LocalConfiguration = configuration;
            Logger = logger;
            XmlLogger = xmlLogger;
            XmlProcessor = xmlProcessor;
            LocalInstrumentCode = Instrumentation.Core;

            Logger.LogDebug("communicator initialized with custom configuration");
        }

        /// <summary>
        /// Sign a message using the SigningCertificate
        /// </summary>
        protected internal string Sign(string xml)
        {
            return XmlProcessor.AddSignature(xml);
        }

        /// <summary>
        /// Verify an incoming message's signature using the AcquirerCertificate
        /// </summary>
        protected internal bool VerifySignature(string xml)
        {
            return XmlProcessor.VerifySignature(xml);
        }

        /// <summary>
        /// Verify that a message is correct according to the XML schemas
        /// </summary>
        protected internal bool VerifySchema(string xml)
        {
            return XmlProcessor.VerifySchema(xml);
        }

        /// <summary>
        /// Perform the http(s) request and return the result
        /// </summary>
        protected internal string PerformRequest(string xml, string url)
        {
            Logger.LogInformation("sending request to {Url}", url);

            try
            {
                VerifySchema(xml);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "request xml schema is not valid");

                throw new CommunicatorException("Request XML schema is not valid.", e);
            }

            XmlLogger.LogXmlMessage(xml);

            var content = "";

            using (var httpClient = new HttpClient())
            {
                //httpClient.DefaultRequestHeaders.Add("Content-Type", "text/xml; charset='utf-8'");
                var task = httpClient.PostAsync(url, new StringContent(xml, System.Text.Encoding.UTF8, "text/xml"));

                var result = task.Result;
                Logger.LogDebug("result status: {StatusCode}", result.StatusCode);

                if (!result.IsSuccessStatusCode)
                {
                    Logger.LogDebug("http request failed: {StatusCode}", result.StatusCode);

                    throw new CommunicatorException("Http request failed, code=" + result.StatusCode);
                }

                content = result.Content.ReadAsStringAsync().Result;
            }

            XmlLogger.LogXmlMessage(content);

            try
            {
                VerifySchema(content);
                Logger.LogDebug("Response XML schema is valid.");
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message, "response xml schema is not valid.");

                throw new CommunicatorException("Response XML schema is not valid.", e);
            }

            var signatureIsValid = VerifySignature(content);
            Logger.LogDebug("signature is valid: {SignatureIsValid}", signatureIsValid);

            if (!signatureIsValid)
            {
                Logger.LogWarning("request xml signature is not valid");

                throw new CommunicatorException("Response XML signature is not valid.");
            }

            return content;
        }

        /// <summary>
        ///     Sends a directory request to the URL specified in Configuration.AcquirerUrl_DirectoryReq
        /// </summary>
        /// <returns>
        ///     A DirectoryResponse object which contains the response from the server (a list of debtor banks), or error information when an error occurs
        /// </returns>
        public DirectoryResponse Directory()
        {
            try
            {
                Logger.LogDebug("sending new directory request");
                Logger.LogDebug("building idx message");
                var directoryRequest = new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetDirectoryRequest();

                Logger.LogDebug("signing message");
                var xml = Sign(directoryRequest);

                var content = PerformRequest(xml, LocalConfiguration.Acquirer.DirectoryRequestUrl);

                return DirectoryResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);

                return DirectoryResponse.Get(e);
            }
        }

        /// <summary>
        ///     Sends a new mandate request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="newMandateRequest">A NewMandateRequest object</param>
        /// <returns>
        ///     A NewMandateResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        public NewMandateResponse NewMandate(NewMandateRequest newMandateRequest)
        {
            try
            {
                Logger.LogDebug("sending new eMandate transaction");

                Logger.LogDebug("building eMandate");
                var document = new EMandateMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetNewMandate(newMandateRequest);

                Logger.LogDebug("building idx message");

                var transactionRequest =
                    new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetTransactionRequest(newMandateRequest, document);

                Logger.LogDebug("signing message");
                var xml = Sign(transactionRequest);

                var content = PerformRequest(xml, LocalConfiguration.Acquirer.TransactionRequestUrl);

                return NewMandateResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);

                return NewMandateResponse.Get(e);
            }
        }

        /// <summary>
        ///     Sends a transaction status request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="statusRequest">A StatusRequest object</param>
        /// <returns>
        ///     A StatusResponse object which contains the response from the server (transaction id, status message), or error information when an error occurs
        /// </returns>
        public StatusResponse GetStatus(StatusRequest statusRequest)
        {
            try
            {
                Logger.LogDebug("sending new status request");
                Logger.LogDebug("building idx message");

                var request =
                    new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetStatusRequest(statusRequest);

                Logger.LogDebug("signing message");
                var xml = Sign(request);
                var content = PerformRequest(xml, LocalConfiguration.Acquirer.StatusRequestUrl);

                return StatusResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);

                return StatusResponse.Get(e);
            }
        }

        /// <summary>
        ///     Sends an amendment request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="amendmentRequest">An AmendmentRequest object</param>
        /// <returns>
        ///     An AmendmentResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        public AmendmentResponse Amend(AmendmentRequest amendmentRequest)
        {
            try
            {
                Logger.LogDebug("sending new amend request");
                Logger.LogDebug("building eMandate");
                var document = new EMandateMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetAmend(amendmentRequest);

                Logger.LogDebug("building idx message");

                var request =
                    new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetTransactionRequest(amendmentRequest, document);

                Logger.LogDebug("signing message");
                var xml = Sign(request);

                var content = PerformRequest(xml, LocalConfiguration.Acquirer.TransactionRequestUrl);

                return AmendmentResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);

                return AmendmentResponse.Get(e);
            }
        }
    }

    /// <summary>
    /// Communicator class, to be used for sending messages where LocalInstrumentationCode = B2B
    /// </summary>
    public class B2BCommunicator : CoreCommunicator, IB2BCommunicator
    {
        private readonly ILogger<B2BCommunicator> _logger;

        /// <summary>
        /// Initializes the B2BCommunication by specifying a Configuration
        /// </summary>
        public B2BCommunicator(
            IConfiguration configuration,
            IXmlLogger xmlLogger,
            IXmlProcessor xmlProcessor,
            ILogger<B2BCommunicator> logger,
            ILogger<CoreCommunicator> coreCommunicatorLogger)
            : base(configuration, xmlLogger, xmlProcessor, coreCommunicatorLogger)
        {
            _logger = logger;
            LocalInstrumentCode = Instrumentation.B2B;
        }

        /// <summary>
        ///     Sends a cancellation request to the URL specified in Configuration.AcquirerUrl_TransactionReq
        /// </summary>
        /// <param name="cancellationRequest">A CancellationRequest object</param>
        /// <returns>
        ///     A CancellationResponse object which contains the response from the server (transaction id, issuer authentication URL), or error information when an error occurs
        /// </returns>
        public CancellationResponse Cancel(CancellationRequest cancellationRequest)
        {
            try
            {
                Logger.LogDebug("sending new eMandate transaction");
                Logger.LogDebug("building eMandate");

                var document = new EMandateMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetCancel(cancellationRequest);

                Logger.LogDebug("building idx message");

                var request =
                    new IDxMessageBuilder(LocalConfiguration, LocalInstrumentCode).GetTransactionRequest(cancellationRequest, document);

                Logger.LogDebug("signing message");
                var xml = Sign(request);

                var content = PerformRequest(xml, LocalConfiguration.Acquirer.TransactionRequestUrl);

                return CancellationResponse.Parse(content, Logger);
            }
            catch (Exception e)
            {
                Logger.LogError(e, e.Message);

                return CancellationResponse.Get(e);
            }
        }
    }
}