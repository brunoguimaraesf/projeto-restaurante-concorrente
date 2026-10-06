using System;
using System.Threading;
using System.Threading.Tasks;

namespace Restaurante
{
    /// <summary>
    /// Usado so na demonstracao de deadlock. Nao conserta nada: o programa
    /// continua travado, como tem que ficar. Ele so percebe que ninguem avancou
    /// e explica na tela o que aconteceu.
    /// </summary>
    public sealed class VigiaDeDeadlock
    {
        private const int SegundosSemProgresso = 5;

        private readonly Cozinheiro[] _cozinheiros;
        private readonly Garcom[] _garcons;
        private readonly CancellationTokenSource _parar = new CancellationTokenSource();

        public VigiaDeDeadlock(Cozinheiro[] cozinheiros, Garcom[] garcons)
        {
            _cozinheiros = cozinheiros;
            _garcons = garcons;
        }

        public void Iniciar()
        {
            Task.Factory.StartNew(Vigiar, TaskCreationOptions.LongRunning);
        }

        public void Parar()
        {
            _parar.Cancel();
        }

        private void Vigiar()
        {
            int ultimoProgresso = Progresso();
            int segundosParado = 0;
            bool jaAvisou = false;

            while (!_parar.IsCancellationRequested)
            {
                Thread.Sleep(1000);

                int agora = Progresso();
                if (agora != ultimoProgresso)
                {
                    ultimoProgresso = agora;
                    segundosParado = 0;
                    continue;
                }

                segundosParado++;

                if (segundosParado >= SegundosSemProgresso && !jaAvisou)
                {
                    jaAvisou = true;
                    Avisar();
                }
            }
        }

        private int Progresso()
        {
            int total = 0;
            foreach (var garcom in _garcons)
                total += garcom.Entregues;
            foreach (var cozinheiro in _cozinheiros)
                total += cozinheiro.Preparados + cozinheiro.Recusados;
            return total;
        }

        private void Avisar()
        {
            var original = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine();
            Console.WriteLine("================================================================");
            Console.WriteLine(" DEADLOCK: ninguem avancou nos ultimos " + SegundosSemProgresso + " segundos.");
            Console.WriteLine("================================================================");
            Console.WriteLine(" Um cozinheiro pegou a TABUA e esta esperando a FACA.");
            Console.WriteLine(" Outro pegou a FACA e esta esperando a TABUA.");
            Console.WriteLine(" Cada um segura o que o outro precisa, e nenhum solta.");
            Console.WriteLine();
            Console.WriteLine(" O relatorio final NUNCA vai aparecer. Encerre com Ctrl+C.");
            Console.WriteLine();
            Console.WriteLine(" Conserto (modo seguro): ordem fixa, tabua sempre antes da faca.");
            Console.WriteLine("================================================================");
            Console.WriteLine();

            Console.ForegroundColor = original;
        }
    }
}
