package restaurante;

import java.util.Collections;
import java.util.List;

/** Item do cardapio. Imutavel: varias threads leem sem lock. */
public final class Prato {

    private final String nome;
    private final double preco;
    private final int preparoMs;
    private final boolean usaForno;
    private final boolean usaTabuaEFaca;
    private final List<String> ingredientes;

    public Prato(String nome,
                 double preco,
                 int preparoMs,
                 boolean usaForno,
                 boolean usaTabuaEFaca,
                 List<String> ingredientes) {
        this.nome = nome;
        this.preco = preco;
        this.preparoMs = preparoMs;
        this.usaForno = usaForno;
        this.usaTabuaEFaca = usaTabuaEFaca;
        this.ingredientes = Collections.unmodifiableList(List.copyOf(ingredientes));
    }

    public String getNome() {
        return nome;
    }

    public double getPreco() {
        return preco;
    }

    /** Preparo simulado, em milissegundos. */
    public int getPreparoMs() {
        return preparoMs;
    }

    public boolean usaForno() {
        return usaForno;
    }

    public boolean usaTabuaEFaca() {
        return usaTabuaEFaca;
    }

    public List<String> getIngredientes() {
        return ingredientes;
    }

    @Override
    public String toString() {
        return nome;
    }
}
