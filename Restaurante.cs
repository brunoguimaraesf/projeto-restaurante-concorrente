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
        public int TotalDePedidos = 60;
        public int CapacidadeDaFila = 10;

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

            log.Sistema(">>> Abrindo: " + _config.Atendentes + " atendente(s), " +
                        _config.Cozinheiros + " cozinheiro(s), fila de " +
                        _config.CapacidadeDaFila + ".");

            var cozinheiros = new Cozinheiro[_config.Cozinheiros];
            var tarefasDosCozinheiros = new Task[_config.Cozinheiros];
            for (int i = 0; i < _config.Cozinheiros; i++)
            {
                var cozinheiro = new Cozinheiro(i + 1, fila, log);
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
            cronometro.Stop();

            int preparados = 0;
            foreach (var cozinheiro in cozinheiros)
                preparados += cozinheiro.Preparados;

            log.Sistema(">>> Fechado.");
            Console.WriteLine();
            Console.WriteLine("Pedidos recebidos ... " + recebidos);
            Console.WriteLine("Pratos preparados ... " + preparados);
            Console.WriteLine("Tempo total ......... " +
                              cronometro.Elapsed.TotalSeconds.ToString("N1") + " s");
            Console.WriteLine();
        }

        /// <summary>
        /// LongRunning da uma thread dedicada a cada integrante da equipe: eles
        /// bloqueiam de verdade (Sleep, fila cheia), entao nao faria sentido
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
