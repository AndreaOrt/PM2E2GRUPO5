using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace PM2E2GRUPO5
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListSitios : ContentPage
    {
        List<Model.ListarSitios> lista = new List<Model.ListarSitios>();

        Object objSitioGlobal = null;
        string idGlobal = "";
        string sitioGlobal = "";
        string latitud = "";
        string longitud = "";
        public ListSitios()
        {
            InitializeComponent();
            ObtenerListaSitios();
        }

        private async void ObtenerListaSitios()
        {
            using (HttpClient cliente = new HttpClient())
            {
                sl.IsVisible = true;
                spinner.IsRunning = true;
                var respuesta = await cliente.GetAsync("https://pm-examen.herokuapp.com/ApiSitios");

                if (respuesta.IsSuccessStatusCode)
                {
                    string contenido = respuesta.Content.ReadAsStringAsync().Result.ToString();

                    dynamic dyn = JsonConvert.DeserializeObject(contenido);
                    byte[] newBytes = null;


                    if (contenido.Length > 28)
                    {

                        foreach (var item in dyn.sitio)
                        {
                            string img64 = item.fotografia.ToString();
                            newBytes = Convert.FromBase64String(img64);
                            var stream = new MemoryStream(newBytes);

                            lista.Add(new Model.ListarSitios(
                                            item.pk.ToString(), item.descripcion.ToString(),
                                            item.longitud.ToString(), item.latitud.ToString(),
                                            ImageSource.FromStream(() => stream),
                                            img64
                                            ));

                        }
                    }
                    else
                    {
                        await DisplayAlert("Notificación", $"Lista vacía, ingrese datos", "Ok");

                    }

                    lsSitios.ItemsSource = null;

                    lsSitios.ItemsSource = lista;
                }
            }

            sl.IsVisible = false;
            spinner.IsRunning = false;


        }

        private void lsSitios_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            idGlobal = null;
            var sitioSelecionado = (Model.ListarSitios)e.SelectedItem;

            idGlobal = sitioSelecionado.id;
            sitioGlobal = sitioSelecionado.descripcion;
            latitud = sitioSelecionado.lalitud;
            longitud = sitioSelecionado.longitud;

            objSitioGlobal = new
            {
                id = sitioSelecionado.id,
                latitud = sitioSelecionado.lalitud,
                longitud = sitioSelecionado.longitud,
                descripcion = sitioSelecionado.descripcion,
                imagen = ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(sitioSelecionado.img64)))

            };

            //var detalle = new DetalleProducto();
            //detalle.BindingContext = producto;
            //await Navigation.PushAsync(detalle);
        }

        private async void btnEliminar_Clicked(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(idGlobal) || !string.IsNullOrEmpty(sitioGlobal))
            {
                bool res = await DisplayAlert("Notificación", $"¿Esta seguro de eliminar el sitio {sitioGlobal}?", "Sí", "Cancelar");

                if (res)
                {

                    object sitio = new
                    {
                        ID = idGlobal
                    };

                    Uri RequestUri = new Uri("https://pm-examen.herokuapp.com/ApiSitios");
                    var client = new HttpClient();
                    var json = JsonConvert.SerializeObject(sitio);

                    HttpRequestMessage request = new HttpRequestMessage
                    {
                        Content = new StringContent(json, Encoding.UTF8, "application/json"),
                        Method = HttpMethod.Delete,
                        RequestUri = RequestUri
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Notificación", $"Registro eliminado con éxito", "Ok");
                        lista.Clear();
                        ObtenerListaSitios();

                    }
                    else
                    {
                        await DisplayAlert("Notificación", $"Ha ocurrido un error", "Ok");

                    }

                }
            }
            else
            {
                await DisplayAlert("Notificación", $"Por favor, seleccione un registro", "Ok");

            }
        }

        private async void btnActualizar_Clicked(object sender, EventArgs e)
        {
            if (objSitioGlobal != null)
            {
                var detalle = new ModificarDatosPage();
                detalle.BindingContext = objSitioGlobal;
                await Navigation.PushAsync(detalle);
            }
            else
            {
                await DisplayAlert("Notificación", $"Por favor, seleccione un registro", "Ok");

            }
        }

        private async void btnVerMapa_Clicked(System.Object sender, System.EventArgs e)
        {
            if (objSitioGlobal != null)
            {

                bool validar = await DisplayAlert("Advertencia", "Desea ingresar a Google Maps", "Yes", "No");

                if (validar == true)
                {
                    Console.WriteLine(objSitioGlobal.GetType());


                    if (!double.TryParse(latitud, out double lat))
                        return;
                    if (!double.TryParse(longitud, out double lng))
                        return;
                    await Map.OpenAsync(lat, lng, new MapLaunchOptions
                    {

                        NavigationMode = NavigationMode.Driving

                    });


                }
                else
                {
                    return;
                }

            }
            else
            {
                await DisplayAlert("Notificación", $"Por favor, seleccione un registro", "Ok");

            }
        }
    }


}