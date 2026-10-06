using System.Collections.Concurrent;
using System.Threading;

namespace Restaurante
{
    /// <summary>
    /// REQUISITO 3 - N cozinheiros consomem a mesma fila ao mesmo tempo.
    /// Nesta etapa o preparo e so o Thread.Sleep do prato: ainda nao existem
    /// forno, utensilios nem estoque.
    /// </summary>
    public sealed class Cozinheiro
    {
        private readonly int _id;
        private readonly BlockingCollection<Pedido> _fila;
        private readonly Log _log;

        public Cozinheiro(int id, BlockingCollection<Pedido> fila, Log log)
        {
            _id = id;
            _fila = fila;
            _log = log;
        }

        public string Nome
        {
            get { return "Cozinheiro " + _id; }
        }

        public int Preparados { get; private set; }

        public void Trabalhar()
        {
            // GetConsumingEnumerable fica pegando pedidos ate a fila ser
            // fechada com CompleteAdding e esvaziar. E o que faz o cozinheiro
            // terminar sozinho em vez de esperar para sempre.
            foreach (var pedido in _fila.GetConsumingEnumerable())
            {
                _log.Evento(Nome, "pegou " + pedido + " da fila");

                Thread.Sleep(pedido.Prato.PreparoMs);

                Preparados++;
                _log.Evento(Nome, pedido + " pronto");
            }

            _log.Evento(Nome, "encerrou com " + Preparados + " prato(s)");
        }
    }
}
