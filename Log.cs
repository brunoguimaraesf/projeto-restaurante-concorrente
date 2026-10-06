using System;
using System.Diagnostics;

namespace Restaurante
{
    /// <summary>
    /// REQUISITO 10 - log de cada evento com horario e quem fez.
    /// Formato: [00:03.214] Cozinheiro 2 . Pizza #14 entrou no forno (2/2)
    /// </summary>
    public sealed class Log
    {
        private readonly Stopwatch _cronometro = Stopwatch.StartNew();
        private readonly bool _ligado;

        public Log(bool ligado)
        {
            _ligado = ligado;
        }

        public TimeSpan Agora
        {
            get { return _cronometro.Elapsed; }
        }

        public void Evento(string quem, string oQue)
        {
            if (!_ligado)
                return;

            // Console.WriteLine ja e sincronizado internamente, entao varias
            // threads escrevendo nao embaralham as linhas.
            Console.WriteLine(Carimbo() + " " + quem.PadRight(13) + " · " + oQue);
        }

        /// <summary>Mensagem do sistema, sem "quem".</summary>
        public void Sistema(string oQue)
        {
            if (!_ligado)
                return;

            Console.WriteLine(Carimbo() + " " + oQue);
        }

        private string Carimbo()
        {
            var t = _cronometro.Elapsed;
            return "[" + t.Minutes.ToString("00") + ":" +
                   t.Seconds.ToString("00") + "." +
                   t.Milliseconds.ToString("000") + "]";
        }
    }
}
