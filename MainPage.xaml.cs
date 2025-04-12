using MeuTempo.Models;
using MeuTempo.Services;
using System.Net;
using Microsoft.Maui.Devices.Sensors;

namespace MeuTempo;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnBuscarClima(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CidadeEntry.Text))
        {
            await DisplayAlert("Aviso", "Por favor, digite uma cidade", "OK");
            return;
        }

        try
        {
          

            var cidade = CidadeEntry.Text.Trim();
            var tempo = await DataService.GetPrevisao(cidade);

            if (tempo == null)
            {
                await DisplayAlert("Erro", "Cidade não encontrada", "OK");
                return;
            }

            // Preencher os dados
            CidadeLabel.Text = tempo.Cidade;
            TemperaturaLabel.Text = $"{tempo.Temp:0}°C";
            DescricaoLabel.Text = tempo.DescricaoDetalhada?.FirstCharToUpper();
            IconeClima.Source = $"https://openweathermap.org/img/wn/{tempo.Icone}@2x.png";

            SensacaoLabel.Text = $"Sensação: {tempo.SensacaoTermica:0}°C";
            MinMaxLabel.Text = $"Min: {tempo.Minima:0}°C / Max: {tempo.Maxima:0}°C";
            UmidadeLabel.Text = $"Umidade: {tempo.Umidade}%";
            VentoLabel.Text = $"Vento: {tempo.VelocidadeVento:0} m/s";
            VisibilidadeLabel.Text = tempo.Visibilidade == 10000 ?
                "Visibilidade: ≥10 km" :
                $"Visib.: {(tempo.Visibilidade / 1000):0.0} km";

            if (tempo.NascerDoSol.HasValue && tempo.PorDoSol.HasValue)
            {
                SolLabel.Text = $"🌅 {tempo.NascerDoSol:HH:mm} / 🌇 {tempo.PorDoSol:HH:mm}";
            }

            ResultadoFrame.IsVisible = true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            await DisplayAlert("Sem Conexão", "Verifique sua conexão com a internet e tente novamente", "OK");
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
        }
       
    }

    private async void OnBuscarPorLocalizacao(object sender, EventArgs e)
    {
        try
        {

            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Best, TimeSpan.FromSeconds(10));
                var location = await Geolocation.GetLocationAsync(request);

                if (location == null)
                {
                    await DisplayAlert("Erro", "Não foi possível obter a localização", "OK");
                    return;
                }

                CidadeEntry.Text = await GetCityNameFromCoordinates(location.Latitude, location.Longitude);
                OnBuscarClima(sender, e);
            }
            else
            {
                await DisplayAlert("Permissão Necessária", "A localização é necessária para esta função", "OK");
            }
        }
        catch (FeatureNotSupportedException)
        {
            await DisplayAlert("Erro", "Geolocalização não suportada neste dispositivo", "OK");
        }
        catch (FeatureNotEnabledException)
        {
            await DisplayAlert("Erro", "GPS desativado. Ative a localização nas configurações", "OK");
        }
        catch (PermissionException)
        {
            await DisplayAlert("Erro", "Permissão de localização negada. Por favor, conceda a permissão nas configurações", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao obter localização: {ex.Message}", "OK");
        }
      
    }

    private async Task<string> GetCityNameFromCoordinates(double latitude, double longitude)
    {
        try
        {
            var url = $"http://api.openweathermap.org/geo/1.0/reverse?lat={latitude}&lon={longitude}&limit=1&appid=d7182c31d48e824956d1e1324d3dc723";

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic[]>(json);
                    return data?[0]?.name ?? "Local Desconhecido";
                }
            }
        }
        catch
        {
            // Se houver erro, retorna as coordenadas
        }
        return $"{latitude:0.00}, {longitude:0.00}";
    }
}

public static class StringExtensions
{
    public static string FirstCharToUpper(this string input) =>
        input switch
        {
            null => null,
            "" => "",
            _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
        };
}