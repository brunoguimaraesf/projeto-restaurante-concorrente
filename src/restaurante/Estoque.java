package restaurante;

import java.util.LinkedHashMap;
import java.util.LinkedHashSet;
import java.util.Map;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

/**
 * Estoque de ingredientes, 40 unidades de cada (requisito 6).
 *
 * O ConcurrentHashMap e atomico por chave, mas o prato precisa de varias
 * chaves de uma vez - por isso a reserva inteira roda dentro de um synchronized.
 */
public final class Estoque {

    public static final int QUANTIDADE_INICIAL = 40;

    private final Map<String, Integer> quantidades = new ConcurrentHashMap<>();
    private final Set<String> ordemDeExibicao = new LinkedHashSet<>();
    private final Object trava = new Object();

    public Estoque() {
        for (Prato prato : Cardapio.PRATOS) {
            for (String ingrediente : prato.getIngredientes()) {
                quantidades.putIfAbsent(ingrediente, QUANTIDADE_INICIAL);
                ordemDeExibicao.add(ingrediente);
            }
        }
    }

    /** Reserva 1 unidade de cada ingrediente do prato. Tudo ou nada. */
    public boolean reservar(Prato prato) {
        synchronized (trava) {
            for (String ingrediente : prato.getIngredientes()) {
                if (quantidades.getOrDefault(ingrediente, 0) <= 0) {
                    return false;
                }
            }
            for (String ingrediente : prato.getIngredientes()) {
                quantidades.merge(ingrediente, -1, Integer::sum);
            }
            return true;
        }
    }

    /** Primeiro ingrediente zerado, para o log da recusa. */
    public String primeiroEmFalta(Prato prato) {
        synchronized (trava) {
            for (String ingrediente : prato.getIngredientes()) {
                if (quantidades.getOrDefault(ingrediente, 0) <= 0) {
                    return ingrediente;
                }
            }
            return "nenhum";
        }
    }

    /** Copia do estoque na ordem do cardapio, para o relatorio. */
    public Map<String, Integer> situacaoAtual() {
        synchronized (trava) {
            Map<String, Integer> copia = new LinkedHashMap<>();
            for (String ingrediente : ordemDeExibicao) {
                copia.put(ingrediente, quantidades.getOrDefault(ingrediente, 0));
            }
            return copia;
        }
    }

    public boolean temAlgumNegativo() {
        synchronized (trava) {
            return quantidades.values().stream().anyMatch(q -> q < 0);
        }
    }
}
