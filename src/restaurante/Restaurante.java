package restaurante;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Monta o restaurante, roda a simulacao e imprime o resumo.
 * Encerramento: join nos dois atendentes -> encerrar a fila -> join nos
 * cozinheiros. Fechar antes perderia pedidos; nunca fechar deixaria os
 * cozinheiros bloqueados para sempre no take().
 */
public final class Restaurante {

    private final int quantidadeDeCozinheiros;
    private final int totalDePedidos;

    public Restaurante(int quantidadeDeCozinheiros, int totalDePedidos) {
        this.quantidadeDeCozinheiros = quantidadeDeCozinheiros;
        this.totalDePedidos = totalDePedidos;
    }

    public void abrir() throws InterruptedException {
        Log.iniciar();
        Log.linha("===== RESTAURANTE CONCORRENTE - Entrega 1 =====");
        Log.linha("Atendentes: 2 | Cozinheiros: " + quantidadeDeCozinheiros
                + " | Fila: capacidade " + FilaDePedidos.CAPACIDADE
                + " | Pedidos: " + totalDePedidos);
        Log.linha("");

        FilaDePedidos fila = new FilaDePedidos();
        AtomicInteger proximoNumero = new AtomicInteger(1);
        AtomicInteger cozinheirosPreparando = new AtomicInteger();

        List<Cozinheiro> cozinheiros = new ArrayList<>();
        List<Thread> threadsDosCozinheiros = new ArrayList<>();
        for (int i = 1; i <= quantidadeDeCozinheiros; i++) {
            Cozinheiro cozinheiro = new Cozinheiro("Cozinheiro " + i, fila, cozinheirosPreparando);
            cozinheiros.add(cozinheiro);
            threadsDosCozinheiros.add(new Thread(cozinheiro, "cozinheiro-" + i));
        }

        List<Atendente> atendentes = new ArrayList<>();
        List<Thread> threadsDosAtendentes = new ArrayList<>();
        for (int i = 1; i <= 2; i++) {
            Atendente atendente = new Atendente("Atendente " + i, fila, proximoNumero, totalDePedidos);
            atendentes.add(atendente);
            threadsDosAtendentes.add(new Thread(atendente, "atendente-" + i));
        }

        long inicio = System.nanoTime();

        threadsDosCozinheiros.forEach(Thread::start);
        threadsDosAtendentes.forEach(Thread::start);

        for (Thread thread : threadsDosAtendentes) {
            thread.join();
        }
        Log.evento("Gerente", "os dois atendentes terminaram - fila fechada para novos pedidos");

        fila.encerrar(quantidadeDeCozinheiros);

        for (Thread thread : threadsDosCozinheiros) {
            thread.join();
        }

        long totalMs = (System.nanoTime() - inicio) / 1_000_000L;
        imprimirResumo(atendentes, cozinheiros, fila, totalMs);
    }

    private void imprimirResumo(List<Atendente> atendentes,
                                List<Cozinheiro> cozinheiros,
                                FilaDePedidos fila,
                                long totalMs) {
        int gerados = atendentes.stream().mapToInt(Atendente::getPedidosGerados).sum();
        int preparados = cozinheiros.stream().mapToInt(Cozinheiro::getPratosPreparados).sum();
        long somaDosPreparos = cozinheiros.stream().mapToLong(Cozinheiro::getTempoPreparandoMs).sum();

        Log.linha("");
        Log.linha("===== RESUMO - Entrega 1 =====");
        Log.linha("Cozinheiros: " + quantidadeDeCozinheiros
                + " | Fila: capacidade " + FilaDePedidos.CAPACIDADE);
        Log.linha("");
        Log.linha(preencher("Pedidos gerados", 30) + gerados);
        for (Atendente atendente : atendentes) {
            Log.linha("  " + preencher(atendente.getNome(), 28) + atendente.getPedidosGerados());
        }
        Log.linha(preencher("Pratos preparados", 30) + preparados);
        for (Cozinheiro cozinheiro : cozinheiros) {
            Log.linha("  " + preencher(cozinheiro.getNome(), 28) + cozinheiro.getPratosPreparados()
                    + "  (" + segundos(cozinheiro.getTempoPreparandoMs()) + " de fogao)");
        }
        Log.linha(preencher("Vezes que a fila encheu", 30) + fila.getVezesQueEncheu());
        Log.linha(preencher("Pedidos sobrando na fila", 30) + fila.tamanhoAtual());
        Log.linha("");
        Log.linha(preencher("Tempo total", 30) + segundos(totalMs));
        Log.linha(preencher("Soma dos preparos", 30) + segundos(somaDosPreparos)
                + "  (se fosse sequencial, seria esse o tempo)");
        Log.linha(preencher("Ganho da concorrencia", 30)
                + String.format(Locale.forLanguageTag("pt-BR"), "%.2fx",
                        totalMs == 0 ? 0 : (double) somaDosPreparos / totalMs));
        Log.linha("");
        Log.linha(verificacao(gerados == totalDePedidos,
                "todos os " + totalDePedidos + " pedidos foram gerados"));
        Log.linha(verificacao(preparados == gerados,
                "pedidos gerados = pratos preparados (nenhum pedido se perdeu)"));
        Log.linha(verificacao(fila.tamanhoAtual() == 0, "a fila terminou vazia"));
    }

    private static String verificacao(boolean ok, String texto) {
        return (ok ? "[OK]   " : "[ERRO] ") + texto;
    }

    private static String segundos(long ms) {
        return String.format(Locale.forLanguageTag("pt-BR"), "%.1f s", ms / 1000.0);
    }

    private static String preencher(String texto, int largura) {
        StringBuilder sb = new StringBuilder(texto).append(' ');
        while (sb.length() < largura) {
            sb.append('.');
        }
        return sb.append(' ').toString();
    }
}
