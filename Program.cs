using System;
using System.Globalization;
using System.Text;

namespace Restaurante
{
    public static class Program
    {
        public static void Main()
        {
            PrepararConsole();
            new Restaurante(new Configuracao()).Executar();
        }

        private static void PrepararConsole()
        {
            try
            {
                Console.OutputEncoding = Encoding.UTF8;
            }
            catch (Exception)
            {
                // alguns terminais nao aceitam trocar o encoding
            }

            try
            {
                var brasil = new CultureInfo("pt-BR");
                CultureInfo.CurrentCulture = brasil;
                CultureInfo.DefaultThreadCurrentCulture = brasil;
            }
            catch (Exception)
            {
                // sem pt-BR a saida sai no formato padrao da maquina
            }
        }
    }
}
