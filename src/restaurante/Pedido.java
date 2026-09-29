package restaurante;

/**
 * Um pedido feito por um atendente e preparado por um cozinheiro.
 *
 * Tambem e imutavel: depois de criado, o pedido atravessa a fila e chega no
 * cozinheiro sem que ninguem precise de lock para le-lo.
 */
public final class Pedido {

    private final int numero;
    private final Prato prato;
    private final String origem;

    public Pedido(int numero, Prato prato, String origem) {
        this.numero = numero;
        this.prato = prato;
        this.origem = origem;
    }

    public int getNumero() {
        return numero;
    }

    public Prato getPrato() {
        return prato;
    }

    /** Nome do atendente que gerou o pedido. */
    public String getOrigem() {
        return origem;
    }

    /** Ex.: "Pizza #14" */
    public String descricaoCurta() {
        return prato.getNome() + " #" + numero;
    }

    @Override
    public String toString() {
        return descricaoCurta() + " (" + origem + ")";
    }
}
