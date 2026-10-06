using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Restaurante
{
    public interface ICaixa
    {
        void Registrar(Prato prato);
        decimal FaturamentoRegistrado { get; }
        IReadOnlyDictionary<string, int> VendasPorPrato { get; }
    }

    /// <summary>
    /// REQUISITO 8 - versao correta.
    ///
    /// "faturamento += preco" sao tres passos (ler, somar, gravar) e decimal
    /// tem 16 bytes, entao nem a gravacao sozinha e atomica. Interlocked nao
    /// atende decimal, por isso o lock.
    /// </summary>
    public sealed class CaixaSeguro : ICaixa
    {
        private readonly object _trava = new object();
        private decimal _faturamento;
        private readonly ConcurrentDictionary<string, int> _vendas =
            new ConcurrentDictionary<string, int>();

        public void Registrar(Prato prato)
        {
            lock (_trava)
            {
                _faturamento += prato.Preco;
            }

            // AddOrUpdate resolve "insere se nao existe, senao incrementa" em
            // uma unica operacao atomica.
            _vendas.AddOrUpdate(prato.Nome, 1, (nome, atual) => atual + 1);
        }

        public decimal FaturamentoRegistrado
        {
            get { lock (_trava) { return _faturamento; } }
        }

        public IReadOnlyDictionary<string, int> VendasPorPrato
        {
            get { return new Dictionary<string, int>(_vendas); }
        }
    }

    /// <summary>
    /// BUG 1 (secao 6) - race condition no caixa.
    ///
    /// A soma vira ler, dormir e gravar, sem lock. Dois garcons leem o mesmo
    /// valor, somam seu preco sobre essa mesma base e o segundo sobrescreve o
    /// primeiro: a venda do primeiro some do caixa.
    /// </summary>
    public sealed class CaixaComRace : ICaixa
    {
        private decimal _faturamento;
        private readonly ConcurrentDictionary<string, int> _vendas =
            new ConcurrentDictionary<string, int>();

        public void Registrar(Prato prato)
        {
            decimal lido = _faturamento;

            // Janela aberta de propósito. Com 1ms o bug so aparecia em metade
            // das execucoes: os pratos ficam prontos a cada 85ms mais ou menos,
            // entao dois registros quase nunca caiam na mesma janela.
            Thread.Sleep(20);

            _faturamento = lido + prato.Preco;

            _vendas.AddOrUpdate(prato.Nome, 1, (nome, atual) => atual + 1);
        }

        public decimal FaturamentoRegistrado
        {
            get { return _faturamento; }
        }

        public IReadOnlyDictionary<string, int> VendasPorPrato
        {
            get { return new Dictionary<string, int>(_vendas); }
        }
    }
}
