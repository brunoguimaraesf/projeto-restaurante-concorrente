package restaurante;

import java.util.concurrent.ThreadLocalRandom;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Produtor: gera pedidos aleatorios em intervalos aleatorios (requisito 1).
 * Os dois atendentes dividem o mesmo alvo de pedidos; o numero de cada um vem
 * de AtomicInteger.getAndIncrement() - ler, somar e gravar numa operacao so.
 * Com um int comum (numero++) sairiam numeros repetidos: race condition.
 */
public final class Atendente implements Runnable {

    private static final int INTERVALO_MINIMO_MS = 60;
    private static final int INTERVALO_MAXIMO_MS = 320;

    private final String nome;
    private final FilaDePedidos fila;
    private final AtomicInteger proximoNumero;
    private final int totalDePedidos;

    private int pedidosGerados;

    public Atendente(String nome,
                     FilaDePedidos fila,
                     AtomicInteger proximoNumero,
                     int totalDePedidos) {
        this.nome = nome;
        this.fila = fila;
        this.proximoNumero = proximoNumero;
        this.totalDePedidos = totalDePedidos;
    }

    @Override
    public void run() {
        Log.evento(nome, "abriu o caderno de pedidos");
        try {
            while (true) {
                Thread.sleep(ThreadLocalRandom.current()
                        .nextInt(INTERVALO_MINIMO_MS, INTERVALO_MAXIMO_MS + 1));

                int numero = proximoNumero.getAndIncrement();
                if (numero > totalDePedidos) {
                    break;
                }

                Pedido pedido = new Pedido(numero, Cardapio.sortear(), nome);
                Log.evento(nome, "anotou " + pedido.descricaoCurta());

                if (fila.estaCheia()) {
                    Log.evento(nome, "fila cheia (" + FilaDePedidos.CAPACIDADE + "/"
                            + FilaDePedidos.CAPACIDADE + ") - esperando vaga para "
                            + pedido.descricaoCurta());
                }

                fila.adicionar(pedido);  // trava aqui se a fila estiver cheia
                pedidosGerados++;

                Log.evento(nome, pedido.descricaoCurta() + " entrou na fila ("
                        + fila.tamanhoAtual() + "/" + FilaDePedidos.CAPACIDADE + ")");
            }
            Log.evento(nome, "encerrou o turno - anotou " + pedidosGerados + " pedidos");
        } catch (InterruptedException e) {
            Thread.currentThread().interrupt();  // nunca engolir a interrupcao
            Log.evento(nome, "foi interrompido");
        }
    }

    public String getNome() {
        return nome;
    }

    public int getPedidosGerados() {
        return pedidosGerados;
    }
}
