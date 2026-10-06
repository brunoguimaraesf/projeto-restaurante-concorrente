using System;
using System.Collections.Generic;
using System.Text;

namespace Restaurante
{
    /// <summary>
    /// SECAO 7 - relatorio final com as tres verificacoes automaticas.
    /// </summary>
    public sealed class Relatorio
    {
        public string Modo = "SEGURO";
        public int Cozinheiros;
        public int Fornos;
        public TimeSpan? Fechamento;

        public int Recebidos;
        public int Entregues;
        public int Recusados;
        public int NaoAtendidos;

        public IReadOnlyDictionary<string, int> Vendas = new Dictionary<string, int>();
        public decimal FaturamentoEsperado;
        public decimal FaturamentoRegistrado;
        public IReadOnlyDictionary<string, int> EstoqueFinal = new Dictionary<string, int>();
        public TimeSpan TempoTotal;

        /// <summary>Imprime o relatorio e devolve true se as tres verificacoes passaram.</summary>
        public bool Imprimir()
        {
            Console.WriteLine();
            Console.WriteLine("===== RELATÓRIO · Restaurante Concorrente =====");
            Console.WriteLine("Modo: " + Modo +
                              " | Cozinheiros: " + Cozinheiros +
                              " | Fornos: " + Fornos +
                              " | Fechamento: " + (Fechamento.HasValue
                                  ? Fechamento.Value.TotalSeconds.ToString("N0") + " s"
                                  : "sem fechamento"));
            Console.WriteLine();

            Console.WriteLine("Pedidos recebidos ............ " + Recebidos);
            Console.WriteLine("Entregues .................... " + Entregues);
            Console.WriteLine("Recusados (sem ingrediente) .. " + Recusados);
            Console.WriteLine("Não atendidos (fechamento) ... " + NaoAtendidos);
            Console.WriteLine();

            Console.WriteLine("Vendas: " + Juntar(Vendas, Cardapio.Itens));
            Console.WriteLine("Faturamento esperado ......... " + FaturamentoEsperado.ToString("C2"));
            Console.WriteLine("Faturamento registrado ....... " + FaturamentoRegistrado.ToString("C2"));
            Console.WriteLine("Estoque final: " + JuntarEstoque());
            Console.WriteLine();

            bool v1 = Recebidos == Entregues + Recusados + NaoAtendidos;
            Verificar(v1, "recebidos = entregues + recusados + não atendidos",
                      Recebidos + " != " + Entregues + " + " + Recusados + " + " + NaoAtendidos +
                      " (faltam " + (Recebidos - Entregues - Recusados - NaoAtendidos) + " pedidos)");

            bool v2 = FaturamentoRegistrado == FaturamentoEsperado;
            Verificar(v2, "faturamento registrado = faturamento esperado",
                      "o caixa perdeu " + (FaturamentoEsperado - FaturamentoRegistrado).ToString("C2"));

            bool v3 = !TemIngredienteNegativo();
            Verificar(v3, "nenhum ingrediente com estoque negativo",
                      "negativos: " + ListarNegativos());

            Console.WriteLine("Tempo total: " + TempoTotal.TotalSeconds.ToString("N1") + " s");
            Console.WriteLine();

            return v1 && v2 && v3;
        }

        private void Verificar(bool passou, string regra, string detalhe)
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = passou ? ConsoleColor.Green : ConsoleColor.Red;
            Console.Write(passou ? "[OK]   " : "[ERRO] ");
            Console.ForegroundColor = original;
            Console.WriteLine(regra);

            if (!passou)
                Console.WriteLine("       -> " + detalhe);
        }

        private bool TemIngredienteNegativo()
        {
            foreach (var item in EstoqueFinal)
            {
                if (item.Value < 0)
                    return true;
            }
            return false;
        }

        private string ListarNegativos()
        {
            var texto = new StringBuilder();
            foreach (var item in EstoqueFinal)
            {
                if (item.Value < 0)
                {
                    if (texto.Length > 0)
                        texto.Append(" | ");
                    texto.Append(item.Key + " " + item.Value);
                }
            }
            return texto.Length == 0 ? "(nenhum)" : texto.ToString();
        }

        private string Juntar(IReadOnlyDictionary<string, int> vendas, Prato[] ordem)
        {
            var texto = new StringBuilder();
            foreach (var prato in ordem)
            {
                int quantidade;
                vendas.TryGetValue(prato.Nome, out quantidade);

                if (texto.Length > 0)
                    texto.Append(" | ");
                texto.Append(prato.Nome + " " + quantidade);
            }
            return texto.ToString();
        }

        private string JuntarEstoque()
        {
            var texto = new StringBuilder();
            foreach (var ingrediente in Cardapio.TodosOsIngredientes)
            {
                int quantidade;
                EstoqueFinal.TryGetValue(ingrediente, out quantidade);

                if (texto.Length > 0)
                    texto.Append(" | ");
                texto.Append(ingrediente + " " + quantidade);
            }
            return texto.ToString();
        }
    }
}
