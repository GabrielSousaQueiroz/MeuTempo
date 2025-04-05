using MeuTempo.Models;
using Newtonsoft.Json.Linq;
using System.Net;

namespace MeuTempo.Services
{
    public class DataService
    {
        public static async Task<Tempo?> GetPrevisao(string cidade)
        {
            string chave = "d7182c31d48e824956d1e1324d3dc723";
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={cidade}" +
                        $"&units=metric&appid={chave}&lang=pt_br";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Verifica conexão com internet antes de fazer a requisição
                    if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                    {
                        throw new HttpRequestException("Sem conexão com a internet", null, HttpStatusCode.ServiceUnavailable);
                    }

                    HttpResponseMessage resp = await client.GetAsync(url);

                    // Tratamento dos status codes
                    if (!resp.IsSuccessStatusCode)
                    {
                        string errorMessage = resp.StatusCode switch
                        {
                            HttpStatusCode.NotFound => "Cidade não encontrada",
                            HttpStatusCode.Unauthorized => "Chave API inválida",
                            HttpStatusCode.ServiceUnavailable => "Serviço indisponível",
                            _ => $"Erro na requisição: {resp.StatusCode}"
                        };
                        throw new HttpRequestException(errorMessage, null, resp.StatusCode);
                    }

                    string json = await resp.Content.ReadAsStringAsync();
                    var dados = JObject.Parse(json);

                    // Conversão dos horários do sol
                    DateTimeOffset? sunrise = dados["sys"]?["sunrise"] != null ?
                        DateTimeOffset.FromUnixTimeSeconds((long)dados["sys"]["sunrise"]) : null;

                    DateTimeOffset? sunset = dados["sys"]?["sunset"] != null ?
                        DateTimeOffset.FromUnixTimeSeconds((long)dados["sys"]["sunset"]) : null;

                    return new Tempo
                    {
                        Latitude = (double?)dados["coord"]?["lat"],
                        Longitude = (double?)dados["coord"]?["lon"],
                        Cidade = dados["name"]?.ToString(),
                        Temp = (double?)dados["main"]?["temp"],
                        SensacaoTermica = (double?)dados["main"]?["feels_like"],
                        Minima = (double?)dados["main"]?["temp_min"],
                        Maxima = (double?)dados["main"]?["temp_max"],
                        Umidade = (int?)dados["main"]?["humidity"],
                        Visibilidade = (int?)dados["visibility"],
                        CondicaoPrincipal = dados["weather"]?[0]?["main"]?.ToString(),
                        DescricaoDetalhada = dados["weather"]?[0]?["description"]?.ToString(),
                        Icone = dados["weather"]?[0]?["icon"]?.ToString(),
                        VelocidadeVento = (double?)dados["wind"]?["speed"],
                        NascerDoSol = sunrise?.DateTime,
                        PorDoSol = sunset?.DateTime
                    };
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao obter previsão: {ex.Message}");
                    throw; // Re-lança a exceção para tratamento na UI
                }
            }
        }
    }
}