package restaurante;

/**
 * Ponto de entrada. O menu completo (modo seguro, os tres bugs e a comparacao
 * 1 x N cozinheiros) e requisito da Entrega 3; aqui a quantidade de cozinheiros
 * e de pedidos ja e configuravel por parametro.
 *
 * Uso:
 *   java -cp out restaurante.Program
 *   java -cp out restaurante.Program 4 60
 *       (primeiro parametro = cozinheiros, segundo = total de pedidos)
 */
public final class Program {

    private static final int COZINHEIROS_PADRAO = 4;
    private static final int PEDIDOS_PADRAO = 60;

    public static void main(String[] args) {
        int cozinheiros = lerInteiro(args, 0, COZINHEIROS_PADRAO);
        int pedidos = lerInteiro(args, 1, PEDIDOS_PADRAO);

        if (cozinheiros < 1 || pedidos < 1) {
            System.out.println("Parametros invalidos. Use: java -cp out restaurante.Program <cozinheiros> <pedidos>");
            return;
        }

        try {
            new Restaurante(cozinheiros, pedidos).abrir();
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            System.out.println("Simulacao interrompida.");
        }
    }

    private static int lerInteiro(String[] args, int posicao, int padrao) {
        if (args.length <= posicao) {
            return padrao;
        }
        try {
            return Integer.parseInt(args[posicao].trim());
        } catch (NumberFormatException e) {
            return padrao;
        }
    }
}
