package restaurante;

import java.util.concurrent.atomic.AtomicInteger;

/**
 * Consumidor: retira pedidos da fila e prepara (requisito 3).
 * Fica bloqueado no retirar() enquanto a fila estiver vazia, sem gastar CPU.
 * Nesta entrega o preparo e so o Thread.sleep do prato.
 */
public final class Cozinheiro implements Runnable {

    private final String nome;
    private final FilaDePedidos fila;
    /** So para o log mostrar quantos estao cozinhando ao mesmo tempo. */
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
                Pedido pedido = fila.retirar();
                if (pedido == null) {  // expediente encerrado
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
            Thread.sleep(pedido.getPrato().getPreparoMs());
            tempoPreparandoMs += (System.nanoTime() - inicio) / 1_000_000L;
            pratosPreparados++;
            Log.evento(nome, pedido.descricaoCurta() + " ficou pronto");
        } finally {
            cozinheirosPreparando.decrementAndGet();  // volta mesmo se der excecao
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
