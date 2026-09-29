package restaurante;

/** Pedido anotado por um atendente e preparado por um cozinheiro. Imutavel. */
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

    /** Atendente que anotou o pedido. */
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
