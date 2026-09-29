# Compila e executa o Restaurante Concorrente (Windows / PowerShell).
# Uso:  .\run.ps1            -> 4 cozinheiros, 60 pedidos
#       .\run.ps1 1 60       -> 1 cozinheiro, 60 pedidos
$ErrorActionPreference = "Stop"

# Console em UTF-8 para o separador "·" sair certo.
chcp 65001 > $null
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Acha o JDK mesmo que o terminal tenha sido aberto antes da instalacao
# (nesse caso o PATH do terminal ainda esta desatualizado).
function Find-JdkBin {
    if (Get-Command javac -ErrorAction SilentlyContinue) { return $null }

    $candidatos = @()
    if ($env:JAVA_HOME) { $candidatos += (Join-Path $env:JAVA_HOME "bin") }
    $javaHomeMaquina = [Environment]::GetEnvironmentVariable("JAVA_HOME", "Machine")
    if ($javaHomeMaquina) { $candidatos += (Join-Path $javaHomeMaquina "bin") }

    $pastas = @(
        "$env:ProgramFiles\Eclipse Adoptium",
        "$env:ProgramFiles\Java",
        "$env:ProgramFiles\Microsoft",
        "$env:ProgramFiles\Amazon Corretto",
        "${env:ProgramFiles(x86)}\Java"
    )
    foreach ($pasta in $pastas) {
        if (Test-Path $pasta) {
            Get-ChildItem $pasta -Directory -ErrorAction SilentlyContinue |
                ForEach-Object { $candidatos += (Join-Path $_.FullName "bin") }
        }
    }

    foreach ($c in $candidatos) {
        if (Test-Path (Join-Path $c "javac.exe")) { return $c }
    }
    return "NAO_ENCONTRADO"
}

$jdkBin = Find-JdkBin
if ($jdkBin -eq "NAO_ENCONTRADO") {
    Write-Host "JDK nao encontrado." -ForegroundColor Red
    Write-Host "Instale o JDK 21:  winget install --id EclipseAdoptium.Temurin.21.JDK -e"
    Write-Host "Depois feche e abra o VS Code de novo."
    exit 1
}
if ($jdkBin) {
    $env:Path = "$jdkBin;$env:Path"
    Write-Host "Usando o JDK em $jdkBin" -ForegroundColor DarkGray
}

$raiz = Split-Path -Parent $MyInvocation.MyCommand.Definition
$saida = Join-Path $raiz "out"
$fontes = Get-ChildItem -Path (Join-Path $raiz "src") -Recurse -Filter *.java | ForEach-Object { $_.FullName }

Write-Host "Compilando..." -ForegroundColor DarkGray
javac -encoding UTF-8 -d $saida $fontes
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

java "-Dstdout.encoding=UTF-8" "-Dfile.encoding=UTF-8" -cp $saida restaurante.Program @args
