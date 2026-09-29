package restaurante;

/**
 * Ponto de entrada.
 * Uso: java -cp out restaurante.Program [cozinheiros] [pedidos]
 * O menu completo e requisito da Entrega 3.
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
