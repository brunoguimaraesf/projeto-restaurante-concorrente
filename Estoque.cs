using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Restaurante
{
    /// <summary>
    /// REQUISITO 6 - estoque de ingredientes.
    ///
    /// O ConcurrentDictionary sozinho nao resolve: ele garante que cada chamada
    /// e segura, nao que a sequencia "verificar e descontar" seja atomica.
    /// Por isso a reserva inteira acontece dentro de um unico lock.
    /// </summary>
    public sealed class Estoque
    {
        private readonly object _trava = new object();
        private readonly ConcurrentDictionary<string, int> _itens =
            new ConcurrentDictionary<string, int>();

        public Estoque(int quantidadeInicial)
        {
            foreach (var ingrediente in Cardapio.TodosOsIngredientes)
                _itens[ingrediente] = quantidadeInicial;
        }

        /// <summary>
        /// Reserva 1 unidade de cada ingrediente do prato. Se faltar algum,
        /// devolve false sem descontar nada.
        /// </summary>
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
}
