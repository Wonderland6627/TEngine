#!/bin/bash

cd "$(dirname "$0")"
echo "当前目录: $(pwd)"

export WORKSPACE="$(realpath ../../)"
export LUBAN_DLL="${WORKSPACE}/Tools/Luban/Luban.dll"
export CONF_ROOT="$(pwd)"
export DATA_OUTPATH="${WORKSPACE}/../piratecat_slime_express/config/luban/data"
export CODE_OUTPATH="${WORKSPACE}/../piratecat_slime_express/config/luban/gen"

dotnet "${LUBAN_DLL}" \
    -t server \
    -c javascript-json \
    -d json \
    --conf "${CONF_ROOT}/luban.conf" \
    -x outputCodeDir="${CODE_OUTPATH}" \
    -x outputDataDir="${DATA_OUTPATH}"

echo "操作完成，按任意键退出..."
read -k1
