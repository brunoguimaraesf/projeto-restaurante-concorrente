package restaurante;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;
import java.util.Map;
import java.util.concurrent.atomic.AtomicInteger;

/**
 * Monta o restaurante, roda a simulacao e imprime o relatorio.
 * Encerramento, nesta ordem: join atendentes -> encerrar fila -> join
 * cozinheiros -> fechar balcao -> join garcom.
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
        Log.linha("===== RESTAURANTE CONCORRENTE - Entrega 2 =====");
        Log.linha("Atendentes: 2 | Cozinheiros: " + quantidadeDeCozinheiros
                + " | Fornos: " + Cozinha.FORNOS
                + " | Fila: capacidade " + FilaDePedidos.CAPACIDADE
                + " | Pedidos: " + totalDePedidos);
        Log.linha("Estoque inicial: " + Estoque.QUANTIDADE_INICIAL + " unidades de cada ingrediente");
        Log.linha("");

        FilaDePedidos fila = new FilaDePedidos();
        Estoque estoque = new Estoque();
        Cozinha cozinha = new Cozinha();
        Balcao balcao = new Balcao();
        AtomicInteger proximoNumero = new AtomicInteger(1);
        AtomicInteger cozinheirosPreparando = new AtomicInteger();

        Garcom garcom = new Garcom("Garcom", balcao);
        Thread threadDoGarcom = new Thread(garcom, "garcom");

        List<Cozinheiro> cozinheiros = new ArrayList<>();
        List<Thread> threadsDosCozinheiros = new ArrayList<>();
        for (int i = 1; i <= quantidadeDeCozinheiros; i++) {
            Cozinheiro cozinheiro = new Cozinheiro("Cozinheiro " + i, fila, estoque,
                    cozinha, balcao, cozinheirosPreparando);
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

        threadDoGarcom.start();
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
        Log.evento("Gerente", "cozinha vazia - fechando o balcao");

        balcao.fechar();
        threadDoGarcom.join();

        long totalMs = (System.nanoTime() - inicio) / 1_000_000L;
        imprimirRelatorio(atendentes, cozinheiros, garcom, fila, balcao, estoque, totalMs);
    }

    private void imprimirRelatorio(List<Atendente> atendentes,
                                   List<Cozinheiro> cozinheiros,
                                   Garcom garcom,
                                   FilaDePedidos fila,
                                   Balcao balcao,
                                   Estoque estoque,
                                   long totalMs) {
        int recebidos = atendentes.stream().mapToInt(Atendente::getPedidosGerados).sum();
        int preparados = cozinheiros.stream().mapToInt(Cozinheiro::getPratosPreparados).sum();
        int recusados = cozinheiros.stream().mapToInt(Cozinheiro::getPedidosRecusados).sum();
        int entregues = garcom.getPratosEntregues();
        long somaDosPreparos = cozinheiros.stream().mapToLong(Cozinheiro::getTempoPreparandoMs).sum();

        Log.linha("");
        Log.linha("===== RELATORIO - Entrega 2 =====");
        Log.linha("Cozinheiros: " + quantidadeDeCozinheiros
                + " | Fornos: " + Cozinha.FORNOS
                + " | Fila: capacidade " + FilaDePedidos.CAPACIDADE);
        Log.linha("");
        Log.linha(preencher("Pedidos recebidos", 32) + recebidos);
        for (Atendente atendente : atendentes) {
            Log.linha("  " + preencher(atendente.getNome(), 30) + atendente.getPedidosGerados());
        }
        Log.linha(preencher("Preparados", 32) + preparados);
        for (Cozinheiro cozinheiro : cozinheiros) {
            Log.linha("  " + preencher(cozinheiro.getNome(), 30) + cozinheiro.getPratosPreparados()
                    + "  (recusou " + cozinheiro.getPedidosRecusados()
                    + ", " + segundos(cozinheiro.getTempoPreparandoMs()) + " de preparo)");
        }
        Log.linha(preencher("Entregues pelo garcom", 32) + entregues);
        Log.linha(preencher("Recusados (sem ingrediente)", 32) + recusados);
        Log.linha("");
        Log.linha("Estoque final: " + formatarEstoque(estoque.situacaoAtual()));
        Log.linha("");
        Log.linha(preencher("Vezes que a fila encheu", 32) + fila.getVezesQueEncheu());
        Log.linha(preencher("Pedidos sobrando na fila", 32) + fila.tamanhoAtual());
        Log.linha(preencher("Pratos sobrando no balcao", 32) + balcao.tamanhoAtual());
        Log.linha(preencher("Tempo total", 32) + segundos(totalMs));
        Log.linha(preencher("Soma dos preparos", 32) + segundos(somaDosPreparos));
        Log.linha(preencher("Paralelismo medio", 32)
                + String.format(Locale.forLanguageTag("pt-BR"), "%.2fx",
                        totalMs == 0 ? 0 : (double) somaDosPreparos / totalMs));
        Log.linha("");
        Log.linha(verificacao(recebidos == entregues + recusados,
                "recebidos = entregues + recusados"));
        Log.linha(verificacao(!estoque.temAlgumNegativo(),
                "nenhum ingrediente com estoque negativo"));
        Log.linha(verificacao(fila.tamanhoAtual() == 0 && balcao.tamanhoAtual() == 0,
                "fila e balcao terminaram vazios"));
        Log.linha(verificacao(preparados == entregues,
                "todo prato preparado foi entregue (nenhum esquecido no balcao)"));
    }

    private static String formatarEstoque(Map<String, Integer> estoque) {
        StringBuilder sb = new StringBuilder();
        for (Map.Entry<String, Integer> item : estoque.entrySet()) {
            if (sb.length() > 0) {
                sb.append(" | ");
            }
            sb.append(item.getKey()).append(" ").append(item.getValue());
        }
        return sb.toString();
    }

    private static String verificacao(boolean ok, String texto) {
        return (ok ? "[OK]   " : "[ERRO] ") + texto;
    }

    private static String segundos(long ms) {
        return String.format(Locale.forLanguageTag("pt-BR"), "%.1f s", ms / 1000.0);
    }

    private static String preencher(String texto, int largura) {
        StringBuilder sb = new StringBuilder(texto).append(" ");
        while (sb.length() < largura) {
            sb.append(".");
        }
        return sb.append(" ").toString();
    }
}
