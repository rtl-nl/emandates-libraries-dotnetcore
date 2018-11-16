namespace eMandates.Merchant.Library.Logging
{
    public interface IXmlLogger
    {
        /// <summary>
        /// Logs a request/response xml message to the directory specified in the configuration.
        /// </summary>
        void LogXmlMessage(string content);
    }
}