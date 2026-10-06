namespace Restaurante
{
    /// <summary>
    /// REQUISITO 7 - dorme ate o sino tocar e esvazia o balcao, registrando
    /// cada entrega no log.
    /// </summary>
    public sealed class Garcom
    {
        private readonly Balcao _balcao;
        private readonly Log _log;

        public Garcom(Balcao balcao, Log log)
        {
            _balcao = balcao;
            _log = log;
        }

        public string Nome
        {
            get { return "Garcom"; }
        }

        public int Entregues { get; private set; }

        public void Trabalhar()
        {
            while (true)
            {
                // O tempo limite nao e enfeite: EncerrarCozinha toca o sino uma
                // vez so. Sem ele o garcom poderia dormir para sempre.
                _balcao.EsperarSino(200);

                // Esvazia o balcao INTEIRO, em vez de levar um prato e voltar a
                // dormir: varios sinos seguidos viram um aviso so.
                Pedido pedido;
                while (_balcao.TentarRetirar(out pedido))
                {
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
