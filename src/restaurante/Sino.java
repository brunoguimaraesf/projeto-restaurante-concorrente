package restaurante;

/**
 * Sino do balcao: o AutoResetEvent do C#, que Java nao tem, feito com
 * wait/notify. Quem acorda consome o aviso, e toques seguidos viram um so.
 */
public final class Sino {

    private boolean tocou;

    public synchronized void tocar() {
        tocou = true;
        notify();
    }

    /** Dorme ate o sino tocar e zera o aviso. */
    public synchronized void esperar() throws InterruptedException {
        while (!tocou) {
            wait();  // while, nao if: protege de wake-up espurio
        }
        tocou = false;
    }
}
