using Newtonsoft.Json;

namespace eMandates.Merchant.Library.Configuration
{
    public class ServiceLogsConfiguration
    {
        /// <summary>
        /// A directory on the disk where the library saves ISO pain raw messages.
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// This tells the library that it should save ISO pain raw messages or not. Default is true.
        /// </summary>
        [JsonProperty("Enabled")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// A string that describes a pattern to distinguish the ISO pain raw messages. For example,
        /// %Y-%M-%D\%h%m%s.%f-%a.xml -> 102045.924-AcquirerTrxReq.xml
        /// </summary>
        /// <remarks>
        /// %Y = current year
        /// %M = current month
        /// %D = current day
        /// %h = current hour
        /// %m = current minute
        /// %s = current second
        /// %f = current millisecond
        /// %a = current action
        /// </remarks>
        public string Pattern { get; set; }
    }
}