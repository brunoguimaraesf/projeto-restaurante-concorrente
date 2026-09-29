# Restaurante Concorrente

Projeto prático de **Desenvolvimento para Concorrência** — 7º Período · UniRV.
Simulação de um restaurante onde atendentes geram pedidos, vários cozinheiros
preparam ao mesmo tempo e (a partir da Entrega 2) disputam fornos, utensílios e
ingredientes.

> **Linguagem:** Java (em vez de C#/.NET). O mapeamento de cada ferramenta de
> concorrência do enunciado para o equivalente em Java está na tabela abaixo.

Repositório: https://github.com/brunoguimaraesf/projeto-restaurante-concorrente

## Integrantes

- *(preencher)* Nome do integrante 1 — matrícula
- *(preencher)* Nome do integrante 2 — matrícula

## Status das entregas

| Entrega | Conteúdo | Status |
|---|---|---|
| 1 — 28/09 | Esqueleto, fila de pedidos, atendentes e cozinheiros | ✅ pronta |
| 2 — 01/10 | Fornos, tábua/faca, estoque, balcão, sino e garçom | ⬜ |
| 3 — 05/10 | Caixa, gerente/cancelamento, relatório e bugs | ⬜ |
| Final — 08/10 | README, ensaio e apresentação | ⬜ |

## Como executar

Precisa do **JDK 21 ou superior**. Para conferir: `java -version`.
Se não tiver, no Windows: `winget install --id EclipseAdoptium.Temurin.21.JDK -e`

### Windows (PowerShell)

```powershell
.\run.ps1           # 4 cozinheiros, 60 pedidos (padrão)
.\run.ps1 1 60      # 1 cozinheiro, 60 pedidos
.\run.ps1 8 60      # 8 cozinheiros, 60 pedidos
```

O `run.ps1` compila e executa. Se o `javac` não estiver no PATH — acontece
quando o terminal foi aberto antes de instalar o JDK — ele procura o JDK
sozinho em `JAVA_HOME`, Eclipse Adoptium, Java, Microsoft e Corretto.

### Linux / macOS

```bash
./run.sh
./run.sh 1 60
```

### Na mão (qualquer sistema)

```bash
javac -encoding UTF-8 -d out src/restaurante/*.java
java -cp out restaurante.Program 4 60
```

Parâmetros: `<quantidade de cozinheiros> <total de pedidos>`.
Sem parâmetros, usa 4 cozinheiros e 60 pedidos.

### Se der erro

| Erro | Solução |
|---|---|
| `run.ps1 não é reconhecido` | falta o `.\` na frente: `.\run.ps1` |
| `a execução de scripts foi desabilitada` | rode `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass` e tente de novo |
| `javac não é reconhecido` (rodando na mão) | feche e abra o terminal, ou use o `.\run.ps1`, que acha o JDK sozinho |
| acentos e `·` saindo errados | rode `chcp 65001` antes — o `run.ps1` já faz isso |

## O que a Entrega 1 já faz

| # | Requisito do enunciado | Onde está |
|---|---|---|
| 1 | 2 atendentes geram 60 pedidos aleatórios, em intervalos aleatórios | `Atendente.java` |
| 2 | Fila de pedidos com capacidade 10; cheia, o atendente espera | `FilaDePedidos.java` |
| 3 | N cozinheiros (configurável) consomem a fila ao mesmo tempo | `Cozinheiro.java`, `Restaurante.java` |
| 10 | Log de cada evento com horário e quem fez | `Log.java` |

Nesta etapa o preparo é só o `Thread.sleep` do prato — ainda **sem** forno,
utensílios ou estoque (isso é a Entrega 2).

## Saída de exemplo

Trecho do log com os 4 cozinheiros preparando ao mesmo tempo e o atendente
travado porque a fila encheu:

```
[00:02.065] Atendente 2    · Hamburguer #22 entrou na fila (10/10)
[00:02.138] Atendente 1    · anotou Hamburguer #23
[00:02.139] Atendente 1    · fila cheia (10/10) - esperando vaga para Hamburguer #23
[00:02.327] Cozinheiro 3   · Pizza #9 ficou pronto
[00:02.327] Cozinheiro 3   · pegou Hamburguer #13 - comecou o preparo (cozinhando agora: 4)
[00:02.327] Atendente 1    · Hamburguer #23 entrou na fila (10/10)
```

Repare na sequência das três últimas linhas: o atendente só consegue enfileirar
o pedido **no mesmo instante** em que um cozinheiro libera uma vaga.

E o resumo do fim, com as verificações automáticas:

```
===== RESUMO - Entrega 1 =====
Cozinheiros: 4 | Fila: capacidade 10

Pedidos gerados .............. 60
  Atendente 1 ................ 29
  Atendente 2 ................ 31
Pratos preparados ............ 60
  Cozinheiro 1 ............... 17  (10,6 s de fogao)
  Cozinheiro 2 ............... 13  (10,4 s de fogao)
  Cozinheiro 3 ............... 15  (10,6 s de fogao)
  Cozinheiro 4 ............... 15  (10,2 s de fogao)
Vezes que a fila encheu ...... 34
Pedidos sobrando na fila ..... 0

Tempo total .................. 11,1 s
Soma dos preparos ............ 41,7 s  (se fosse sequencial, seria esse o tempo)
Ganho da concorrencia ........ 3,77x

[OK]   todos os 60 pedidos foram gerados
[OK]   pedidos gerados = pratos preparados (nenhum pedido se perdeu)
[OK]   a fila terminou vazia
```

Os cozinheiros não preparam a mesma quantidade de pratos (17, 13, 15, 15) — e
isso é o esperado: quem pega um prato rápido volta antes para a fila. O que
precisa fechar é o total.

## Equivalências C# → Java

| Enunciado (C#) | Usado aqui (Java) | Observação |
|---|---|---|
| `Task` produtora/consumidora | `Thread` + `Runnable` | uma thread por atendente e por cozinheiro |
| `BlockingCollection<Pedido>(10)` | `ArrayBlockingQueue<Pedido>(10)` | fila limitada e bloqueante |
| `Add` / `Take` | `put` / `take` | bloqueiam quando cheia / vazia |
| `CompleteAdding()` | **não existe** → pedido *sentinela* | uma sentinela por cozinheiro (ver `FilaDePedidos.encerrar`) |
| `GetConsumingEnumerable()` | `take()` em laço até receber a sentinela | |
| `Interlocked.Increment` | `AtomicInteger.getAndIncrement()` | numeração dos pedidos sem race condition |
| `lock (obj) { ... }` | `synchronized (obj) { ... }` | usado no `Log` para as linhas não embaralharem |
| `SemaphoreSlim(2)` | `Semaphore(2)` | **Entrega 2** (fornos) |
| `ConcurrentDictionary` | `ConcurrentHashMap` | **Entrega 2** (estoque) |
| `ConcurrentQueue` + `AutoResetEvent` | `ConcurrentLinkedQueue` + `Semaphore`/`wait-notify` | **Entrega 2** (balcão e sino) |
| `CancellationTokenSource` | `Thread.interrupt()` / `AtomicBoolean` + `ScheduledExecutorService` | **Entrega 3** (gerente) |
| `Thread.Sleep` | `Thread.sleep` | preparo simulado |

## Estrutura do projeto

```
RestauranteConcorrente/
├── src/restaurante/     código-fonte (um arquivo por classe)
├── run.ps1              compila e executa no Windows
├── run.sh               compila e executa no Linux/macOS
└── out/                 .class gerados (fora do Git)
```

## Classes

| Classe | Responsabilidade |
|---|---|
| `Prato` | item do cardápio (nome, preço, tempo de preparo, forno, utensílios, ingredientes) — imutável |
| `Cardapio` | os quatro pratos e o sorteio aleatório |
| `Pedido` | número, prato e qual atendente anotou — imutável |
| `FilaDePedidos` | fila bloqueante de capacidade 10 e o encerramento do expediente |
| `Atendente` | produtor: gera pedidos em intervalos aleatórios |
| `Cozinheiro` | consumidor: retira da fila e prepara |
| `Log` | log de eventos com horário e responsável |
| `Restaurante` | monta tudo, controla a ordem do encerramento e imprime o resumo |
| `Program` | ponto de entrada e leitura dos parâmetros |

## Pontos de concorrência para explicar na apresentação

- **Produtor/consumidor com fila limitada.** A `ArrayBlockingQueue(10)` faz o
  freio (*back-pressure*): quando os cozinheiros não dão conta, a fila enche e o
  atendente fica bloqueado dentro do `put()` até abrir vaga. O resumo mostra
  quantas vezes isso aconteceu. Fila infinita não daria esse controle.
- **Bloqueio em vez de espera ocupada.** O cozinheiro parado no `take()` não
  gasta CPU: a thread dorme e o sistema operacional a acorda quando chega
  pedido. Um `while (fila.isEmpty())` queimaria um núcleo à toa.
- **Numeração dos pedidos.** Os dois atendentes dividem o mesmo alvo de 60
  pedidos usando `AtomicInteger.getAndIncrement()` — ler, somar e gravar em uma
  operação atômica só. Com um `int` comum (`numero++`) os dois poderiam ler o
  mesmo valor e sairiam pedidos repetidos ou menos de 60 no total: a race
  condition da Aula 2.
- **Encerramento em três passos.** `join()` nos dois atendentes → só então
  `fila.encerrar()` → `join()` nos cozinheiros. Fechar antes perderia pedidos;
  nunca fechar deixaria os cozinheiros bloqueados para sempre no `take()`
  (falha de *liveness*, e o programa não terminaria sozinho).
- **Por que sentinela e não uma flag booleana.** Uma flag `acabou = true` não
  acorda quem já está bloqueado dentro do `take()`. A sentinela entra na fila
  como um pedido comum, respeita a ordem FIFO (chega depois de todos os pedidos
  reais) e acorda exatamente um cozinheiro — por isso são N sentinelas para N
  cozinheiros.
- **Imutabilidade.** `Prato` e `Pedido` não mudam depois de criados, então
  atravessam a fila e são lidos por várias threads sem lock nenhum.
- **Log sincronizado.** `System.out.println` é thread-safe, mas duas chamadas
  seguidas não são atômicas entre si. A linha é montada inteira e impressa
  dentro de um `synchronized`, senão as mensagens saem embaralhadas.

## Concorrência × paralelismo (Aula 1)

Com 1 cozinheiro os 60 pedidos são preparados um atrás do outro; com N
cozinheiros eles são preparados **ao mesmo tempo** em núcleos diferentes:

```powershell
.\run.ps1 1 60
.\run.ps1 4 60
```

| Cozinheiros | Tempo total | Vezes que a fila encheu |
|---|---|---|
| 1 | 41,3 s | 46 |
| 4 | 11,1 s | 34 |

Os ~41 s de preparo não somem — eles são divididos entre os 4 cozinheiros. Por
isso o resumo imprime também a **soma dos preparos**: ela fica em ~41 s nos
dois casos, o que muda é o tempo de parede.

Com 1 cozinheiro a fila enche **mais** vezes: um consumidor só não dá conta de
dois produtores, e o gargalo aparece como o atendente esperando vaga.

(A opção de menu com essa comparação lado a lado é requisito da Entrega 3.)

## Ferramentas de IA usadas

- Claude (Anthropic) — geração inicial do código e da documentação.

