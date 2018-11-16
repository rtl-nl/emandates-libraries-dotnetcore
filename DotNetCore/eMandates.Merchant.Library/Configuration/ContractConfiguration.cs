namespace eMandates.Merchant.Library.Configuration
{
    public class ContractConfiguration
    {
        /// <summary>
        /// Contract ID as supplied to you by the creditor bank.
        /// If the Contract ID has less than 9 digits, use leading zeros to fill out the field.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Contract SubId as supplied to you by the creditor bank.
        /// If you do not have a ContractSubId, use 0 for this field.
        /// </summary>
        public string SubId { get; set; }
    }
}