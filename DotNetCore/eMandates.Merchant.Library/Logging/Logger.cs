using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using eMandates.Merchant.Library.Configuration;
using Microsoft.Extensions.Logging;

namespace eMandates.Merchant.Library.Logging
{
    /// <summary>
    /// The default logger used by the library
    /// </summary>
    internal class XmlLogger : IXmlLogger
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<XmlLogger> _logger;

        public XmlLogger(IConfiguration configuration, ILogger<XmlLogger> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Logs a request/response xml message to the directory specified in the configuration.
        /// </summary>
        public void LogXmlMessage(string content)
        {
            if (!_configuration.ServiceLogs.IsEnabled)
            {
                return;
            }
            var xml = new XmlDocument();
            xml.LoadXml(content);
            var now = DateTime.Now;

            var fileName = _configuration.ServiceLogs.Pattern;

            fileName = fileName.Replace("%Y", now.ToString("yyyy"));
            fileName = fileName.Replace("%M", now.ToString("MM"));
            fileName = fileName.Replace("%D", now.ToString("dd"));
            fileName = fileName.Replace("%h", now.ToString("HH"));
            fileName = fileName.Replace("%m", now.ToString("mm"));
            fileName = fileName.Replace("%s", now.ToString("ss"));
            fileName = fileName.Replace("%f", now.ToString("fff"));
            fileName = fileName.Replace("%a", Sanitize(xml.DocumentElement.LocalName));
            fileName = Path.Combine(_configuration.ServiceLogs.Location, fileName);

            _logger.LogDebug("writing to: {FileName}", fileName);
            var file = new FileInfo(fileName);

            _logger.LogDebug("creating: {DirectoryName}", file.DirectoryName);
            Directory.CreateDirectory(file.DirectoryName);

            File.WriteAllText(file.FullName, content);
        }

        private static readonly Regex Sanitizer = new Regex("[" + Regex.Escape(new string(Path.GetInvalidFileNameChars())) + "]");
        private static string Sanitize(string fileName)
        {
            return Sanitizer.Replace(fileName, "");
        }
    }
}
