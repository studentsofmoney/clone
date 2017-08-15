using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using WeatherService.ServiceReference;
using System.Net;
using Newtonsoft.Json.Linq;
namespace WeatherService
{
    public partial class formWeather : Form
    {
        internal Cities.NewDataSet cn;
        public formWeather()
        {
            InitializeComponent();
            BasicHttpBinding binding = new BasicHttpBinding();
            binding.MaxReceivedMessageSize = 20000000;

            EndpointAddress address = new EndpointAddress("http://www.webservicex.com/globalweather.asmx");

            GlobalWeatherSoapClient gwsc = new GlobalWeatherSoapClient(binding, address);

            var cities = gwsc.GetCitiesByCountry("");
            XmlSerializer result = new XmlSerializer(typeof(Cities.NewDataSet));

            cn = (Cities.NewDataSet)result.Deserialize(new StringReader(cities));

            var Countries = cn.Table.Select(m => m.Country).Distinct();
            comboBoxCountries.Items.AddRange(Countries.ToArray());

        }

        private void comboBoxCountries_SelectedIndexChanged(object sender, EventArgs e)
        {

            var rr = cn.Table.Where(m => m.Country == comboBoxCountries.Text).Select(c => c.City);

            comboBoxCities.Items.Clear();
            comboBoxCities.Items.AddRange(rr.ToArray());
        }

        private void comboBoxCities_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            string ServiceAddress ="https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20weather.forecast%20where%20woeid%20in%20(select%20woeid%20from%20geo.places(1)%20where%20text%3D%22"+comboBoxCities.Text+"%2C"+comboBoxCountries.Text+"%22)&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
            WebClient request = new WebClient();
            string Response= request.DownloadString(ServiceAddress);
            dynamic forecastJson = JObject.Parse(Response);// used dynamic so it'll not give errors till exe time.

            if (forecastJson["query"].count==1)
            {
                foreach (var ForecastPerDay in forecastJson["query"].results.channel.item.forecast)
                {
                    ForecastOfOneDay dailyforecast = new ForecastOfOneDay { Date = ForecastPerDay.date, Day = ForecastPerDay.day, High = ForecastPerDay.high, Low = ForecastPerDay.low, Discription = ForecastPerDay.text };
                    richTextBoxWeatherDetails.Text += dailyforecast.Getstring() + "\n";
                }
            }
            else
            {
                richTextBoxWeatherDetails.Text = "Error";
            }
            
        }
        class ForecastOfOneDay
        {
            public string Date, Day, High, Low, Discription;
            public ForecastOfOneDay()
            {
                Date = Day = High = Low = Discription = "";
            }
            public string Getstring()
            {
                return ("Date = " + Date + " | Day = " + Day + " | High = " + High + " | Low = " + Low + " | Discription = " + Discription + " |");
            }
        }


    }
}
