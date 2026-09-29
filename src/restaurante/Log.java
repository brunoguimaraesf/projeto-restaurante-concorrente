package restaurante;

import java.io.PrintStream;
import java.nio.charset.StandardCharsets;

/**
 * Log de eventos (requisito 10).
 * Formato: [mm:ss.SSS] Ator · mensagem
 */
public final class Log {

    private static final Object TRAVA = new Object();
    private static final PrintStream SAIDA =
            new PrintStream(new java.io.FileOutputStream(java.io.FileDescriptor.out),
                            true, StandardCharsets.UTF_8);

    private static volatile long inicioNanos = System.nanoTime();

    private Log() {
    }

    public static void iniciar() {
        inicioNanos = System.nanoTime();
    }

    public static long decorridoMs() {
        return (System.nanoTime() - inicioNanos) / 1_000_000L;
    }

    /**
     * Registra um evento. O horario e lido DENTRO do synchronized: lido antes,
     * duas threads poderiam ler o relogio em uma ordem e imprimir em outra.
     */
    public static void evento(String ator, String mensagem) {
        synchronized (TRAVA) {
            SAIDA.println("[" + carimbo(decorridoMs()) + "] "
                    + preencher(ator, 14) + " · " + mensagem);
        }
    }

    public static void linha(String texto) {
        synchronized (TRAVA) {
            SAIDA.println(texto);
        }
    }

    public static String carimbo(long ms) {
        return String.format("%02d:%02d.%03d", ms / 60_000L, (ms % 60_000L) / 1_000L, ms % 1_000L);
    }

    private static String preencher(String texto, int largura) {
        if (texto.length() >= largura) {
            return texto;
        }
        return texto + " ".repeat(largura - texto.length());
    }
}
