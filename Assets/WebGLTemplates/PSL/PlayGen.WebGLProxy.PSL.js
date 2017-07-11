(function() {
	if (!window.PlayGen) {
		window.PlayGen = {};
	}
	if (!window.PlayGen.PSL) {
		window.PlayGen.PSL = {};
	}

	window.PlayGen.PSL.PSLClient = new PSLClient();
        
	function onAPIInitialised(result)
	{
		window.PlayGen.PSL.GameInfo = window.PlayGen.PSL.PSLClient.getGameInfo();
	}

	window.addEventListener('load', function() {
		window.PlayGen.PSL.PSLClient.initialise(null, null, onAPIInitialised);
	});

	window.PlayGen.PSL.SetGameInfo = function() {
		window.PlayGen.GameInstance.SendMessage("WebGLProxy", "OnPSLGameInfoLoaded", JSON.stringify(window.PlayGen.PSL.GameInfo));
	}
})();
