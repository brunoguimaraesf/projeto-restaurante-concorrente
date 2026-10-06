namespace Restaurante
{
    /// <summary>Um pedido anotado por um atendente.</summary>
    public sealed class Pedido
    {
        public Pedido(int numero, Prato prato, int atendenteId)
        {
            Numero = numero;
            Prato = prato;
            AtendenteId = atendenteId;
        }

        public int Numero { get; }
        public Prato Prato { get; }
        public int AtendenteId { get; }

        public override string ToString()
        {
            return Prato.Nome + " #" + Numero;
        }
    }
}
