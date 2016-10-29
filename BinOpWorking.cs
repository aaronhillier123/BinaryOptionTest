using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BinaryOption
{

    class zone
    {
        public int name;
        public int total;
        public int higher;
        public int lower;
        public bool direct;
        public double maxPer;

        public zone()
        {
            name = 0;
            total = 0;
            higher = 0;
            lower = 0;
            direct = true;
            maxPer = 0;
        }

        public void calc()
        {
            double hper = (higher / total);
            double lper = (lower / total);
            if (hper >= lper)
            {
                this.maxPer = hper;
                this.direct = true;
            }
            else if (hper < lper)
            {
                this.maxPer = lper;
                this.direct = false;
            }
        }
    }

    class asset //class to represent each forex
    {
        //essential variables
        public string name;
        public int position;
        public List<double> Xpoints;
        public List<double> Ypoints;

        //statistics for the last five minutes of record
        public double fiveslope;
        public double fiveavg;
        public double fiversq;
        public double fivemax;
        public double fivemin;

        //statistics for the last thirty minutes of record
        public double thirtyslope;
        public double thirtyavg;
        public double thirtyrsq;
        public double thirtymax;
        public double thirtymin;

        //determined by statistics
        public int state;
        public int previousState;
        public double previous;
        public double current;
        public bool nextDir;



        public asset() //blank constructor
        {
            name = "no_name";
            position = 0;
            Xpoints = new List<double>();
            Ypoints = new List<double>();

            fiveslope = 0;
            fiveavg = 0;
            fiversq = 0;
            fivemax = 0;
            fivemin = 0;

            thirtyslope = 0;
            thirtyavg = 0;
            thirtyrsq = 0;
            thirtymax = 0;
            thirtymin = 0;

            previous = 0;
            current = 0;
            state = 0;
            nextDir = true;

        }

        //calculates and fills the assets variables
        public void fill()
        {
            if (state > 0)
            {
                previousState = state;
            }

            List<double> a = this.Xpoints;
            List<double> b = this.Ypoints;
            int N = 0;

            double xysumf = 0;
            double xsumf = 0;
            double ysumf = 0;
            double xsqsumf = 0;
            double ysqsumf = 0;
            double maxf = 0;
            double minf = 1000;

            double xysumt = 0;
            double xsumt = 0;
            double ysumt = 0;
            double ysqsumt = 0;
            double xsqsumt = 0;
            double maxt = 0;
            double mint = 1000;

            //Number of elements, current element, previous element
            N = a.Count();
            this.current = b[N - 1];

            this.previous = b[N - 2];


            //calculate five minute asset variables
            if (N >= 60)
            {
                for (int i = Convert.ToInt32((N - 60)); i < N; ++i)
                {
                    xsumf = xsumf + a[i];
                    ysumf = ysumf + b[i];
                    xysumf = xysumf + (a[i] * b[i]);
                    xsqsumf = xsqsumf + (a[i] * a[i]);
                    ysqsumf = ysqsumf + (b[i] * b[i]);

                    if (b[i] > maxf)
                    {
                        maxf = b[i];
                    }

                    if (b[i] < minf)
                    {
                        minf = b[i];
                    }

                }

                //fill the five minute asset variables
                this.fiveslope = (((60 * xysumf) - (xsumf * ysumf)) / ((60 * xsqsumf) - (xsumf * xsumf)));
                this.fiversq = ((60 * xysumf - (xsumf * ysumf)) * (60 * xysumf - (xsumf * ysumf))) / (((60 * xsqsumf) - (xsumf * xsumf)) * ((60 * ysqsumf) - (ysumf * ysumf)));
                this.fivemax = maxf;
                this.fivemin = minf;
                this.fiveavg = (ysumf / 60);
            }

            //calculate thirty minute asset variables
            if (N > 300)
            {
                for (int i = Convert.ToInt32((N - 300)); i < N; ++i)
                {
                    xsumt = xsumt + a[i];
                    ysumt = ysumt + b[i];
                    xysumt = xysumt + (a[i] * b[i]);
                    xsqsumt = xsqsumt + (a[i] * a[i]);
                    ysqsumt = ysqsumt + (b[i] * b[i]);

                    if (maxt < b[i])
                    {
                        maxt = b[i];
                    }

                    if (mint > b[i])
                    {
                        mint = b[i];
                    }
                }

                //fill the thirty minute asset variables
                this.thirtyslope = (((300 * xysumt) - (xsumt * ysumt)) / ((300 * xsqsumt) - (xsumt * xsumt)));
                this.thirtyrsq = ((300 * xysumt - (xsumt * ysumt)) * (300 * xysumt - (xsumt * ysumt))) / (((300 * xsqsumt) - (xsumt * xsumt)) * ((300 * ysqsumt) - (ysumt * ysumt)));
                this.thirtymax = maxt;
                this.thirtymin = mint;
                this.thirtyavg = (ysumt / 300);

            }

            //find which state the asset is currently in


            if (fiveslope >= 0 && thirtyslope >= 0)
            {
                if (current >= fivemax && current >= thirtymax)
                {
                    if (current >= fiveavg && current >= thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 1;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 2;
                        }
                    }
                }
                else if (current >= fivemax && current < thirtymax)
                {
                    if (current >= thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 3;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 4;
                        }
                    }
                    else if (current < thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 5;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 6;
                        }
                    }
                }
                else if (current < fivemax && current < thirtymax)
                {
                    if (current >= previous)
                    {
                        if (current >= fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 7;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 8;
                            }
                        }
                        else if (current >= fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 9;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 10;
                            }
                        }
                        else if (current < fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 11;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 12;
                            }
                        }
                        else if (current < fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 13;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 14;
                            }
                        }
                    }
                    else if (current < previous)
                    {
                        if (current >= fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 15;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 16;
                            }
                        }
                        else if (current >= fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 17;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 18;
                            }
                        }
                        else if (current < fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 19;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 20;
                            }
                        }
                        else if (current < fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 21;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 22;
                            }
                        }
                    }
                }
            }
            else if (fiveslope >= 0 && thirtyslope < 0)
            {
                if (current >= fivemax && current >= thirtymax)
                {
                    if (current >= fiveavg && current >= thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 23;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 24;
                        }
                    }
                }
                else if (current >= fivemax && current < thirtymax)
                {
                    if (current >= thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 25;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 26;
                        }
                    }
                    else if (current < thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 27;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 28;
                        }
                    }
                }
                else if (current < fivemax && current < thirtymax)
                {
                    if (current >= previous)
                    {
                        if (current >= fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 29;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 30;
                            }
                        }
                        else if (current >= fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 31;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 32;
                            }
                        }
                        else if (current < fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 33;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 34;
                            }
                        }
                        else if (current < fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 35;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 36;
                            }
                        }
                    }
                    else if (current < previous)
                    {
                        if (current >= fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 37;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 38;
                            }
                        }
                        else if (current >= fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 39;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 40;
                            }
                        }
                        else if (current < fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 41;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 42;
                            }
                        }
                        else if (current < fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 43;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 44;
                            }
                        }
                    }
                }
            }
            else if (fiveslope < 0 && thirtyslope >= 0)
            {
                if (current >= fivemax && current >= thirtymax)
                {
                    if (current >= fiveavg && current >= thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 45;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 46;
                        }
                    }
                }
                else if (current >= fivemax && current < thirtymax)
                {
                    if (current >= thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 47;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 48;
                        }
                    }
                    else if (current < thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 49;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 50;
                        }
                    }
                }
                else if (current < fivemax && current < thirtymax)
                {
                    if (current >= previous)
                    {
                        if (current >= fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 51;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 52;
                            }
                        }
                        else if (current >= fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 53;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 54;
                            }
                        }
                        else if (current < fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 55;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 56;
                            }
                        }
                        else if (current < fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 57;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 58;
                            }
                        }
                    }
                    else if (current < previous)
                    {
                        if (current >= fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 59;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 60;
                            }
                        }
                        else if (current >= fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 61;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 62;
                            }
                        }
                        else if (current < fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 63;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 64;
                            }
                        }
                        else if (current < fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 65;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 66;
                            }
                        }
                    }
                }
            }
            else if (fiveslope < 0 && thirtyslope < 0)
            {
                if (current >= fivemax && current >= thirtymax)
                {
                    if (current >= fiveavg && current >= thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 67;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 68;
                        }
                    }
                }
                else if (current >= fivemax && current < thirtymax)
                {
                    if (current >= thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 69;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 70;
                        }
                    }
                    else if (current < thirtyavg)
                    {
                        if (thirtyrsq >= .4)
                        {
                            state = 71;
                        }
                        else if (thirtyrsq < .4)
                        {
                            state = 72;
                        }
                    }
                }
                else if (current < fivemax && current < thirtymax)
                {
                    if (current >= previous)
                    {
                        if (current >= fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 73;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 74;
                            }
                        }
                        else if (current >= fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 75;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 76;
                            }
                        }
                        else if (current < fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 77;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 78;
                            }
                        }
                        else if (current < fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 79;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 80;
                            }
                        }
                    }
                    else if (current < previous)
                    {
                        if (current >= fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 81;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 82;
                            }
                        }
                        else if (current >= fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 83;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 84;
                            }
                        }
                        else if (current < fiveavg && current >= thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 85;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 86;
                            }
                        }
                        else if (current < fiveavg && current < thirtyavg)
                        {
                            if (thirtyrsq >= .4)
                            {
                                state = 87;
                            }
                            else if (thirtyrsq < .4)
                            {
                                state = 88;
                            }
                        }
                    }
                }
            }
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            //all essential values needed to run the program
            asset first = new asset();
            asset chosen_one = new asset();
            string username = "binoptest@gmail.com";
            string password = "B1noptest";
            string previous;
            int openedForex = 0;
            double inc = 1;
            int inc2 = 0;
            int time = 0;
            int current_bet = 1;
            bool ready = false;
            bool running = false;
            bool higher = false;
            bool firstbet = true;
            IWebElement readyelem;
            IWebElement win;
            IWebElement closed;
            List<zone> Zones = new List<zone>();


            // Open browser and navigate to Marketsworld
            IWebDriver driver = new ChromeDriver();
            driver.Navigate().GoToUrl("http://www.marketsworld.com");
            System.Threading.Thread.Sleep(10000);
            driver.FindElement(By.Name("user[email]")).SendKeys(username);
            driver.FindElement(By.Name("user[password]")).SendKeys(password);
            driver.FindElement(By.Name("submit")).Click();
            IWebElement query = driver.FindElement(By.XPath("//*[@id='navi']/li[2]/a"));
            query.Click();
            driver.FindElement(By.XPath("//*[@id='graphs_area']/form/div[1]/a/i")).Click();

            //initialize array of zones
            for (int i = 0; i < 89; ++i)
            {
                zone a = new zone();
                a.name = i;
                Zones.Add(a);
            }

            //// Open all Forex's who are currently open 
            for (int i = 1; i < 12; ++i)
            {
                driver.FindElement(By.XPath("//*[@id='content_header']/div/div/div[1]/a")).Click();
                System.Threading.Thread.Sleep(500);
                Boolean isPresent = driver.FindElement(By.XPath("//*[@id='add_asset']/ul/li[2]")).Displayed;
                if (isPresent == true)
                {
                    driver.FindElement(By.XPath("//*[@id='add_asset']/ul/li[2]")).Click();
                }
                System.Threading.Thread.Sleep(200);
                String kk = driver.FindElement(By.XPath("//*[@id='Forex']/ul/li[" + i + "]/a/span[2]")).Text;
                Console.WriteLine(kk);
                if (kk != "closed")
                {

                    try
                    {
                        driver.FindElement(By.XPath("//*[@id='Forex']/ul/li[" + i + "]")).Click();
                        //driver.FindElement(By.CssSelector("# Forex > ul > li:nth-child(" + i + ") > a")).Click();

                        ++openedForex;
                    }
                    catch { }
                }
                System.Threading.Thread.Sleep(500);
            }

            //create list of open assets and initialize them by name and position on webpage
            List<asset> assets = new List<asset>();
            for (int i = 1; i <= openedForex; ++i)
            {
                asset a = new asset();
                a.name = driver.FindElement(By.XPath("//*[@id='graphs_area']/form[" + i + "]/div[1]/h3")).Text;
                a.position = i;
                assets.Add(a);
            }


            //Loop that scrapes data and uses it to calculate information on assets every five seconds
            while (1 == 1)
            {
                for (int i = 0; i < assets.Count(); ++i)
                {
                    //adds current market price value to asset_price list
                    store(assets[i], driver, inc, inc2);

                    //add all data to zones array about previous zones
                    if (assets[i].previousState != 0)
                    {
                        ++Zones[assets[i].previousState].total;
                        if (assets[i].current >= assets[i].previous)
                        {
                            ++Zones[assets[i].previousState].higher;
                        }
                        else if (assets[i].current < assets[i].previous)
                        {
                            ++Zones[assets[i].previousState].lower;
                        }
                    }

                    //calculate slopes of each asset
                    if (assets[i].Xpoints.Count() > 60)
                    {
                        assets[i].fill();
                    }
                    //calculate priority of each asset
                    if (assets[i].Xpoints.Count() > 600)
                    {
                        running = true;
                    }
                    if (assets[i].fiveslope != 0)
                    {
                        Console.WriteLine(assets[i].name + " state = " + assets[i].state);
                    }
                }

                //see if bet is still running
                if (running == true)
                {

                    //bet is still running
                    try
                    {
                        readyelem = driver.FindElement(By.XPath("//*[@id='active_investments']/tbody/tr/td[2]"));
                        ready = false;
                    }
                    //bet is not running
                    catch
                    {
                        ready = true;
                    }
                }

                //decide value of current_bet


                //if the program has been started up, and no bet is current underway, makes a bet
                if (running == true && ready == true)
                {

                    //see if any forexs have closed since the begining, if so, close their boxes
                    for (int i = 1; i <= assets.Count(); ++i)
                    {
                        try
                        {
                            closed = driver.FindElement(By.XPath("//*[@id='graphs_area']/form[" + i + "]/div[2]/div[1]/p"));
                            if (closed.Text == "Closed")
                            {
                                driver.FindElement(By.XPath("//*[@id='graphs_area']/form[" + i + "]/div[1]/a/i")).Click();
                                assets.RemoveAt(i - 1);
                            }
                        }
                        catch { }
                    }


                    chosen_one = choose(assets, Zones);    //choose asset to bet on

                    higher = chosen_one.nextDir;     //choose which direction to bet


                    //see if previous bet won or lost
                    try
                    {
                        win = driver.FindElement(By.XPath("//*[@id='completed_investments']/tbody/tr[1]/td[10]/i"));
                    }
                    catch
                    {
                        driver.Navigate().Refresh();
                        System.Threading.Thread.Sleep(5000);
                        win = driver.FindElement(By.XPath("//*[@id='completed_investments']/tbody/tr[1]/td[10]/i"));
                    }
                    previous = win.GetAttribute("title");

                    //change bet accordingly
                    if (previous == "Lost: Out-of-the-Money")  //if the previous bet lost
                    {
                        current_bet = Convert.ToInt32(Math.Floor(2.5 * Convert.ToDouble(current_bet)));
                    }
                    else if (previous == "Won: In-the-Money")
                    {   //if the previous bet won
                        current_bet = 1;
                    }

                    //disregard first bet scenario
                    if (firstbet == true)
                    {
                        current_bet = 1;
                        firstbet = false;
                    }

                    //see if bet exceeds maximum bet size
                    if (current_bet > 500)
                    {
                        current_bet = 1;
                    }


                    //OBSOLETE
                    //if (current_bet == 12)
                    //{
                    //    if(higher == true)
                    //    {
                    //        higher = false;
                    //    }
                    //    else if(higher == false)
                    //    {
                    //        higher = true;
                    //   }
                    //}

                    //make bet
                    try
                    {
                        bet(chosen_one, higher, current_bet, driver);
                        ready = false;
                    }
                    catch
                    {
                        driver.Navigate().Refresh();
                        System.Threading.Thread.Sleep(5000);
                    }

                }




                ++inc;
                ++inc2;
                ++time;


                System.Threading.Thread.Sleep(5000);

            }

            Console.ReadKey();

        }

        //Function to take current market values and add them to asset instance
        public static void store(asset a, IWebDriver b, double inc, int inc2)
        {

            try
            {
                double tempmarket = Convert.ToDouble(b.FindElement(By.XPath("//*[@id='graphs_area']/form[" + a.position + "]/div[2]/div[1]/div[1]/p")).Text);
                a.Xpoints.Add(inc);
                a.Ypoints.Add(tempmarket);
                if (a.Xpoints.Count > 301)
                {
                    a.Xpoints.RemoveAt(0);
                    a.Ypoints.RemoveAt(0);
                }
                Console.WriteLine(a.name + " ( " + inc + ", " + a.Ypoints[inc2] + " )");
                Debug.WriteLine(inc + " " + a.Xpoints[inc2]);
                ++inc;
                ++inc2;
            }
            catch { };
        }


        //Function to choose the asset out of a list of assets with best possibility of winning bet
        public static asset choose(List<asset> a, List<zone> b)
        {
            int current_i = 0;
            double curper = 0;
            double maxPercent = 0;
            int curState = 0;
            for (int i = 0; i < a.Count; ++i)
            {
                curState = a[i].state;
                b[curState].calc();
                curper = b[curState].maxPer;

                if (curper > maxPercent)
                {
                    maxPercent = curper;
                    current_i = i;
                    a[current_i].nextDir = b[curState].direct;
                }
            }


            return a[current_i];
        }

        public static void bet(asset a, bool dir, int amount, IWebDriver b)
        {

            //type in current_bet into input box
            IWebElement curass = b.FindElement(By.XPath("//*[@id='graphs_area']/form[" + a.position + "]/div[2]/div[1]/div[2]/div/p[3]/input"));
            curass.Clear();
            curass.SendKeys(amount.ToString());

            //click wich direction we want to bet
            if (dir == true)
            {
                b.FindElement(By.XPath("//*[@id='graphs_area']/form[" + a.position + "]/div[2]/div[1]/div[1]/a[1]")).Click();
            }
            else if (dir == false)
            {
                b.FindElement(By.XPath("//*[@id='graphs_area']/form[" + a.position + "]/div[2]/div[1]/div[1]/a[2]")).Click();
            }

            //click the buy button
            b.FindElement(By.XPath("//*[@id='graphs_area']/form[" + a.position + "]/div[2]/div[1]/div[2]/input")).Click();
        }
    }


}