package restaurante;

import java.io.PrintStream;
import java.nio.charset.StandardCharsets;

/**
 * Log de eventos com horario e responsavel (requisito 10).
 *
 * Formato: [mm:ss.SSS] Ator · mensagem
 * Ex.:     [00:03.214] Cozinheiro 2 · Pizza #14 comecou o preparo
 *
 * Por que o synchronized aqui?
 * System.out.println e thread-safe, mas duas chamadas seguidas NAO sao atomicas
 * entre si: com varias threads escrevendo, as linhas embaralham e o horario
 * impresso pode sair fora de ordem. Montamos a linha inteira antes e imprimimos
 * dentro de um bloco sincronizado, entao cada evento sai inteiro e em ordem.
 */
public final class Log {

    private static final Object TRAVA = new Object();
    private static final PrintStream SAIDA =
            new PrintStream(new java.io.FileOutputStream(java.io.FileDescriptor.out),
                            true, StandardCharsets.UTF_8);

    private static volatile long inicioNanos = System.nanoTime();

    private Log() {
    }

    /** Zera o cronometro do log. Chamado no comeco de cada simulacao. */
    public static void iniciar() {
        inicioNanos = System.nanoTime();
    }

    /** Milissegundos decorridos desde iniciar(). */
    public static long decorridoMs() {
        return (System.nanoTime() - inicioNanos) / 1_000_000L;
    }

    /** Registra um evento: quem fez e o que aconteceu. */
    public static void evento(String ator, String mensagem) {
        // O carimbo e lido DENTRO do bloco sincronizado, de proposito: se o
        // horario fosse lido antes, duas threads poderiam ler o relogio em uma
        // ordem e imprimir em outra, e o log sairia com horarios fora de ordem.
        // E a mesma ideia de "ler e gravar tem de ser uma operacao so".
        synchronized (TRAVA) {
            SAIDA.println("[" + carimbo(decorridoMs()) + "] "
                    + preencher(ator, 14) + " · " + mensagem);
        }
    }

    /** Escreve uma linha solta (cabecalhos, menus, relatorio). */
    public static void linha(String texto) {
        synchronized (TRAVA) {
            SAIDA.println(texto);
        }
    }

    /** Formata milissegundos como mm:ss.SSS */
    public static String carimbo(long ms) {
        long minutos = ms / 60_000L;
        long segundos = (ms % 60_000L) / 1_000L;
        long milis = ms % 1_000L;
        return String.format("%02d:%02d.%03d", minutos, segundos, milis);
    }

    private static String preencher(String texto, int largura) {
        if (texto.length() >= largura) {
            return texto;
        }
        return texto + " ".repeat(largura - texto.length());
    }
}
