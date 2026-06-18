#!/bin/bash

set -e

echo "========================================="
echo " Validación General Proyecto .NET"
echo "========================================="

ERRORS=0

check() {
    if [ $? -eq 0 ]; then
        echo "[OK] $1"
    else
        echo "[ERROR] $1"
        ERRORS=$((ERRORS + 1))
    fi
}

echo ""
echo "1. Verificando .NET SDK..."
dotnet --version > /dev/null 2>&1
check ".NET SDK instalado"

echo ""
echo "2. Restaurando paquetes..."
dotnet restore
check "Restore completado"

echo ""
echo "3. Compilando solución..."
dotnet build --configuration Release --no-restore
check "Build exitoso"

echo ""
echo "4. Ejecutando pruebas..."
if find . -name "*Tests.csproj" | grep -q .; then
    dotnet test --configuration Release --no-build
    check "Tests ejecutados"
else
    echo "[WARN] No se encontraron proyectos de pruebas"
fi

echo ""
echo "5. Verificando formato de código..."
dotnet format --verify-no-changes > /dev/null 2>&1
check "Formato correcto"

echo ""
echo "6. Buscando secretos comunes..."
if grep -R -iE \
    "(password\s*=|apikey|api_key|secret|connectionstring)" \
    . \
    --exclude-dir=bin \
    --exclude-dir=obj \
    --exclude=validate.sh \
    >/dev/null 2>&1; then

    echo "[WARN] Posibles secretos encontrados"
else
    echo "[OK] No se detectaron secretos evidentes"
fi

echo ""
echo "7. Verificando appsettings..."

for file in $(find . -name "appsettings*.json"); do
    python3 -m json.tool "$file" >/dev/null 2>&1

    if [ $? -eq 0 ]; then
        echo "[OK] $file"
    else
        echo "[ERROR] JSON inválido: $file"
        ERRORS=$((ERRORS + 1))
    fi
done

echo ""
echo "========================================="

if [ $ERRORS -eq 0 ]; then
    echo "VALIDACIÓN COMPLETADA EXITOSAMENTE"
    exit 0
else
    echo "VALIDACIÓN FINALIZADA CON $ERRORS ERROR(ES)"
    exit 1
fi