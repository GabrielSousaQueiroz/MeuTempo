namespace MeuTempo.Models
{
    public class Tempo
    {
        // Localização
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? Cidade { get; set; }

        // Temperatura
        public double? Temp { get; set; }
        public double? SensacaoTermica { get; set; }
        public double? Minima { get; set; }
        public double? Maxima { get; set; }

        // Condições atmosféricas
        public int? Umidade { get; set; }
        public int? Visibilidade { get; set; } // Em metros

        // Descrição do clima
        public string? CondicaoPrincipal { get; set; }
        public string? DescricaoDetalhada { get; set; }
        public string? Icone { get; set; }

        // Vento
        public double? VelocidadeVento { get; set; }

        // Sol
        public DateTime? NascerDoSol { get; set; }
        public DateTime? PorDoSol { get; set; }
    }
}