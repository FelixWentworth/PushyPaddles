(function() {
	if (!window.PlayGen) {
		window.PlayGen = {};
	}
	if (!window.PlayGen.UnityProxy) {
		window.PlayGen.UnityProxy = {};
	}

	window.PlayGen.UnityProxy.WebGLLoaded = function() {
		console.log('Unity signalled WebGL Loaded');

		function unload() {
			console.debug('window unloading');
			service.EndSession();
			window.PlayGen.UnityProxy.BeforeWindowUnload();
		}

		window.addEventListener('beforeunload', unload);
		window.PlayGen.PSL.SetGameInfo()
	}

	window.PlayGen.UnityProxy.BeforeWindowUnload = function() {
		window.PlayGen.GameInstance.SendMessage('WebGLProxy', 'OnWindowBeforeUnload', '');
	}
})();
