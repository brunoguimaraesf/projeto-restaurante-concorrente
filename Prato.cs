namespace Restaurante
{
    /// <summary>Um item do cardapio.</summary>
    public sealed class Prato
    {
        public Prato(string nome, decimal preco, int preparoMs, bool usaForno,
                     bool usaTabuaEFaca, params string[] ingredientes)
        {
            Nome = nome;
            Preco = preco;
            PreparoMs = preparoMs;
            UsaForno = usaForno;
            UsaTabuaEFaca = usaTabuaEFaca;
            Ingredientes = ingredientes;
        }

        public string Nome { get; }
        public decimal Preco { get; }
        public int PreparoMs { get; }
        public bool UsaForno { get; }
        public bool UsaTabuaEFaca { get; }

        /// <summary>Cada prato gasta 1 unidade de cada ingrediente da lista.</summary>
        public string[] Ingredientes { get; }
    }

    public static class Cardapio
    {
        public const string Massa = "massa";
        public const string Queijo = "queijo";
        public const string Tomate = "tomate";
        public const string Carne = "carne";
        public const string Alface = "alface";
        public const string Pao = "pao";

        public static readonly string[] TodosOsIngredientes =
        {
            Massa, Queijo, Tomate, Carne, Alface, Pao
        };

        public static readonly Prato Pizza =
            new Prato("Pizza", 45.00m, 800, true, false, Massa, Queijo, Tomate);

        public static readonly Prato Lasanha =
            new Prato("Lasanha", 38.00m, 1000, true, false, Massa, Queijo, Carne);

        public static readonly Prato Salada =
            new Prato("Salada", 22.00m, 400, false, true, Alface, Tomate);

        public static readonly Prato Hamburguer =
            new Prato("Hamburguer", 30.00m, 500, false, true, Pao, Carne, Queijo);

        /// <summary>Pizza e Lasanha usam forno; Salada e Hamburguer usam tabua e faca.</summary>
        public static readonly Prato[] Itens = { Pizza, Lasanha, Salada, Hamburguer };
    }
}
