using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp4
{
    public static class DriverFactory
    {
        public static IWebDriver GetDriver(string browser)
        {
            switch (browser)
            {
                case "edge":
                    return new EdgeDriver();
                case "chrome":
                    return new ChromeDriver();
                case "firefox":
                    return new FirefoxDriver();
                default:
                    break;
            }

            throw new ArgumentException("Browser is not supporred!");
        }
    }
}
