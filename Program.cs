using System;
using System.Globalization;
using System.Text;

namespace Restaurante
{
    /// <summary>REQUISITO 11 - menu no console.</summary>
    public static class Program
    {
        private static int _fechamentoEmSegundos = 15;
        private static int _cozinheiros = 4;

        public static int Main(string[] args)
        {
            PrepararConsole();

            string opcaoDireta = null;

            foreach (var arg in args)
            {
                if (arg.StartsWith("--fechamento=", StringComparison.Ordinal))
                    _fechamentoEmSegundos = Inteiro(arg, "--fechamento=", _fechamentoEmSegundos);
                else if (arg.StartsWith("--cozinheiros=", StringComparison.Ordinal))
                    _cozinheiros = Inteiro(arg, "--cozinheiros=", _cozinheiros);
                else
                    opcaoDireta = arg;
            }

            // Permite rodar uma opcao direto da linha de comando:
            //   dotnet run -- 1
            //   dotnet run -- 1 --fechamento=10 --cozinheiros=2
            if (opcaoDireta != null)
            {
                Executar(opcaoDireta);
                return 0;
            }

            while (true)
            {
                MostrarMenu();
                Console.Write(" Escolha uma opcao: ");

                string escolha = Console.ReadLine();
                if (escolha == null)
                    return 0; // fim da entrada (execucao nao interativa)

                escolha = escolha.Trim();
                if (escolha == "0")
                {
                    Console.WriteLine(" Encerrando.");
                    return 0;
                }

                Console.WriteLine();
                if (!Executar(escolha))
                    Console.WriteLine(" Opcao invalida: " + escolha);

                Console.WriteLine();
            }
        }

        private static void MostrarMenu()
        {
            Console.WriteLine(new string('=', 64));
            Console.WriteLine(" RESTAURANTE CONCORRENTE");
            Console.WriteLine(new string('=', 64));
            Console.WriteLine(" 1  Modo seguro (" + _cozinheiros + " cozinheiros, fechamento em " +
                              _fechamentoEmSegundos + "s)");
            Console.WriteLine();
            Console.WriteLine(" Modo inseguro - demonstracoes de bug");
            Console.WriteLine(" 2  Bug 1 - race condition no caixa");
            Console.WriteLine(" 3  Bug 2 - deadlock nos utensilios  (TRAVA DE PROPOSITO)");
            Console.WriteLine(" 4  Bug 3 - estoque negativo");
            Console.WriteLine();
            Console.WriteLine(" 5  Comparacao 1 x " + _cozinheiros +
                              " cozinheiros (60 pedidos, sem fechamento)");
            Console.WriteLine(" 0  Sair");
            Console.WriteLine(new string('=', 64));
        }

        private static bool Executar(string opcao)
        {
            switch (opcao)
            {
                case "1":
                    Rodar(ModoDeExecucao.Seguro);
                    return true;

                case "2":
                    Explicar("BUG 1 - RACE CONDITION NO CAIXA",
                        "O caixa soma o faturamento sem lock: le, dorme 20ms e grava.",
                        "Dois registros simultaneos leem o mesmo valor e um sobrescreve",
                        "o outro, entao vendas somem do caixa.",
                        "Esperado: [ERRO] faturamento registrado = faturamento esperado.");
                    Rodar(ModoDeExecucao.BugCaixa);
                    return true;

                case "3":
                    Explicar("BUG 2 - DEADLOCK NOS UTENSILIOS",
                        "A Salada pega a tabua e depois a faca. O Hamburguer pega a faca",
                        "e depois a tabua. Cada um segura o que o outro precisa.",
                        "O programa VAI TRAVAR e o relatorio nunca vai aparecer.",
                        "Encerre com Ctrl+C quando o aviso de deadlock aparecer.");
                    Rodar(ModoDeExecucao.BugDeadlock);
                    return true;

                case "4":
                    Explicar("BUG 3 - ESTOQUE NEGATIVO",
                        "O estoque verifica se ha ingrediente e desconta em passos",
                        "separados, sem protecao. Dois cozinheiros passam na mesma",
                        "verificacao e os dois descontam.",
                        "Esperado: [ERRO] nenhum ingrediente com estoque negativo.");
                    Rodar(ModoDeExecucao.BugEstoque);
                    return true;

                case "5":
                    Comparar();
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// As demonstracoes de bug precisam de gente suficiente para a disputa
        /// acontecer: deadlock exige pelo menos 2 cozinheiros, e o estoque
        /// negativo exige varios reservando ao mesmo tempo.
        /// </summary>
        private static int EscolherCozinheiros(ModoDeExecucao modo)
        {
            switch (modo)
            {
                case ModoDeExecucao.BugDeadlock: return Math.Max(2, _cozinheiros);
                case ModoDeExecucao.BugCaixa: return Math.Max(8, _cozinheiros);
                case ModoDeExecucao.BugEstoque: return Math.Max(8, _cozinheiros);
                default: return _cozinheiros;
            }
        }

        private static void Rodar(ModoDeExecucao modo)
        {
            // As duas demonstracoes de corrida (caixa e estoque) precisam que
            // os pedidos cheguem em rajada. No ritmo normal os cozinheiros
            // ficam famintos esperando pedido, trabalham um de cada vez e nunca
            // chegam juntos no recurso disputado: o gargalo vira o atendente e
            // a disputa que queremos mostrar simplesmente nao acontece.
            bool emRajada = modo == ModoDeExecucao.BugCaixa ||
                            modo == ModoDeExecucao.BugEstoque;

            var config = new Configuracao
            {
                Modo = modo,
                Cozinheiros = EscolherCozinheiros(modo),

                // Mais garcons no bug do caixa: e preciso ter dois registrando
                // ao mesmo tempo para a race acontecer. Com poucos, um esvazia
                // o balcao sozinho enquanto o outro dorme.
                Garcons = modo == ModoDeExecucao.BugCaixa ? 4 : 2,
                Fechamento = TimeSpan.FromSeconds(_fechamentoEmSegundos),
                Narrar = true,

                // Na demonstracao de estoque negativo o estoque comeca baixo de
                // propósito: o erro so pode nascer no instante em que um
                // ingrediente passa de 1 para 0, com dois cozinheiros dentro da
                // janela ao mesmo tempo. Com 40 unidades esse instante so
                // chegaria no fim do expediente e o bug ficava intermitente.
                EstoqueInicial = modo == ModoDeExecucao.BugEstoque ? 6 : 40,

                IntervaloMinMs = emRajada ? 0 : 80,
                IntervaloMaxMs = emRajada ? 5 : 260
            };

            var relatorio = new Restaurante(config).Executar();
            bool tudoOk = relatorio.Imprimir();

            if (modo != ModoDeExecucao.Seguro && tudoOk)
            {
                Console.WriteLine(" ATENCAO: nesta execucao o bug nao apareceu. Rode de novo;");
                Console.WriteLine(" erro de concorrencia nao e deterministico, e isso faz parte da licao.");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// REQUISITO 11 - comparacao 1 x N cozinheiros, com os mesmos 60
        /// pedidos e sem fechamento, para que as duas rodadas processem
        /// exatamente a mesma carga.
        /// </summary>
        private static void Comparar()
        {
            Console.WriteLine("Comparação · 60 pedidos, sem fechamento");
            Console.WriteLine();

            double tempoCom1 = Medir(1);
            double tempoComN = Medir(_cozinheiros);

            Console.WriteLine("1 cozinheiro ...... " + tempoCom1.ToString("N1") + " s");
            Console.WriteLine(_cozinheiros + " cozinheiros ..... " + tempoComN.ToString("N1") + " s");
            Console.WriteLine();
            Console.WriteLine("Ganho: " + (tempoCom1 / tempoComN).ToString("N2") + "x com " +
                              _cozinheiros + " cozinheiros.");
            Console.WriteLine();
            Console.WriteLine("O ganho nao chega a " + _cozinheiros + "x porque os cozinheiros disputam");
            Console.WriteLine("recursos: so cabem 2 pratos no forno ao mesmo tempo, e a tabua e a");
            Console.WriteLine("faca sao usadas por um cozinheiro de cada vez. Essas partes do");
            Console.WriteLine("trabalho continuam sendo feitas em serie por mais gente que se");
            Console.WriteLine("contrate, que e exatamente o que a Lei de Amdahl descreve.");
            Console.WriteLine();
        }

        private static double Medir(int cozinheiros)
        {
            Console.WriteLine("  rodando com " + cozinheiros + " cozinheiro(s)...");

            var config = new Configuracao
            {
                Modo = ModoDeExecucao.Seguro,
                Cozinheiros = cozinheiros,
                Fechamento = null, // sem fechamento
                Narrar = false
            };

            var relatorio = new Restaurante(config).Executar();
            Console.WriteLine("  entregues: " + relatorio.Entregues +
                              " | recusados: " + relatorio.Recusados +
                              " | tempo: " + relatorio.TempoTotal.TotalSeconds.ToString("N1") + " s");
            Console.WriteLine();

            return relatorio.TempoTotal.TotalSeconds;
        }

        private static void Explicar(string titulo, params string[] linhas)
        {
            Console.WriteLine(new string('=', 64));
            Console.WriteLine(" " + titulo);
            Console.WriteLine(new string('=', 64));
            foreach (var linha in linhas)
                Console.WriteLine(" " + linha);
            Console.WriteLine(new string('=', 64));
            Console.WriteLine();
        }

        private static int Inteiro(string arg, string prefixo, int padrao)
        {
            int valor;
            return int.TryParse(arg.Substring(prefixo.Length), out valor) && valor > 0
                ? valor
                : padrao;
        }

        private static void PrepararConsole()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch (Exception)
            {
                // alguns terminais nao aceitam trocar o encoding
            }

            try
            {
                var brasil = new CultureInfo("pt-BR");
                CultureInfo.CurrentCulture = brasil;
                CultureInfo.DefaultThreadCurrentCulture = brasil;
            }
            catch (Exception)
            {
                // sem pt-BR o relatorio sai no formato padrao da maquina
            }
        }
    }
}
