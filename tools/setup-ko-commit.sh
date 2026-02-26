#!/usr/bin/env bash
set -euo pipefail

# Automatically configure a Korean commit template for this repository
TOPLEVEL=$(git rev-parse --show-toplevel 2>/dev/null || true)
if [ -z "$TOPLEVEL" ]; then
  echo "Not inside a git repository." 1>&2
  exit 1
fi

TEMPLATE_REL=".gittemplates/ko_commit.template"
ABS_TEMPLATE="$TOPLEVEL/$TEMPLATE_REL"
mkdir -p "$TOPLEVEL/.gittemplates"

# Write the template content if not already present or different
if [ ! -f "$ABS_TEMPLATE" ]; then
  cat > "$ABS_TEMPLATE" << 'EOF'
로그인 흐름 버그 수정
네트워크 응답 처리 및 UI 로직 개선을 포함합니다.

- 로그인 입력 검증 강화
- 서버 응답 파싱 안정성 개선
- 예외 처리 및 로그 추가

# 커밋 본문에는 변경 이유와 주요 변경점을 기술합니다.
# 필요 시 이슈 번호를 참조합니다. 예: #123
EOF
  echo "Created commit template at $ABS_TEMPLATE"
else
  echo "Commit template already exists at $ABS_TEMPLATE"
fi

git -C "$TOPLEVEL" config commit.template "$ABS_TEMPLATE"
echo "Configured local git to use Korean commit template: $ABS_TEMPLATE"
