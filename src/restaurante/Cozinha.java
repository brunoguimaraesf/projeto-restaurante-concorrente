package restaurante;

import java.util.concurrent.Semaphore;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Recursos disputados pelos cozinheiros: 2 fornos (requisito 4) e o par
 * tabua/faca (requisito 5).
 */
public final class Cozinha {

    public static final int FORNOS = 2;

    private final Semaphore fornos = new Semaphore(FORNOS, true);
    private final AtomicInteger fornosOcupados = new AtomicInteger();

    private final Object tabua = new Object();
    private final Object faca = new Object();

    /** Assa no forno. Bloqueia enquanto os dois fornos estiverem ocupados. */
    public void assar(Pedido pedido, String cozinheiro) throws InterruptedException {
        if (fornos.availablePermits() == 0) {
            Log.evento(cozinheiro, pedido.descricaoCurta() + " esperando forno livre ("
                    + FORNOS + "/" + FORNOS + ")");
        }
        fornos.acquire();
        try {
            int ocupados = fornosOcupados.incrementAndGet();
            Log.evento(cozinheiro, pedido.descricaoCurta() + " entrou no forno ("
                    + ocupados + "/" + FORNOS + ")");
            Thread.sleep(pedido.getPrato().getPreparoMs());
            Log.evento(cozinheiro, pedido.descricaoCurta() + " saiu do forno");
        } finally {
            fornosOcupados.decrementAndGet();
            fornos.release();
        }
    }

    /** Monta o prato na bancada. ORDEM FIXA: tabua, depois faca. */
    public void montar(Pedido pedido, String cozinheiro) throws InterruptedException {
        synchronized (tabua) {
            Log.evento(cozinheiro, "pegou a tabua para " + pedido.descricaoCurta());
            synchronized (faca) {
                Log.evento(cozinheiro, "pegou a faca - montando " + pedido.descricaoCurta());
                Thread.sleep(pedido.getPrato().getPreparoMs());
            }
        }
        Log.evento(cozinheiro, "devolveu faca e tabua - " + pedido.descricaoCurta() + " montado");
    }

    public int getFornosOcupados() {
        return fornosOcupados.get();
    }
}
