#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")"

export PATH="$PATH:$HOME/.dotnet/tools"
command -v reportgenerator >/dev/null || dotnet tool install -g dotnet-reportgenerator-globaltool

find tests -name TestResults -type d -prune -exec rm -rf {} + 2>/dev/null || true

dotnet test --nologo --settings coverlet.runsettings

reportgenerator \
  -reports:'tests/**/coverage.cobertura.xml' \
  -targetdir:TestResults/coverage \
  -reporttypes:'Html;TextSummary'

echo
echo '--- Summary ---'
cat TestResults/coverage/Summary.txt | sed -n '1,25p'
echo
echo "HTML: $(pwd)/TestResults/coverage/index.html"

if [[ "${OSTYPE:-}" == darwin* ]]; then
  open TestResults/coverage/index.html
fi
