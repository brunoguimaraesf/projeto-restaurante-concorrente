package restaurante;

import java.util.concurrent.ThreadLocalRandom;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Atendente: gera pedidos aleatorios em intervalos aleatorios (requisito 1).
 * E o PRODUTOR do padrao produtor/consumidor.
 *
 * Os dois atendentes dividem o mesmo alvo de 60 pedidos. Quem pega qual numero
 * e decidido por um AtomicInteger compartilhado: getAndIncrement() le, soma e
 * grava em UMA operacao atomica (instrucao CAS do processador). Com um int
 * comum (numero++), os dois atendentes poderiam ler o mesmo valor e gerar dois
 * pedidos com o mesmo numero, ou gerar menos de 60 no total: e exatamente a
 * race condition da Aula 2.
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
                // Tempo entre um cliente e outro.
                Thread.sleep(ThreadLocalRandom.current()
                        .nextInt(INTERVALO_MINIMO_MS, INTERVALO_MAXIMO_MS + 1));

                // Reserva um numero de pedido de forma atomica.
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

                // Se a fila estiver cheia, a thread para AQUI ate abrir vaga.
                fila.adicionar(pedido);
                pedidosGerados++;

                Log.evento(nome, pedido.descricaoCurta() + " entrou na fila ("
                        + fila.tamanhoAtual() + "/" + FilaDePedidos.CAPACIDADE + ")");
            }
            Log.evento(nome, "encerrou o turno - anotou " + pedidosGerados + " pedidos");
        } catch (InterruptedException e) {
            // Boa pratica: nunca engolir a interrupcao. Restaura a flag para
            // quem chamou saber que a thread foi interrompida.
            Thread.currentThread().interrupt();
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
