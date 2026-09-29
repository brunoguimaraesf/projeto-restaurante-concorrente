package restaurante;

import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Fila de pedidos com capacidade 10 (requisito 2).
 *
 * Equivalente em Java do BlockingCollection<Pedido>(10) do C#:
 *
 *   C#                              Java
 *   ------------------------------  ---------------------------------------
 *   new BlockingCollection<T>(10)   new ArrayBlockingQueue<T>(10)
 *   Add(item)  (bloqueia se cheia)  put(item)   (bloqueia se cheia)
 *   Take()     (bloqueia se vazia)  take()      (bloqueia se vazia)
 *   CompleteAdding()                nao existe -> usamos "pedido sentinela"
 *   GetConsumingEnumerable()        take() em loop ate receber a sentinela
 *
 * Como a fila e LIMITADA, ela funciona como freio (back-pressure): quando os
 * cozinheiros nao dao conta, a fila enche e o proprio atendente fica parado no
 * put() ate abrir vaga. Nada de fila infinita estourando a memoria.
 *
 * Encerramento (a "dica" da Entrega 1): a fila so pode ser encerrada depois que
 * OS DOIS atendentes terminarem. Como a ArrayBlockingQueue e FIFO, as N
 * sentinelas entram depois de todos os pedidos reais; cada cozinheiro retira
 * exatamente uma sentinela e sai. Se encerrassemos antes, perderiamos pedidos;
 * se nunca encerrassemos, os cozinheiros ficariam bloqueados para sempre no
 * take() (problema de liveness).
 */
public final class FilaDePedidos {

    public static final int CAPACIDADE = 10;

    /** Pedido falso que significa "acabou o expediente, pode sair". */
    private static final Pedido FIM_DO_EXPEDIENTE = new Pedido(-1, Cardapio.PIZZA, "sistema");

    private final BlockingQueue<Pedido> fila = new ArrayBlockingQueue<>(CAPACIDADE);
    private final AtomicBoolean encerrada = new AtomicBoolean(false);
    private final AtomicInteger vezesQueEncheu = new AtomicInteger();

    /**
     * Coloca um pedido na fila. Se a fila estiver cheia, a thread do atendente
     * fica bloqueada aqui dentro do put() ate um cozinheiro liberar espaco.
     */
    public void adicionar(Pedido pedido) throws InterruptedException {
        if (fila.remainingCapacity() == 0) {
            vezesQueEncheu.incrementAndGet();
        }
        fila.put(pedido);
    }

    /** True se a fila esta na capacidade maxima neste instante. */
    public boolean estaCheia() {
        return fila.remainingCapacity() == 0;
    }

    /**
     * Retira o proximo pedido, bloqueando enquanto a fila estiver vazia.
     *
     * @return o pedido, ou null quando o expediente acabou e nao ha mais nada
     *         para preparar (equivale ao fim do GetConsumingEnumerable do C#)
     */
    public Pedido retirar() throws InterruptedException {
        Pedido pedido = fila.take();
        if (pedido == FIM_DO_EXPEDIENTE) {
            return null;
        }
        return pedido;
    }

    /**
     * Fecha a fila para novos pedidos e avisa cada cozinheiro para sair.
     * Chamar SOMENTE depois que todos os atendentes terminarem.
     */
    public void encerrar(int quantidadeDeCozinheiros) throws InterruptedException {
        if (!encerrada.compareAndSet(false, true)) {
            return;
        }
        for (int i = 0; i < quantidadeDeCozinheiros; i++) {
            fila.put(FIM_DO_EXPEDIENTE);
        }
    }

    public int tamanhoAtual() {
        return fila.size();
    }

    /** Quantas vezes um atendente encontrou a fila cheia e teve de esperar. */
    public int getVezesQueEncheu() {
        return vezesQueEncheu.get();
    }
}
