// implement an external method to return the path to the streaming asset directory relative to the web application root
// this depends on the function in PlayGen.UnitProxy, but must be a jslib plugin or there is no way to return a value synchronously

var WebGLStreamingAssetsLocatorPlugin = { 
	GetStreamingAssetsPath: function () {
		var debug = true;
		debug && console.debug("GetStreamingAssetsPath");

		var path = window.PlayGen.UnityProxy.GetApplicationBase() + window.PlayGen.UnityProxy.Config.StreamingAssetsPath;

		var buffer = _malloc(lengthBytesUTF8(path) + 1);
			
		debug && console.debug("HttpRequest::writeStringToMemory");
		writeStringToMemory(path, buffer);
		return buffer;
	}
};

mergeInto(LibraryManager.library, WebGLStreamingAssetsLocatorPlugin);
