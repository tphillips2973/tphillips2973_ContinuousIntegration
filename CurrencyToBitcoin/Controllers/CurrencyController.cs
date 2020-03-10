using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CurrencyToBitcoin.Models;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Google.Cloud.Firestore;

namespace CurrencyToBitcoin.Controllers
{
    [Route("api/[controller]")]
    [Produces("application/json")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        #region Class Declarations
        private readonly CurrencyContext _context;

        public CurrencyController(CurrencyContext context)
        {
            _context = context;
        }

        //URL and Parameters for getting USD value
        private string currencyURL = @"https://api.exchangeratesapi.io/";
        private string currencyURLParameters = "latest?base=CAD&symbols=USD";

        //URL and Parameters for getting BitCoin value
        private string cryptocurrencyURL = @"https://api.coindesk.com/v1/bpi/currentprice/";
        private string cryptocurrencyURLParameters = @"USD.json";
        #endregion

        //Used to get either current USD dollar value based on baseCurrency entered by User
        //Or to get current BitCoin value
        public static string GetUpdatedCurrencyData(string URL, string parameters)
        {
            string result = "";
            var client = new RestClient(URL);
            var request = new RestRequest(parameters);
            request.OnBeforeDeserialization = resp => { resp.ContentType = "application/json"; };
            IRestResponse response = client.Execute(request);
            try
            {
                if (response.IsSuccessful || response.StatusCode == HttpStatusCode.OK)
                {
                    var content = JsonConvert.DeserializeObject<JToken>(response.Content);
                    dynamic api = JObject.Parse(content.ToString());
                    if (URL == "https://api.exchangeratesapi.io/")
                        result = (string)api.rates.USD.ToString();
                    if (URL == "https://api.coindesk.com/v1/bpi/currentprice/")
                        result = (string)api.bpi.USD.rate.Value.ToString();
                }
                else
                {
                    return response.Content.ToString();
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return result;
        }

        #region GET Methods
        // GET: api/currency/GetUpdatedCurrency?baseCurrency=CAD&cashValue=12000
        [HttpGet]
        [HttpGet("GetUpdatedCurrencyFromQuery")]
        public string[][] GetUpdatedCurrencyFromQuery([FromQuery]string baseCurrency, [FromQuery]double cashValue)
        {
            return GetUpdatedCurrency(baseCurrency, cashValue);
        }

        // GET: api/currency/GetUpdatedCurrencyFromBody/CAD/12000
        [HttpGet]
        [HttpGet("GetUpdatedCurrencyFromBody/{baseCurrency}/{cashValue}")]
        public string[][] GetUpdatedCurrencyFromBody(string baseCurrency, double cashValue)
        {
            return GetUpdatedCurrency(baseCurrency, cashValue);
        }

        public string[][] GetUpdatedCurrency(string baseCurrency, double cashValue)
        {
            try
            {
                //Get Value inputted in USD
                cashValue = Math.Round(cashValue, 2);
                currencyURLParameters = (@"latest?base=" + baseCurrency + @"&symbols=USD");
                double convertedToUSDValue = Math.Round(cashValue *
                    Convert.ToDouble(GetUpdatedCurrencyData(currencyURL, currencyURLParameters)), 2);

                //Get Current BitCoin Value and 
                double bitCoinTotalValue = Math.Round(Convert.ToDouble
                    (GetUpdatedCurrencyData(cryptocurrencyURL, cryptocurrencyURLParameters)), 2);
                double bitCoinEqual = Math.Round(convertedToUSDValue / bitCoinTotalValue, 2);
                
                string[][] result = new[]
                { new[] { "Value in " + baseCurrency, cashValue.ToString() },
                    new[] { "Value in USD", convertedToUSDValue.ToString() },
                    new[] { "Value of Bitcoin", bitCoinTotalValue.ToString(),
                        "Number of Bitcoins you can purchase (Rounded)", Math.Floor(bitCoinEqual).ToString(),
                        "Amount of Bitcoins in value", bitCoinEqual.ToString() } };

                AddToFirebase(result, baseCurrency);
                //Return formatted response
                return result;

            }
            catch (Exception ex)
            {
                if (ex.Message == "Input string was not in a correct format.")
                    return new[]
                    { new[] { "*ERROR* You entered a " +
                              "baseCurrency that is not allowed. Please see the list below." },
                        new[] { "CAD", "HKD", "ISK", "PHP", "DKK", "HUF", "CZK", "AUD",
                            "RON", "SEK", "IDR", "INR", "BRL", "RUB", "HRK", "JPY", "THB", "CHF", "SGD",
                            "PLN", "BGN", "TRY", "CNY", "NOK", "NZD", "ZAR", "USD", "MXN", "ILS", "GBP",
                            "KRW", "MYR",} };
                if (ex.Message != "Input string was not in a correct format.")
                    return new[]
                    { new[] { "*ERROR*",
                        "An unknown error occurred, please double check your inputted variables and try again." } };
            }
            return new[]
            { new[] { "*ERROR*",
                "An unknown error occurred, please double check your inputted variables and try again." } };
        }

        public async void AddToFirebase(string[][] results, string baseCurrency)
        {
            double baseValue = 0;
            double usdValue = 0;
            double bitcoinValue = 0;
            foreach (string[] resultList in results)
            {
                if (resultList.Contains("Value in " + baseCurrency))
                {
                    baseValue = Convert.ToDouble(resultList[1]);
                }
                if (resultList.Contains("Value in USD"))
                {
                    usdValue = Convert.ToDouble(resultList[1]);
                }
                if (resultList.Contains("Value of Bitcoin"))
                {
                    bitcoinValue = Convert.ToDouble(resultList[5]);
                }
            }

            var jsonString = Path.GetFullPath(@"..\CurrencyToBitcoin\Scripts\currency-to-bitcoin-api-firebase-adminsdk-r2262-a5e9d662fa.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", jsonString);
            FirestoreDb db = FirestoreDb.Create("currency-to-bitcoin-api");

            Debug.WriteLine("Created Cloud Firestore client with project ID: {0}", "currency-to-bitcoin-api");

            Dictionary<string, object> result = new Dictionary<string, object>
            {
                { "baseCurrency", baseCurrency },
                { "valueBitcoin", bitcoinValue },
                { "value" + baseCurrency, baseValue },
                { "valueUSD", usdValue }
            };
            CollectionReference colRef = db.Collection("results");
            await colRef.AddAsync(result);
        }

        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // GET: api/Currency/GetSpecificCurrency/5
        [HttpGet]
        [HttpGet("GetSpecificCurrency/{id}")]
        public async Task<ActionResult<Currency>> GetSpecificCurrency([FromBody]long id)
        {
            var currency = await _context.Currencies.FindAsync(id);

            if (currency == null)
            {
                return NotFound();
            }

            return currency;
        }

        // GET: api/Currency/GetAllPostedCurrencies
        [HttpGet]
        [HttpGet("GetAllPostedCurrencies")]
        public async Task<ActionResult<IEnumerable<Currency>>> GetSongs() => await _context.Currencies.ToListAsync();

        #endregion

        // POST: api/Currency
        [HttpPost]
        public async Task<ActionResult<Currency>> PostCurrencyInfo([FromForm] Currency currency)
        {
            currency.cashUSDValue = Math.Round(currency.cashValue *
                    Convert.ToDouble(GetUpdatedCurrencyData(currencyURL, currencyURLParameters)), 2);
            currency.bitcoinUSDValue = Math.Round(Convert.ToDouble
                    (GetUpdatedCurrencyData(cryptocurrencyURL, cryptocurrencyURLParameters)), 2);
            currency.totalBitCoinValue = Math.Round(currency.cashUSDValue / currency.bitcoinUSDValue, 2);
            currency.totalBitCoinPurchasable = Math.Floor(currency.totalBitCoinValue);

            _context.Currencies.Add(currency);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSongs", new { currency.id }, currency);
        }

        // DELETE: api/Currency/5
        [Route("{id:long}")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<Currency>> DeleteCurrencyInfo(long id)
        {
            var currency = await _context.Currencies.FindAsync(id);
            if (currency == null)
            {
                return NotFound();
            }

            _context.Currencies.Remove(currency);
            await _context.SaveChangesAsync();

            return currency;
        }

        private bool CurrencyIDExists(long id)
        {
            return _context.Currencies.Any(e => e.id == id);
        }
    }
}
