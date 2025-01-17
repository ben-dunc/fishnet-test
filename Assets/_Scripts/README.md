# How to build, WebGL
1. Use bayou transport.
2. Use 7770 port on both bayou transport and edgegap hosting.
3. Disable auto-connect scripts for client
4. Enable "Start on Headless" in ServerManager component.
    a. Make sure to start docker! Just open & close the docker app.
5. Build & deploy Edgegap container.
6. Copy url & port into Bayou component. The url goes to "client address", and the port goes to server "Port"
7. Play!