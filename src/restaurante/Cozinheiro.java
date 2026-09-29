package restaurante;

import java.util.concurrent.atomic.AtomicInteger;

/**
 * Cozinheiro: retira pedidos da fila e prepara (requisito 3).
 * E o CONSUMIDOR do padrao produtor/consumidor.
 *
 * Varios cozinheiros rodam ao mesmo tempo em threads diferentes. Cada um fica
 * bloqueado no retirar() enquanto a fila estiver vazia - sem gastar CPU em
 * espera ocupada (busy wait) - e acorda quando chega pedido.
 *
 * Nesta Entrega 1 o preparo e so o Thread.sleep do prato. Forno, tabua/faca e
 * estoque entram na Entrega 2.
 */
public final class Cozinheiro implements Runnable {

    private final String nome;
    private final FilaDePedidos fila;
    /** Contador compartilhado, so para o log mostrar o paralelismo acontecendo. */
    private final AtomicInteger cozinheirosPreparando;

    private int pratosPreparados;
    private long tempoPreparandoMs;

    public Cozinheiro(String nome, FilaDePedidos fila, AtomicInteger cozinheirosPreparando) {
        this.nome = nome;
        this.fila = fila;
        this.cozinheirosPreparando = cozinheirosPreparando;
    }

    @Override
    public void run() {
        Log.evento(nome, "entrou na cozinha");
        try {
            while (true) {
                // Bloqueia aqui ate chegar pedido. Retorna null quando o
                // expediente acabou e a fila foi esvaziada.
                Pedido pedido = fila.retirar();
                if (pedido == null) {
                    break;
                }
                prepara(pedido);
            }
            Log.evento(nome, "saiu da cozinha - preparou " + pratosPreparados + " pratos");
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            Log.evento(nome, "foi interrompido");
        }
    }

    private void prepara(Pedido pedido) throws InterruptedException {
        int emPreparo = cozinheirosPreparando.incrementAndGet();
        Log.evento(nome, "pegou " + pedido.descricaoCurta()
                + " - comecou o preparo (cozinhando agora: " + emPreparo + ")");
        try {
            long inicio = System.nanoTime();
            Thread.sleep(pedido.getPrato().getPreparoMs());  // preparo simulado
            tempoPreparandoMs += (System.nanoTime() - inicio) / 1_000_000L;
            pratosPreparados++;
            Log.evento(nome, pedido.descricaoCurta() + " ficou pronto");
        } finally {
            // finally garante que o contador volta mesmo se der excecao ou
            // interrupcao no meio do preparo.
            cozinheirosPreparando.decrementAndGet();
        }
    }

    public String getNome() {
        return nome;
    }

    public int getPratosPreparados() {
        return pratosPreparados;
    }

    public long getTempoPreparandoMs() {
        return tempoPreparandoMs;
    }
}
