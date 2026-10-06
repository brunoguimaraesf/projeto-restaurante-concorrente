using System.Threading;

namespace Restaurante
{
    /// <summary>Os recursos compartilhados da cozinha: os fornos e os utensilios.</summary>
    public sealed class Cozinha
    {
        private readonly SemaphoreSlim _fornos;   // REQUISITO 4
        private readonly int _totalDeFornos;
        private int _fornosEmUso;

        // REQUISITO 5 - um lock para cada utensilio.
        private readonly object _tabua = new object();
        private readonly object _faca = new object();

        /// <summary>Quando ligado, o Hamburguer pega os utensilios na ordem trocada (bug 2).</summary>
        private readonly bool _ordemInvertida;

        private readonly Log _log;

        public Cozinha(int totalDeFornos, bool ordemInvertida, Log log)
        {
            _totalDeFornos = totalDeFornos;
            _fornos = new SemaphoreSlim(totalDeFornos, totalDeFornos);
            _ordemInvertida = ordemInvertida;
            _log = log;
        }

        /// <summary>
        /// REQUISITO 4 - o semaforo conta as vagas: o terceiro cozinheiro fica
        /// parado no Wait ate um dos dois primeiros liberar o forno.
        /// </summary>
        public void Assar(Pedido pedido, string quem)
        {
            _fornos.Wait();
            try
            {
                int emUso = Interlocked.Increment(ref _fornosEmUso);
                _log.Evento(quem, pedido + " entrou no forno (" + emUso + "/" + _totalDeFornos + ")");

                Thread.Sleep(pedido.Prato.PreparoMs);

                _log.Evento(quem, pedido + " saiu do forno");
            }
            finally
            {
                Interlocked.Decrement(ref _fornosEmUso);
                _fornos.Release();
            }
        }

        /// <summary>
        /// REQUISITO 5 - no modo seguro a ordem e sempre tabua e depois faca.
        /// Com ordem fixa o ciclo de espera nao se forma e o deadlock deixa de
        /// ser possivel.
        ///
        /// No modo inseguro o Hamburguer inverte: pega a faca e depois a tabua.
        /// Com uma Salada e um Hamburguer ao mesmo tempo, cada um segura o que
        /// o outro precisa.
        /// </summary>
        public void UsarTabuaEFaca(Pedido pedido, string quem)
        {
            bool inverter = _ordemInvertida && pedido.Prato == Cardapio.Hamburguer;

            if (inverter)
            {
                _log.Evento(quem, pedido + " pegou a FACA e quer a tabua");
                lock (_faca)
                {
                    Thread.Sleep(100); // deixa a outra thread pegar a tabua
                    lock (_tabua)
                    {
                        Preparar(pedido, quem);
                    }
                }
            }
            else
            {
                _log.Evento(quem, pedido + " pegou a TABUA e quer a faca");
                lock (_tabua)
                {
                    Thread.Sleep(100); // deixa a outra thread pegar a faca
                    lock (_faca)
                    {
                        Preparar(pedido, quem);
                    }
                }
            }
        }

        private void Preparar(Pedido pedido, string quem)
        {
            _log.Evento(quem, pedido + " na bancada (tabua + faca)");
            Thread.Sleep(pedido.Prato.PreparoMs);
            _log.Evento(quem, pedido + " saiu da bancada");
        }
    }
}
