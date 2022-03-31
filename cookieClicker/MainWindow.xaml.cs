using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
//using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
//using System.Windows.Media.Imaging;
//using System.Windows.Navigation;
//using System.Windows.Shapes;
//using System.Windows.Controls.Primitives;
using System.Threading;
using System.Numerics;
using System.Security.Cryptography;

namespace cookieClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class cookieGiver
    {
        public string Name { get; set; }
        public BigInteger Price { get; set; }
        public BigInteger CPC { get; set; }
        public int Number { get; set; }

        public cookieGiver(string name, BigInteger price, BigInteger cookiesPerClick, int number)
        {
            Name = name;
            Price = price;
            Number = number;
            if (number != 0)
            {
                CPC = cookiesPerClick * number;
            }
            else
            {
                CPC = cookiesPerClick;
            }
            
        }
    }

    

    internal class cookieMain
    { 
        public cookieGiver[] cookieGivers { get; set; }
        public int CookieClickCount { get;  set; }

        public cookieMain(int cookieClickCount)
        {
            createGivers();
            CookieClickCount = cookieClickCount;
        }

        private void createGivers()
        {
            FileStore f = new FileStore("prices.csv", @"priceInfo\" , "");
            string[][] file = f.fileLoad();
            
            cookieGivers = new cookieGiver[file.Length];

            for (int i = 0; i < file.Length; i++)
            {
                for (int j = 0; j < file[i].Length; j++)
                {
                    // name, price, cookiesPerClick, number
                    cookieGiver giver = new cookieGiver(file[i][0], BigInteger.Parse(file[i][1]), BigInteger.Parse(file[i][2]), 0);
                    // cookieGiver giver = new cookieGiver("Gran", 100, 10, 0);
                    cookieGivers[i] = (giver);
                }
            }
        }

        
    }

    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();



        cookieMain cook = new cookieMain(0);
        BigInteger totalCookies = 0;
        BigInteger CookiesPS = 1;
        

        public MainWindow()
        {
            InitializeComponent();
            makeCursor();
            

            for (int i = 0; i < cook.cookieGivers.Length; i++)
            {
                Button b = new Button();
                
                b.Name = $"BTN{cook.cookieGivers[i].Name}";
                b.Content = $"{cook.cookieGivers[i].Name.ToString()} - £{cook.cookieGivers[i].Price}";
                b.Background = Brushes.White;
                

                b.Tag = i;

                // https://stackoverflow.com/questions/4815629/how-do-i-pass-variables-to-a-buttons-event-method

                 b.Click += (sender, RoutedEventArgs) => {  buyAsync(sender, RoutedEventArgs); };

                buyPanel.Children.Add(b);
            }

            
            dispatcherTimer.Tick += loopUpdate;
            //dispatcherTimer.Tick += cursorUpdate;
            dispatcherTimer.Interval = new TimeSpan(1000);
            dispatcherTimer.Start();


            Thread thread = new Thread(cursorUpdate);
            thread.Start();
        }


        int cookieCursors = 0;

        private void makeCursor()
        {
            Button b = new Button();
            b.Name = $"BTNCursor";
            b.Content = $"Cursor - £{15}";
            b.Background = Brushes.White;
            b.Tag = 15;
            b.Click += (sender, RoutedEventArgs) => { buyAsyncCursor(sender, RoutedEventArgs); };

            buyPanel.Children.Add(b);
        }
        

        private async Task buyAsyncCursor(object sender, RoutedEventArgs e) //EventArgs //  cookieGiver person
        {
            Button button = (Button)sender;
            string s = button.Tag.ToString();
            BigInteger price = BigInteger.Parse(s);

            if (price <= totalCookies)
            {
                totalCookies -= price;
                cookieCursors++;

                price = new BigInteger(Convert.ToDouble(price.ToString()) * 1.25);
                button.Content = $"Cursor - £{price}";
                button.Tag = price;
            }
            else
            {
                boughtWarn.Content = $"You can't afford \n another cursor";
                boughtWarn.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                boughtWarn.Visibility = Visibility.Collapsed;
            }
        }


        private void cursorUpdate()
        {
            while (true)
            {
                if (cookieCursors != 0)
                {
                    totalCookies += cookieCursors * CookiesPS;
                    Thread.Sleep(1000);
                }
            }
        }


        private void loopUpdate(object sender, EventArgs e)
        {
            Cursors.Content = $"Total Cursors: {cookieCursors}";
            CPS.Content = $"cookies Per Click: {CookiesPS}";
            TotalCookiesLB.Content = $"Total Cookies: {totalCookies}";
            
            // Console.WriteLine(CookiesPS);

            if (totalCookies < 0)
            {
                MessageBox.Show("You have gone cookie bankrupt");
            }
        }

        private async Task buyAsync(object sender, RoutedEventArgs e) //EventArgs //  cookieGiver person
        {
            Button button = (Button)sender;
            cookieGiver c = cook.cookieGivers[Convert.ToInt32(button.Tag)];

            if (c.Price <= totalCookies)
            {
                totalCookies -= c.Price;
                c.Number++;
                CookiesPS += c.CPC;

                c.Price = new BigInteger(Convert.ToDouble(c.Price.ToString()) * 1.25);
                button.Content = $"{c.Name} - £{c.Price}";

                boughtWarn.Content = $"You bought {c.Name}";
                boughtWarn.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                boughtWarn.Visibility = Visibility.Collapsed;
            }
            else
            {
                boughtWarn.Content = $"You can't afford {c.Name}";
                boughtWarn.Visibility = Visibility.Visible;
                await Task.Delay(2000);
                boughtWarn.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            Environment.Exit(0);

        }
        


        private void cookieClick_Click(object sender, RoutedEventArgs e)
        {
            totalCookies += CookiesPS;
        }
    }
    public class FileStore
    {
        public string Name { get; set; }
        public string PathStr { get; set; }
        public string Body { get; set; }

        public FileStore(string name, string path, string body)
        {
            Name = name; // @"priceInfo\"
            PathStr = path; // @"prices.csv"
            Body = body;
        }



        internal virtual string[][] fileLoad()
        {
            // string folderPath = System.IO.Path.GetFullPath(@"..\..\..\") + @"priceInfo\prices.csv";
            string folderPath = System.IO.Path.GetFullPath(PathStr+Name); // @"priceInfo\prices.csv"
            string separator = ",";
            string[] lines = File.ReadAllLines(folderPath);

            lines = lines.Skip(1).ToArray();

            string[][] file = new string[lines.Length][];

            for (int i = 0; i < lines.Length; i++)
            {
                file[i] = lines[i].Split(separator);
            }

            
            return file;
        }
    }
}
