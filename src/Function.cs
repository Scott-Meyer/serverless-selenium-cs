using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace serverless_selenium_cs
{
    public class Function
    {
        
        /// <summary>
        /// A simple function that translates input into a flambda struct, then passes it to Othercode class
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(flambda input, ILambdaContext context)
        {
            Othercode ocode = new Othercode(input);
            return ocode.run();
        }
    }
    public struct flambda
    {
        public string url;
        public string width;
        public string height;
    }
    public class Othercode
    {
        private flambda input;
        public Othercode(flambda inputs)
        {
            input = inputs;
        }
        public string run()
        {
            // Orientation matters mostly for proper chart sizing. We open up 2 chrome processes, one for landscape and one for portrait.
            string currentOrientation = int.Parse(input.width) >= 1000 ? ".Landscape" : "Portrait";
            string strWidth;
            string strHeight;
            DetermineSizeFromOrientation(currentOrientation, out strWidth, out strHeight);

            IWebDriver driver = null;
            try
            {
                string host = new Uri(input.url).Host;

                // Start up the web driver.
                driver = get_driver(input.width, input.height);
                driver.Navigate().GoToUrl(input.url);
                // Allow the dashboards to load, to allow the web report sections done increment or decrement the counter.
                Thread.Sleep(3000);
            //    cancellationToken.ThrowIfCancellationRequested();
                // Make the chrome driver wait a little bit longer than the request cancellation time out period,
                // thus when the chrome driver times out, the request must also be timed out too.
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(32));
                // This loop ensures that the report is *really* ready.
                bool isReady = false;
                while (!isReady)
                {
                    try
                    {
                        wait.Until(d => (bool)((d as IJavaScriptExecutor).ExecuteScript("return window.webReportSectionsDone") ?? false) == true); // Wait for our page to tell us we are done loading.
                    }
                    catch (WebDriverTimeoutException)
                    {
                        // Catch timeout exceptions here as this means that at least one of the sections times out while loading.
                        // If one section times out in a report, the entire report should not be generated.
            //            cancellationToken.ThrowIfCancellationRequested();
                    }
                    catch (InvalidOperationException) { } // Catch invalid operation exception if the page is not loaded yet but code need to access 'webReportSectionsDone'

                    Thread.Sleep(500); // Wait half a second to ensure we're done processing
                    // Check if still ready. If still ready, then exit loop.
                    IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                    isReady = (bool)js.ExecuteScript("return window.webReportSectionsDone");
            //        cancellationToken.ThrowIfCancellationRequested();
                }

                //Get source
                string outputContents = driver.PageSource;

                driver.Navigate().GoToUrl(new Uri(input.url).GetLeftPart(UriPartial.Authority) + "/#/logout"); // Initiate a logout request so the next time we use this instance, we don't reuse the session.

                return outputContents;
            }
            finally
            {
                // Clean up driver resources
                if (driver != null)
                {
                    driver.Quit();
                }
            }
        }

        /// <summary>
        /// A function to get a IWebDriver while in AWS land.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public IWebDriver get_driver(string width, string height)
        {
            ChromeOptions chrome_options = new ChromeOptions();

            string _tmp_folder = "/tmp/{Guid.NewGuid()}";

            if (!System.IO.Directory.Exists(_tmp_folder))
            {
                System.IO.Directory.CreateDirectory(_tmp_folder);
            }

            if (!System.IO.Directory.Exists(_tmp_folder + "/user-data"))
            {
                System.IO.Directory.CreateDirectory(_tmp_folder + "/user-data");
            }

            if (!System.IO.Directory.Exists(_tmp_folder + "/data-path"))
            {
                System.IO.Directory.CreateDirectory(_tmp_folder + "/data-path");
            }

            if (!System.IO.Directory.Exists(_tmp_folder + "/cache-dir"))
            {
                System.IO.Directory.CreateDirectory(_tmp_folder + "/cache-dir");
            }

            chrome_options.AddArgument("--headless");
            chrome_options.AddArgument("--no-sandbox");
            chrome_options.AddArgument("--disable-gpu");
            chrome_options.AddArgument(string.Format("--window-size={0}x{1}", width, height));
            chrome_options.AddArgument(string.Format("--user-data-dir={0}", _tmp_folder + "/user-data"));
            chrome_options.AddArgument("--hide-scrollbars");
            chrome_options.AddArgument("--enable-logging");
            chrome_options.AddArgument("--log-level=0");
            chrome_options.AddArgument("--v=99");
            chrome_options.AddArgument("--single-process");
            chrome_options.AddArgument(string.Format("--data-path={0}", _tmp_folder + "/data-path"));
            chrome_options.AddArgument("--ignore-certificate-errors");
            chrome_options.AddArgument(string.Format("--homedir={0}", _tmp_folder));
            chrome_options.AddArgument(string.Format("--disk-cache-dir={0}", _tmp_folder + "/cache-dir"));
            chrome_options.AddArgument("user-agent=Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            chrome_options.BinaryLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/bin/headless-chromium";

            //Create the reference for our browser
            IWebDriver driver = new ChromeDriver(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/bin", chrome_options);

            //Return the driver.
            return driver;
        }

        /// <summary>
        /// Determine the width and height for the report from report orientation.
        /// </summary>
        /// <param name="orientation">Report orientation</param>
        /// <param name="strWidth"></param>
        /// <param name="strHeight"></param>
        public static void DetermineSizeFromOrientation(string orientation,
            out string strWidth, out string strHeight)
        {
            // These numbers were based off the sizes set in the css theme files.
            // Some may be slightly tweaked for the web.
            // The first set is landscape orientation, second is portrait.
            // For the portrait theme labels to display correctly in PDF report,
            // and in order to render the chart in a proper size for portrait theme,
            // the minimum width should be 1428 for both one-column and two-column report layout.
            // CHANING THIS WILL AFFECT THE WEB PDF REPORT CHART SIZE GREATLY.
            // SO PLEASE DO NOT CHANGE THIS UNLESS YOU HAVE A GOOD REASON.
            strWidth = orientation == "Landscape" ? "1932" : "1428";
            strHeight = orientation == "Landscape" ? "1428" : "1932";
        }
    }
}
