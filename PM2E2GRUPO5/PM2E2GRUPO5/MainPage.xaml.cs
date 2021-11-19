using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Maps;
using Plugin.Geolocator;
using Plugin.Media;
using System.IO;
using Xamarin.Forms.Xaml;
using PM2E2GRUPO5.Model;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;


namespace PM2E2GRUPO5
{
    
    public partial class MainPage : ContentPage
    {
        
        
        public MainPage()
        {
            InitializeComponent();            

            var localizacion = CrossGeolocator.Current;
            if (localizacion.IsGeolocationEnabled)//Servicio de Geolocalizacion existente
            {

            }
            else
            {
                DisplayAlert("Permisos Geolocalizacion", "Por favor, de Acceso a su ubicacion/geolocalizacion de manera manual en dispositivo", "OK");
            }
        }


        //Funcion con Validacion

        private async void Localizacion()
        {

            var Gps = CrossGeolocator.Current;

            if (Gps.IsGeolocationEnabled)//VALIDA QUE EL GPS ESTA ENCENDIDO
            {
                var localizacion = CrossGeolocator.Current;
                var posicion = await localizacion.GetPositionAsync();
                txtLongitud.Text = posicion.Longitude.ToString();
                txtLatitud.Text = posicion.Latitude.ToString();
            }

            else
            {
                await DisplayAlert("Gps Desactivado", "Por favor, Active su GPS/Ubicacion", "OK");
            }

        }

        //Evento para capturar la ubicacion        
        private void btnNewUbication_Clicked(object sender, EventArgs e)
        {
            Localizacion();
            txtDescripcion.Text = "";            
        }

        private async void btnTomarFoto_Clicked(object sender, EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await DisplayAlert("Alerta", "Cámara no disponible", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                //Directory = "Sample",
                //Name = "test.jpg"
                SaveToAlbum = true
            });

            if (file == null)
                return;

            //await DisplayAlert("File Location", file.Path, "OK");

            pathFoto.Text = file.AlbumPath;

            //convertir a arreglo de bytes
            byte[] fileByte = System.IO.File.ReadAllBytes(file.AlbumPath);

            //convertir a base64
            string pathBase64 = Convert.ToBase64String(fileByte);
            

            fotografia.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });
        }

        private async void btnBuscarFoto_Clicked(System.Object sender, System.EventArgs e)
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert("Alerta", "No se puede elegir una foto", "OK");
                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync();

            if (file == null)
                return;
            pathFoto.Text = file.Path;
           

            fotografia.Source = ImageSource.FromStream(() =>
            {
                var stream = file.GetStream();
                return stream;
            });
        }

        public void Borrar()
        {
            txtDescripcion.Text = "";
            txtLatitud.Text = "";
            txtLongitud.Text = "";            
            
        }


        private async void btnSalvarUbicacion_Clicked(object sender, EventArgs e)
        {           

            //validamos que los campos no esten vacios
            /*if (txtLatitud.Text == "")
            {
                await DisplayAlert("Sin Datos", "Para Obtener la Lactitud y Longitud presionar <<Nueva Ubicacion>> ", "Ok");
            }
            else */
            if (String.IsNullOrEmpty(txtDescripcion.Text))
            {
                await DisplayAlert("Campo Vacio", "Por favor, Ingrese una Descripcion de la Ubicacion ", "Ok");
            }
            else
            {
                //Nos Preparamos para Guardar

                string imagen =  pathFoto.Text;

                //convertir a arreglo de bytes
                byte[] fileByte = System.IO.File.ReadAllBytes(imagen);

                //convertir a base64
                string pathBase64 = Convert.ToBase64String(fileByte);
                
                //Double b = Convert.ToDouble(txtLatitud.Text);

                Salvar save = new Salvar
                {
                    descripcion = txtDescripcion.Text,
                    longitud = txtLongitud.Text,
                    latitud = txtLatitud.Text,
                    fotografia = pathBase64,
                };
                
                Uri RequestUri = new Uri("https://pm-examen.herokuapp.com/ApiSitios");

                var client = new HttpClient();
                var json = JsonConvert.SerializeObject(save);
                var contentJson = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(RequestUri, contentJson);
                
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    String jsonx = response.Content.ReadAsStringAsync().Result;

                    JObject jsons = JObject.Parse(jsonx);

                    
                    String Mensaje = jsons["Mensaje"].ToString();

                    await DisplayAlert("Success", "Datos guardados correctamente", "Ok");                    

                    Borrar();

                }
                else
                {
                    await DisplayAlert("Error", "Estamos en mantenimiento", "Ok");
                }

            }
            

        }

        private async void btnUbicacionesSalvadas_Clicked(object sender, EventArgs e)
        {
            var a = new ListSitios();
            await Navigation.PushAsync(a);
        }

        private async void btnListUbication_Clicked(object sender, EventArgs e)
        {
            var a = new ListSitios();
            await Navigation.PushAsync(a);
        }
    }
}
