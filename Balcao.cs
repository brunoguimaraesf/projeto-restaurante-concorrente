using System.Collections.Concurrent;
using System.Threading;

namespace Restaurante
{
    /// <summary>
    /// REQUISITO 7 - o balcao de pratos prontos e o sino que acorda o garcom.
    ///
    /// O AutoResetEvent nao conta avisos: se tres cozinheiros tocarem o sino
    /// enquanto o garcom esta ocupado, ele acorda uma vez so. Por isso o garcom
    /// precisa esvaziar o balcao inteiro ao acordar.
    /// </summary>
    public sealed class Balcao
    {
        private readonly ConcurrentQueue<Pedido> _pratosProntos = new ConcurrentQueue<Pedido>();
        private readonly AutoResetEvent _sino = new AutoResetEvent(false);
        private volatile bool _cozinhaEncerrada;

        public void Depositar(Pedido pedido)
        {
            _pratosProntos.Enqueue(pedido);
            _sino.Set();
        }

        public bool TentarRetirar(out Pedido pedido)
        {
            return _pratosProntos.TryDequeue(out pedido);
        }

        public void EsperarSino(int tempoLimiteMs)
        {
            _sino.WaitOne(tempoLimiteMs);
        }

        /// <summary>
        /// Avisa que nao vem mais prato e toca o sino uma ultima vez. Sem isto
        /// o garcom dormiria para sempre e o programa nunca terminaria.
        /// </summary>
        public void EncerrarCozinha()
        {
            _cozinhaEncerrada = true;
            _sino.Set();
        }

        public bool CozinhaEncerrada
        {
            get { return _cozinhaEncerrada; }
        }

        public int Restantes
        {
            get { return _pratosProntos.Count; }
        }
    }
}
