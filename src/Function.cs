using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

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
        public Othercode(flambda input)
        {

        }
        public string run()
        {
            IWebDriver driver = get_driver();
            Console.WriteLine("Got Driver");

            driver.Url = "https://www.google.com/";
            IWebElement element = driver.FindElement(By.Name("q"));
            element.SendKeys("21 buttons");
            element.Submit();

            //driver.FindElement(By.Name("btnK")).Click();
            System.Threading.Thread.Sleep(5000);

            string first_google_result_title = driver.FindElement(By.XPath("(//div[@class=\"rc\"]//a)[1]")).GetAttribute("innerHTML");

            return first_google_result_title;
        }
        public IWebDriver get_driver()
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
            chrome_options.AddArgument("--window-size=1280x1696");
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
    }
}
