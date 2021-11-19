using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Geolocator;
using Plugin.Media;
using PM2E2GRUPO5.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace PM2E2GRUPO5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ModificarDatosPage : ContentPage
    {
        public ModificarDatosPage()
        {
            InitializeComponent();
        }

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

        //Extrae la ubicacion nueva tanto la longitud como la latitud
        private void btnNewUbication_Clicked(object sender, EventArgs e)
        {
            Localizacion();
        }

        //actualizamos mediante la api
        private async void btnSalvarUbicacion_Clicked(object sender, EventArgs e)
        {


            if (String.IsNullOrEmpty(txtDescripcion.Text))
            {
                await DisplayAlert("Campo Vacio", "Por favor, Ingrese una Descripcion de la Ubicacion ", "Ok");
            }
            else
            {
                //Nos Preparamos para Guardar

                string imagen = pathFoto.Text;

                //convertir a arreglo de bytes
                byte[] fileByte = System.IO.File.ReadAllBytes(imagen);

                //convertir a base64
                string pathBase64 = Convert.ToBase64String(fileByte);

                //Double b = Convert.ToDouble(txtLatitud.Text);


                Modificar Modificar = new Modificar
                {
                    ID = Convert.ToInt32(idSitio.Text),
                    descripcion = txtDescripcion.Text,
                    longitud = txtLongitud.Text,
                    latitud = txtLatitud.Text,
                    fotografia = pathBase64,
                };


                Uri RequestUri = new Uri("https://pm-examen.herokuapp.com/ApiSitios");

                var client = new HttpClient();
                var json = JsonConvert.SerializeObject(Modificar);
                var contentJson = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync(RequestUri, contentJson);


                
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

        //podemos subir fotos desde nuestra galeria y muestra
        private async void btnBuscarFoto_Clicked(object sender, EventArgs e)
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

        //Activa la camara para poder mostrar y subir la foto
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


        private async void btnListUbication_Clicked(object sender, EventArgs e)
        {
            var a = new ListSitios();
            await Navigation.PushAsync(a);
        }


        public void Borrar()
        {
            txtDescripcion.Text = "";
            txtLatitud.Text = "";
            txtLongitud.Text = "";

        }
    }
}
