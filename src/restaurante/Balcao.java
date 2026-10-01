package restaurante;

import java.util.Queue;
import java.util.concurrent.ConcurrentLinkedQueue;

/**
 * Balcao de pratos prontos (requisito 7). Varios cozinheiros colocam, o garcom
 * retira. Cada prato colocado toca o sino.
 */
public final class Balcao {

    private final Queue<Pedido> pratosProntos = new ConcurrentLinkedQueue<>();
    private final Sino sino = new Sino();

    private volatile boolean fechado;

    public void colocar(Pedido pedido) {
        pratosProntos.add(pedido);
        sino.tocar();
    }

    /** @return o proximo prato, ou null se o balcao estiver vazio */
    public Pedido retirar() {
        return pratosProntos.poll();
    }

    public void esperarSino() throws InterruptedException {
        sino.esperar();
    }

    /** Fim do expediente: toca o sino para o garcom acordar e ir embora. */
    public void fechar() {
        fechado = true;
        sino.tocar();
    }

    public boolean estaFechado() {
        return fechado;
    }

    public boolean estaVazio() {
        return pratosProntos.isEmpty();
    }

    public int tamanhoAtual() {
        return pratosProntos.size();
    }
}
