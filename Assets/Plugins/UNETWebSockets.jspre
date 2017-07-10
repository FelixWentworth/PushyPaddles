// overwrite the unity webgl player _JS_UNETWebSockets_SocketCreate method
// intercept the generated host:port url and replace with value from window.UNETWebSocketURL if set
// this works around the secure websocket and relative path limitations of NetworkClient

Object.defineProperty(Module, "asmLibraryArg", {
	set: function (value) {
		value._JS_UNETWebSockets_SocketCreate = function (hostId, url) {
				var str = Pointer_stringify(url);
				console.log("_JS_UNETWebSockets_SocketCreate url: " + str + ", UNETWebSocketURL: " + window.UNETWebSocketURL);
				var socket = {
						socket: new WebSocket(window.UNETWebSocketURL || str, ["unitygame"]),
						buffer: new Uint8Array(0),
						error: null,
						id: hostId,
						state: UNETWebSocketsInstances.HostStates.Created,
						inQueue: false,
						messages: []
				};
				socket.socket.onopen = (function() {
						socket.state = UNETWebSocketsInstances.HostStates.Opening;
						_JS_UNETWebSockets_HostsContainingMessagesPush(socket)
				});
				socket.socket.onmessage = (function(e) {
						if (e.data instanceof Blob) {
								var reader = new FileReader;
								reader.addEventListener("loadend", (function() {
										var array = new Uint8Array(reader.result);
										_JS_UNETWebSockets_HostsContainingMessagesPush(socket);
										socket.messages.push(array)
								}));
								reader.readAsArrayBuffer(e.data)
						}
				});
				socket.socket.onclose = (function(e) {
						if (socket.state == UNETWebSocketsInstances.HostStates.Closed) return;
						socket.state = UNETWebSocketsInstances.HostStates.Closing;
						_JS_UNETWebSockets_HostsContainingMessagesPush(socket)
				});
				socket.socket.onerror = (function(e) {
						console.log("Error: " + e.data + " socket will be closed");
						socket.state = UNETWebSocketsInstances.HostStates.Closing;
						_JS_UNETWebSockets_HostsContainingMessagesPush(socket)
				});
				UNETWebSocketsInstances.hosts[socket.id] = socket
		}
		Module._asmLibraryArg = value;
	},
	get: function () {
		return Module._asmLibraryArg;
	},
});