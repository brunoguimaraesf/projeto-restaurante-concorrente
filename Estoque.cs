using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Restaurante
{
    public interface IEstoque
    {
        /// <summary>
        /// Reserva 1 unidade de cada ingrediente do prato. Se faltar algum,
        /// devolve false sem descontar nada.
        /// </summary>
        bool TentarReservar(Prato prato);

        IReadOnlyDictionary<string, int> Situacao();
    }

    /// <summary>
    /// REQUISITO 6 - versao correta.
    ///
    /// O ConcurrentDictionary sozinho nao resolve: ele garante que cada chamada
    /// e segura, nao que a sequencia "verificar e descontar" seja atomica.
    /// Por isso a reserva inteira acontece dentro de um unico lock.
    /// </summary>
    public sealed class EstoqueSeguro : IEstoque
    {
        private readonly object _trava = new object();
        private readonly ConcurrentDictionary<string, int> _itens =
            new ConcurrentDictionary<string, int>();

        public EstoqueSeguro(int quantidadeInicial)
        {
            foreach (var ingrediente in Cardapio.TodosOsIngredientes)
                _itens[ingrediente] = quantidadeInicial;
        }

        public bool TentarReservar(Prato prato)
        {
            lock (_trava)
            {
                // Confere todos antes de mexer em qualquer um, para nao
                // acontecer reserva parcial.
                foreach (var ingrediente in prato.Ingredientes)
                {
                    if (_itens[ingrediente] <= 0)
                        return false;
                }

                foreach (var ingrediente in prato.Ingredientes)
                    _itens[ingrediente] = _itens[ingrediente] - 1;

                return true;
            }
        }

        public IReadOnlyDictionary<string, int> Situacao()
        {
            lock (_trava)
            {
                return new Dictionary<string, int>(_itens);
            }
        }
    }

    /// <summary>
    /// BUG 3 (secao 6) - estoque negativo.
    ///
    /// Verificar e descontar viram passos separados, sem protecao. Dois
    /// cozinheiros leem queijo = 1 e os dois passam na verificacao; o primeiro
    /// desconta e deixa 0, o segundo desconta sobre esse 0 e deixa -1.
    /// </summary>
    public sealed class EstoqueSemAtomicidade : IEstoque
    {
        private readonly ConcurrentDictionary<string, int> _itens =
            new ConcurrentDictionary<string, int>();

        public EstoqueSemAtomicidade(int quantidadeInicial)
        {
            foreach (var ingrediente in Cardapio.TodosOsIngredientes)
                _itens[ingrediente] = quantidadeInicial;
        }

        public bool TentarReservar(Prato prato)
        {
            foreach (var ingrediente in prato.Ingredientes)
            {
                int atual = _itens[ingrediente]; // verifica
                if (atual <= 0)
                    return false;

                // Janela aberta de propósito. Com 1ms o bug so aparecia de vez
                // em quando, porque cada cozinheiro passa centenas de
                // milissegundos preparando entre uma reserva e a proxima.
                Thread.Sleep(50);

                _itens[ingrediente] = _itens[ingrediente] - 1; // desconta
            }

            return true;
        }

        public IReadOnlyDictionary<string, int> Situacao()
        {
            return new Dictionary<string, int>(_itens);
        }
    }
}
