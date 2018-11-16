namespace eMandates.Merchant.Library.Configuration
{
    public class MerchantConfiguration
    {
        /// <summary>
        /// A valid URL to which the debtor banks redirects to, after the debtor has authorized a transaction.
        /// </summary>
        public string ReturnUrl { get; set; }
    }
}