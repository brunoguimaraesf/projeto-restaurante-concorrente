package restaurante;

import java.util.Collections;
import java.util.List;

/**
 * Um item do cardapio.
 *
 * A classe e imutavel (todos os campos sao final e a lista de ingredientes e
 * somente leitura). Isso importa em concorrencia: um objeto imutavel pode ser
 * lido por varias threads ao mesmo tempo sem lock nenhum, porque ninguem
 * consegue alterar o estado dele depois de construido.
 */
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

    /** Tempo de preparo simulado, em milissegundos (Thread.sleep). */
    public int getPreparoMs() {
        return preparoMs;
    }

    /** Usado a partir da Entrega 2 (SemaphoreSlim/Semaphore com 2 fornos). */
    public boolean usaForno() {
        return usaForno;
    }

    /** Usado a partir da Entrega 2 (um lock para a tabua e outro para a faca). */
    public boolean usaTabuaEFaca() {
        return usaTabuaEFaca;
    }

    /** Usado a partir da Entrega 2 (reserva atomica no estoque). */
    public List<String> getIngredientes() {
        return ingredientes;
    }

    @Override
    public String toString() {
        return nome;
    }
}
