using MeuTempo.Models;
using MeuTempo.Services;
using System.Net;

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
                ResultadoFrame.IsVisible = false;
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
            VisibilidadeLabel.Text = $"Visib.: {(tempo.Visibilidade / 1000):0.0} km";

            if (tempo.NascerDoSol.HasValue && tempo.PorDoSol.HasValue)
            {
                SolLabel.Text = $"🌅 {tempo.NascerDoSol:HH:mm} / 🌇 {tempo.PorDoSol:HH:mm}";
            }

            ResultadoFrame.IsVisible = true;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            await DisplayAlert("Sem Conexão", "Verifique sua conexão com a internet e tente novamente", "OK");
            ResultadoFrame.IsVisible = false;
        }
        catch (HttpRequestException ex)
        {
            await DisplayAlert("Erro", ex.Message, "OK");
            ResultadoFrame.IsVisible = false;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
            ResultadoFrame.IsVisible = false;
        }
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