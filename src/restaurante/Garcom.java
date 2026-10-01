package restaurante;

/**
 * Garcom: dorme ate o sino tocar e entrega os pratos (requisito 7).
 * Como o sino junta varios toques num so, cada vez que acorda ele esvazia o
 * balcao inteiro - senao pratos ficariam esquecidos.
 */
public final class Garcom implements Runnable {

    private static final int TEMPO_DE_ENTREGA_MS = 120;

    private final String nome;
    private final Balcao balcao;

    private int pratosEntregues;

    public Garcom(String nome, Balcao balcao) {
        this.nome = nome;
        this.balcao = balcao;
    }

    @Override
    public void run() {
        Log.evento(nome, "entrou no salao");
        try {
            while (true) {
                balcao.esperarSino();
                esvaziarBalcao();
                if (balcao.estaFechado() && balcao.estaVazio()) {
                    break;
                }
            }
            Log.evento(nome, "saiu do salao - entregou " + pratosEntregues + " pratos");
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();
            Log.evento(nome, "foi interrompido");
        }
    }

    private void esvaziarBalcao() throws InterruptedException {
        Pedido pedido;
        while ((pedido = balcao.retirar()) != null) {
            Thread.sleep(TEMPO_DE_ENTREGA_MS);  // levar ate a mesa
            pratosEntregues++;
            Log.evento(nome, "entregou " + pedido.descricaoCurta()
                    + " (restam " + balcao.tamanhoAtual() + " no balcao)");
        }
    }

    public String getNome() {
        return nome;
    }

    public int getPratosEntregues() {
        return pratosEntregues;
    }
}
