# Restaurante Concorrente

Projeto prático de **Desenvolvimento para Concorrência** — 7º Período · UniRV.

Linha de produção de um restaurante em C#: atendentes geram pedidos, cozinheiros
preparam disputando fornos, utensílios e estoque, e garçons entregam e registram
no caixa. Um gerente fecha a casa no prazo.

## Integrantes

- Bruno Gabriel Guimarães Fernandes
- João Víctor Severino da Silva

## Status das entregas

| Entrega | Conteúdo | Status |
|---|---|---|
| 1 — 28/09 | Esqueleto, fila de pedidos, atendentes e cozinheiros | ✅ |
| 2 — 01/10 | Fornos, tábua/faca, estoque, balcão, sino e garçom | ✅ |
| 3 — 05/10 | Caixa, gerente/cancelamento, relatório e bugs | ✅ |
| Final — 08/10 | README, ensaio e apresentação | ✅ |

## Como rodar

Precisa do SDK .NET 8 ou superior.

```bash
dotnet run
```

O menu abre no console. Também dá para chamar uma opção direto:

```bash
dotnet run -- 1                   # modo seguro
dotnet run -- 1 --fechamento=6    # fecha em 6s (sobram pedidos)
dotnet run -- 1 --cozinheiros=2   # muda o número de cozinheiros
dotnet run -- 2                   # bug 1
dotnet run -- 5                   # comparação 1 x N
```

## Menu (requisito 11)

| Opção | O que faz |
|---|---|
| 1 | Modo seguro: expediente completo + relatório com as três verificações |
| 2 | Bug 1 — race condition no caixa |
| 3 | Bug 2 — deadlock nos utensílios (**trava de propósito**) |
| 4 | Bug 3 — estoque negativo |
| 5 | Comparação 1 x 4 cozinheiros (60 pedidos, sem fechamento) |
| 0 | Sair |

Detalhando cada uma:

**1 — Modo seguro.** Roda o expediente completo com 2 atendentes, 4 cozinheiros,
2 fornos e 2 garçons, narrando cada evento no log. O gerente fecha em 15 s. No
fim imprime o relatório, e as três verificações dão `[OK]` em qualquer execução.
É o ponto de comparação para as demonstrações seguintes.

**2 — Bug 1, race condition no caixa.** Mesmo cenário, trocando só o `CaixaSeguro`
pelo `CaixaComRace`. O faturamento é somado sem `lock`, então vendas somem.
Termina com `[ERRO]` na verificação do faturamento.

**3 — Bug 2, deadlock nos utensílios.** A Salada pega tábua → faca e o Hambúrguer
pega faca → tábua. **O programa trava de propósito** e o relatório nunca aparece.
Depois de 5 segundos sem progresso, um vigia explica na tela o que aconteceu.
Encerre com Ctrl+C.

**4 — Bug 3, estoque negativo.** Troca o `EstoqueSeguro` pelo
`EstoqueSemAtomicidade`, que confere e desconta em passos separados. Termina com
`[ERRO]` e ingredientes negativos no estoque final.

**5 — Comparação.** Roda os mesmos 60 pedidos duas vezes, sem fechamento, com 1 e
com 4 cozinheiros, e mostra os dois tempos. Use `--cozinheiros=N` para comparar
1 × N em vez de 1 × 4.

**0 — Sair.**

## Onde cada requisito está

| # | Requisito | Onde |
|---|---|---|
| 1 | 2 atendentes, 60 pedidos aleatórios, intervalos aleatórios | `Atendente.cs` |
| 2 | Fila de capacidade 10; cheia, o atendente espera | `BlockingCollection` em `Restaurante.cs` |
| 3 | N cozinheiros consomem a fila ao mesmo tempo | `Cozinheiro.cs` |
| 4 | No máximo 2 pratos no forno | `SemaphoreSlim` em `Cozinha.cs` |
| 5 | Tábua e faca com um lock cada, sempre na mesma ordem | `Cozinha.UsarTabuaEFaca` |
| 6 | Estoque reserva tudo de uma vez e nunca fica negativo | `EstoqueSeguro` |
| 7 | Prato pronto vai ao balcão e o sino acorda o garçom | `Balcao.cs` + `Garcom.cs` |
| 8 | Caixa com lock no faturamento e AddOrUpdate nas vendas | `CaixaSeguro` |
| 9 | Gerente fecha após X segundos | `CancellationTokenSource` em `Restaurante.cs` |
| 10 | Log de cada evento com horário e autor | `Log.cs` |
| 11 | Menu no console | `Program.cs` |

Uma responsabilidade por classe. Os componentes que têm versão com bug ficam
atrás de interfaces (`ICaixa`, `IEstoque`), e a `Cozinha` recebe um sinalizador
de ordem invertida. É isso que permite rodar exatamente o mesmo cenário trocando
só a peça defeituosa.

## Decisões que valem explicar

**Dois garçons, não um.** Com um só, o caixa nunca é tocado por duas threads ao
mesmo tempo, e a race condition do requisito 8 não teria como acontecer nem para
ser demonstrada. Com dois garçons dividindo o balcão, o caixa vira um recurso
realmente disputado.

**O gerente e o prato atual (requisito 9).** O token vai para o `TryTake` da
fila, para o cozinheiro parar de *pegar* pedidos na hora do fechamento. Mas o
preparo em si (`Thread.Sleep`, forno, bancada) não olha o token: o prato que já
está na mão vai até o fim. Dá para ver no log:

```
[00:05.918] Atendente 2   · anotou Pizza #48 (fila: 10)
[00:06.004] >>> GERENTE: restaurante fechado. Atendentes param; cada cozinheiro termina o prato atual.
[00:06.013] Atendente 1   · encerrou com 17 pedido(s) anotado(s)
[00:06.016] Cozinheiro 3  · Lasanha #41 saiu do forno
[00:06.016] Cozinheiro 3  · Lasanha #41 pronto, foi para o balcao (sino)
```

(a linha `fila: 10` logo antes também mostra o requisito 2 funcionando: a fila
bateu no teto e o atendente ficou esperando.)

**O garçom espera com tempo limite.** O `EncerrarCozinha` toca o sino uma vez, e
`AutoResetEvent` libera um garçom por vez. Sem o limite de 200ms no
`EsperarSino`, o outro garçom dormiria para sempre e o programa não terminaria —
o problema de liveness que o enunciado avisa.

**O garçom esvazia o balcão inteiro.** Vários sinos seguidos viram um aviso só.
Por isso, ao acordar, o garçom roda um `while` até o balcão ficar vazio, em vez
de levar um prato e voltar a dormir.

## As três verificações (seção 7)

| Verificação | Pega qual bug |
|---|---|
| recebidos = entregues + recusados + não atendidos | pedidos sumindo |
| faturamento registrado = faturamento esperado | race no caixa |
| nenhum ingrediente com estoque negativo | estoque sem atomicidade |

O **faturamento esperado** é somado no fim, sequencialmente, percorrendo os
pedidos que os garçons efetivamente entregaram. Como esse cálculo não tem
concorrência nenhuma, ele serve de fonte de verdade contra o valor que o caixa
acumulou durante a execução.

Exemplo do modo seguro:

```
===== RELATÓRIO · Restaurante Concorrente =====
Modo: SEGURO | Cozinheiros: 4 | Fornos: 2 | Fechamento: 15 s

Pedidos recebidos ............ 60
Entregues .................... 58
Recusados (sem ingrediente) .. 2
Não atendidos (fechamento) ... 0

Vendas: Pizza 21 | Lasanha 10 | Salada 19 | Hamburguer 8
Faturamento esperado ......... R$ 1.983,00
Faturamento registrado ....... R$ 1.983,00
Estoque final: massa 9 | queijo 1 | tomate 0 | carne 22 | alface 21 | pao 32

[OK]   recebidos = entregues + recusados + não atendidos
[OK]   faturamento registrado = faturamento esperado
[OK]   nenhum ingrediente com estoque negativo
Tempo total: 16,1 s
```

## As três demonstrações de bug (seção 6)

Erro de concorrência só aparece quando existe disputa de verdade. Como o
enunciado autoriza, cada demonstração alarga a janela do erro — e, nos casos em
que a janela sozinha não bastava, também ajusta o cenário para que a disputa
aconteça. O que muda está documentado em cada caso.

### Bug 1 — race condition no caixa

`CaixaComRace` troca o `lock` por ler, dormir 20ms e gravar. Dois garçons leem o
mesmo valor e um sobrescreve o outro.

```
Faturamento esperado ......... R$ 1.983,00
Faturamento registrado ....... R$ 1.734,00

[ERRO] faturamento registrado = faturamento esperado
       -> o caixa perdeu R$ 249,00
```

Além da janela maior, esta demonstração usa 4 garçons e pedidos em rajada: com o
ritmo normal um garçom esvazia o balcão sozinho enquanto o outro dorme, e os dois
nunca registram ao mesmo tempo.

**Conserto:** `lock` em volta de ler, somar e gravar (`CaixaSeguro`).

### Bug 2 — deadlock nos utensílios

A Salada pega a tábua e depois a faca; o Hambúrguer pega a faca e depois a
tábua, com um `Thread.Sleep(100)` entre as duas para garantir que cada um pegue
o seu primeiro recurso antes de pedir o segundo. Nesse modo os pedidos alternam
Salada e Hambúrguer, para que os dois pratos estejam sempre na cozinha ao mesmo
tempo e o travamento aconteça em toda apresentação.

O programa **trava** e o relatório nunca aparece. Um vigia percebe que ninguém
avançou por 5 segundos e explica na tela o que houve — ele não conserta nada,
só evita que a apresentação vire uma tela parada sem explicação. Encerre com
Ctrl+C.

**Conserto:** ordem fixa, tábua sempre antes da faca. Com todo mundo pegando na
mesma ordem o ciclo de espera não se forma.

### Bug 3 — estoque negativo

`EstoqueSemAtomicidade` verifica se há ingrediente e desconta em passos
separados, sem proteção.

```
Estoque final: massa 0 | queijo 0 | tomate -1 | carne -2 | alface 0 | pao -1

[ERRO] nenhum ingrediente com estoque negativo
       -> negativos: tomate -1 | carne -2 | pao -1
```

Esta demonstração precisou de dois ajustes além da janela de 50ms, porque o erro
só pode nascer no instante exato em que um ingrediente passa de 1 para 0:

- **estoque inicial de 6** em vez de 40, para que esse instante chegue logo no
  começo, quando ainda há muita gente pedindo;
- **atendentes em rajada** e 8 cozinheiros, porque no ritmo normal os cozinheiros
  ficam famintos esperando pedido e nunca chegam juntos no estoque — o gargalo
  vira o atendente e a disputa não acontece.

**Conserto:** reservar todos os ingredientes do prato dentro de um único `lock`,
conferindo tudo antes de descontar qualquer coisa (`EstoqueSeguro`). Vale notar
que o `ConcurrentDictionary` sozinho não resolve: ele garante que cada chamada é
segura, não que a sequência "verificar e descontar" seja atômica. Coleção segura
não é o mesmo que lógica segura.

## Comparação 1 x N cozinheiros (requisito 11)

Os mesmos 60 pedidos, sem fechamento, com 1 e com N cozinheiros. Os pratos são
sorteados uma vez só, com semente fixa, antes de qualquer thread começar — então
as duas rodadas recebem exatamente a mesma carga.

```
Comparação · 60 pedidos, sem fechamento

1 cozinheiro ...... 41,4 s
4 cozinheiros ..... 16,1 s

Ganho: 2,56x com 4 cozinheiros.
```

O ganho não chega a 4x porque os cozinheiros disputam recursos: só cabem 2 pratos
no forno ao mesmo tempo, e a tábua e a faca são usadas por um de cada vez. Essas
partes continuam sendo feitas em série por mais gente que se contrate, que é o
que a Lei de Amdahl descreve.

## Ferramentas de IA usadas

| Ferramenta | Usada para |
|---|---|
| Claude (Claude Code) | Escrever as classes em C#, calibrar as demonstrações de bug e redigir este README |

O enunciado permite gerar o código com IA, então o cuidado foi não aceitar nada
no papel: cada comportamento foi conferido rodando o programa e medindo o
resultado. O que foi verificado:

- **Modo seguro**: 7 execuções, todas com as três verificações `[OK]` — incluindo
  com 8 cozinheiros e com fechamento curto, para forçar mais disputa e deixar
  pedidos não atendidos.
- **Bug 1 (caixa)**: na primeira versão o erro aparecia em apenas 2 de 4
  execuções. A janela estava em 1 ms, e os pratos ficam prontos a cada ~85 ms,
  então dois registros quase nunca caíam juntos. Com janela de 20 ms, 4 garçons e
  pedidos em rajada, passou a aparecer em 6 de 6.
- **Bug 3 (estoque)**: começou aparecendo em 1 de 5 execuções. O negativo só nasce
  no instante em que um ingrediente passa de 1 para 0, e com estoque 40 esse
  instante só chegava no fim do expediente. Diminuir o estoque e aumentar os
  cozinheiros não bastou — o gargalo era o atendente, e os cozinheiros ficavam
  ociosos esperando pedido em vez de disputar o estoque. Só com os pedidos em
  rajada passou a aparecer em 6 de 6.
- **Requisito 9 (fechamento)**: a primeira versão deixava o cozinheiro pegar um
  pedido **novo** 12 ms depois do fechamento, porque o token não era passado ao
  `TryTake`. Corrigido e confirmado em 5 execuções: zero pedidos pegos após o
  fechamento, e o prato que já estava no forno sempre termina.

Essas medições estão descritas em cada seção acima, junto do motivo de cada
ajuste.
