using Newtonsoft.Json.Linq;
using System;
using System.Collections.Specialized;
using System.Globalization;

namespace Exchange_Rate_App
{
    class Program
    {
        // Using API - exchangeratesapi.io

        string text = System.IO.File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "Rates.txt");
        private string latestRatesAPILink = "http://api.exchangeratesapi.io/v1/latest?access_key=6212dcfef232b08ecc9a82634dd9d3a1&base=EUR";
        private string HistoricalRatesAPILink = "http://api.exchangeratesapi.io/v1/2023-12-13?access_key=6212dcfef232b08ecc9a82634dd9d3a1&base=EUR&symbols=JPY";

        JObject? latestRateslist;
        JObject? HistoricalRatesList;

        public int programFunction = 0;
        public int convertAmount = 0;

        public string? conFrom;
        public string? conTo;
        public string? conDate;

        ConsoleColor defaultConCol = ConsoleColor.DarkCyan;
        ConsoleColor defaultErrorCol = ConsoleColor.Red;
        ConsoleColor defaultInputCol = ConsoleColor.Cyan;

        static void Main()
        {
            Program p = new();  // god bless those
            p.AppStartup();    // two lines of code †
        }

        private void AppStartup()
        {
            Console.Title = "Exchange Rate Application 0.1";
            Console.ForegroundColor = defaultConCol;
            Console.WindowHeight = 30;
            Console.WindowWidth = 125;
            GetAPIRates();
            AppFunctionality();
        }
        private void AppFunctionality()
        {
            FunctionList();
            FunctionSelect();
        }
        private void FunctionList()
        {
            ColorTextMessage("[Function List]", defaultInputCol);
            TextMessage("1 - Currency converter");
            TextMessage("2 - Historical currency converter");
            ColorTextMessage("3 - Exit", ConsoleColor.DarkRed);
        }
        private void FunctionSelect()
        {
            TextMessage("Select Function: ");

            ConsoleCol(defaultInputCol);
            programFunction = Convert.ToInt32(Console.ReadLine());
            ConsoleCol(defaultConCol);
            
            switch (programFunction)
            {
                case 1: 
                    FromToCoversion();
                    break;
                case 2:
                    HistoricalCoversion();
                    break;
                case 3:
                    Environment.Exit(0);
                    break;
                default:
                    ErrorMessage("No Function Selected!");
                    TextMessage("Press Enter To Continue...");
                    Console.ReadLine();
                    Console.Clear();
                    AppFunctionality();
                    break;
            }
        }

        private async void GetAPIRates()
        {
            try
            {
                HttpClient client = new HttpClient();                                    // Create Client instance
                var responseTask = client.GetAsync(latestRatesAPILink).Result;          // Get Rates from API
                string massageTask = await responseTask.Content.ReadAsStringAsync();   // Read API file
                latestRateslist = JObject.Parse(massageTask);                         // Parse JSON file
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
            }
        }

        // ----------- From-To Coversion ----------- //

        private void FromToCoversion()
        {
            GetFromCurrency();
            GetToCurrency();
            GetConversionAmount();

            try
            {
                TextMessage("");
                ColorTextMessage(convertAmount + conFrom + " -> " + ConversionMath() + conTo, ConsoleColor.Blue);
                TextMessage("");
                AppFunctionality();
            }
            catch(Exception ex)
            {
                ErrorMessage(ex.Message);
            }
        }
        private void GetFromCurrency()
        {
            TextMessage("Type \"help\" to view all currencies");
            TextMessage("Convert from(XXX): ");
            ConsoleCol(defaultInputCol);
            conFrom = Console.ReadLine().ToUpper();
            ConsoleCol(defaultConCol);
            if (conFrom == "HELP") 
            {
                HelpMessage();
                GetFromCurrency();
            }
        }
        private void GetToCurrency()
        {
            TextMessage("Convert to(XXX): ");
            ConsoleCol(defaultInputCol);
            conTo = Console.ReadLine().ToUpper();
            ConsoleCol(defaultConCol);
            if (conTo == "HELP")
            {
                HelpMessage();
                GetToCurrency();
            }
        }
        private void GetConversionAmount()
        {
            TextMessage("Convert amount: ");
            ConsoleCol(defaultInputCol);
            convertAmount = Convert.ToInt32(Console.ReadLine());
            ConsoleCol(defaultConCol);
        }
        private float ConversionMath()
        {
            float math = float.Parse(latestRateslist["rates"][conTo].ToString()) / float.Parse(latestRateslist["rates"][conFrom].ToString());
            math = MathF.Round(math * convertAmount);
            return math;
        }

        // ------ From-To Historical Coversion ------ //

        private void HistoricalCoversion()
        {
            GetFromCurrency();
            GetToCurrency();
            GetConversionAmount();
            GetConversionDate();
            GetHistoricalAPIRates();

            ConsoleCol(defaultInputCol);
            if(HistoricalRatesList["rates"] != null)
            {
                TextMessage("");
                TextMessage("   " + convertAmount + conFrom);
                TextMessage("   |"); TextMessage("   v");
                TextMessage("   " + CalculateHistoricalRates(conTo).ToString() + conTo);
                ConsoleCol(defaultInputCol);
                TextMessage("");
            }
            else
            {
                ErrorMessage("Could not find data in given date");
                TextMessage("");
            }
            AppFunctionality();
        }
        private void GetConversionDate()
        {
            TextMessage("Conversion Date(yyyy-mm-dd): ");
            ConsoleCol(defaultInputCol);
            conDate = Console.ReadLine();
            ConsoleCol(defaultConCol);
            HistoricalRatesAPILink = ("http://api.exchangeratesapi.io/v1/" + conDate + "?access_key=6212dcfef232b08ecc9a82634dd9d3a1&base=" + conFrom + "&symbols=" + conTo);
        }
        private async void GetHistoricalAPIRates()
        {
            try
            {
                HttpClient client = new HttpClient();                                    // Create Client instance
                var responseTask = client.GetAsync(HistoricalRatesAPILink).Result;          // Get Rates from API
                string massageTask = await responseTask.Content.ReadAsStringAsync();   // Read API file
                HistoricalRatesList = JObject.Parse(massageTask);                         // Parse JSON file
            }
            catch (Exception ex)
            {
                ErrorMessage(ex.Message);
            }
        }
        private float CalculateHistoricalRates(string con)
        {
            var returnNum = float.Parse(GetTextFromString(HistoricalRatesList["rates"].ToString(), con + "\":", "}"), CultureInfo.InvariantCulture.NumberFormat);
            returnNum = MathF.Round(returnNum * convertAmount);
            return returnNum;
        }

        // ----------------------------------------- //

        private void ErrorMessage(String text)
        {
            Console.ForegroundColor = defaultErrorCol;
            Console.WriteLine(text);
            Console.ForegroundColor = defaultConCol;
        }
        private void TextMessage(String text)
        {
            Console.WriteLine(text);
        }
        private void ColorTextMessage(String text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ForegroundColor = defaultConCol;
        }
        private void ConsoleCol(ConsoleColor ConCol)
        {
            Console.ForegroundColor = ConCol;
        }
        private void HelpMessage()
        {
            Console.WriteLine(text);
        }
        private string GetTextFromString(string source, string textStart, string textEnd)
        {
            if (source.Contains(textStart) && source.Contains(textEnd))
            {
                int start, end;
                start = source.IndexOf(textStart, 0) + textStart.Length;
                end = source.IndexOf(textEnd, start);
                return source.Substring(start, end - start);
            }
            else
            {
                return "[Error]";
            }
        }
    }
}



