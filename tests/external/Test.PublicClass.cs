using Internal.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skender.Stock.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace External.Tests
{
    public class MyQuote : Quote
    {
        public bool MyProperty { get; set; }
        public decimal MyClose { get; set; }
    }

    public class MyIndicator : EmaResult
    {
        public bool MyProperty { get; set; }
        public float MyEma { get; set; }
    }

    public class MyGenericQuote : IQuote
    {
        // required base properties
        DateTime IQuote.Date => CloseDate;
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        decimal IQuote.Close => CloseValue;
        public decimal Volume { get; set; }

        // custom properties
        public int MyOtherProperty { get; set; }
        public DateTime CloseDate { get; set; }
        public decimal CloseValue { get; set; }
    }


    [TestClass]
    public class PublicClassTests
    {

        [TestMethod()]
        public void ValidateHistory()
        {
            IEnumerable<Quote> history = History.GetHistory();
            history = Cleaners.ValidateHistory(history);

            Indicator.GetSma(history, 5);
        }

        [TestMethod()]
        public void ReadQuoteClass()
        {
            IEnumerable<Quote> history = History.GetHistory();
            List<Quote> h = Cleaners.ValidateHistory(history);

            Quote f = h.FirstOrDefault();
            Console.WriteLine("Date:{0},Close:{1}", f.Date, f.Close);
        }

        [TestMethod()]
        public void DerivedQuoteClass()
        {
            // can use a derive Quote class
            MyQuote myQuote = new MyQuote
            {
                Date = DateTime.Now,
                MyProperty = true
            };

            Assert.AreEqual(true, myQuote.MyProperty);
        }

        [TestMethod()]
        public void DerivedQuoteClassLinq()
        {
            IEnumerable<Quote> history = History.GetHistory();
            history = Cleaners.ValidateHistory(history);

            // can use a derive Quote class using Linq

            IEnumerable<MyQuote> myHistory = history
                .Select(x => new MyQuote
                {
                    Date = x.Date,
                    MyClose = x.Close,
                    MyProperty = false
                });

            Assert.IsTrue(myHistory.Any());
        }

        [TestMethod()]
        public void CustomQuoteClass()
        {
            List<MyGenericQuote> myGenericHistory = History.GetHistory()
                .Select(x => new MyGenericQuote
                {
                    CloseDate = x.Date,
                    Open = x.Open,
                    High = x.High,
                    Low = x.Low,
                    CloseValue = x.Close,
                    Volume = x.Volume,
                    MyOtherProperty = 123456
                })
                .ToList();

            List<EmaResult> results = Indicator.GetEma(myGenericHistory, 20)
                .ToList();

            // assertions

            // proper quantities
            // should always be the same number of results as there is history
            Assert.AreEqual(502, results.Count);
            Assert.AreEqual(483, results.Where(x => x.Ema != null).Count());

            // sample values
            EmaResult r1 = results[501];
            Assert.AreEqual(249.3519m, Math.Round((decimal)r1.Ema, 4));

            EmaResult r2 = results[249];
            Assert.AreEqual(255.3873m, Math.Round((decimal)r2.Ema, 4));

            EmaResult r3 = results[29];
            Assert.AreEqual(216.6228m, Math.Round((decimal)r3.Ema, 4));
        }

        [TestMethod()]
        public void DerivedIndicatorClass()
        {
            // can use a derive Indicator class
            MyIndicator myIndicator = new MyIndicator
            {
                Date = DateTime.Now,
                MyEma = 123.456f,
                MyProperty = false
            };

            Assert.AreEqual(false, myIndicator.MyProperty);
        }

        [TestMethod()]
        public void DerivedIndicatorClassLinq()
        {
            IEnumerable<Quote> history = History.GetHistory();
            IEnumerable<EmaResult> emaResults = Indicator.GetEma(history, 14);

            // can use a derive Indicator class using Linq

            IEnumerable<MyIndicator> myIndicatorResults = emaResults
                .Where(x => x.Ema != null)
                .Select(x => new MyIndicator
                {
                    Date = x.Date,
                    MyEma = (float)x.Ema,
                    MyProperty = false
                });

            Assert.IsTrue(myIndicatorResults.Any());
        }

    }
}
