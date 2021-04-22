# Port

Port is a Kubernetes port forward manager that keeps port forwarding connections persistent. 
It comes both with a CLI and a UI.

The application is still in early alpha and currently supports port forwarding and comes with a simple web UI.

![Continuous Delivery](https://github.com/Fresa/Port/workflows/Continuous%20Delivery/badge.svg)

## Installation
The web server is released as a self contained executable.

Have a look at the [release section](https://github.com/Fresa/Port/releases) to find the latest release tag and replace `<VERSION>` with it below.

### Windows
```ps
$VERSION="<VERSION>"
Invoke-WebRequest -Uri https://github.com/Fresa/Port/releases/download/$VERSION/port-$VERSION-win-x64.zip -O port.zip
Get-Item port.zip | Expand-Archive -DestinationPath "$env:LOCALAPPDATA\Port"
Remove-Item port.zip
```
### Linux
```bash
VERSION="<VERSION>"; \
sudo curl -Lo port.zip https://github.com/Fresa/Port/releases/download/$VERSION/port-$VERSION-linux-x64.zip; \
sudo unzip -d /usr/local/bin/port/ port.zip; \
sudo rm ./port.zip
```

## Usage
The server is started by executing the executable.

### Windows
```ps
.\Port.Server.exe
```
### Linux
```bash
./Port.Server
```

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License
[MIT](https://github.com/Fresa/Port/blob/master/LICENSE)
