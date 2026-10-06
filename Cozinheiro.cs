using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Restaurante
{
    /// <summary>
    /// REQUISITO 3 - N cozinheiros consomem a mesma fila ao mesmo tempo.
    /// E a peca que encosta em todos os recursos compartilhados: fila, estoque,
    /// fornos, utensilios e balcao.
    /// </summary>
    public sealed class Cozinheiro
    {
        private readonly int _id;
        private readonly BlockingCollection<Pedido> _fila;
        private readonly IEstoque _estoque;
        private readonly Cozinha _cozinha;
        private readonly Balcao _balcao;
        private readonly Log _log;

        public Cozinheiro(int id, BlockingCollection<Pedido> fila, IEstoque estoque,
                          Cozinha cozinha, Balcao balcao, Log log)
        {
            _id = id;
            _fila = fila;
            _estoque = estoque;
            _cozinha = cozinha;
            _balcao = balcao;
            _log = log;
        }

        public string Nome
        {
            get { return "Cozinheiro " + _id; }
        }

        public int Recusados { get; private set; }
        public int Preparados { get; private set; }

        public void Trabalhar(CancellationToken token)
        {
            // REQUISITO 9: o token so aparece aqui, na hora de PEGAR pedido.
            // Depois de pegar, o cozinheiro vai ate o fim: termina o prato
            // atual mesmo com o restaurante ja fechado.
            while (!token.IsCancellationRequested)
            {
                Pedido pedido;

                try
                {
                    if (!_fila.TryTake(out pedido, 50, token))
                    {
                        // Sem fechamento o fim vem por aqui: a fila foi marcada
                        // como completa e nao sobrou nada nela.
                        if (_fila.IsCompleted)
                            break;

                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                Atender(pedido);
            }

            _log.Evento(Nome, "encerrou (" + Preparados + " prato(s), " +
                              Recusados + " recusado(s))");
        }

        private void Atender(Pedido pedido)
        {
            _log.Evento(Nome, "pegou " + pedido + " da fila");

            // REQUISITO 6 - sem ingrediente o pedido e recusado e nao chega a
            // ocupar forno nem bancada.
            if (!_estoque.TentarReservar(pedido.Prato))
            {
                Recusados++;
                _log.Evento(Nome, pedido + " RECUSADO por falta de ingrediente");
                return;
            }

            if (pedido.Prato.UsaForno)
                _cozinha.Assar(pedido, Nome);          // REQUISITO 4
            else if (pedido.Prato.UsaTabuaEFaca)
                _cozinha.UsarTabuaEFaca(pedido, Nome); // REQUISITO 5
            else
                Thread.Sleep(pedido.Prato.PreparoMs);

            // REQUISITO 7 - vai para o balcao e toca o sino.
            _balcao.Depositar(pedido);
            Preparados++;
            _log.Evento(Nome, pedido + " pronto, foi para o balcao (sino)");
        }
    }
}
