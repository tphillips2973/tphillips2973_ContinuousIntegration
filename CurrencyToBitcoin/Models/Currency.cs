using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace CurrencyToBitcoin.Models
{
    public class Currency
    {
        public long id { get; set; }

        [Required]
        public string baseCurrency { get; set; }
        [Required]
        public double cashValue { get; set; }
        public double cashUSDValue { get; set; }
        public double bitcoinUSDValue { get; set; }
        public double totalBitCoinPurchasable { get; set; }
        public double totalBitCoinValue { get; set; }
    }
}
