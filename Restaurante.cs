using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Restaurante
{
    public sealed class Configuracao
    {
        public int Atendentes = 2;
        public int Cozinheiros = 4;
        public int Fornos = 2;
        public int TotalDePedidos = 60;
        public int CapacidadeDaFila = 10;
        public int EstoqueInicial = 40;

        /// <summary>Intervalo aleatorio entre dois pedidos do mesmo atendente.</summary>
        public int IntervaloMinMs = 80;
        public int IntervaloMaxMs = 260;
    }

    /// <summary>Monta a linha de producao e roda o expediente.</summary>
    public sealed class Restaurante
    {
        private readonly Configuracao _config;

        public Restaurante(Configuracao config)
        {
            _config = config;
        }

        public void Executar()
        {
            var cronometro = Stopwatch.StartNew();
            var log = new Log(true);

            // REQUISITO 2 - fila com capacidade limitada.
            var fila = new BlockingCollection<Pedido>(_config.CapacidadeDaFila);
            var estoque = new Estoque(_config.EstoqueInicial);
            var cozinha = new Cozinha(_config.Fornos, log);
            var balcao = new Balcao();
            var garcom = new Garcom(balcao, log);

            log.Sistema(">>> Abrindo: " + _config.Atendentes + " atendente(s), " +
                        _config.Cozinheiros + " cozinheiro(s), " + _config.Fornos +
                        " forno(s), fila de " + _config.CapacidadeDaFila +
                        ", estoque de " + _config.EstoqueInicial + " por ingrediente.");

            var tarefaDoGarcom = Rodar(garcom.Trabalhar);

            var cozinheiros = new Cozinheiro[_config.Cozinheiros];
            var tarefasDosCozinheiros = new Task[_config.Cozinheiros];
            for (int i = 0; i < _config.Cozinheiros; i++)
            {
                var cozinheiro = new Cozinheiro(i + 1, fila, estoque, cozinha, balcao, log);
                cozinheiros[i] = cozinheiro;
                tarefasDosCozinheiros[i] = Rodar(cozinheiro.Trabalhar);
            }

            var tarefasDosAtendentes = new Task<int>[_config.Atendentes];
            int porAtendente = _config.TotalDePedidos / _config.Atendentes;
            for (int i = 0; i < _config.Atendentes; i++)
            {
                var atendente = new Atendente(i + 1, fila, log,
                                              _config.IntervaloMinMs, _config.IntervaloMaxMs);
                int quantos = i == _config.Atendentes - 1
                    ? _config.TotalDePedidos - porAtendente * (_config.Atendentes - 1)
                    : porAtendente;
                int primeiro = porAtendente * i + 1;

                tarefasDosAtendentes[i] = Rodar(() => atendente.Trabalhar(quantos, primeiro));
            }

            Task.WaitAll(tarefasDosAtendentes);

            int recebidos = 0;
            foreach (var tarefa in tarefasDosAtendentes)
                recebidos += tarefa.Result;

            // So depois que os DOIS atendentes terminam. Fechar antes faria o
            // outro dar erro; nunca fechar deixaria os cozinheiros esperando
            // para sempre.
            fila.CompleteAdding();

            Task.WaitAll(tarefasDosCozinheiros);

            // So agora o garcom pode esvaziar o balcao e sair.
            balcao.EncerrarCozinha();
            tarefaDoGarcom.Wait();

            cronometro.Stop();

            int recusados = 0;
            foreach (var cozinheiro in cozinheiros)
                recusados += cozinheiro.Recusados;

            log.Sistema(">>> Fechado.");
            Imprimir(recebidos, garcom.Entregues, recusados, estoque, cronometro.Elapsed);
        }

        private static void Imprimir(int recebidos, int entregues, int recusados,
                                     Estoque estoque, TimeSpan duracao)
        {
            Console.WriteLine();
            Console.WriteLine("Pedidos recebidos ... " + recebidos);
            Console.WriteLine("Entregues ........... " + entregues);
            Console.WriteLine("Recusados ........... " + recusados);

            var texto = "";
            foreach (var ingrediente in Cardapio.TodosOsIngredientes)
            {
                int quantidade;
                estoque.Situacao().TryGetValue(ingrediente, out quantidade);
                if (texto.Length > 0)
                    texto += " | ";
                texto += ingrediente + " " + quantidade;
            }

            Console.WriteLine("Estoque final: " + texto);
            Console.WriteLine("Tempo total ......... " + duracao.TotalSeconds.ToString("N1") + " s");
            Console.WriteLine();
        }

        /// <summary>
        /// LongRunning da uma thread dedicada a cada integrante da equipe: eles
        /// bloqueiam de verdade (Sleep, lock, semaforo), entao nao faria sentido
        /// ocupar threads do Thread Pool.
        /// </summary>
        private static Task Rodar(Action acao)
        {
            return Task.Factory.StartNew(acao, TaskCreationOptions.LongRunning);
        }

        private static Task<T> Rodar<T>(Func<T> acao)
        {
            return Task.Factory.StartNew(acao, TaskCreationOptions.LongRunning);
        }
    }
}
