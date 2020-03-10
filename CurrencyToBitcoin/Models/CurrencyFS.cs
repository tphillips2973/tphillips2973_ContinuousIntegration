using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace CurrencyToBitcoin.Models
{
    [FirestoreData]
    public class CurrencyFS
    {
        [FirestoreProperty]
        public string baseCurrency { get; set; }
        [FirestoreProperty]
        public double valueBaseCurrency { get; set; }
        [FirestoreProperty]
        public double valueBitcoin { get; set; }
        [FirestoreProperty]
        public double valueUSD { get; set; }
    }
}
