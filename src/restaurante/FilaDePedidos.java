package restaurante;

import java.util.concurrent.ArrayBlockingQueue;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Fila de pedidos com capacidade 10 (requisito 2).
 * Equivale ao BlockingCollection<Pedido>(10) do C#: ArrayBlockingQueue, com
 * put/take no lugar de Add/Take. Java nao tem CompleteAdding, entao o fim do
 * expediente e avisado por um pedido sentinela (ver encerrar).
 */
public final class FilaDePedidos {

    public static final int CAPACIDADE = 10;

    /** Pedido falso que significa "acabou o expediente". */
    private static final Pedido FIM_DO_EXPEDIENTE = new Pedido(-1, Cardapio.PIZZA, "sistema");

    private final BlockingQueue<Pedido> fila = new ArrayBlockingQueue<>(CAPACIDADE);
    private final AtomicBoolean encerrada = new AtomicBoolean(false);
    private final AtomicInteger vezesQueEncheu = new AtomicInteger();

    /** Bloqueia no put() enquanto a fila estiver cheia (back-pressure). */
    public void adicionar(Pedido pedido) throws InterruptedException {
        if (fila.remainingCapacity() == 0) {
            vezesQueEncheu.incrementAndGet();
        }
        fila.put(pedido);
    }

    /**
     * Bloqueia enquanto a fila estiver vazia.
     * @return o pedido, ou null quando o expediente acabou
     */
    public Pedido retirar() throws InterruptedException {
        Pedido pedido = fila.take();
        return pedido == FIM_DO_EXPEDIENTE ? null : pedido;
    }

    /**
     * Uma sentinela por cozinheiro. Como a fila e FIFO, elas entram depois de
     * todos os pedidos reais e cada cozinheiro retira exatamente uma.
     * Chamar SO depois que os dois atendentes terminarem.
     */
    public void encerrar(int quantidadeDeCozinheiros) throws InterruptedException {
        if (!encerrada.compareAndSet(false, true)) {
            return;
        }
        for (int i = 0; i < quantidadeDeCozinheiros; i++) {
            fila.put(FIM_DO_EXPEDIENTE);
        }
    }

    public boolean estaCheia() {
        return fila.remainingCapacity() == 0;
    }

    public int tamanhoAtual() {
        return fila.size();
    }

    public int getVezesQueEncheu() {
        return vezesQueEncheu.get();
    }
}
