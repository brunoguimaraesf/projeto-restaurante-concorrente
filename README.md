# Restaurante Concorrente

Projeto prático de **Desenvolvimento para Concorrência** — 7º Período · UniRV.
Simulação de um restaurante onde atendentes geram pedidos, vários cozinheiros
preparam ao mesmo tempo disputando fornos, utensílios e ingredientes, e um
garçom entrega os pratos.

> **Linguagem:** Java (em vez de C#/.NET). O mapeamento de cada ferramenta de
> concorrência do enunciado para o equivalente em Java está na tabela abaixo.

Repositório: https://github.com/brunoguimaraesf/projeto-restaurante-concorrente

## Integrantes

- Bruno Gabriel Guimarães Fernandes
- João Víctor Severino da Silva 

## Status das entregas

| Entrega | Conteúdo | Status |
|---|---|---|
| 1 — 28/09 | Esqueleto, fila de pedidos, atendentes e cozinheiros | ✅ pronta |
| 2 — 01/10 | Fornos, tábua/faca, estoque, balcão, sino e garçom | ✅ pronta |
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

## Requisitos implementados

| # | Requisito do enunciado | Onde está |
|---|---|---|
| 1 | 2 atendentes geram 60 pedidos aleatórios, em intervalos aleatórios | `Atendente.java` |
| 2 | Fila de pedidos com capacidade 10; cheia, o atendente espera | `FilaDePedidos.java` |
| 3 | N cozinheiros (configurável) consomem a fila ao mesmo tempo | `Cozinheiro.java`, `Restaurante.java` |
| 4 | No máximo 2 pratos no forno ao mesmo tempo | `Cozinha.assar` |
| 5 | Tábua e faca têm um lock cada, pegos sempre na mesma ordem | `Cozinha.montar` |
| 6 | O estoque reserva todos os ingredientes de uma vez e nunca fica negativo | `Estoque.reservar` |
| 7 | O prato pronto vai para o balcão e o sino acorda o garçom | `Balcao.java`, `Sino.java`, `Garcom.java` |
| 10 | Log de cada evento com horário e quem fez | `Log.java` |

Faltam os requisitos 8 (caixa), 9 (gerente/cancelamento) e 11 (menu), que são
da Entrega 3.

## Como o pedido anda pelo sistema

```
Atendente ──► Fila (cap. 10) ──► Cozinheiro ──► Estoque: reserva atômica
                                                   │
                                      sem ingrediente ──► RECUSADO
                                                   │
                                       Pizza/Lasanha ──► Forno (2 vagas)
                                       Salada/Burger ──► tábua ──► faca
                                                   │
                                                   ▼
                                          Balcão + sino ──► Garçom ──► entregue
```

Todo pedido termina **entregue** ou **recusado** — é uma das verificações
automáticas do relatório.

## Saída de exemplo

Os dois fornos ocupados e, ao mesmo tempo, outro cozinheiro na tábua e faca:

```
[00:03.224] Cozinheiro 2   · pegou a tabua para Salada #12
[00:03.225] Cozinheiro 2   · pegou a faca - montando Salada #12
[00:03.230] Cozinheiro 1   · Lasanha #14 entrou no forno (2/2)
[00:03.879] Cozinheiro 2   · Lasanha #15 esperando forno livre (2/2)
```

Pedido recusado por falta de ingrediente:

```
[00:14.955] Cozinheiro 2   · Pizza #52 RECUSADO - acabou o(a) queijo
[00:14.955] Cozinheiro 2   · Pizza #53 RECUSADO - acabou o(a) queijo
```

Garçom esvaziando o balcão — repare no `restam 2`, que é o balcão acumulando
enquanto ele leva um prato até a mesa:

```
[00:02.672] Garcom         · entregou Pizza #9 (restam 2 no balcao)
[00:02.793] Garcom         · entregou Lasanha #8 (restam 1 no balcao)
```

Relatório final:

```
===== RELATORIO - Entrega 2 =====
Cozinheiros: 4 | Fornos: 2 | Fila: capacidade 10

Pedidos recebidos .............. 60
  Atendente 1 .................. 30
  Atendente 2 .................. 30
Preparados ..................... 53
  Cozinheiro 1 ................. 13  (recusou 2, 13,2 s de preparo)
  Cozinheiro 2 ................. 13  (recusou 4, 12,5 s de preparo)
  Cozinheiro 3 ................. 12  (recusou 1, 12,4 s de preparo)
  Cozinheiro 4 ................. 15  (recusou 0, 13,0 s de preparo)
Entregues pelo garcom .......... 53
Recusados (sem ingrediente) .... 7

Estoque final: massa 14 | queijo 0 | tomate 10 | carne 17 | alface 27 | pao 26

Vezes que a fila encheu ........ 37
Pedidos sobrando na fila ....... 0
Pratos sobrando no balcao ...... 0
Tempo total .................... 13,8 s
Soma dos preparos .............. 51,1 s
Paralelismo medio .............. 3,71x

[OK]   recebidos = entregues + recusados
[OK]   nenhum ingrediente com estoque negativo
[OK]   fila e balcao terminaram vazios
[OK]   todo prato preparado foi entregue (nenhum esquecido no balcao)
```

O **queijo zera** quase sempre: ele entra em 3 dos 4 pratos, então 60 pedidos
pedem ~45 unidades e o estoque só tem 40. É por isso que aparecem recusas — e
é de propósito, como diz o enunciado.

## Equivalências C# → Java

| Enunciado (C#) | Usado aqui (Java) | Onde |
|---|---|---|
| `Task` produtora/consumidora | `Thread` + `Runnable` | `Atendente`, `Cozinheiro`, `Garcom` |
| `BlockingCollection<Pedido>(10)` | `ArrayBlockingQueue<Pedido>(10)` | `FilaDePedidos` |
| `Add` / `Take` | `put` / `take` | `FilaDePedidos` |
| `CompleteAdding()` | **não existe** → pedido *sentinela* | `FilaDePedidos.encerrar` |
| `GetConsumingEnumerable()` | `take()` em laço até a sentinela | `Cozinheiro.run` |
| `Interlocked.Increment` | `AtomicInteger.getAndIncrement()` | `Atendente` |
| `lock (obj) { ... }` | `synchronized (obj) { ... }` | `Cozinha`, `Estoque`, `Log` |
| `SemaphoreSlim(2)` | `Semaphore(2, true)` | `Cozinha.assar` |
| `ConcurrentDictionary` | `ConcurrentHashMap` | `Estoque` |
| `ConcurrentQueue` | `ConcurrentLinkedQueue` | `Balcao` |
| `AutoResetEvent` | **não existe** → `wait`/`notify` | `Sino` |
| `CancellationTokenSource` | `Thread.interrupt()` / `AtomicBoolean` | **Entrega 3** |
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
| `Prato` | item do cardápio (preço, preparo, forno, utensílios, ingredientes) — imutável |
| `Cardapio` | os quatro pratos e o sorteio aleatório |
| `Pedido` | número, prato e qual atendente anotou — imutável |
| `FilaDePedidos` | fila bloqueante de capacidade 10 e o encerramento do expediente |
| `Atendente` | produtor: gera pedidos em intervalos aleatórios |
| `Cozinheiro` | consumidor: reserva ingredientes, prepara e manda para o balcão |
| `Estoque` | 40 unidades de cada ingrediente, com reserva atômica |
| `Cozinha` | os 2 fornos (semáforo) e a tábua e a faca (locks) |
| `Balcao` | fila de pratos prontos |
| `Sino` | sinalização entre cozinheiros e garçom (o `AutoResetEvent`) |
| `Garcom` | dorme no sino e esvazia o balcão |
| `Log` | log de eventos com horário e responsável |
| `Restaurante` | monta tudo, controla a ordem do encerramento e imprime o relatório |
| `Program` | ponto de entrada e leitura dos parâmetros |

## Pontos de concorrência para explicar na apresentação

### Produtor/consumidor com fila limitada

A `ArrayBlockingQueue(10)` faz o freio (*back-pressure*): quando os cozinheiros
não dão conta, a fila enche e o atendente fica bloqueado dentro do `put()` até
abrir vaga. O relatório mostra quantas vezes isso aconteceu. Fila infinita não
daria esse controle.

### Bloqueio em vez de espera ocupada

O cozinheiro parado no `take()` e o garçom parado no `wait()` não gastam CPU: a
thread dorme e o sistema operacional a acorda. Um `while (fila.isEmpty())`
queimaria um núcleo à toa.

### Numeração dos pedidos

Os dois atendentes dividem o mesmo alvo usando `AtomicInteger.getAndIncrement()`
— ler, somar e gravar em uma operação só. Com um `int` comum (`numero++`) os
dois poderiam ler o mesmo valor e sairiam números repetidos: a race condition
da Aula 2.

### Semáforo nos fornos

`Semaphore(2)` é um contador de vagas, não um lock: ele deixa **duas** threads
passarem ao mesmo tempo e segura a terceira no `acquire()`. O `release()` fica
num `finally` — um forno travado para sempre travaria a cozinha inteira. Usamos
a versão *fair* (`new Semaphore(2, true)`), que atende na ordem da fila e evita
starvation de quem chegou primeiro.

### Ordem fixa nos utensílios

Tábua e faca são dois locks separados, sempre pegos na ordem **tábua → faca**.
Se um prato pegasse na ordem inversa, dois cozinheiros poderiam segurar cada um
metade do que o outro precisa, e nenhum solta: deadlock. Essa é exatamente a
demonstração de bug da Entrega 3.

Detalhe que costuma virar pergunta: o `Thread.sleep` dentro do `synchronized`
**não solta o lock** (diferente do `wait()`). É proposital — os utensílios ficam
ocupados durante todo o preparo.

### Reserva atômica no estoque

`ConcurrentHashMap` garante atomicidade **por chave**, mas o prato precisa de
várias chaves de uma vez. Verificar todas e depois descontar são dois passos, e
entre um e outro outra thread pode descontar — estoque negativo. Por isso a
reserva inteira roda dentro de um `synchronized`: ou reserva todos os
ingredientes, ou não reserva nenhum. É o "coleção segura ≠ lógica segura" da
Aula 6.

### O sino junta avisos

`Sino` reproduz o `AutoResetEvent`: quem acorda consome o aviso e o sino volta a
ficar mudo. Vários toques seguidos viram **um** aviso só. Por isso o garçom
esvazia o balcão **inteiro** cada vez que acorda — se pegasse um prato por
toque, pratos ficariam esquecidos. O log mostra isso no `restam N no balcao`.

O `wait()` está dentro de um `while (!tocou)`, não de um `if`: protege contra
*spurious wakeup*, quando a thread acorda sem ninguém ter chamado `notify`.

### Encerramento em cinco passos

```
join atendentes → encerrar fila → join cozinheiros → fechar balcão → join garçom
```

Cada passo depende do anterior. Fechar a fila antes perderia pedidos; não
fechar deixaria os cozinheiros bloqueados para sempre no `take()`; não fechar o
balcão deixaria o garçom dormindo no `wait()` e o programa nunca terminaria
(falha de *liveness*).

### Por que sentinela e não uma flag booleana

Uma flag `acabou = true` não acorda quem já está bloqueado dentro do `take()`.
A sentinela entra na fila como um pedido comum, respeita a ordem FIFO (chega
depois de todos os pedidos reais) e acorda exatamente um cozinheiro — por isso
são N sentinelas para N cozinheiros.

### Imutabilidade

`Prato` e `Pedido` não mudam depois de criados, então atravessam a fila e o
balcão e são lidos por várias threads sem lock nenhum.

### Log sincronizado

`System.out.println` é thread-safe, mas duas chamadas seguidas não são atômicas
entre si. A linha é montada inteira e impressa dentro de um `synchronized` — e
o horário é lido **dentro** do bloco, senão duas threads poderiam ler o relógio
em uma ordem e imprimir em outra.

## Concorrência × paralelismo (Aula 1)

Com 1 cozinheiro os pedidos são preparados um atrás do outro; com N cozinheiros
eles são preparados **ao mesmo tempo** em núcleos diferentes:

```powershell
.\run.ps1 1 60
.\run.ps1 4 60
```

| Cozinheiros | Tempo total |
|---|---|
| 1 | 41,8 s |
| 2 | 21,8 s |
| 4 | 14,6 s |
| 8 | 16,4 s |

De 1 para 4 o tempo cai quase 3x. De 4 para 8 **não melhora** — e esse é o ponto
interessante: os recursos compartilhados viram o gargalo. São só 2 fornos e um
único par tábua/faca, então a partir de certo número de cozinheiros eles só
ficam esperando uns aos outros. Mais threads não é sinônimo de mais velocidade.

(A opção de menu com essa comparação lado a lado é requisito da Entrega 3.)

## Ferramentas de IA usadas

- Claude (Anthropic) — geração inicial do código e da documentação.
