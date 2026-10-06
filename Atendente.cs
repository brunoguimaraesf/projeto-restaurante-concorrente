using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Restaurante
{
    /// <summary>
    /// REQUISITO 1 - gera pedidos aleatorios em intervalos aleatorios.
    /// REQUISITO 2 - escreve em uma fila de capacidade limitada: quando ela
    /// enche, o Add bloqueia e o atendente espera.
    /// </summary>
    public sealed class Atendente
    {
        private readonly int _id;
        private readonly BlockingCollection<Pedido> _fila;
        private readonly Log _log;
        private readonly Random _sorteio;
        private readonly int _intervaloMin;
        private readonly int _intervaloMax;

        public Atendente(int id, BlockingCollection<Pedido> fila, Log log,
                         int intervaloMin, int intervaloMax)
        {
            _id = id;
            _fila = fila;
            _log = log;
            _intervaloMin = intervaloMin;
            _intervaloMax = intervaloMax;
            // Random nao e thread-safe: cada atendente tem o seu.
            _sorteio = new Random(1000 + id);
        }

        public string Nome
        {
            get { return "Atendente " + _id; }
        }

        /// <summary>Anota os pedidos que lhe cabem e devolve quantos anotou.</summary>
        public int Trabalhar(int quantosPedidos, int primeiroNumero)
        {
            int anotados = 0;

            for (int i = 0; i < quantosPedidos; i++)
            {
                var prato = Cardapio.Itens[_sorteio.Next(Cardapio.Itens.Length)];
                var pedido = new Pedido(primeiroNumero + i, prato, _id);

                // Bloqueia aqui quando a fila esta cheia.
                _fila.Add(pedido);

                anotados++;
                _log.Evento(Nome, "anotou " + pedido + " (fila: " + _fila.Count + ")");

                Thread.Sleep(_sorteio.Next(_intervaloMin, _intervaloMax + 1));
            }

            _log.Evento(Nome, "encerrou com " + anotados + " pedido(s) anotado(s)");
            return anotados;
        }
    }
}
