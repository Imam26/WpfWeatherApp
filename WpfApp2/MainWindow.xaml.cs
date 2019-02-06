using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using SharpVectors.Converters;
using SharpVectors.Renderers.Wpf;

namespace WpfApp2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();            
        }

        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            int days = 4;

            List<PlaceCoord> placeCoords = new List<PlaceCoord>();
            placeCoords.Add(new PlaceCoord { Name="Астана", Lat = "51.1801000", Lon= "71.4459800" });
            placeCoords.Add(new PlaceCoord { Name = "Алматы", Lat = "43.2566700", Lon = "76.9286100" });
            placeCoords.Add(new PlaceCoord { Name = "Павлодар", Lat = "52.2833300", Lon = "76.9666700" });
            placeCoords.Add(new PlaceCoord { Name = "Экибастуз", Lat = "51.7237100", Lon = "75.3228700" });
            placeCoords.Add(new PlaceCoord { Name = "Кокшетау", Lat = "53.2833300", Lon = "69.4000000" });

            string selectedItem = ((ComboBoxItem)cityList.SelectedItem).Content.ToString();
            PlaceCoord findPlaceCoord = placeCoords.Find(item => item.Name == selectedItem);

            if (findPlaceCoord == null)
            {
                MessageBox.Show("В БД отсутсвует информация по данной местности");
                return;
            }

            string allDataRequestUriStr = "https://api.weather.yandex.ru/v1/forecast?lat=" + findPlaceCoord.Lat + "&lon=" + findPlaceCoord.Lon
                + "&lang=ru_RU&limit=7&hours=false&extra=false";

                  
            WebRequest request = WebRequest.Create(allDataRequestUriStr);
            request.Headers.Add("X-Yandex-API-Key: 99e485d2-7855-440e-a07c-f7ca00df1faa");
            WebResponse response = request.GetResponse();

            dynamic allData;
            List<string> iconsXML = new List<string>();

            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    allData = JsonConvert.DeserializeObject(reader.ReadToEnd());                            
                }
            }

            SetImageElement(allData);
            SetWeatherInfoBlock(allData.forecasts);
        }

        private void CityList_SelectionChanged(object sender, RoutedEventArgs e)
        {
            
        }

        private void SetWeatherInfoBlock(dynamic data)
        {
            var textBlocks = weatherInfoGrid.Children.OfType<TextBlock>();

            for(int i = 0; i<textBlocks.Count(); i++)
            {
                textBlocks.ElementAt(i).Text = "Дата: " + data[i].date + "\n"
                        + "Температура воздуха: " + data[i].parts.day_short.temp + "\n"
                        + "Ощущается: " + data[i].parts.day_short.feels_like + "\n"
                        + "Скорость ветра: " + data[i].parts.day_short.wind_speed + " м/с\n"
                        + "Влажность воздуха: " + data[i].parts.day_short.humidity + " %\n";
            }
        }

        private void SetImageElement(dynamic allData)
        {
            WpfDrawingSettings settings = new WpfDrawingSettings();
            settings.IncludeRuntime = true;
            settings.TextAsGeometry = false;

            StreamSvgConverter converter = new StreamSvgConverter(settings);                        

            WebRequest request;

            var imagesElement = weatherInfoGrid.Children.OfType<Image>();

            for (int i = 0; i < imagesElement.Count(); i++)
            {
                string iconDataRequestUriStr = "https://yastatic.net/weather/i/icons/blueye/color/svg/"
                    + allData.forecasts[i].parts.day_short.icon + ".svg";

                request = WebRequest.Create(iconDataRequestUriStr);

                using (MemoryStream memStream = new MemoryStream())
                {
                    using (Stream stream = request.GetResponse().GetResponseStream())
                    {
                        if (converter.Convert(stream, memStream))
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = memStream;
                            bitmap.EndInit();

                            imagesElement.ElementAt(i).Source = bitmap;
                        }
                    }
                }                                       
            }            
        }
    }
}
