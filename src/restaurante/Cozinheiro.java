package restaurante;

import java.util.concurrent.atomic.AtomicInteger;

/**
 * Consumidor: retira pedidos da fila e prepara (requisito 3).
 * Reserva os ingredientes, disputa forno ou tabua/faca e manda o prato para o
 * balcao. Sem ingrediente, o pedido e recusado antes de ocupar qualquer recurso.
 */
public final class Cozinheiro implements Runnable {

    private final String nome;
    private final FilaDePedidos fila;
    private final Estoque estoque;
    private final Cozinha cozinha;
    private final Balcao balcao;
    /** So para o log mostrar quantos estao cozinhando ao mesmo tempo. */
    private final AtomicInteger cozinheirosPreparando;

    private int pratosPreparados;
    private int pedidosRecusados;
    private long tempoPreparandoMs;

    public Cozinheiro(String nome,
                      FilaDePedidos fila,
                      Estoque estoque,
                      Cozinha cozinha,
                      Balcao balcao,
                      AtomicInteger cozinheirosPreparando) {
        this.nome = nome;
        this.fila = fila;
        this.estoque = estoque;
        this.cozinha = cozinha;
        this.balcao = balcao;
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
            Log.evento(nome, "saiu da cozinha - preparou " + pratosPreparados
                    + " pratos, recusou " + pedidosRecusados);
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            Log.evento(nome, "foi interrompido");
        }
    }

    private void prepara(Pedido pedido) throws InterruptedException {
        Prato prato = pedido.getPrato();

        if (!estoque.reservar(prato)) {
            pedidosRecusados++;
            Log.evento(nome, pedido.descricaoCurta() + " RECUSADO - acabou o(a) "
                    + estoque.primeiroEmFalta(prato));
            return;
        }

        int emPreparo = cozinheirosPreparando.incrementAndGet();
        Log.evento(nome, "pegou " + pedido.descricaoCurta()
                + " - ingredientes reservados (cozinhando agora: " + emPreparo + ")");
        try {
            long inicio = System.nanoTime();
            if (prato.usaForno()) {
                cozinha.assar(pedido, nome);
            } else if (prato.usaTabuaEFaca()) {
                cozinha.montar(pedido, nome);
            } else {
                Thread.sleep(prato.getPreparoMs());
            }
            tempoPreparandoMs += (System.nanoTime() - inicio) / 1_000_000L;
            pratosPreparados++;

            // Loga antes de colocar: depois do colocar() o garcom ja pode ter
            // entregado, e as duas linhas sairiam fora de ordem.
            Log.evento(nome, pedido.descricaoCurta() + " foi para o balcao");
            balcao.colocar(pedido);
        } finally {
            cozinheirosPreparando.decrementAndGet();
        }
    }

    public String getNome() {
        return nome;
    }

    public int getPratosPreparados() {
        return pratosPreparados;
    }

    public int getPedidosRecusados() {
        return pedidosRecusados;
    }

    public long getTempoPreparandoMs() {
        return tempoPreparandoMs;
    }
}
