package restaurante;

import java.util.List;
import java.util.concurrent.ThreadLocalRandom;

/** Cardapio minimo do enunciado. Lista estatica e imutavel. */
public final class Cardapio {

    public static final Prato PIZZA = new Prato(
            "Pizza", 45.00, 800, true, false, List.of("massa", "queijo", "tomate"));

    public static final Prato LASANHA = new Prato(
            "Lasanha", 38.00, 1000, true, false, List.of("massa", "queijo", "carne"));

    public static final Prato SALADA = new Prato(
            "Salada", 22.00, 400, false, true, List.of("alface", "tomate"));

    public static final Prato HAMBURGUER = new Prato(
            "Hamburguer", 30.00, 500, false, true, List.of("pao", "carne", "queijo"));

    public static final List<Prato> PRATOS = List.of(PIZZA, LASANHA, SALADA, HAMBURGUER);

    private Cardapio() {
    }

    /** ThreadLocalRandom: um gerador por thread, sem disputa pelo mesmo estado. */
    public static Prato sortear() {
        return PRATOS.get(ThreadLocalRandom.current().nextInt(PRATOS.size()));
    }
}
