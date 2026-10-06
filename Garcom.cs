using System.Collections.Concurrent;

namespace Restaurante
{
    /// <summary>
    /// REQUISITO 7 - dorme ate o sino tocar, esvazia o balcao e registra cada
    /// prato no caixa (REQUISITO 8).
    ///
    /// O restaurante roda com mais de um garcom: com um so, nenhuma soma de
    /// faturamento aconteceria em paralelo e a race condition do modo inseguro
    /// nao teria como acontecer.
    /// </summary>
    public sealed class Garcom
    {
        private readonly int _id;
        private readonly Balcao _balcao;
        private readonly ICaixa _caixa;
        private readonly Log _log;

        /// <summary>
        /// Guarda os entregues para o relatorio recalcular o faturamento
        /// esperado no fim, sem concorrencia.
        /// </summary>
        private readonly ConcurrentQueue<Pedido> _entregues = new ConcurrentQueue<Pedido>();

        public Garcom(int id, Balcao balcao, ICaixa caixa, Log log)
        {
            _id = id;
            _balcao = balcao;
            _caixa = caixa;
            _log = log;
        }

        public string Nome
        {
            get { return "Garcom " + _id; }
        }

        public int Entregues { get; private set; }

        public ConcurrentQueue<Pedido> PedidosEntregues
        {
            get { return _entregues; }
        }

        public void Trabalhar()
        {
            while (true)
            {
                // O tempo limite nao e enfeite: EncerrarCozinha toca o sino uma
                // vez so e o AutoResetEvent libera um garcom por vez. Sem ele o
                // outro ficaria dormindo para sempre.
                _balcao.EsperarSino(200);

                // Esvazia o balcao INTEIRO, em vez de levar um prato e voltar a
                // dormir: varios sinos seguidos viram um aviso so.
                Pedido pedido;
                while (_balcao.TentarRetirar(out pedido))
                {
                    _caixa.Registrar(pedido.Prato);
                    _entregues.Enqueue(pedido);
                    Entregues++;
                    _log.Evento(Nome, "entregou " + pedido +
                                      " (" + pedido.Prato.Preco.ToString("C2") + ")");
                }

                if (_balcao.CozinhaEncerrada && _balcao.Restantes == 0)
                    break;
            }

            _log.Evento(Nome, "encerrou com " + Entregues + " prato(s) entregue(s)");
        }
    }
}
