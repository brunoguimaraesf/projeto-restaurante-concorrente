using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante
{
    public enum ModoDeExecucao
    {
        Seguro,
        BugCaixa,     // race condition no caixa
        BugDeadlock,  // utensilios pegos em ordens diferentes
        BugEstoque    // verificar e descontar em passos separados
    }

    public sealed class Configuracao
    {
        public ModoDeExecucao Modo = ModoDeExecucao.Seguro;
        public int Atendentes = 2;
        public int Cozinheiros = 4;

        /// <summary>Mais de um garcom: e o que faz o caixa ser disputado de verdade.</summary>
        public int Garcons = 2;

        public int Fornos = 2;
        public int TotalDePedidos = 60;
        public int CapacidadeDaFila = 10;
        public int EstoqueInicial = 40;

        /// <summary>Intervalo aleatorio entre dois pedidos do mesmo atendente.</summary>
        public int IntervaloMinMs = 80;
        public int IntervaloMaxMs = 260;

        /// <summary>null = sem fechamento (usado na comparacao 1 x N).</summary>
        public TimeSpan? Fechamento = TimeSpan.FromSeconds(15);

        public bool Narrar = true;
    }

    /// <summary>Monta o restaurante, roda o expediente e gera o relatorio.</summary>
    public sealed class Restaurante
    {
        private readonly Configuracao _config;
        private readonly Prato[] _pratoSorteado;

        public Restaurante(Configuracao config)
        {
            _config = config;

            // REQUISITO 1 - o sorteio e feito uma vez so, com semente fixa,
            // antes de qualquer thread comecar. Assim nenhum Random precisa ser
            // compartilhado e as duas rodadas da comparacao 1 x N recebem
            // exatamente a mesma carga.
            var sorteio = new Random(42);
            _pratoSorteado = new Prato[config.TotalDePedidos + 2];
            for (int i = 0; i < _pratoSorteado.Length; i++)
                _pratoSorteado[i] = Cardapio.Itens[sorteio.Next(Cardapio.Itens.Length)];
        }

        public Relatorio Executar()
        {
            var cronometro = Stopwatch.StartNew();
            var log = new Log(_config.Narrar);

            // REQUISITO 2 - fila com capacidade limitada.
            var fila = new BlockingCollection<Pedido>(_config.CapacidadeDaFila);

            IEstoque estoque = _config.Modo == ModoDeExecucao.BugEstoque
                ? (IEstoque)new EstoqueSemAtomicidade(_config.EstoqueInicial)
                : new EstoqueSeguro(_config.EstoqueInicial);

            ICaixa caixa = _config.Modo == ModoDeExecucao.BugCaixa
                ? (ICaixa)new CaixaComRace()
                : new CaixaSeguro();

            var cozinha = new Cozinha(_config.Fornos,
                                      _config.Modo == ModoDeExecucao.BugDeadlock,
                                      log);
            var balcao = new Balcao();

            // REQUISITO 9 - o gerente e um CancellationTokenSource com prazo.
            using (var gerente = _config.Fechamento.HasValue
                       ? new CancellationTokenSource(_config.Fechamento.Value)
                       : null)
            {
                var token = gerente == null ? CancellationToken.None : gerente.Token;

                if (gerente != null)
                {
                    token.Register(() => log.Sistema(
                        ">>> GERENTE: restaurante fechado. Atendentes param; cada cozinheiro termina o prato atual."));
                }

                log.Sistema(">>> Abrindo: " + _config.Atendentes + " atendente(s), " +
                            _config.Cozinheiros + " cozinheiro(s), " + _config.Fornos +
                            " forno(s), " + _config.Garcons + " garcom(ns), fila de " +
                            _config.CapacidadeDaFila + ", estoque de " +
                            _config.EstoqueInicial + " por ingrediente.");

                var garcons = new Garcom[_config.Garcons];
                var tarefasDosGarcons = new Task[_config.Garcons];
                for (int i = 0; i < _config.Garcons; i++)
                {
                    var garcom = new Garcom(i + 1, balcao, caixa, log);
                    garcons[i] = garcom;
                    tarefasDosGarcons[i] = Rodar(garcom.Trabalhar);
                }

                var cozinheiros = new Cozinheiro[_config.Cozinheiros];
                var tarefasDosCozinheiros = new Task[_config.Cozinheiros];
                for (int i = 0; i < _config.Cozinheiros; i++)
                {
                    var cozinheiro = new Cozinheiro(i + 1, fila, estoque, cozinha, balcao, log);
                    cozinheiros[i] = cozinheiro;
                    tarefasDosCozinheiros[i] = Rodar(() => cozinheiro.Trabalhar(token));
                }

                VigiaDeDeadlock vigia = null;
                if (_config.Modo == ModoDeExecucao.BugDeadlock)
                {
                    vigia = new VigiaDeDeadlock(cozinheiros, garcons);
                    vigia.Iniciar();
                }

                var tarefasDosAtendentes = new Task<int>[_config.Atendentes];
                int porAtendente = _config.TotalDePedidos / _config.Atendentes;
                for (int i = 0; i < _config.Atendentes; i++)
                {
                    var atendente = new Atendente(i + 1, fila, EscolherPrato, log,
                                                  _config.IntervaloMinMs, _config.IntervaloMaxMs);
                    int quantos = i == _config.Atendentes - 1
                        ? _config.TotalDePedidos - porAtendente * (_config.Atendentes - 1)
                        : porAtendente;
                    int primeiro = porAtendente * i + 1;

                    tarefasDosAtendentes[i] = Rodar(() => atendente.Trabalhar(quantos, primeiro, token));
                }

                Task.WaitAll(tarefasDosAtendentes);

                int recebidos = 0;
                foreach (var tarefa in tarefasDosAtendentes)
                    recebidos += tarefa.Result;

                // So depois que os DOIS atendentes terminam. Fechar antes faria
                // o outro dar erro; nunca fechar deixaria os cozinheiros
                // esperando para sempre.
                fila.CompleteAdding();

                Task.WaitAll(tarefasDosCozinheiros);

                if (vigia != null)
                    vigia.Parar();

                // REQUISITO 9 - o que sobrou na fila vira "nao atendido".
                int naoAtendidos = fila.Count;

                balcao.EncerrarCozinha();
                Task.WaitAll(tarefasDosGarcons);

                cronometro.Stop();

                int recusados = 0;
                foreach (var cozinheiro in cozinheiros)
                    recusados += cozinheiro.Recusados;

                log.Sistema(">>> Fechado. " + naoAtendidos + " pedido(s) nao atendido(s).");

                return MontarRelatorio(recebidos, garcons, recusados, naoAtendidos,
                                       caixa, estoque, cronometro.Elapsed);
            }
        }

        private Relatorio MontarRelatorio(int recebidos, Garcom[] garcons, int recusados,
                                          int naoAtendidos, ICaixa caixa, IEstoque estoque,
                                          TimeSpan duracao)
        {
            // Faturamento esperado: somado no fim, sequencialmente, sem
            // concorrencia. E a fonte de verdade contra a qual o valor que o
            // caixa acumulou durante a execucao e conferido.
            decimal esperado = 0m;
            int entregues = 0;
            foreach (var garcom in garcons)
            {
                entregues += garcom.Entregues;
                foreach (var pedido in garcom.PedidosEntregues)
                    esperado += pedido.Prato.Preco;
            }

            return new Relatorio
            {
                Modo = NomeDoModo(),
                Cozinheiros = _config.Cozinheiros,
                Fornos = _config.Fornos,
                Fechamento = _config.Fechamento,
                Recebidos = recebidos,
                Entregues = entregues,
                Recusados = recusados,
                NaoAtendidos = naoAtendidos,
                Vendas = caixa.VendasPorPrato,
                FaturamentoEsperado = esperado,
                FaturamentoRegistrado = caixa.FaturamentoRegistrado,
                EstoqueFinal = estoque.Situacao(),
                TempoTotal = duracao
            };
        }

        private string NomeDoModo()
        {
            switch (_config.Modo)
            {
                case ModoDeExecucao.BugCaixa: return "INSEGURO (race no caixa)";
                case ModoDeExecucao.BugDeadlock: return "INSEGURO (deadlock nos utensilios)";
                case ModoDeExecucao.BugEstoque: return "INSEGURO (estoque negativo)";
                default: return "SEGURO";
            }
        }

        /// <summary>
        /// No modo de deadlock o sorteio vira alternancia Salada / Hamburguer,
        /// para os dois pratos que disputam tabua e faca estarem sempre na
        /// cozinha ao mesmo tempo e o travamento acontecer em toda apresentacao.
        /// </summary>
        private Prato EscolherPrato(int numeroDoPedido)
        {
            if (_config.Modo == ModoDeExecucao.BugDeadlock)
                return numeroDoPedido % 2 == 0 ? Cardapio.Salada : Cardapio.Hamburguer;

            return _pratoSorteado[numeroDoPedido % _pratoSorteado.Length];
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
