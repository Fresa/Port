#!/bin/bash
: ${INSTALL_DIR:="/usr/local/bin"}
: ${USE_SUDO:="true"}

help () {
  echo "Accepted cli arguments are:"
  echo -e "\t[--help|-h ] ->> prints this help"
  echo -e "\t[--version|-v <desired_version>]"
  echo -e "\te.g. --version v3.0.0 or -v v3"
  echo -e "\t[--no-sudo]  ->> install without sudo"
}

runAsRoot() {
  if [ $EUID -ne 0 -a "$USE_SUDO" = "true" ]; then
    sudo "${@}"
  else
    "${@}"
  fi
}

cleanup() {
  if [[ -d "${PORT_TMP_ROOT:-}" ]]; then
    rm -rf "$PORT_TMP_ROOT"
  fi
}

set -u
while [[ $# -gt 0 ]]; do
  case $1 in
    '--version'|-v)
       shift
       if [[ $# -ne 0 ]]; then
           export VERSION="${1}"
       else
           echo -e "Please provide the desired version. e.g. --version v3.0.0 or -v v3"
           exit 0
       fi
       ;;
    '--no-sudo')
       USE_SUDO="false"
       ;;
    '--help'|-h)
       help
       exit 0
       ;;
    *) exit 1
       ;;
  esac
  shift
done
set +u

if [ -z "$VERSION" ]; then
  echo -e "Missing argument 'version'"
  help
  exit 1
fi

PORT_FOLDER=$INSTALL_DIR/port-bin/$VERSION
if [[ ! -d "$PORT_FOLDER" ]]; then
    runAsRoot mkdir -p $PORT_FOLDER
fi
if [[ $(ls -A $PORT_FOLDER) ]]; then
    echo "$PORT_FOLDER exists and is not empty, exiting"
    exit 1
fi

PORT_TMP_ROOT="$(mktemp -dt port-XXXXXX)"
curl -SL -o $PORT_TMP_ROOT/port.zip https://github.com/Fresa/Port/releases/download/$VERSION/port-$VERSION-linux-x64.zip
runAsRoot unzip -d $PORT_FOLDER $PORT_TMP_ROOT/port.zip

runAsRoot ln -f -s $PORT_FOLDER/Port.Server $INSTALL_DIR/port
echo "port installed into $PORT_FOLDER and available at $INSTALL_DIR/port"
cleanup
