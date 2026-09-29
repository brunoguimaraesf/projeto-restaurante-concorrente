#!/usr/bin/env bash
# Compila e executa o Restaurante Concorrente (Linux / macOS).
# Uso:  ./run.sh            -> 4 cozinheiros, 60 pedidos
#       ./run.sh 1 60       -> 1 cozinheiro, 60 pedidos
set -euo pipefail

raiz="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

echo "Compilando..."
find "$raiz/src" -name "*.java" -print0 | xargs -0 javac -encoding UTF-8 -d "$raiz/out"

java -Dstdout.encoding=UTF-8 -Dfile.encoding=UTF-8 -cp "$raiz/out" restaurante.Program "$@"
