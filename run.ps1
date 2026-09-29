# Compila e executa o Restaurante Concorrente (Windows / PowerShell).
# Uso:  .\run.ps1            -> 4 cozinheiros, 60 pedidos
#       .\run.ps1 1 60       -> 1 cozinheiro, 60 pedidos
$ErrorActionPreference = "Stop"

# Console em UTF-8 para os acentos e o separador "·" sairem certos.
chcp 65001 > $null
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$raiz = Split-Path -Parent $MyInvocation.MyCommand.Definition
$saida = Join-Path $raiz "out"
$fontes = Get-ChildItem -Path (Join-Path $raiz "src") -Recurse -Filter *.java | ForEach-Object { $_.FullName }

Write-Host "Compilando..." -ForegroundColor DarkGray
javac -encoding UTF-8 -d $saida $fontes
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

java "-Dstdout.encoding=UTF-8" "-Dfile.encoding=UTF-8" -cp $saida restaurante.Program @args
