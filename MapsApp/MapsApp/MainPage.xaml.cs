using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Services.Maps;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace MapsApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Geolocator geolocator = null;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            geolocator = new Geolocator();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.
            getGeoposition();
            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        /// <summary>
        /// Obtiene la posición del dispositivo
        /// </summary>
        private async void getGeoposition()
        {
            progressRibbon.Visibility = Windows.UI.Xaml.Visibility.Visible;
            ring.IsActive = true;
            Geoposition pos = await geolocator.GetGeopositionAsync();
            
            map.Center = pos.Coordinate.Point;
            map.ZoomLevel = 20;
            map.LandmarksVisible = true;
            MapIcon icon = new MapIcon();
            icon.Location = pos.Coordinate.Point;
            icon.Title = "Nuevo Pin"; 
            
            map.MapElements.Add(icon);
            //position.Text = pos.CivicAddress.Country; CivicAddres siempre regresa nulo en windows phone
            position.Text = await getAddress(pos.Coordinate.Point);
            ring.IsActive = false;
            progressRibbon.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        /// <summary>
        /// Obtiene la dirección asociada a un Geopoint
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private async Task<string> getAddress(Geopoint point)
        {
            string address = "Sin dirección encontrada";
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(point);
            if (result.Status == MapLocationFinderStatus.Success && result.Locations.Count > 0)
            {
                address =  String.Format("{2} {3}, Colonia {4} Delegación {5},C.P. {6} {1}, {0}",
                    result.Locations[0].Address.Country,
                    result.Locations[0].Address.Town,
                    result.Locations[0].Address.Street,
                    result.Locations[0].Address.StreetNumber,
                    result.Locations[0].Address.District,
                    result.Locations[0].Address.Neighborhood,
                    result.Locations[0].Address.PostCode
                    );
            }
            return address;
        }
    }
}
